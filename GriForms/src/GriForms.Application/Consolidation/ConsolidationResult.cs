using GriForms.Domain.Enums;

namespace GriForms.Application.Consolidation;

public record ConsolidatedRow(
    int SiteId, string SiteName, string DisclosureNo, string QuestionKey,
    string? PeriodKey, decimal? Number, string? Text, string? Unit, ValueState State);

public record Coverage(IReadOnlyList<int> ApprovedSiteIds, IReadOnlyList<int> MissingSiteIds);

public record ConsolidationResult(int TopicId, int Year, Coverage Coverage, IReadOnlyList<ConsolidatedRow> Rows);
