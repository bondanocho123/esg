using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Tenancy;

// Halaman Pengaturan Organisasi untuk Admin tenant aktif. Semua query memakai filter tenant di AppDbContext,
// jadi site, peran, dan pengguna tenant lain tidak terlihat dan tidak bisa diubah dari sini.
public sealed class TenantSettings(
    AppDbContext db, IUserAccounts accounts, ICurrentUser user, ITenantContext tenant, IClock clock,
    ILogger<TenantSettings> logger) : ITenantSettings
{
    public async Task<IReadOnlyList<SiteView>> GetSitesAsync(CancellationToken ct)
    {
        await EnsureTenantAdminAsync(ct);
        var sites = await db.Sites.AsNoTracking().OrderBy(s => s.Code).ToListAsync(ct);
        return sites.Select(TenancyRules.ToView).ToList();
    }

    public async Task<int> AddSiteAsync(SiteInput site, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        var input = TenancyRules.Site(site);
        await EnsureSiteCodeFreeAsync(input.Code, exceptSiteId: null, ct);

        var entity = new Site { TenantId = tenantId, Code = input.Code, Name = input.Name, Location = input.Location };
        try
        {
            // Satu transaksi: site tidak boleh tersimpan tanpa jejak audit (Id site dibutuhkan audit)
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            db.Sites.Add(entity);
            await db.SaveChangesAsync(ct);
            db.AuditEntries.Add(Audit(tenantId, nameof(Site), entity.Id.ToString(), "Created", null, Describe(entity), actor));
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Kode site {input.Code} sudah dipakai di organisasi ini.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }

        using (Scope(tenantId, actor))
            logger.LogInformation("Site {SiteCode} ditambahkan", input.Code);
        return entity.Id;
    }

    public async Task UpdateSiteAsync(int siteId, SiteInput site, bool isActive, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        var input = TenancyRules.Site(site);

        var entity = await db.Sites.FirstOrDefaultAsync(s => s.Id == siteId, ct)
            ?? throw new InvalidOperationException($"Site {siteId} tidak ditemukan.");
        await EnsureSiteCodeFreeAsync(input.Code, exceptSiteId: siteId, ct);

        var before = Describe(entity);
        entity.Code = input.Code;
        entity.Name = input.Name;
        entity.Location = input.Location;
        entity.IsActive = isActive;

        db.AuditEntries.Add(Audit(tenantId, nameof(Site), entity.Id.ToString(), "Updated", before, Describe(entity), actor));
        await SaveAsync(input.Code, ct);

        using (Scope(tenantId, actor))
            logger.LogInformation("Site {SiteId} diubah menjadi {SiteCode}", siteId, input.Code);
    }

    public async Task<IReadOnlyList<MemberView>> GetMembersAsync(CancellationToken ct)
    {
        await EnsureTenantAdminAsync(ct);

        var roles = await db.UserSiteRoles.AsNoTracking()
            .OrderBy(r => r.Role).ThenBy(r => r.SiteId)
            .Select(r => new { r.Id, r.UserId, r.Role, r.SiteId, SiteName = r.Site != null ? r.Site.Name : null })
            .ToListAsync(ct);
        var emails = await accounts.GetEmailsAsync(roles.Select(r => r.UserId).Distinct().ToList(), ct);

        return roles
            .GroupBy(r => r.UserId)
            .Select(g => new MemberView(
                g.Key,
                emails.GetValueOrDefault(g.Key) ?? g.Key,
                g.Select(r => new MemberRole(r.Id, r.Role, r.SiteId, r.SiteName)).ToList()))
            .OrderBy(m => m.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<AccountAssignment> AddMemberAsync(
        string email, string? initialPassword, RoleName role, int? siteId, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        await EnsureRoleScopeAsync(role, siteId, ct);

        var provisioning = new AccountProvisioning(db, accounts, clock);
        AccountAssignment? account = null;
        try
        {
            account = await provisioning.EnsureAsync(email, initialPassword, ct);
            await AddRoleAsync(account.UserId, role, siteId, tenantId, actor, ct);
            await db.SaveChangesAsync(ct);

            using (Scope(tenantId, actor))
                logger.LogInformation("{Email} ditambahkan sebagai {Role} (akun baru: {Created})", account.Email, role, account.Created);
            return account;
        }
        catch
        {
            await provisioning.CompensateAsync(account);
            throw;
        }
    }

    public async Task GrantRoleAsync(string userId, RoleName role, int? siteId, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        await EnsureRoleScopeAsync(role, siteId, ct);

        var known = await accounts.GetEmailsAsync([userId], ct);
        if (!known.ContainsKey(userId))
            throw new InvalidOperationException("Pengguna tidak ditemukan.");

        await AddRoleAsync(userId, role, siteId, tenantId, actor, ct);
        await SaveAsync(code: null, ct);

        using (Scope(tenantId, actor))
            logger.LogInformation("Peran {Role} site {SiteId} diberikan ke {TargetUserId}", role, siteId, userId);
    }

    public async Task RevokeRoleAsync(int roleId, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);

        var role = await db.UserSiteRoles.FirstOrDefaultAsync(r => r.Id == roleId, ct)
            ?? throw new InvalidOperationException("Peran tidak ditemukan.");

        if (role is { Role: RoleName.Admin, SiteId: null }
            && !await db.UserSiteRoles.AnyAsync(r => r.Id != roleId && r.Role == RoleName.Admin && r.SiteId == null, ct))
            throw new InvalidOperationException("Organisasi harus punya minimal satu Admin.");

        db.UserSiteRoles.Remove(role);
        db.AuditEntries.Add(Audit(tenantId, nameof(UserSiteRole), role.UserId, "Revoked", TenancyRules.Describe(role), null, actor));
        await SaveAsync(code: null, ct);

        using (Scope(tenantId, actor))
            logger.LogInformation("Peran {Role} site {SiteId} dicabut dari {TargetUserId}", role.Role, role.SiteId, role.UserId);
    }

    public async Task ResetPasswordAsync(string userId, string temporaryPassword, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        if (string.IsNullOrWhiteSpace(temporaryPassword))
            throw new InvalidOperationException("Isi kata sandi sementara.");
        if (userId == actor)
            throw new InvalidOperationException("Untuk akun sendiri, gunakan halaman ganti kata sandi.");

        // db.UserSiteRoles sudah dibatasi ke tenant aktif: pengguna tenant lain dianggap tidak ditemukan
        if (!await db.UserSiteRoles.AnyAsync(r => r.UserId == userId, ct))
            throw new InvalidOperationException("Pengguna tidak ditemukan di organisasi ini.");
        if (await db.PlatformAdmins.AnyAsync(p => p.UserId == userId, ct))
            throw new InvalidOperationException("Kata sandi PlatformAdmin tidak bisa diatur ulang dari organisasi.");
        // IgnoreQueryFilters: keanggotaan di tenant lain justru yang diperiksa
        if (await db.UserSiteRoles.IgnoreQueryFilters().AnyAsync(r => r.UserId == userId && r.TenantId != tenantId, ct))
            throw new InvalidOperationException("Pengguna ini juga anggota organisasi lain, jadi kata sandinya tidak bisa diatur ulang dari sini.");

        // Flag wajib ganti disimpan lebih dulu: bila langkah berikutnya gagal di tengah, kata sandi yang diketahui
        // Admin tidak pernah berlaku tanpa kewajiban ganti. Bila Identity menolak, flag dikembalikan seperti semula.
        var profile = await db.FindOrAddProfileAsync(userId, ct);
        var wasRequired = profile.MustChangePassword;
        profile.MustChangePassword = true;
        profile.UpdatedAt = clock.UtcNow;
        db.AuditEntries.Add(Audit(tenantId, "UserAccount", userId, "PasswordReset", null, null, actor));
        await SaveAsync(code: null, ct);

        try
        {
            await accounts.ResetPasswordAsync(userId, temporaryPassword, CancellationToken.None);
        }
        catch
        {
            db.ChangeTracker.Clear();
            var restore = await db.UserProfiles.FirstAsync(p => p.UserId == userId, CancellationToken.None);
            restore.MustChangePassword = wasRequired;
            db.AuditEntries.Add(Audit(tenantId, "UserAccount", userId, "PasswordResetFailed", null, null, actor));
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        using (Scope(tenantId, actor))
            logger.LogInformation("Kata sandi {TargetUserId} diatur ulang; wajib diganti saat login berikutnya", userId);
    }

    private async Task<(string UserId, int TenantId)> EnsureTenantAdminAsync(CancellationToken ct)
    {
        var userId = user.UserId;
        // db.UserSiteRoles sudah dibatasi ke tenant aktif: Admin tenant lain tidak lolos
        if (userId is null || tenant.TenantId is not { } tenantId
            || !await db.UserSiteRoles.AnyAsync(r => r.UserId == userId && r.Role == RoleName.Admin && r.SiteId == null, ct))
            throw new UnauthorizedAccessException("Hanya Admin organisasi aktif yang boleh mengubah pengaturan organisasi.");
        return (userId, tenantId);
    }

    private async Task EnsureRoleScopeAsync(RoleName role, int? siteId, CancellationToken ct)
    {
        TenancyRules.RoleScope(role, siteId);
        if (siteId is { } id && !await db.Sites.AnyAsync(s => s.Id == id, ct))
            throw new InvalidOperationException($"Site {id} tidak ditemukan.");
    }

    private async Task EnsureSiteCodeFreeAsync(string code, int? exceptSiteId, CancellationToken ct)
    {
        if (await db.Sites.AnyAsync(s => s.Code == code && s.Id != exceptSiteId, ct))
            throw new InvalidOperationException($"Kode site {code} sudah dipakai di organisasi ini.");
    }

    private async Task AddRoleAsync(string userId, RoleName role, int? siteId, int tenantId, string actor, CancellationToken ct)
    {
        if (await db.UserSiteRoles.AnyAsync(r => r.UserId == userId && r.Role == role && r.SiteId == siteId, ct))
            return;

        var entity = new UserSiteRole { TenantId = tenantId, UserId = userId, Role = role, SiteId = siteId };
        db.UserSiteRoles.Add(entity);
        db.AuditEntries.Add(Audit(tenantId, nameof(UserSiteRole), userId, "Granted", null, TenancyRules.Describe(entity), actor));
    }

    // code: kode site yang sedang disimpan, untuk pesan bila unique index (TenantId, Code) menolak simpanan bersamaan
    private async Task SaveAsync(string? code, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (code is not null)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Kode site {code} sudah dipakai di organisasi ini.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
    }

    private static string Describe(Site s) =>
        $"{s.Code}: {s.Name}{(s.Location is null ? "" : $" ({s.Location})")}{(s.IsActive ? "" : " [nonaktif]")}";

    private IDisposable? Scope(int tenantId, string userId) =>
        logger.BeginScope(new Dictionary<string, object?> { ["TenantId"] = tenantId, ["UserId"] = userId });

    private AuditEntry Audit(int tenantId, string entityType, string entityId, string action,
        string? oldValue, string? newValue, string userId) =>
        TenancyRules.Audit(tenantId, entityType, entityId, action, oldValue, newValue, userId, clock.UtcNow);
}
