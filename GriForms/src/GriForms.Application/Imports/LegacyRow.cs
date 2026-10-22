namespace GriForms.Application.Imports;

// Satu baris sheet Data-Konsolidasi dari file Excel lama
public record LegacyRow(
    int? SiteId, string NoQz, string Qz, string? Period,
    decimal? Number, string? Text, string? Unit);
