namespace GriForms.Domain.Entities;

// Satu nilai sensor. Value memakai double (REAL di SQLite), bukan decimal (TEXT), agar AVG/MIN/MAX bisa di SQL.
// Telemetry tidak menulis ke Answer: nilai sensor hanya saran yang diterima contributor.
public class TelemetryReading
{
    public long Id { get; set; }
    public int DeviceId { get; set; }
    public string Metric { get; set; } = "";      // "temperature", "humidity"
    public double Value { get; set; }
    public string Unit { get; set; } = "";        // "C", "%"
    public DateTime RecordedAt { get; set; }      // UTC, dari device (NTP) atau ReceivedAt bila tidak valid
    public DateTime ReceivedAt { get; set; }      // UTC, jam server

    public TelemetryDevice Device { get; set; } = null!;
}
