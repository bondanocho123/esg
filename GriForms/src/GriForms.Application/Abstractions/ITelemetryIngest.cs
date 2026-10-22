using GriForms.Application.Telemetry;

namespace GriForms.Application.Abstractions;

// Dipakai endpoint HTTP sekarang, dan nanti oleh consumer MQTT tanpa mengubah logika ingest
public interface ITelemetryIngest
{
    Task<IngestResult> IngestAsync(string deviceKey, TelemetryPayload payload, CancellationToken ct);
}
