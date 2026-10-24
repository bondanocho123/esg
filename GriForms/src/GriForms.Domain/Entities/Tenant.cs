namespace GriForms.Domain.Entities;

// Perusahaan tambang atau klien konsultan ESG. Site tetap menjadi unit pengisian di dalam tenant.
public class Tenant
{
    // Pemilik semua data sebelum multitenant (Z Resource)
    public const int LegacyId = 1;

    public int Id { get; set; }
    public string Code { get; set; } = "";   // mis. "ZRES"
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
