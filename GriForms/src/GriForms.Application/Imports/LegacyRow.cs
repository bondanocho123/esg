namespace GriForms.Application.Imports;

// Satu baris sheet Data-Konsolidasi dari file Excel lama.
// SiteCode: kolom A (Site_ID) sebagai teks, dipetakan ke Site.Code di tenant aktif (bukan Site.Id global).
public record LegacyRow(
    string? SiteCode, string NoQz, string Qz, string? Period,
    decimal? Number, string? Text, string? Unit);

public sealed record MappedLegacyRow(int SiteId, LegacyRow Row);

// Rejected: Site_ID kosong atau Code yang tidak dikenal di tenant aktif; masuk laporan impor, tidak diimpor
public sealed record LegacySiteMapping(IReadOnlyList<MappedLegacyRow> Mapped, IReadOnlyList<LegacyRow> Rejected);
