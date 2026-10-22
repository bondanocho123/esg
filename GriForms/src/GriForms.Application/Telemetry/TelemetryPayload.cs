namespace GriForms.Application.Telemetry;

// Body POST /api/telemetry. Device dikenali dari header X-Device-Key, jadi deviceId tidak dikirim.
public record TelemetryPayload(DateTime? RecordedAt, List<TelemetryReadingInput>? Readings);

public record TelemetryReadingInput(string? Metric, double? Value, string? Unit);

public sealed record IngestResult(bool IsSuccess, int StatusCode, string? Error, int Count = 0)
{
    public static IngestResult Accepted(int count) => new(true, 202, null, count);
    public static IngestResult BadRequest(string error) => new(false, 400, error);
    public static IngestResult Unauthorized(string error) => new(false, 401, error);
    public static IngestResult Forbidden(string error) => new(false, 403, error);
    public static IngestResult Unprocessable(string error) => new(false, 422, error);
}
