using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Web.Data;

// IUserAccounts di atas UserManager<ApplicationUser>. Profil, peran, dan flag wajib ganti kata sandi
// tidak diurus di sini, melainkan di Infrastructure (UserProfile dan UserSiteRole di AppDbContext).
public sealed class IdentityUserAccounts(UserManager<ApplicationUser> users) : IUserAccounts
{
    public async Task<AccountInfo?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(email);
        return user is null ? null : new AccountInfo(user.Id, user.Email ?? email);
    }

    public async Task<AccountInfo> CreateAsync(string email, string initialPassword, CancellationToken ct)
    {
        // EmailConfirmed langsung true: RequireConfirmedAccount aktif, sementara pengirim email masih IdentityNoOpEmailSender
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await users.CreateAsync(user, initialPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Akun {email} tidak bisa dibuat: {string.Join(" ", result.Errors.Select(e => e.Description))}");

        return new AccountInfo(user.Id, email);
    }

    public async Task DeleteAsync(string userId, CancellationToken ct)
    {
        if (await users.FindByIdAsync(userId) is { } user)
            await users.DeleteAsync(user);
    }

    public async Task ResetPasswordAsync(string userId, string newPassword, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Akun tidak ditemukan.");

        // Lewat token reset agar aturan kata sandi Identity tetap berlaku dan security stamp ikut diperbarui
        var token = await users.GeneratePasswordResetTokenAsync(user);
        var result = await users.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Kata sandi tidak bisa diatur ulang: {string.Join(" ", result.Errors.Select(e => e.Description))}");

        // Akun yang terkunci karena salah kata sandi dibuka kembali
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetEmailsAsync(IReadOnlyCollection<string> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<string, string>();

        return await users.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.UserName ?? u.Id, ct);
    }
}
