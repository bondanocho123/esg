using GriForms.Application.Abstractions;
using GriForms.Application.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Excel;

// Site_ID di file lama dipetakan ke Site.Code di tenant aktif. db.Sites sudah dibatasi ke tenant aktif
// (filter tenant), jadi Code yang sama di tenant lain tidak pernah ikut terpetakan.
public sealed class LegacySiteMapper(IAppReadDb db, ITenantContext tenant, ILogger<LegacySiteMapper> logger) : ILegacySiteMapper
{
    public async Task<LegacySiteMapping> MapAsync(IEnumerable<LegacyRow> rows, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var sites = await db.Sites.AsNoTracking()
            .Select(s => new { s.Id, s.Code })
            .ToDictionaryAsync(s => s.Code, s => s.Id, ct);

        var mapped = new List<MappedLegacyRow>();
        var rejected = new List<LegacyRow>();

        foreach (var row in rows)
        {
            if (row.SiteCode is { } code && sites.TryGetValue(code.Trim(), out var siteId))
            {
                mapped.Add(new MappedLegacyRow(siteId, row));
                continue;
            }

            rejected.Add(row);
            logger.LogWarning("Import ditolak: Site_ID {SiteCode} tidak dikenal di tenant {TenantId} ({NoQz})",
                row.SiteCode, tenant.TenantId, row.NoQz);
        }

        return new LegacySiteMapping(mapped, rejected);
    }
}
