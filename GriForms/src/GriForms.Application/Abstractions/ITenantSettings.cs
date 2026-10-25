using GriForms.Application.Tenancy;
using GriForms.Domain.Enums;

namespace GriForms.Application.Abstractions;

// Halaman Pengaturan Organisasi, khusus Admin tenant aktif; selain itu UnauthorizedAccessException.
// Semua operasi hanya menyentuh tenant aktif. Input tidak valid: InvalidOperationException.
public interface ITenantSettings
{
    Task<IReadOnlyList<SiteView>> GetSitesAsync(CancellationToken ct);

    Task<int> AddSiteAsync(SiteInput site, CancellationToken ct);

    Task UpdateSiteAsync(int siteId, SiteInput site, bool isActive, CancellationToken ct);

    Task<IReadOnlyList<MemberView>> GetMembersAsync(CancellationToken ct);

    // Aturan akun sama dengan IOrganizationAdmin.AssignTenantAdminAsync, lalu peran diberikan di tenant aktif
    Task<AccountAssignment> AddMemberAsync(string email, string? initialPassword, RoleName role, int? siteId, CancellationToken ct);

    Task GrantRoleAsync(string userId, RoleName role, int? siteId, CancellationToken ct);

    Task RevokeRoleAsync(int roleId, CancellationToken ct);

    // Kata sandi sementara untuk anggota tenant aktif; pengguna wajib menggantinya saat login berikutnya.
    // Ditolak untuk akun sendiri, PlatformAdmin, dan pengguna yang juga anggota organisasi lain
    // (Admin satu organisasi tidak boleh mengambil alih akun yang punya akses di luar organisasinya).
    Task ResetPasswordAsync(string userId, string temporaryPassword, CancellationToken ct);
}
