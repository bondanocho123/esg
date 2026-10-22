using GriForms.Application.Telemetry;

namespace GriForms.Application.Abstractions;

public interface ITelemetryQueries
{
    Task<TelemetryAccess> GetAccessAsync(string userId, CancellationToken ct);

    // Site aktif yang boleh dilihat, untuk filter dan pendaftaran device
    Task<IReadOnlyList<SiteOption>> GetSitesAsync(TelemetryAccess access, CancellationToken ct);

    // Device pada site yang boleh dilihat, beserta nilai terakhir per metrik
    Task<IReadOnlyList<DeviceSnapshot>> GetDevicesAsync(TelemetryAccess access, CancellationToken ct);

    // Kosong bila device tidak ada atau berada di luar site yang boleh dilihat
    Task<IReadOnlyList<TelemetryPoint>> GetSeriesAsync(TelemetryAccess access, int deviceId, DateTime fromUtc, CancellationToken ct);

    // Avg/Min/Max per bulan, dihitung di SQL (Value disimpan REAL)
    Task<IReadOnlyList<MonthlyAggregate>> GetMonthlyAggregateAsync(int deviceId, string metric, int year, CancellationToken ct);
}
