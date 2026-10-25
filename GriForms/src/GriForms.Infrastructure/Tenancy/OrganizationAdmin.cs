using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Tenancy;

// Halaman Organisasi. PlatformAdmin mengelola tenant mana pun tanpa menjadi anggotanya, jadi layanan ini
// membaca dengan IgnoreQueryFilters dan menulis lewat SaveChangesAcrossTenantsAsync dengan TenantId eksplisit.
// Hak diperiksa ulang ke tabel PlatformAdmin di setiap operasi, tidak hanya di halaman.
public sealed class OrganizationAdmin(
    AppDbContext db, IUserAccounts accounts, ICurrentUser user, IClock clock, ILogger<OrganizationAdmin> logger)
    : IOrganizationAdmin
{
    public async Task<IReadOnlyList<TenantSummary>> GetTenantsAsync(CancellationToken ct)
    {
        await EnsurePlatformAdminAsync(ct);

        var tenants = await db.Tenants.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
        // IgnoreQueryFilters: daftar site dan Admin semua tenant untuk halaman Organisasi
        var sites = await db.Sites.IgnoreQueryFilters().AsNoTracking().OrderBy(s => s.Code).ToListAsync(ct);
        var admins = await db.UserSiteRoles.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.Role == RoleName.Admin && r.SiteId == null)
            .Select(r => new { r.TenantId, r.UserId })
            .ToListAsync(ct);
        var emails = await accounts.GetEmailsAsync(admins.Select(a => a.UserId).Distinct().ToList(), ct);

        return tenants.Select(t => new TenantSummary(
                t.Id, t.Code, t.Name, t.IsActive,
                sites.Where(s => s.TenantId == t.Id).Select(TenancyRules.ToView).ToList(),
                admins.Where(a => a.TenantId == t.Id)
                    .Select(a => emails.GetValueOrDefault(a.UserId) ?? a.UserId)
                    .Distinct().Order().ToList()))
            .ToList();
    }

    public async Task<int> CreateTenantAsync(NewTenant input, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        ArgumentNullException.ThrowIfNull(input);

        var code = TenancyRules.Code(input.Code, "Kode organisasi");
        var name = TenancyRules.Name(input.Name, "Nama organisasi");
        var sites = (input.Sites ?? []).Select(TenancyRules.Site).ToList();
        if (sites.GroupBy(s => s.Code).FirstOrDefault(g => g.Count() > 1) is { } duplicate)
            throw new InvalidOperationException($"Kode site {duplicate.Key} dipakai lebih dari sekali.");
        if (await db.Tenants.AnyAsync(t => t.Code == code, ct))
            throw new InvalidOperationException($"Kode organisasi {code} sudah dipakai.");

        var now = clock.UtcNow;
        var tenant = new Tenant { Code = code, Name = name };
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            db.Tenants.Add(tenant);
            await db.SaveChangesAcrossTenantsAsync(ct);   // Id tenant dibutuhkan site dan audit

            foreach (var site in sites)
                db.Sites.Add(new Site { TenantId = tenant.Id, Code = site.Code, Name = site.Name, Location = site.Location });

            db.AuditEntries.Add(Audit(tenant.Id, nameof(Tenant), tenant.Id.ToString(), "Created", null,
                $"{code}: {name}; site: {string.Join(", ", sites.Select(s => s.Code))}", actor, now));

            await db.SaveChangesAcrossTenantsAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Kode organisasi {code} sudah dipakai.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }

        using (Scope(tenant.Id, actor))
            logger.LogInformation("Organisasi {TenantCode} dibuat dengan {SiteCount} site", code, sites.Count);
        return tenant.Id;
    }

    public async Task SetTenantActiveAsync(int tenantId, bool isActive, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new InvalidOperationException($"Organisasi {tenantId} tidak ditemukan.");
        if (tenant.IsActive == isActive) return;

        tenant.IsActive = isActive;
        db.AuditEntries.Add(Audit(tenant.Id, nameof(Tenant), tenant.Id.ToString(), isActive ? "Activated" : "Deactivated",
            (!isActive).ToString(), isActive.ToString(), actor, clock.UtcNow));
        await SaveAsync(ct);

        using (Scope(tenant.Id, actor))
            logger.LogInformation("Organisasi {TenantCode} {Status}", tenant.Code, isActive ? "diaktifkan" : "dinonaktifkan");
    }

    public async Task<int> AddSiteAsync(int tenantId, SiteInput site, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var input = TenancyRules.Site(site);
        if (!await db.Tenants.AnyAsync(t => t.Id == tenantId, ct))
            throw new InvalidOperationException($"Organisasi {tenantId} tidak ditemukan.");

        // IgnoreQueryFilters: PlatformAdmin menambah site ke tenant yang bukan tenant aktifnya
        if (await db.Sites.IgnoreQueryFilters().AnyAsync(s => s.TenantId == tenantId && s.Code == input.Code, ct))
            throw new InvalidOperationException($"Kode site {input.Code} sudah dipakai di organisasi ini.");

        var entity = new Site { TenantId = tenantId, Code = input.Code, Name = input.Name, Location = input.Location };
        try
        {
            // Satu transaksi: site tidak boleh tersimpan tanpa jejak audit (Id site dibutuhkan audit)
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            db.Sites.Add(entity);
            await db.SaveChangesAcrossTenantsAsync(ct);
            db.AuditEntries.Add(Audit(tenantId, nameof(Site), entity.Id.ToString(), "Created", null,
                $"{input.Code}: {input.Name}", actor, clock.UtcNow));
            await db.SaveChangesAcrossTenantsAsync(ct);
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

    public async Task SetSiteActiveAsync(int siteId, bool isActive, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        // IgnoreQueryFilters: PlatformAdmin mengubah site di tenant yang bukan tenant aktifnya
        var site = await db.Sites.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == siteId, ct)
            ?? throw new InvalidOperationException($"Site {siteId} tidak ditemukan.");
        if (site.IsActive == isActive) return;

        site.IsActive = isActive;
        db.AuditEntries.Add(Audit(site.TenantId, nameof(Site), site.Id.ToString(), isActive ? "Activated" : "Deactivated",
            (!isActive).ToString(), isActive.ToString(), actor, clock.UtcNow));
        await SaveAsync(ct);

        using (Scope(site.TenantId, actor))
            logger.LogInformation("Site {SiteCode} {Status}", site.Code, isActive ? "diaktifkan" : "dinonaktifkan");
    }

    public async Task<AccountAssignment> AssignTenantAdminAsync(int tenantId, string email, string? initialPassword, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        if (!await db.Tenants.AnyAsync(t => t.Id == tenantId, ct))
            throw new InvalidOperationException($"Organisasi {tenantId} tidak ditemukan.");

        var provisioning = new AccountProvisioning(db, accounts, clock);
        AccountAssignment? account = null;
        try
        {
            account = await provisioning.EnsureAsync(email, initialPassword, ct);
            var userId = account.UserId;

            // IgnoreQueryFilters: tenant tujuan bukan tenant aktif PlatformAdmin
            var exists = await db.UserSiteRoles.IgnoreQueryFilters().AnyAsync(r =>
                r.TenantId == tenantId && r.UserId == userId && r.Role == RoleName.Admin && r.SiteId == null, ct);
            if (!exists)
            {
                var role = new UserSiteRole { TenantId = tenantId, UserId = userId, Role = RoleName.Admin };
                db.UserSiteRoles.Add(role);
                db.AuditEntries.Add(Audit(tenantId, nameof(UserSiteRole), userId, "Granted", null,
                    TenancyRules.Describe(role), actor, clock.UtcNow));
            }

            await SaveAsync(ct);

            using (Scope(tenantId, actor))
                logger.LogInformation("{Email} ditetapkan sebagai Admin organisasi (akun baru: {Created})", account.Email, account.Created);
            return account;
        }
        catch
        {
            await provisioning.CompensateAsync(account);
            throw;
        }
    }

    private async Task<string> EnsurePlatformAdminAsync(CancellationToken ct)
    {
        var userId = user.UserId;
        if (userId is null || !await db.PlatformAdmins.AnyAsync(p => p.UserId == userId, ct))
            throw new UnauthorizedAccessException("Hanya PlatformAdmin yang boleh mengelola organisasi.");
        return userId;
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAcrossTenantsAsync(ct);
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
    }

    private IDisposable? Scope(int tenantId, string userId) =>
        logger.BeginScope(new Dictionary<string, object?> { ["TenantId"] = tenantId, ["UserId"] = userId });

    private static AuditEntry Audit(int tenantId, string entityType, string entityId, string action,
        string? oldValue, string? newValue, string userId, DateTime at) =>
        TenancyRules.Audit(tenantId, entityType, entityId, action, oldValue, newValue, userId, at);
}
