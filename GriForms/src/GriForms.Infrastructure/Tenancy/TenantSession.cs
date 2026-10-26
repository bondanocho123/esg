using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Tenancy;

// Menentukan tenant aktif per circuit atau request (dipanggil middleware dan circuit handler di Web):
// 1. UserProfile.ActiveTenantId bila pengguna masih anggota tenant itu dan tenantnya aktif;
// 2. bila tidak, tenant aktif pertama dari keanggotaannya (urut nama), lalu disimpan ke UserProfile;
// 3. bila tidak ada keanggotaan aktif, atau kata sandi awal belum diganti, tenant aktif kosong.
public sealed class TenantSession(AppDbContext db, TenantContext tenant, IClock clock, ILogger<TenantSession> logger) : ITenantSession
{
    private bool _started;

    public SessionInfo Current { get; private set; } = SessionInfo.Anonymous;

    public async Task<SessionInfo> StartAsync(string? userId, CancellationToken ct)
    {
        if (_started && Current.UserId == userId) return Current;

        _started = true;
        tenant.Use(null);
        Current = SessionInfo.Anonymous;
        if (string.IsNullOrEmpty(userId)) return Current;

        var isPlatformAdmin = await db.PlatformAdmins.AsNoTracking().AnyAsync(p => p.UserId == userId, ct);
        var profile = await db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        var memberships = await MembershipsAsync(userId, ct);

        if (profile is { MustChangePassword: true })
        {
            // Fail closed: tidak ada tenant aktif sampai kata sandi awal diganti
            Current = new SessionInfo(userId, null, memberships, isPlatformAdmin, IsTenantAdmin: false, MustChangePassword: true);
            return Current;
        }

        var active = memberships.FirstOrDefault(m => m.Id == profile?.ActiveTenantId) ?? memberships.FirstOrDefault();
        if (active is not null && active.Id != profile?.ActiveTenantId)
            await SaveActiveTenantAsync(userId, active.Id, ct);

        tenant.Use(active?.Id);

        // Setelah tenant.Use: UserSiteRoles sudah dibatasi ke tenant aktif
        var roles = active is null ? [] : await db.UserSiteRoles.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => new { r.Role, r.SiteId })
            .ToListAsync(ct);
        var isTenantAdmin = roles.Any(r => r.Role == RoleName.Admin && r.SiteId == null);

        Current = new SessionInfo(userId, active, memberships, isPlatformAdmin, isTenantAdmin, MustChangePassword: false)
        {
            Roles = roles.Select(r => r.Role).ToHashSet(),
        };
        return Current;
    }

    public async Task<bool> SwitchAsync(int tenantId, CancellationToken ct)
    {
        if (Current is not { UserId: { } userId, MustChangePassword: false }) return false;

        // Keanggotaan dibaca ulang: id dari klien tidak dipercaya, dan keanggotaan bisa sudah dicabut
        var target = (await MembershipsAsync(userId, ct)).FirstOrDefault(m => m.Id == tenantId);
        if (target is null)
        {
            logger.LogWarning("Pergantian ke tenant {RequestedTenantId} ditolak untuk {UserId}: bukan anggota tenant aktif itu",
                tenantId, userId);
            return false;
        }

        await SaveActiveTenantAsync(userId, target.Id, ct);
        _started = false;
        await StartAsync(userId, ct);
        logger.LogInformation("{UserId} berganti ke tenant {TenantId}", userId, target.Id);
        return true;
    }

    public async Task<bool> IsPlatformAdminAsync(string userId, CancellationToken ct)
    {
        if (_started && Current.UserId == userId) return Current.IsPlatformAdmin;
        return await db.PlatformAdmins.AsNoTracking().AnyAsync(p => p.UserId == userId, ct);
    }

    private async Task<IReadOnlyList<TenantOption>> MembershipsAsync(string userId, CancellationToken ct)
    {
        // IgnoreQueryFilters: resolusi keanggotaan tenant perlu melihat peran pengguna di semua tenant
        var tenantIds = db.UserSiteRoles.IgnoreQueryFilters()
            .Where(r => r.UserId == userId)
            .Select(r => r.TenantId);

        return await db.Tenants.AsNoTracking()
            .Where(t => t.IsActive && tenantIds.Contains(t.Id))
            .OrderBy(t => t.Name)
            .Select(t => new TenantOption(t.Id, t.Code, t.Name))
            .ToListAsync(ct);
    }

    private async Task SaveActiveTenantAsync(string userId, int tenantId, CancellationToken ct)
    {
        var profile = await db.FindOrAddProfileAsync(userId, ct);
        profile.ActiveTenantId = tenantId;
        profile.UpdatedAt = clock.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // Mis. dua request pertama bersamaan membuat profil yang sama; pilihan tetap berlaku untuk sesi ini
            db.Entry(profile).State = EntityState.Detached;
            logger.LogWarning(ex, "Gagal menyimpan tenant aktif {TenantId} untuk {UserId}", tenantId, userId);
        }
    }
}
