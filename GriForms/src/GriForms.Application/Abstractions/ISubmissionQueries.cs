using GriForms.Application.Submissions;

namespace GriForms.Application.Abstractions;

public interface ISubmissionQueries
{
    Task<SubmissionView?> GetAsync(int submissionId, CancellationToken ct);
}
