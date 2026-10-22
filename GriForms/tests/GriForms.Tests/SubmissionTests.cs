using GriForms.Domain.Entities;
using GriForms.Domain.Enums;

namespace GriForms.Tests;

public class SubmissionTests
{
    [Fact]
    public void Transition_JalurNormal_SampaiApproved()
    {
        var s = new Submission();
        s.Transition(SubmissionStatus.Submitted, RoleName.Contributor, null);
        s.Transition(SubmissionStatus.Reviewed, RoleName.Reviewer, null);
        s.Transition(SubmissionStatus.Approved, RoleName.Approver, null);
        Assert.Equal(SubmissionStatus.Approved, s.Status);
    }

    [Theory]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.Approved, RoleName.Approver)]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.Submitted, RoleName.Reviewer)]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.Draft, RoleName.Admin)]
    [InlineData(SubmissionStatus.Draft, (SubmissionStatus)99, RoleName.Admin)]
    public void Transition_TidakDiizinkan_Melempar(SubmissionStatus from, SubmissionStatus to, RoleName role)
    {
        var s = new Submission { Status = from };
        Assert.Throws<InvalidOperationException>(() => s.Transition(to, role, "x"));
        Assert.Equal(from, s.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Transition_Reject_TanpaKomentar_Melempar(string? comment)
    {
        var s = new Submission { Status = SubmissionStatus.Submitted };
        Assert.Throws<InvalidOperationException>(() => s.Transition(SubmissionStatus.Rejected, RoleName.Reviewer, comment));
        Assert.Equal(SubmissionStatus.Submitted, s.Status);
    }

    [Fact]
    public void Transition_Reject_DenganKomentar_Berhasil()
    {
        var s = new Submission { Status = SubmissionStatus.Submitted };
        s.Transition(SubmissionStatus.Rejected, RoleName.Reviewer, "Data tidak lengkap");
        Assert.Equal(SubmissionStatus.Rejected, s.Status);
    }

    [Fact]
    public void Transition_PeriodeClosed_Melempar()
    {
        var s = new Submission { Period = new ReportingPeriod { Status = PeriodStatus.Closed } };
        Assert.Throws<InvalidOperationException>(() => s.Transition(SubmissionStatus.Submitted, RoleName.Contributor, null));
        Assert.Equal(SubmissionStatus.Draft, s.Status);
    }

    [Theory]
    [InlineData(SubmissionStatus.Draft, RoleName.Contributor, true)]
    [InlineData(SubmissionStatus.Rejected, RoleName.Contributor, true)]
    [InlineData(SubmissionStatus.Submitted, RoleName.Contributor, false)]
    [InlineData(SubmissionStatus.Approved, RoleName.Contributor, false)]
    [InlineData(SubmissionStatus.Draft, RoleName.Reviewer, false)]
    [InlineData(SubmissionStatus.Rejected, RoleName.Admin, false)]
    public void IsEditableBy_SesuaiStatusDanPeran(SubmissionStatus status, RoleName role, bool expected)
    {
        Assert.Equal(expected, new Submission { Status = status }.IsEditableBy(role));
    }
}
