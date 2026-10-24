using GriForms.Application.Abstractions;
using GriForms.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Web.Services;

public static class StartupBootstrap
{
    // Bootstrap PlatformAdmin dan Admin tenant (IPlatformBootstrap) setelah kedua DbContext dimigrasi.
    // Migration tetap diterapkan manual (dotnet ef database update, lihat Data/Migrations/catatan-migration.txt)
    // agar bisa ditinjau dan app.db di-backup dulu; selama masih ada migration tertunda, bootstrap dilewati.
    public static async Task RunPlatformBootstrapAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Bootstrap");
        var bootstrap = services.GetRequiredService<IPlatformBootstrap>();

        try
        {
            var identityPending = (await services.GetRequiredService<ApplicationDbContext>().Database.GetPendingMigrationsAsync()).Any();
            if (identityPending || !await bootstrap.IsSchemaCurrentAsync(CancellationToken.None))
            {
                logger.LogWarning(
                    "Bootstrap dilewati: masih ada migration tertunda. Jalankan dotnet ef database update untuk " +
                    "ApplicationDbContext dan AppDbContext, lalu jalankan ulang aplikasi.");
                return;
            }

            await bootstrap.RunAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Bootstrap gagal tidak menyimpan perubahan peran (satu transaksi); aplikasi tetap jalan dan mencoba lagi saat restart
            logger.LogError(ex, "Bootstrap PlatformAdmin gagal");
        }
    }
}
