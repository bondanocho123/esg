using GriForms.Application.Abstractions;
using GriForms.Domain.Entities;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Tenancy;

public sealed class UserProfiles(AppDbContext db, IClock clock, ILogger<UserProfiles> logger) : IUserProfiles
{
    public Task<bool> MustChangePasswordAsync(string userId, CancellationToken ct) =>
        db.UserProfiles.AsNoTracking().AnyAsync(p => p.UserId == userId && p.MustChangePassword, ct);

    public async Task CompletePasswordChangeAsync(string userId, CancellationToken ct)
    {
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is not { MustChangePassword: true }) return;

        var now = clock.UtcNow;
        profile.MustChangePassword = false;
        profile.UpdatedAt = now;

        // Ganti kata sandi wajib terjadi sebelum tenant aktif ditentukan. Audit dicatat di tenant keanggotaan
        // pengguna, atau di tenant 1 bila belum punya keanggotaan (mis. PlatformAdmin baru).
        // IgnoreQueryFilters: tenant aktif masih kosong selama flag aktif.
        var tenantId = profile.ActiveTenantId
            ?? await db.UserSiteRoles.IgnoreQueryFilters()
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.TenantId)
                .Select(r => (int?)r.TenantId)
                .FirstOrDefaultAsync(ct)
            ?? Tenant.LegacyId;

        db.AuditEntries.Add(new AuditEntry
        {
            TenantId = tenantId,
            EntityType = "UserAccount",
            EntityId = userId,
            Action = "PasswordChanged",
            NewValue = "Kata sandi awal sudah diganti",   // tanpa nilai kata sandi
            UserId = userId,
            At = now,
        });

        await db.SaveChangesAcrossTenantsAsync(ct);
        logger.LogInformation("{UserId} menyelesaikan ganti kata sandi wajib", userId);
    }
}
