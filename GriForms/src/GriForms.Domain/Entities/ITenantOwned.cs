namespace GriForms.Domain.Entities;

// Entitas akar milik satu tenant. Entitas turunan (Answer, Signoff, TelemetryReading) mengikuti induknya.
public interface ITenantOwned
{
    int TenantId { get; set; }
}
