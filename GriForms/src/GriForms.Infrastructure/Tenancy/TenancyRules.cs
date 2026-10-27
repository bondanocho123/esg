using System.Net.Mail;
using System.Text.RegularExpressions;
using GriForms.Application.Tenancy;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;

namespace GriForms.Infrastructure.Tenancy;

// Validasi input tenant, site, akun, dan peran. Pelanggaran: InvalidOperationException berisi pesan untuk pengguna.
internal static partial class TenancyRules
{
    private const int MaxCodeLength = 20;
    private const int MaxNameLength = 100;
    public const int MaxTopicNameLength = 300;
    private const int MaxLocationLength = 200;

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]*$")]
    private static partial Regex CodePattern();

    public static string Code(string? code, string what)
    {
        code = code?.Trim() ?? "";
        if (code.Length == 0)
            throw new InvalidOperationException($"{what} wajib diisi.");
        if (code.Length > MaxCodeLength || !CodePattern().IsMatch(code))
            throw new InvalidOperationException($"{what} hanya boleh huruf, angka, '.', '_' atau '-' (maksimal {MaxCodeLength} karakter).");
        return code;
    }

    public static string Name(string? name, string what, int maxLength = MaxNameLength)
    {
        name = name?.Trim() ?? "";
        if (name.Length == 0)
            throw new InvalidOperationException($"{what} wajib diisi.");
        if (name.Length > maxLength)
            throw new InvalidOperationException($"{what} maksimal {maxLength} karakter.");
        return name;
    }

    public static SiteInput Site(SiteInput? site)
    {
        if (site is null) throw new InvalidOperationException("Data site wajib diisi.");

        var location = string.IsNullOrWhiteSpace(site.Location) ? null : site.Location.Trim();
        if (location is { Length: > MaxLocationLength })
            throw new InvalidOperationException($"Lokasi site maksimal {MaxLocationLength} karakter.");

        return new SiteInput(Code(site.Code, "Kode site"), Name(site.Name, "Nama site"), location);
    }

    public static string Email(string? email)
    {
        email = email?.Trim() ?? "";
        if (email.Length == 0 || !MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
            throw new InvalidOperationException("Email tidak valid.");
        return email;
    }

    // Contributor, Reviewer, Approver per site; Admin dan ReportViewer berlaku untuk semua site di tenant (bab 11)
    public static void RoleScope(RoleName role, int? siteId)
    {
        if (!Enum.IsDefined(role))
            throw new InvalidOperationException("Peran tidak dikenal.");

        var perSite = role is RoleName.Contributor or RoleName.Reviewer or RoleName.Approver;
        if (perSite && siteId is null)
            throw new InvalidOperationException($"Peran {role} wajib memilih site.");
        if (!perSite && siteId is not null)
            throw new InvalidOperationException($"Peran {role} berlaku untuk semua site, jangan memilih site.");
    }

    public static string Describe(UserSiteRole role) =>
        role.SiteId is { } siteId ? $"{role.Role}@site:{siteId}" : $"{role.Role}@semua-site";

    public static SiteView ToView(Site s) => new(s.Id, s.Code, s.Name, s.Location, s.IsActive);

    // Jejak audit untuk perubahan tenancy; dipakai OrganizationAdmin, TenantSettings, dan PlatformBootstrapper
    public static AuditEntry Audit(int tenantId, string entityType, string entityId, string action,
        string? oldValue, string? newValue, string userId, DateTime at) => new()
    {
        TenantId = tenantId,
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        OldValue = oldValue,
        NewValue = newValue,
        UserId = userId,
        At = at,
    };
}
