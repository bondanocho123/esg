using GriForms.Application.Abstractions;
using GriForms.Application.Tenancy;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Tenancy;

// Akun yang sudah ada dipakai apa adanya (kata sandi dan flag tidak disentuh). Akun baru dibuat lewat Identity
// dengan kata sandi awal, dan UserProfile.MustChangePassword = true langsung disimpan sebelum langkah lain.
// Identity dan AppDbContext berbeda transaksi: bila simpanan berikutnya gagal, akun baru dihapus lewat CompensateAsync.
// Karena flag sudah tersimpan lebih dulu, akun yang gagal dihapus tetap wajib ganti kata sandi saat login.
internal sealed class AccountProvisioning(AppDbContext db, IUserAccounts accounts, IClock clock)
{
    public async Task<AccountAssignment> EnsureAsync(string? email, string? initialPassword, CancellationToken ct)
    {
        var address = TenancyRules.Email(email);

        if (await accounts.FindByEmailAsync(address, ct) is { } existing)
            return new AccountAssignment(existing.UserId, existing.Email, Created: false);

        if (string.IsNullOrWhiteSpace(initialPassword))
            throw new InvalidOperationException($"Email {address} belum terdaftar. Isi kata sandi awal untuk membuat akunnya.");

        var created = await accounts.CreateAsync(address, initialPassword, ct);
        var assignment = new AccountAssignment(created.UserId, created.Email, Created: true);
        try
        {
            await MarkMustChangePasswordAsync(created.UserId);
        }
        catch
        {
            await CompensateAsync(assignment);
            throw;
        }
        return assignment;
    }

    // Flag disimpan langsung dan tanpa token pembatalan: akun yang terlanjur dibuat tidak boleh tanpa kewajiban ganti
    private async Task MarkMustChangePasswordAsync(string userId)
    {
        var profile = await db.FindOrAddProfileAsync(userId, CancellationToken.None);
        profile.MustChangePassword = true;
        profile.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(CancellationToken.None);
    }

    public async Task CompensateAsync(AccountAssignment? account)
    {
        db.ChangeTracker.Clear();
        if (account is not { Created: true }) return;

        // Tanpa token pembatalan: kompensasi tetap dijalankan walau request dibatalkan.
        // Akun dihapus lebih dulu: bila gagal, profil berflag tetap ada sehingga akun itu masih wajib ganti kata sandi.
        await accounts.DeleteAsync(account.UserId, CancellationToken.None);
        if (await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == account.UserId) is { } profile)
        {
            db.UserProfiles.Remove(profile);
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
