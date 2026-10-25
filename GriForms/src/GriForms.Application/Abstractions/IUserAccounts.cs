using GriForms.Application.Tenancy;

namespace GriForms.Application.Abstractions;

// Akun login (ASP.NET Core Identity). Diimplementasikan di Web karena UserManager<ApplicationUser> ada di sana.
public interface IUserAccounts
{
    Task<AccountInfo?> FindByEmailAsync(string email, CancellationToken ct);

    // EmailConfirmed langsung true: belum ada pengirim email sungguhan.
    // InvalidOperationException bila Identity menolak (mis. kata sandi terlalu lemah).
    Task<AccountInfo> CreateAsync(string email, string initialPassword, CancellationToken ct);

    // Kompensasi bila penyimpanan profil dan peran gagal setelah akun dibuat
    Task DeleteAsync(string userId, CancellationToken ct);

    // Mengganti kata sandi tanpa kata sandi lama dan membatalkan sesi login lain (security stamp baru).
    // InvalidOperationException bila akun tidak ada atau Identity menolak kata sandinya.
    Task ResetPasswordAsync(string userId, string newPassword, CancellationToken ct);

    Task<IReadOnlyDictionary<string, string>> GetEmailsAsync(IReadOnlyCollection<string> userIds, CancellationToken ct);
}
