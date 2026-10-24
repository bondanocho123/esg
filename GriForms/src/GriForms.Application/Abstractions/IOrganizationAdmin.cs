using GriForms.Application.Tenancy;

namespace GriForms.Application.Abstractions;

// Halaman Organisasi, khusus PlatformAdmin; selain itu UnauthorizedAccessException.
// Input tidak valid atau Code ganda: InvalidOperationException.
public interface IOrganizationAdmin
{
    Task<IReadOnlyList<TenantSummary>> GetTenantsAsync(CancellationToken ct);

    Task<int> CreateTenantAsync(NewTenant input, CancellationToken ct);

    Task SetTenantActiveAsync(int tenantId, bool isActive, CancellationToken ct);

    Task<int> AddSiteAsync(int tenantId, SiteInput site, CancellationToken ct);

    // Email belum terdaftar: akun dibuat dengan initialPassword dan wajib ganti kata sandi.
    // Email sudah terdaftar: hanya peran Admin tenant yang ditambahkan, kata sandi tidak disentuh.
    Task<AccountAssignment> AssignTenantAdminAsync(int tenantId, string email, string? initialPassword, CancellationToken ct);
}
