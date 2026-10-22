using GriForms.Application.Consolidation;

namespace GriForms.Application.Abstractions;

public interface IConsolidationQuery
{
    Task<ConsolidationResult> BuildAsync(int topicId, int year, CancellationToken ct);
}
