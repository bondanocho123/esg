using GriForms.Domain.Entities;
using GriForms.Domain.Enums;

namespace GriForms.Tests.Domain;

public class SubmissionTransitionTests
{
    // Semua baris tabel transisi di bab 10
    [Theory]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.Submitted, RoleName.Contributor, null)]
    [InlineData(SubmissionStatus.Rejected,  SubmissionStatus.Submitted, RoleName.Contributor, null)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Reviewed,  RoleName.Reviewer,    null)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected,  RoleName.Reviewer,    "Angka Maret belum lengkap")]
    [InlineData(SubmissionStatus.Reviewed,  SubmissionStatus.Approved,  RoleName.Approver,    null)]
    [InlineData(SubmissionStatus.Reviewed,  SubmissionStatus.Rejected,  RoleName.Approver,    "Perlu koreksi")]
    [InlineData(SubmissionStatus.Approved,  SubmissionStatus.Draft,     RoleName.Admin,       "Koreksi data")]
    public void Allowed_transition_changes_status(SubmissionStatus from, SubmissionStatus to, RoleName role, string? comment)
    {
        var s = new Submission { Status = from };

        s.Transition(to, role, comment);

        Assert.Equal(to, s.Status);
    }

    [Theory]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.Submitted, RoleName.Reviewer)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Reviewed,  RoleName.Contributor)]
    [InlineData(SubmissionStatus.Reviewed,  SubmissionStatus.Approved,  RoleName.Contributor)]
    [InlineData(SubmissionStatus.Reviewed,  SubmissionStatus.Approved,  RoleName.Reviewer)]
    [InlineData(SubmissionStatus.Approved,  SubmissionStatus.Draft,     RoleName.Approver)]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.Approved,  RoleName.Admin)]
    public void Disallowed_transition_throws_and_keeps_status(SubmissionStatus from, SubmissionStatus to, RoleName role)
    {
        var s = new Submission { Status = from };

        Assert.Throws<InvalidOperationException>(() => s.Transition(to, role, comment: "alasan"));
        Assert.Equal(from, s.Status);
    }

    [Theory]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected, RoleName.Reviewer, null)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected, RoleName.Reviewer, " ")]
    [InlineData(SubmissionStatus.Reviewed,  SubmissionStatus.Rejected, RoleName.Approver, "")]
    [InlineData(SubmissionStatus.Approved,  SubmissionStatus.Draft,    RoleName.Admin,    null)]
    public void Rejecting_or_reopening_requires_comment(SubmissionStatus from, SubmissionStatus to, RoleName role, string? comment)
    {
        var s = new Submission { Status = from };

        Assert.Throws<InvalidOperationException>(() => s.Transition(to, role, comment));
        Assert.Equal(from, s.Status);
    }

    [Theory]
    [InlineData(SubmissionStatus.Draft, RoleName.Contributor, true)]
    [InlineData(SubmissionStatus.Rejected, RoleName.Contributor, true)]
    [InlineData(SubmissionStatus.Submitted, RoleName.Contributor, false)]
    [InlineData(SubmissionStatus.Approved, RoleName.Contributor, false)]
    [InlineData(SubmissionStatus.Draft, RoleName.Reviewer, false)]
    public void Only_contributor_edits_draft_or_rejected(SubmissionStatus status, RoleName role, bool expected)
    {
        var s = new Submission { Status = status };

        Assert.Equal(expected, s.IsEditableBy(role));
    }
}
