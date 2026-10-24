namespace GriForms.Application.Abstractions;

// Flag wajib ganti kata sandi di UserProfile
public interface IUserProfiles
{
    Task<bool> MustChangePasswordAsync(string userId, CancellationToken ct);

    // Dipanggil setelah ChangePasswordAsync berhasil; dicatat di audit bila flag sebelumnya aktif (tanpa nilai kata sandi)
    Task CompletePasswordChangeAsync(string userId, CancellationToken ct);
}
