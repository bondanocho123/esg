using GriForms.Application.Tenancy;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Forms;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Tenancy;
using GriForms.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

// Halaman Organisasi (PlatformAdmin) dan Pengaturan Organisasi (Admin tenant aktif)
public class OrganizationAdminTests : IDisposable
{
    private const int A = Tenant.LegacyId;

    private readonly TestDb _db = new();
    private readonly FakeUserAccounts _accounts = new();

    public OrganizationAdminTests()
    {
        using var db = _db.CreateContext();
        db.Add(new PlatformAdmin { UserId = "platform", GrantedAt = _db.Clock.UtcNow });
        db.Add(new UserSiteRole { UserId = "admin-a", Role = RoleName.Admin });
        db.SaveChanges();
    }

    // PlatformAdmin tanpa keanggotaan: tidak ada tenant aktif
    private OrganizationAdmin Organizations(string userId, out AppDbContext ctx)
    {
        ctx = _db.CreateContext(new FakeTenant(null));
        return new OrganizationAdmin(ctx, _accounts, new FakeUser(userId), _db.Clock, NullLogger<OrganizationAdmin>.Instance);
    }

    private TenantSettings Settings(string userId, int tenantId, out AppDbContext ctx)
    {
        var tenant = new FakeTenant(tenantId);
        ctx = _db.CreateContext(tenant);
        return new TenantSettings(ctx, _accounts, new FakeUser(userId), tenant, _db.Clock, NullLogger<TenantSettings>.Instance);
    }

    [Fact]
    public async Task Platform_admin_creates_tenant_with_sites_and_assigns_new_admin()
    {
        var organizations = Organizations("platform", out var ctx);
        await using var _ = ctx;

        var id = await organizations.CreateTenantAsync(
            new NewTenant("ACME", "Acme Mining", [new SiteInput("1", "Head Office", "Jakarta"), new SiteInput("2", "Tambang", null)]),
            CancellationToken.None);
        var assigned = await organizations.AssignTenantAdminAsync(id, "admin@acme.co", "Awal-12345!", CancellationToken.None);
        var siteId = await organizations.AddSiteAsync(id, new SiteInput("3", "Pelabuhan", null), CancellationToken.None);

        Assert.True(assigned.Created);
        using var check = _db.CreateContext(id);
        Assert.Equal(new[] { "1", "2", "3" }, check.Sites.OrderBy(s => s.Code).Select(s => s.Code).ToArray());
        Assert.Contains(check.Sites.ToList(), s => s.Id == siteId);
        var role = Assert.Single(check.UserSiteRoles);
        Assert.Equal((assigned.UserId, RoleName.Admin, (int?)null), (role.UserId, role.Role, role.SiteId));
        Assert.True(check.UserProfiles.Single(p => p.UserId == assigned.UserId).MustChangePassword);
        var audit = check.AuditEntries.ToList();
        Assert.Contains(audit, e => e.EntityType == nameof(Tenant) && e.Action == "Created");
        Assert.Contains(audit, e => e.EntityType == nameof(UserSiteRole) && e.Action == "Granted");

        var summary = Assert.Single(await organizations.GetTenantsAsync(CancellationToken.None), t => t.Id == id);
        Assert.Equal(new[] { "admin@acme.co" }, summary.AdminEmails);
    }

    [Fact]
    public async Task Existing_account_only_gets_admin_role_and_keeps_its_password()
    {
        var existing = _accounts.Add("uci@jre.com");
        var organizations = Organizations("platform", out var ctx);
        await using var _ = ctx;

        var assigned = await organizations.AssignTenantAdminAsync(A, "uci@jre.com", initialPassword: null, CancellationToken.None);

        Assert.False(assigned.Created);
        Assert.Empty(_accounts.Created);
        using var check = _db.CreateContext();
        Assert.Contains(check.UserSiteRoles.ToList(), r => r.UserId == existing.UserId && r.Role == RoleName.Admin && r.SiteId == null);
        Assert.DoesNotContain(check.UserProfiles.ToList(), p => p.UserId == existing.UserId && p.MustChangePassword);
    }

    [Fact]
    public async Task Duplicate_codes_are_rejected_with_validation_messages()
    {
        var organizations = Organizations("platform", out var ctx);
        await using var _ = ctx;

        await Assert.ThrowsAsync<InvalidOperationException>(() => organizations.CreateTenantAsync(
            new NewTenant("ZRES", "Kode sudah dipakai", []), CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => organizations.CreateTenantAsync(
            new NewTenant("DUP", "Site ganda", [new SiteInput("1", "Satu", null), new SiteInput("1", "Dua", null)]), CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            organizations.AddSiteAsync(A, new SiteInput("1", "Sudah ada di ZRES", null), CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            organizations.AssignTenantAdminAsync(A, "baru@jre.com", initialPassword: null, CancellationToken.None));

        using var check = _db.CreateContext();
        Assert.Single(check.Tenants);
        Assert.Equal(6, check.Sites.Count());
        Assert.Empty(_accounts.Created);
    }

    [Fact]
    public async Task Tenant_admin_cannot_use_platform_features()
    {
        var organizations = Organizations("admin-a", out var ctx);
        await using var _ = ctx;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => organizations.GetTenantsAsync(CancellationToken.None));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            organizations.CreateTenantAsync(new NewTenant("X", "X", []), CancellationToken.None));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            organizations.AddSiteAsync(A, new SiteInput("9", "X", null), CancellationToken.None));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            new FormCatalog(ctx, new FakeUser("admin-a")).GetVersionsAsync(CancellationToken.None));

        Assert.Empty(await new FormCatalog(ctx, new FakeUser("platform")).GetVersionsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Platform_admin_cannot_use_tenant_settings_without_membership()
    {
        var settings = Settings("platform", A, out var ctx);
        await using var _ = ctx;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => settings.GetMembersAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Tenant_admin_manages_sites_and_roles_of_active_tenant_only()
    {
        int tenantB;
        int siteB;
        int roleB;
        using (var db = _db.CreateContext())
        {
            var tenant = new Tenant { Code = "ACME", Name = "Acme Mining" };
            db.Add(tenant);
            db.SaveChanges();
            tenantB = tenant.Id;
        }
        using (var db = _db.CreateContext(tenantB))
        {
            var site = new Site { Code = "1", Name = "Acme HO" };
            db.Add(site);
            db.SaveChanges();
            var role = new UserSiteRole { UserId = "admin-a", Role = RoleName.Contributor, SiteId = site.Id };
            db.Add(role);
            db.SaveChanges();
            (siteB, roleB) = (site.Id, role.Id);
        }
        _accounts.Add("admin-a@jre.com");

        var settings = Settings("admin-a", A, out var ctx);
        await using (ctx)
        {
            // Code sama dengan tenant lain diterima; ganda di tenant sendiri ditolak
            var newSite = await settings.AddSiteAsync(new SiteInput("7", "Site Baru", null), CancellationToken.None);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                settings.AddSiteAsync(new SiteInput("1", "Ganda", null), CancellationToken.None));
            await settings.UpdateSiteAsync(newSite, new SiteInput("7", "Site Baru", "Kalimantan"), isActive: false, CancellationToken.None);

            // Site, peran, dan pengguna tenant B tidak terlihat dan tidak bisa diubah
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                settings.UpdateSiteAsync(siteB, new SiteInput("1", "Diubah", null), isActive: true, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => settings.RevokeRoleAsync(roleB, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                settings.GrantRoleAsync("u-admin-a@jre.com", RoleName.Contributor, siteB, CancellationToken.None));

            var added = await settings.AddMemberAsync("kontributor@jre.com", "Awal-12345!", RoleName.Contributor, 2, CancellationToken.None);
            Assert.True(added.Created);
            Assert.DoesNotContain(await settings.GetSitesAsync(CancellationToken.None), s => s.Id == siteB);
            Assert.DoesNotContain((await settings.GetMembersAsync(CancellationToken.None)).SelectMany(m => m.Roles), r => r.RoleId == roleB);

            // Admin terakhir tidak bisa dicabut
            var adminRole = (await settings.GetMembersAsync(CancellationToken.None))
                .SelectMany(m => m.Roles).Single(r => r.Role == RoleName.Admin);
            await Assert.ThrowsAsync<InvalidOperationException>(() => settings.RevokeRoleAsync(adminRole.RoleId, CancellationToken.None));
        }

        using var checkA = _db.CreateContext();
        var site7 = checkA.Sites.Single(s => s.Code == "7");
        Assert.Equal(("Kalimantan", false), (site7.Location, site7.IsActive));
        Assert.Contains(checkA.UserSiteRoles.ToList(), r => r.UserId == "u-kontributor@jre.com" && r.SiteId == 2);

        using var checkB = _db.CreateContext(tenantB);
        Assert.Equal("Acme HO", checkB.Sites.Single().Name);
        Assert.Single(checkB.UserSiteRoles);
    }

    [Fact]
    public async Task Role_scope_is_validated()
    {
        _accounts.Add("x@jre.com");
        var settings = Settings("admin-a", A, out var ctx);
        await using var _ = ctx;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            settings.GrantRoleAsync("u-x@jre.com", RoleName.Contributor, siteId: null, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            settings.GrantRoleAsync("u-x@jre.com", RoleName.Admin, siteId: 1, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            settings.GrantRoleAsync("tidak-ada", RoleName.ReportViewer, siteId: null, CancellationToken.None));

        await settings.GrantRoleAsync("u-x@jre.com", RoleName.ReportViewer, siteId: null, CancellationToken.None);
        using var check = _db.CreateContext();
        Assert.Contains(check.UserSiteRoles.ToList(), r => r.UserId == "u-x@jre.com" && r.Role == RoleName.ReportViewer);
    }

    [Fact]
    public async Task New_account_keeps_must_change_password_flag_even_when_compensation_cannot_delete_it()
    {
        var settings = Settings("admin-a", A, out var ctx);
        await using var _ = ctx;
        _accounts.FailDelete = true;
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();   // simpanan peran gagal setelah akun dan profil terlanjur dibuat

        await Assert.ThrowsAnyAsync<Exception>(() =>
            settings.AddMemberAsync("baru@jre.com", "Awal-12345!", RoleName.Reviewer, null, cancelled.Token));

        using var check = _db.CreateContext(new FakeTenant(null));
        var profile = Assert.Single(check.UserProfiles.Where(p => p.UserId == "u-baru@jre.com"));
        Assert.True(profile.MustChangePassword);
    }

    [Fact]
    public async Task Failed_member_save_removes_new_account_and_its_profile()
    {
        var settings = Settings("admin-a", A, out var ctx);
        await using var _ = ctx;
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            settings.AddMemberAsync("baru@jre.com", "Awal-12345!", RoleName.Reviewer, null, cancelled.Token));

        Assert.Equal(new[] { "u-baru@jre.com" }, _accounts.Deleted);
        using var check = _db.CreateContext(new FakeTenant(null));
        Assert.Empty(check.UserProfiles.Where(p => p.UserId == "u-baru@jre.com"));
        Assert.Empty(check.UserSiteRoles.IgnoreQueryFilters().Where(r => r.UserId == "u-baru@jre.com"));
    }

    [Fact]
    public async Task Duplicate_site_code_is_rejected_without_leaving_site_or_audit()
    {
        var settings = Settings("admin-a", A, out var ctx);
        await using var _ = ctx;
        var before = _db.CreateContext().AuditEntries.Count();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            settings.AddSiteAsync(new SiteInput("1", "Dobel", null), CancellationToken.None));

        using var check = _db.CreateContext(A);
        Assert.Single(check.Sites, s => s.Code == "1");
        Assert.Equal(before, check.AuditEntries.Count());
    }

    public void Dispose() => _db.Dispose();
}
