using GriForms.Application.Abstractions;

namespace GriForms.Infrastructure.Tenancy;

// Scoped per circuit atau request. Hanya diisi TenantSession dari keanggotaan pengguna, bukan dari input klien.
public sealed class TenantContext : ITenantContext
{
    public int? TenantId { get; private set; }

    internal void Use(int? tenantId) => TenantId = tenantId;
}
