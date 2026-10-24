using GriForms.Application.Tenancy;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Tenancy;
using GriForms.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

// Bootstrap terhadap database yang meniru app.db lama: semua peran ada di tenant 1 setelah migration,
// bondan@jre.com masih Admin global lama, uci@jre.com anggota biasa.
public class PlatformBootstrapTests : IDisposable
{
    private const string Bondan = "bondan@jre.com";
    private const string Uci = "uci@jre.com";
    private const string Lain = "lain@jre.com";

    private readonly TestDb _db = new();
    private readonly FakeUserAccounts _accounts = new();

    private static BootstrapOptions Options(string? platformEmail = Bondan, string? password = null, params string[] zresAdmins) =>
        new(platformEmail, password, new Dictionary<string, IReadOnlyList<string>> { ["ZRES"] = zresAdmins });

    private void SeedLegacyRoles()
    {
        var bondan = _accounts.Add(Bondan);
        var uci = _accounts.Add(Uci);
        var lain = _accounts.Add(Lain);

        using var db = _db.CreateContext();
        db.AddRange(
            new UserSiteRole { UserId = bondan.UserId, SiteId = null, Role = RoleName.Admin },
            new UserSiteRole { UserId = bondan.UserId, SiteId = 2, Role = RoleName.Reviewer },
            new UserSiteRole { UserId = uci.UserId, SiteId = 3, Role = RoleName.Contributor },
            new UserSiteRole { UserId = lain.UserId, SiteId = 1, Role = RoleName.Contributor });
        db.SaveChanges();
    }

    private async Task RunAsync(BootstrapOptions options)
    {
        await using var ctx = _db.CreateContext(new FakeTenant(null));   // startup: tidak ada tenant aktif
        await new PlatformBootstrapper(ctx, _accounts, options, _db.Clock, NullLogger<PlatformBootstrapper>.Instance)
            .RunAsync(CancellationToken.None);
    }

    private List<(string UserId, RoleName Role, int? SiteId)> RolesOfTenant1()
    {
        using var db = _db.CreateContext();
        return db.UserSiteRoles.OrderBy(r => r.Id).AsEnumerable().Select(r => (r.UserId, r.Role, r.SiteId)).ToList();
    }

    [Fact]
    public async Task Legacy_database_moves_admin_from_platform_admin_to_tenant_admin()
    {
        SeedLegacyRoles();

        await RunAsync(Options(zresAdmins: [Uci, "hilang@jre.com"]));

        using var db = _db.CreateContext();
        Assert.Equal("u-" + Bondan, Assert.Single(db.PlatformAdmins).UserId);

        // bondan kehilangan semua peran tenant 1, termasuk peran per site
        Assert.Empty(db.UserSiteRoles.IgnoreQueryFilters().Where(r => r.UserId == "u-" + Bondan));
        var profile = db.UserProfiles.Single(p => p.UserId == "u-" + Bondan);
        Assert.Null(profile.ActiveTenantId);
        Assert.False(profile.MustChangePassword);   // akun lama: kata sandi tidak disentuh

        // uci mendapat Admin tenant 1 dan tetap memegang peran lamanya; pengguna lain tidak berubah
        Assert.Equal(
            new[] { ("u-" + Uci, RoleName.Contributor, (int?)3), ("u-" + Lain, RoleName.Contributor, (int?)1), ("u-" + Uci, RoleName.Admin, (int?)null) },
            RolesOfTenant1().ToArray());

        // Email yang tidak ada tidak membuat akun baru
        Assert.Empty(_accounts.Created);

        var audit = db.AuditEntries.OrderBy(e => e.Id).Select(e => new { e.EntityType, e.Action, e.EntityId, e.UserId }).ToList();
        Assert.Contains(audit, e => e.EntityType == nameof(PlatformAdmin) && e.Action == "Granted" && e.EntityId == "u-" + Bondan);
        Assert.Contains(audit, e => e.EntityType == nameof(UserSiteRole) && e.Action == "Granted" && e.EntityId == "u-" + Uci);
        Assert.Equal(2, audit.Count(e => e.Action == "Revoked" && e.EntityId == "u-" + Bondan));
        Assert.All(audit, e => Assert.Equal("bootstrap", e.UserId));
    }

    [Fact]
    public async Task Second_run_changes_nothing_even_after_admin_restores_a_role_manually()
    {
        SeedLegacyRoles();
        await RunAsync(Options(zresAdmins: [Uci]));

        using (var db = _db.CreateContext())
        {
            db.Add(new UserSiteRole { UserId = "u-" + Bondan, SiteId = 2, Role = RoleName.Reviewer });   // dikembalikan admin
            db.SaveChanges();
        }
        var rolesBefore = RolesOfTenant1();
        int auditBefore;
        using (var db = _db.CreateContext()) auditBefore = db.AuditEntries.Count();

        await RunAsync(Options(zresAdmins: [Uci, Lain]));

        using var check = _db.CreateContext();
        Assert.Single(check.PlatformAdmins);
        Assert.Equal(rolesBefore, RolesOfTenant1());
        Assert.Equal(auditBefore, check.AuditEntries.Count());
    }

    [Fact]
    public async Task Unregistered_platform_admin_email_creates_account_that_must_change_password()
    {
        await RunAsync(Options(password: "Awal-12345!"));

        var created = Assert.Single(_accounts.Created);
        Assert.Equal((Bondan, "Awal-12345!"), created);

        using var db = _db.CreateContext();
        Assert.Equal("u-" + Bondan, Assert.Single(db.PlatformAdmins).UserId);
        Assert.True(db.UserProfiles.Single(p => p.UserId == "u-" + Bondan).MustChangePassword);
        Assert.Empty(db.UserSiteRoles.IgnoreQueryFilters());   // tidak ada keanggotaan tenant
    }

    [Fact]
    public async Task Registered_platform_admin_email_keeps_its_password()
    {
        _accounts.Add(Bondan);

        await RunAsync(Options(password: "Tidak-Dipakai-1!"));

        Assert.Empty(_accounts.Created);
        using var db = _db.CreateContext();
        Assert.Single(db.PlatformAdmins);
        Assert.False(db.UserProfiles.Single(p => p.UserId == "u-" + Bondan).MustChangePassword);
    }

    [Fact]
    public async Task Empty_platform_admin_email_skips_bootstrap()
    {
        SeedLegacyRoles();
        var rolesBefore = RolesOfTenant1();

        await RunAsync(Options(platformEmail: " ", zresAdmins: [Uci]));

        using var db = _db.CreateContext();
        Assert.Empty(db.PlatformAdmins);
        Assert.Equal(rolesBefore, RolesOfTenant1());
    }

    [Fact]
    public async Task Failure_midway_saves_no_role_change_and_removes_new_account()
    {
        _accounts.FailFindFor = Uci;   // gagal saat memproses TenantAdmins, setelah akun PlatformAdmin dibuat

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(Options(password: "Awal-12345!", zresAdmins: [Uci])));

        Assert.Equal(new[] { "u-" + Bondan }, _accounts.Deleted);
        using var db = _db.CreateContext();
        Assert.Empty(db.PlatformAdmins);
        Assert.Empty(db.UserProfiles);
        Assert.Empty(db.AuditEntries);
    }

    [Fact]
    public async Task Failure_midway_on_legacy_database_keeps_old_roles()
    {
        SeedLegacyRoles();
        var rolesBefore = RolesOfTenant1();
        _accounts.FailFindFor = Uci;

        await Assert.ThrowsAsync<InvalidOperationException>(() => RunAsync(Options(zresAdmins: [Uci])));

        using var db = _db.CreateContext();
        Assert.Empty(db.PlatformAdmins);
        Assert.Equal(rolesBefore, RolesOfTenant1());
        Assert.Empty(_accounts.Deleted);   // akun lama tidak pernah dihapus
    }

    public void Dispose() => _db.Dispose();
}
