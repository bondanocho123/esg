using GriForms.Domain.Enums;

namespace GriForms.Application.Abstractions;

public interface ISubmissionWorkflow
{
    // Peran pelaku diberikan pemanggil; otorisasi per site menyusul di fase 3 (bab 11)
    Task TransitionAsync(int submissionId, SubmissionStatus to, RoleName actorRole, string? comment, CancellationToken ct);
}
