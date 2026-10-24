using GriForms.Application.Tenancy;

namespace GriForms.Application.Abstractions;

public interface ITenantSession
{
    // Hasil StartAsync terakhir; Anonymous sebelum StartAsync dipanggil
    SessionInfo Current { get; }

    // Menentukan tenant aktif dari keanggotaan dan UserProfile, lalu mengisi ITenantContext.
    // Dipanggil sekali per circuit atau request; panggilan berikutnya untuk pengguna yang sama memakai hasil yang ada.
    Task<SessionInfo> StartAsync(string? userId, CancellationToken ct);

    // Divalidasi terhadap keanggotaan. False (tenant aktif tidak berubah) bila bukan anggota tenant aktif itu.
    Task<bool> SwitchAsync(int tenantId, CancellationToken ct);

    // Dari tabel PlatformAdmin; memakai Current bila sesi pengguna itu sudah dimulai
    Task<bool> IsPlatformAdminAsync(string userId, CancellationToken ct);
}
