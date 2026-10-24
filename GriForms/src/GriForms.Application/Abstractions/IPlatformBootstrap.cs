namespace GriForms.Application.Abstractions;

// Sekali jalan saat startup, setelah kedua DbContext dimigrasi: menetapkan PlatformAdmin dan Admin tenant
// dari konfigurasi. Tidak berjalan lagi begitu tabel PlatformAdmin berisi.
public interface IPlatformBootstrap
{
    // False bila migration AppDbContext belum diterapkan; RunAsync jangan dipanggil
    Task<bool> IsSchemaCurrentAsync(CancellationToken ct);

    Task RunAsync(CancellationToken ct);
}
