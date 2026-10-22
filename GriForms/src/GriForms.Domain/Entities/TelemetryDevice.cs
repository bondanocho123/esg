namespace GriForms.Domain.Entities;

// Sensor (mis. ESP32 + DHT11) yang mengirim telemetry. Dikenali dari API key, yang disimpan hanya hash-nya.
public class TelemetryDevice
{
    public int Id { get; set; }
    public string Code { get; set; } = "";        // mis. "esp32-ho-01"
    public string Name { get; set; } = "";
    public int SiteId { get; set; }
    public string ApiKeyHash { get; set; } = "";  // SHA-256 heksadesimal
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }     // UTC

    public Site Site { get; set; } = null!;
}
