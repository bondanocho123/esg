using GriForms.Domain.Enums;

namespace GriForms.Application.Tenancy;

public sealed record TenantOption(int Id, string Code, string Name);

// Hasil penentuan tenant aktif untuk satu circuit atau request
public sealed record SessionInfo(
    string? UserId, TenantOption? ActiveTenant, IReadOnlyList<TenantOption> Memberships,
    bool IsPlatformAdmin, bool IsTenantAdmin, bool MustChangePassword)
{
    public static readonly SessionInfo Anonymous = new(null, null, [], false, false, false);

    public int? TenantId => ActiveTenant?.Id;

    // Peran pengguna di tenant aktif (semua site); dipakai menu. Hak per site tetap diperiksa layanan.
    public IReadOnlySet<RoleName> Roles { get; init; } = new HashSet<RoleName>();

    public bool Has(RoleName role) => Roles.Contains(role);
}

public sealed record SiteInput(string Code, string Name, string? Location);

public sealed record SiteView(int Id, string Code, string Name, string? Location, bool IsActive);

public sealed record NewTenant(string Code, string Name, IReadOnlyList<SiteInput> Sites);

public sealed record TenantSummary(
    int Id, string Code, string Name, bool IsActive,
    IReadOnlyList<SiteView> Sites, IReadOnlyList<string> AdminEmails);

public sealed record MemberRole(int RoleId, RoleName Role, int? SiteId, string? SiteName);

public sealed record MemberView(string UserId, string Email, IReadOnlyList<MemberRole> Roles);

public sealed record AccountInfo(string UserId, string Email);

// Created true bila akun baru dibuat dengan kata sandi awal (wajib diganti saat login pertama)
public sealed record AccountAssignment(string UserId, string Email, bool Created);

// Dibaca dari konfigurasi Bootstrap:* (environment atau user-secrets, bukan appsettings.json yang di-commit).
// TenantAdmins: Code tenant ke daftar email yang dijadikan Admin tenant itu.
public sealed record BootstrapOptions(
    string? PlatformAdminEmail, string? PlatformAdminPassword,
    IReadOnlyDictionary<string, IReadOnlyList<string>> TenantAdmins);
