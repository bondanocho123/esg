using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Tenancy;

// Bootstrap sekali jalan saat startup (dipanggil dari Program setelah kedua DbContext dimigrasi; tidak di migration
// AppDbContext karena AspNetUsers belum tentu ada). Hanya berjalan bila tabel PlatformAdmin kosong, sehingga
// perubahan peran oleh admin setelahnya tidak pernah ditimpa. Langkah:
// 1. akun Bootstrap:PlatformAdmin:Email dijadikan PlatformAdmin (dibuat dulu bila belum ada);
// 2. email di Bootstrap:TenantAdmins:{Code} mendapat Admin tenant itu (akun tidak pernah dibuat di sini);
// 3. semua peran tenant 1 milik akun PlatformAdmin dicabut.
// Langkah 1 sampai 3 berjalan dalam satu transaksi AppDbContext.
public sealed class PlatformBootstrapper(
    AppDbContext db, IUserAccounts accounts, BootstrapOptions options, IClock clock, ILogger<PlatformBootstrapper> logger)
    : IPlatformBootstrap
{
    private const string Actor = "bootstrap";

    public async Task<bool> IsSchemaCurrentAsync(CancellationToken ct) =>
        !(await db.Database.GetPendingMigrationsAsync(ct)).Any();

    public async Task RunAsync(CancellationToken ct)
    {
        // IgnoreQueryFilters tidak diperlukan: PlatformAdmin, Tenant, dan UserProfile tidak difilter tenant
        if (await db.PlatformAdmins.AnyAsync(ct))
        {
            logger.LogInformation("Bootstrap dilewati: PlatformAdmin sudah ada");
            return;
        }

        var email = options.PlatformAdminEmail?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            logger.LogWarning("Bootstrap dilewati: Bootstrap:PlatformAdmin:Email kosong. Isi lewat environment atau user-secrets.");
            return;
        }

        var account = await accounts.FindByEmailAsync(email, ct);
        var created = false;
        if (account is null)
        {
            if (string.IsNullOrEmpty(options.PlatformAdminPassword))
            {
                logger.LogWarning("Bootstrap dilewati: akun {Email} belum ada dan Bootstrap:PlatformAdmin:Password kosong", email);
                return;
            }

            account = await accounts.CreateAsync(email, options.PlatformAdminPassword, ct);
            created = true;
            logger.LogInformation("Bootstrap membuat akun PlatformAdmin {Email}", email);
        }

        try
        {
            await ApplyAsync(account, created, ct);
        }
        catch (Exception ex)
        {
            db.ChangeTracker.Clear();
            // Akun baru tanpa profil wajib-ganti-kata-sandi tidak boleh tertinggal; jalan berikutnya membuatnya ulang
            if (created) await accounts.DeleteAsync(account.UserId, CancellationToken.None);
            logger.LogError(ex, "Bootstrap gagal; tidak ada perubahan peran yang disimpan");
            throw;
        }
    }

    private async Task ApplyAsync(AccountInfo platformAdmin, bool created, CancellationToken ct)
    {
        var now = clock.UtcNow;
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Langkah 1
        db.PlatformAdmins.Add(new PlatformAdmin { UserId = platformAdmin.UserId, GrantedAt = now });
        var profile = await db.FindOrAddProfileAsync(platformAdmin.UserId, ct);
        if (created) profile.MustChangePassword = true;   // akun lama: kata sandi dan flag tidak diubah
        profile.ActiveTenantId = null;
        profile.UpdatedAt = now;

        // Kejadian tingkat platform tanpa tenant terkait dicatat di tenant 1 (pemilik data awal), karena
        // AuditEntry.TenantId wajib dan audit perlu terlihat oleh Admin Z Resource yang perannya dicabut di langkah 3
        db.AuditEntries.Add(Audit(Tenant.LegacyId, nameof(PlatformAdmin), platformAdmin.UserId, "Granted", null, platformAdmin.Email, now));

        // Langkah 2, sebelum pencabutan
        foreach (var (code, emails) in options.TenantAdmins)
        {
            var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Code == code, ct);
            if (tenant is null)
            {
                logger.LogWarning("Bootstrap: tenant {TenantCode} di Bootstrap:TenantAdmins tidak ditemukan", code);
                continue;
            }

            foreach (var raw in emails)
                await GrantTenantAdminAsync(tenant, raw?.Trim() ?? "", platformAdmin, now, ct);
        }

        // Langkah 3. IgnoreQueryFilters: bootstrap berjalan tanpa tenant aktif
        var roles = await db.UserSiteRoles.IgnoreQueryFilters()
            .Where(r => r.UserId == platformAdmin.UserId && r.TenantId == Tenant.LegacyId)
            .ToListAsync(ct);
        foreach (var role in roles)
        {
            db.UserSiteRoles.Remove(role);
            db.AuditEntries.Add(Audit(role.TenantId, nameof(UserSiteRole), role.UserId, "Revoked",
                TenancyRules.Describe(role), null, now));
        }

        await db.SaveChangesAcrossTenantsAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation(
            "Bootstrap selesai: {Email} menjadi PlatformAdmin (akun baru: {Created}), {RevokedCount} peran tenant {TenantId} dicabut",
            platformAdmin.Email, created, roles.Count, Tenant.LegacyId);

        // IgnoreQueryFilters: bootstrap berjalan tanpa tenant aktif
        if (!await db.UserSiteRoles.IgnoreQueryFilters()
                .AnyAsync(r => r.TenantId == Tenant.LegacyId && r.Role == RoleName.Admin && r.SiteId == null, ct))
            logger.LogWarning("Tenant {TenantId} tidak punya Admin setelah bootstrap. Tetapkan Admin lewat halaman Organisasi.",
                Tenant.LegacyId);
    }

    private async Task GrantTenantAdminAsync(Tenant tenant, string email, AccountInfo platformAdmin, DateTime now, CancellationToken ct)
    {
        if (email.Length == 0) return;

        if (string.Equals(email, platformAdmin.Email, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Bootstrap: {Email} adalah PlatformAdmin, tidak dijadikan Admin tenant {TenantCode}", email, tenant.Code);
            return;
        }

        var account = await accounts.FindByEmailAsync(email, ct);
        if (account is null)
        {
            logger.LogWarning("Bootstrap: akun {Email} untuk Admin tenant {TenantCode} tidak ditemukan; akun tidak dibuat", email, tenant.Code);
            return;
        }
        if (account.UserId == platformAdmin.UserId) return;

        // IgnoreQueryFilters: bootstrap berjalan tanpa tenant aktif
        var exists = await db.UserSiteRoles.IgnoreQueryFilters().AnyAsync(r =>
            r.TenantId == tenant.Id && r.UserId == account.UserId && r.Role == RoleName.Admin && r.SiteId == null, ct);
        if (exists) return;

        var role = new UserSiteRole { TenantId = tenant.Id, UserId = account.UserId, Role = RoleName.Admin };
        db.UserSiteRoles.Add(role);
        db.AuditEntries.Add(Audit(tenant.Id, nameof(UserSiteRole), account.UserId, "Granted", null, TenancyRules.Describe(role), now));
        logger.LogInformation("Bootstrap: {Email} menjadi Admin tenant {TenantCode}", email, tenant.Code);
    }

    private static AuditEntry Audit(int tenantId, string entityType, string entityId, string action,
        string? oldValue, string? newValue, DateTime at) =>
        TenancyRules.Audit(tenantId, entityType, entityId, action, oldValue, newValue, Actor, at);
}
