namespace GriForms.Application.Abstractions;

// Tenant aktif pada circuit atau request ini. Ditentukan di server (ITenantSession), tidak pernah dari input klien.
// Null berarti tidak ada tenant aktif: query data tenant mengembalikan nol baris (fail closed).
public interface ITenantContext
{
    int? TenantId { get; }
}
