namespace GriForms.Application.Telemetry;

public sealed record MetricSpec(string Metric, string Label, string Unit, double Min, double Max);

// Metrik yang diterima beserta rentang wajarnya. Sensor baru (mis. energy_kwh untuk GRI 302) cukup menambah satu baris.
public static class TelemetryMetrics
{
    public const string Temperature = "temperature";
    public const string Humidity = "humidity";

    // Rentang ukur DHT11
    public static readonly IReadOnlyDictionary<string, MetricSpec> Known =
        new Dictionary<string, MetricSpec>(StringComparer.OrdinalIgnoreCase)
        {
            [Temperature] = new(Temperature, "Suhu", "C", 0, 50),
            [Humidity]    = new(Humidity, "Kelembapan", "%", 20, 90),
        };

    public static MetricSpec? Find(string metric) => Known.GetValueOrDefault(metric);

    // null bila valid
    public static string? Validate(TelemetryReadingInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Metric))
            return "Metric wajib diisi.";
        if (Find(input.Metric) is not { } spec)
            return $"Metric '{input.Metric}' tidak dikenal.";
        if (input.Value is not { } value || !double.IsFinite(value))
            return $"Nilai {spec.Metric} wajib berupa angka.";
        if (!string.IsNullOrWhiteSpace(input.Unit) && !string.Equals(input.Unit.Trim(), spec.Unit, StringComparison.OrdinalIgnoreCase))
            return $"Unit {spec.Metric} harus '{spec.Unit}', bukan '{input.Unit}'.";
        if (value < spec.Min || value > spec.Max)
            return $"Nilai {spec.Metric} {value} di luar rentang {spec.Min} sampai {spec.Max} {spec.Unit}.";
        return null;
    }
}
