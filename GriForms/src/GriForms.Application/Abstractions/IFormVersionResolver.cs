using GriForms.Domain.Entities;

namespace GriForms.Application.Abstractions;

public interface IFormVersionResolver
{
    Task<FormVersion> ResolveAsync(int topicId, int year, CancellationToken ct);
}
