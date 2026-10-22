using GriForms.Application;
using GriForms.Application.Forms;
using GriForms.Application.Validation;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Submissions;
using GriForms.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Tests.Integration;

public class SubmissionWorkflowTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly IValidationEngine _validation =
        new ServiceCollection().AddApplication().BuildServiceProvider().GetRequiredService<IValidationEngine>();

    private async Task TransitionAsync(int id, SubmissionStatus to, RoleName role, string? comment = null)
    {
        await using var ctx = _db.CreateContext();
        var workflow = new SubmissionWorkflow(ctx, ctx, new JsonFormDefinitionReader(), _validation, _db.User, _db.Clock);
        await workflow.TransitionAsync(id, to, role, comment, CancellationToken.None);
    }

    private static Answer RequiredNarrative() => new() { DisclosureNo = "403-5", QuestionKey = "403-5-A", StringValue = "Pelatihan rutin" };

    [Fact]
    public async Task Submit_is_blocked_while_required_answers_are_missing()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TransitionAsync(id, SubmissionStatus.Submitted, RoleName.Contributor));

        using var db = _db.CreateContext();
        Assert.Equal(SubmissionStatus.Draft, db.Submissions.Single(s => s.Id == id).Status);
    }

    [Fact]
    public async Task Full_flow_records_signoffs_and_audit_of_who_and_when()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic(), SubmissionStatus.Draft, RequiredNarrative());

        await TransitionAsync(id, SubmissionStatus.Submitted, RoleName.Contributor);
        await TransitionAsync(id, SubmissionStatus.Reviewed, RoleName.Reviewer);
        await TransitionAsync(id, SubmissionStatus.Approved, RoleName.Approver);

        using var db = _db.CreateContext();
        Assert.Equal(SubmissionStatus.Approved, db.Submissions.Single(s => s.Id == id).Status);
        Assert.Equal(new[] { RoleName.Reviewer, RoleName.Approver },
            db.Signoffs.Where(s => s.SubmissionId == id).OrderBy(s => s.Id).Select(s => s.Role).ToArray());

        var audit = db.AuditEntries.Where(e => e.EntityType == nameof(Submission)).OrderBy(e => e.Id).ToList();
        Assert.Equal(3, audit.Count);
        Assert.All(audit, e =>
        {
            Assert.Equal("user-1", e.UserId);
            Assert.Equal(_db.Clock.UtcNow, e.At);
        });
        Assert.Equal(nameof(SubmissionStatus.Reviewed), audit[^1].OldValue);
    }

    [Fact]
    public async Task Reject_without_comment_is_refused()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic(), SubmissionStatus.Submitted, RequiredNarrative());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TransitionAsync(id, SubmissionStatus.Rejected, RoleName.Reviewer, comment: " "));
    }

    [Fact]
    public async Task Closed_period_is_read_only()
    {
        var id = _db.SeedSubmission(1, _db.SeedTopic(status: PeriodStatus.Closed), SubmissionStatus.Reviewed, RequiredNarrative());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TransitionAsync(id, SubmissionStatus.Approved, RoleName.Approver));
    }

    public void Dispose() => _db.Dispose();
}
