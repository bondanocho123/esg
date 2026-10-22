using GriForms.Application.Abstractions;
using GriForms.Application.Telemetry;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Telemetry;

// Hanya menulis TelemetryReading dan TelemetryDevice.LastSeenAt. Jangan mengubah Answer dari sini:
// AuditInterceptor membaca ICurrentUser, yang tidak tersedia di request HTTP di luar circuit Blazor.
public sealed class TelemetryIngest(
    IAppReadDb db, IUnitOfWork uow, ITelemetryNotifier notifier, IClock clock, ILogger<TelemetryIngest> logger) : ITelemetryIngest
{
    private const int MaxReadingsPerRequest = 50;

    // Jam device di luar jendela ini dianggap belum sinkron NTP (ESP32 mulai dari 1970); pakai jam server
    private static readonly TimeSpan MaxFutureSkew = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);

    public async Task<IngestResult> IngestAsync(string deviceKey, TelemetryPayload payload, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(deviceKey))
            return IngestResult.Unauthorized("Header X-Device-Key wajib diisi.");

        var hash = DeviceApiKey.Hash(deviceKey);
        var device = await db.TelemetryDevices.FirstOrDefaultAsync(d => d.ApiKeyHash == hash, ct);
        if (device is null)
        {
            logger.LogWarning("Telemetry ditolak: API key tidak dikenal");
            return IngestResult.Unauthorized("API key device tidak dikenal.");
        }
        if (!device.IsActive)
        {
            logger.LogWarning("Telemetry ditolak: device {DeviceCode} nonaktif", device.Code);
            return IngestResult.Forbidden($"Device {device.Code} nonaktif.");
        }

        if (payload?.Readings is not { Count: > 0 } inputs)
            return IngestResult.BadRequest("Payload tidak berisi reading.");
        if (inputs.Count > MaxReadingsPerRequest)
            return IngestResult.BadRequest($"Maksimal {MaxReadingsPerRequest} reading per request.");

        // Satu reading tidak valid menolak seluruh payload, agar device tahu ada yang salah
        foreach (var input in inputs)
        {
            if (TelemetryMetrics.Validate(input) is { } error)
            {
                logger.LogWarning("Telemetry ditolak dari {DeviceCode}: {Error}", device.Code, error);
                return IngestResult.Unprocessable(error);
            }
        }

        var now = clock.UtcNow;
        var recordedAt = ResolveRecordedAt(payload.RecordedAt, now);

        foreach (var input in inputs)
        {
            var spec = TelemetryMetrics.Find(input.Metric!)!;
            uow.Add(new TelemetryReading
            {
                DeviceId = device.Id,
                Metric = spec.Metric,
                Value = input.Value!.Value,
                Unit = spec.Unit,   // unit dari daftar metrik, bukan dari device
                RecordedAt = recordedAt,
                ReceivedAt = now,
            });
        }
        device.LastSeenAt = now;

        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Telemetry diterima dari {DeviceCode}: {Count} reading", device.Code, inputs.Count);

        notifier.Publish(device.Id);
        return IngestResult.Accepted(inputs.Count);
    }

    private static DateTime ResolveRecordedAt(DateTime? sent, DateTime now)
    {
        if (sent is not { } value) return now;

        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        return utc > now + MaxFutureSkew || utc < now - MaxAge ? now : utc;
    }
}
