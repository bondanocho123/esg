using GriForms.Application.Imports;

namespace GriForms.Application.Abstractions;

public interface ILegacySiteMapper
{
    // Kolom A dipetakan lewat Site.Code di tenant aktif; Code yang tidak dikenal ditolak, tidak pernah ke site tenant lain
    Task<LegacySiteMapping> MapAsync(IEnumerable<LegacyRow> rows, CancellationToken ct);
}
