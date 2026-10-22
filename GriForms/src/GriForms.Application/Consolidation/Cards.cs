namespace GriForms.Application.Consolidation;

// Kartu hasil konsolidasi. Halaman Blazor dan export Excel membaca DTO yang sama.
public interface ICard
{
    string DisclosureNo { get; }
    string Title { get; }
}

public record NarrativeCard(string DisclosureNo, string Title, IReadOnlyList<NarrativeRow> Rows) : ICard;
public record NarrativeRow(string SiteName, string? Text, bool NotApplicable, bool Missing);

public record QuantitativeCard(string DisclosureNo, string Title, string? Unit, IReadOnlyList<QuantRow> Rows) : ICard;
public record QuantRow(string SiteName, string RowLabel, decimal?[] Months /* 12 */, decimal? Total, bool NotApplicable);
