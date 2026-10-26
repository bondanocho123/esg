using System.Security.Claims;
using GriForms.Application.Forms;
using GriForms.Application.Telemetry;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Consolidation;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Submissions;
using GriForms.Infrastructure.Telemetry;
using GriForms.Infrastructure.Tenancy;
using GriForms.Tests.Support;
using GriForms.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

// Penentuan tenant aktif, pergantian tenant, pengguna tanpa keanggotaan, wajib ganti kata sandi, dan policy
public class TenantSessionTests : IDisposable
{
    private const int A = Tenant.LegacyId;   // "Z Resource"

    private readonly TestDb _db = new();
    private readonly int _b;                  // "Acme Mining": urut nama lebih dulu dari Z Resource
    private readonly int _c;                  // nonaktif

    public TenantSessionTests()
    {
        using var db = _db.CreateContext();
        var b = new Tenant { Code = "ACME", Name = "Acme Mining" };
        var c = new Tenant { Code = "OLD", Name = "Basalt Lama", IsActive = false };
        db.AddRange(b, c);
        db.SaveChanges();
        (_b, _c) = (b.Id, c.Id);
    }

    // Satu "circuit": TenantContext dan AppDbContext yang sama dipakai sesi dan query sesudahnya
    private sealed class Circuit(TestDb testDb) : IAsyncDisposable
    {
        public TenantContext Tenant { get; } = new();
        public AppDbContext Db { get; private set; } = null!;
        public TenantSession Session { get; private set; } = null!;

        public Circuit Open()
        {
            Db = testDb.CreateContext(Tenant);
            Session = new TenantSession(Db, Tenant, testDb.Clock, NullLogger<TenantSession>.Instance);
            return this;
        }

        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }

    private Circuit NewCircuit() => new Circuit(_db).Open();

    private void Member(string userId, int tenantId, RoleName role = RoleName.Contributor)
    {
        using var db = _db.CreateContext(tenantId);
        int? siteId = null;
        if (role == RoleName.Contributor)
        {
            // Peran per site butuh site di tenant itu
            var site = db.Sites.FirstOrDefault();
            if (site is null)
            {
                site = new Site { Code = "1", Name = $"Site {tenantId}" };
                db.Add(site);
                db.SaveChanges();
            }
            siteId = site.Id;
        }
        db.Add(new UserSiteRole { UserId = userId, Role = role, SiteId = siteId });
        db.SaveChanges();
    }

    private void Profile(string userId, int? activeTenantId = null, bool mustChangePassword = false)
    {
        using var db = _db.CreateContext();
        db.Add(new UserProfile { UserId = userId, ActiveTenantId = activeTenantId, MustChangePassword = mustChangePassword });
        db.SaveChanges();
    }

    private void PlatformAdmin(string userId)
    {
        using var db = _db.CreateContext();
        db.Add(new PlatformAdmin { UserId = userId, GrantedAt = _db.Clock.UtcNow });
        db.SaveChanges();
    }

    private int? StoredActiveTenant(string userId)
    {
        using var db = _db.CreateContext();
        return db.UserProfiles.Single(p => p.UserId == userId).ActiveTenantId;
    }

    [Fact]
    public async Task Stored_active_tenant_is_used_when_user_is_still_a_member()
    {
        Member("u1", A);
        Member("u1", _b);
        Profile("u1", activeTenantId: A);

        await using var circuit = NewCircuit();
        var info = await circuit.Session.StartAsync("u1", CancellationToken.None);

        Assert.Equal(A, info.TenantId);
        Assert.Equal(A, circuit.Tenant.TenantId);
        Assert.Equal(new[] { _b, A }, info.Memberships.Select(m => m.Id).ToArray());   // urut nama
    }

    [Fact]
    public async Task Roles_for_menu_come_from_active_tenant_only()
    {
        Member("u1", A, RoleName.Contributor);
        Member("u1", A, RoleName.Admin);
        Member("u1", _b, RoleName.ReportViewer);
        Profile("u1", activeTenantId: A);

        await using var circuit = NewCircuit();
        var info = await circuit.Session.StartAsync("u1", CancellationToken.None);

        Assert.Equal(new[] { RoleName.Contributor, RoleName.Admin }.Order(), info.Roles.Order());
        Assert.True(info.Has(RoleName.Contributor));
        Assert.False(info.Has(RoleName.ReportViewer));   // peran di tenant lain tidak ikut
        Assert.True(info.IsTenantAdmin);
    }

    [Fact]
    public async Task Without_valid_preference_first_active_membership_by_name_is_used_and_saved()
    {
        Member("u1", A);
        Member("u1", _b);
        Member("u1", _c);                      // tenant nonaktif tidak dihitung
        Profile("u1", activeTenantId: _c);

        await using var circuit = NewCircuit();
        var info = await circuit.Session.StartAsync("u1", CancellationToken.None);

        Assert.Equal(_b, info.TenantId);
        Assert.Equal(_b, circuit.Tenant.TenantId);
        Assert.DoesNotContain(info.Memberships, m => m.Id == _c);
        Assert.Equal(_b, StoredActiveTenant("u1"));
    }

    [Fact]
    public async Task User_without_profile_gets_profile_with_first_membership()
    {
        Member("u1", A);

        await using var circuit = NewCircuit();
        var info = await circuit.Session.StartAsync("u1", CancellationToken.None);

        Assert.Equal(A, info.TenantId);
        Assert.False(info.MustChangePassword);
        Assert.Equal(A, StoredActiveTenant("u1"));
    }

    [Fact]
    public async Task Switching_to_tenant_that_is_not_a_membership_is_rejected()
    {
        Member("u1", A);
        Profile("u1", activeTenantId: A);

        await using var circuit = NewCircuit();
        await circuit.Session.StartAsync("u1", CancellationToken.None);

        Assert.False(await circuit.Session.SwitchAsync(_b, CancellationToken.None));
        Assert.False(await circuit.Session.SwitchAsync(_c, CancellationToken.None));
        Assert.False(await circuit.Session.SwitchAsync(9999, CancellationToken.None));
        Assert.Equal(A, circuit.Tenant.TenantId);
        Assert.Equal(A, StoredActiveTenant("u1"));
    }

    [Fact]
    public async Task Switching_tenant_is_saved_and_used_by_the_next_login()
    {
        Member("u1", A);
        Member("u1", _b);
        Profile("u1", activeTenantId: A);

        await using (var circuit = NewCircuit())
        {
            await circuit.Session.StartAsync("u1", CancellationToken.None);
            Assert.True(await circuit.Session.SwitchAsync(_b, CancellationToken.None));
            Assert.Equal(_b, circuit.Tenant.TenantId);
            Assert.Equal(_b, circuit.Db.Sites.Select(s => s.TenantId).Distinct().Single());
        }

        // Login berikutnya (circuit baru, mis. dari perangkat lain)
        await using var next = NewCircuit();
        Assert.Equal(_b, (await next.Session.StartAsync("u1", CancellationToken.None)).TenantId);
        Assert.Equal(_b, next.Tenant.TenantId);
    }

    [Fact]
    public async Task User_without_active_membership_has_no_tenant()
    {
        Member("u1", _c);   // hanya tenant nonaktif

        await using var circuit = NewCircuit();
        var info = await circuit.Session.StartAsync("u1", CancellationToken.None);

        Assert.Null(info.TenantId);
        Assert.Null(circuit.Tenant.TenantId);
        Assert.Empty(circuit.Db.Sites);
    }

    [Fact]
    public async Task Platform_admin_without_membership_sees_no_tenant_data()
    {
        var seed = _db.SeedTopic();
        var submissionId = _db.SeedSubmission(1, seed, SubmissionStatus.Approved,
            new Answer { DisclosureNo = "403-5", QuestionKey = "403-5-A", StringValue = "Data ZRES" });
        using (var db = _db.CreateContext())
        {
            db.Add(new TelemetryDevice { Code = "esp32-01", Name = "Sensor", SiteId = 1, ApiKeyHash = DeviceApiKey.Hash("k") });
            db.SaveChanges();
        }
        PlatformAdmin("platform");

        await using var circuit = NewCircuit();
        var info = await circuit.Session.StartAsync("platform", CancellationToken.None);
        var ctx = circuit.Db;

        Assert.True(info.IsPlatformAdmin);
        Assert.Null(info.TenantId);
        Assert.Null(await new SubmissionQueries(ctx, new JsonFormDefinitionReader()).GetAsync(submissionId, CancellationToken.None));
        var consolidation = await new ConsolidationQuery(ctx).BuildAsync(seed.TopicId, 2026, CancellationToken.None);
        Assert.Empty(consolidation.Rows);
        Assert.Empty(consolidation.Coverage.MissingSiteIds);
        var telemetry = new TelemetryQueries(ctx);
        Assert.Empty(await telemetry.GetDevicesAsync(new TelemetryAccess(true, null), CancellationToken.None));
        Assert.Empty(await telemetry.GetSitesAsync(new TelemetryAccess(true, null), CancellationToken.None));
    }

    [Fact]
    public async Task Must_change_password_keeps_tenant_empty_until_password_is_changed()
    {
        Member("baru", A);
        Profile("baru", mustChangePassword: true);

        await using (var circuit = NewCircuit())
        {
            var info = await circuit.Session.StartAsync("baru", CancellationToken.None);
            Assert.True(info.MustChangePassword);
            Assert.Null(circuit.Tenant.TenantId);
            Assert.False(await circuit.Session.SwitchAsync(A, CancellationToken.None));
        }

        await using (var ctx = _db.CreateContext(new FakeTenant(null)))
        {
            var profiles = new UserProfiles(ctx, _db.Clock, NullLogger<UserProfiles>.Instance);
            Assert.True(await profiles.MustChangePasswordAsync("baru", CancellationToken.None));
            await profiles.CompletePasswordChangeAsync("baru", CancellationToken.None);
            Assert.False(await profiles.MustChangePasswordAsync("baru", CancellationToken.None));
        }

        using (var check = _db.CreateContext())
        {
            var audit = Assert.Single(check.AuditEntries.ToList(), e => e.Action == "PasswordChanged");
            Assert.Equal("baru", audit.EntityId);
        }

        await using var after = NewCircuit();
        Assert.Equal(A, (await after.Session.StartAsync("baru", CancellationToken.None)).TenantId);
        Assert.Equal(A, after.Tenant.TenantId);
    }

    [Theory]
    [InlineData("GET", "/telemetry", 302)]
    [InlineData("GET", "/", 302)]
    [InlineData("GET", "/Account/Manage", 302)]
    [InlineData("POST", "/tenant/switch", 403)]
    [InlineData("POST", "/_blazor/negotiate", 403)]
    [InlineData("GET", "/Account/Manage/ChangePassword", 0)]
    [InlineData("POST", "/Account/Manage/ChangePassword", 0)]
    [InlineData("POST", "/Account/Logout", 0)]
    [InlineData("GET", "/css/site.css", 0)]
    [InlineData("GET", "/export/konsolidasi.xlsx", 302)]
    [InlineData("GET", "/api/laporan.csv", 403)]
    [InlineData("GET", "/_framework/blazor.web.js", 0)]
    public async Task Middleware_blocks_other_pages_while_password_change_is_pending(string method, string path, int expected)
    {
        Member("baru", A);
        Profile("baru", mustChangePassword: true);

        var result = await InvokeMiddlewareAsync("baru", method, path);

        if (expected == 0)
        {
            Assert.True(result.Passed);
            return;
        }

        Assert.False(result.Passed);
        Assert.Equal(expected, result.Status);
        if (expected == 302) Assert.Equal(TenantMiddleware.ChangePasswordPath, result.Location);
    }

    [Fact]
    public async Task Middleware_sends_users_without_tenant_from_home_to_the_right_page()
    {
        PlatformAdmin("platform");
        Member("anggota", A);

        var platform = await InvokeMiddlewareAsync("platform", "GET", "/");
        var stranger = await InvokeMiddlewareAsync("tamu", "GET", "/");
        var member = await InvokeMiddlewareAsync("anggota", "GET", "/");

        Assert.Equal("/organisasi", platform.Location);
        Assert.Equal("/tanpa-organisasi", stranger.Location);
        Assert.True(member.Passed);
    }

    [Fact]
    public async Task Policies_follow_platform_admin_table_and_tenant_admin_role()
    {
        PlatformAdmin("platform");
        Member("admin-a", A, RoleName.Admin);

        Assert.True(await AuthorizeAsync("platform", new PlatformAdminRequirement()));
        Assert.False(await AuthorizeAsync("platform", new TenantAdminRequirement()));   // bukan anggota tenant mana pun
        Assert.False(await AuthorizeAsync("admin-a", new PlatformAdminRequirement()));
        Assert.True(await AuthorizeAsync("admin-a", new TenantAdminRequirement()));
    }

    private async Task<bool> AuthorizeAsync(string userId, IAuthorizationRequirement requirement)
    {
        await using var circuit = NewCircuit();
        var context = new AuthorizationHandlerContext(new[] { requirement }, Principal(userId), null);
        await new TenancyAuthorizationHandler(circuit.Session).HandleAsync(context);
        return context.HasSucceeded;
    }

    private async Task<(int Status, string? Location, bool Passed)> InvokeMiddlewareAsync(string userId, string method, string path)
    {
        await using var circuit = NewCircuit();
        var passed = false;
        var middleware = new TenantMiddleware(_ =>
        {
            passed = true;
            return Task.CompletedTask;
        }, NullLogger<TenantMiddleware>.Instance);
        var http = new DefaultHttpContext { User = Principal(userId) };
        http.Request.Method = method;
        http.Request.Path = path;

        await middleware.InvokeAsync(http, circuit.Session);

        var location = http.Response.Headers.Location.ToString();
        return (http.Response.StatusCode, location.Length == 0 ? null : location, passed);
    }

    private static ClaimsPrincipal Principal(string userId) =>
        new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, authenticationType: "Test"));

    public void Dispose() => _db.Dispose();
}
