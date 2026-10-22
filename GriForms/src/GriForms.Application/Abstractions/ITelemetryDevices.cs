using GriForms.Application.Telemetry;

namespace GriForms.Application.Abstractions;

public interface ITelemetryDevices
{
    // Hanya Admin; selain itu UnauthorizedAccessException. Input tidak valid atau kode ganda: InvalidOperationException.
    // Mengembalikan API key mentah; hanya ditampilkan sekali, yang disimpan hash-nya.
    Task<string> RegisterAsync(TelemetryAccess access, string code, string name, int siteId, CancellationToken ct);
}
