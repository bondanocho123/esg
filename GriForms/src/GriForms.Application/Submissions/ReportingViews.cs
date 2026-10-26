using GriForms.Domain.Enums;

namespace GriForms.Application.Submissions;

public sealed record ReportingPeriodView(int Id, int Year, PeriodStatus Status, int SubmissionCount);

// Tindakan berikutnya pengguna ini untuk satu baris site x topik, sesuai perannya di site itu
public enum NextAction { None, Start, Fill, Review, Approve }

public sealed record WorkspaceRow(
    int SiteId, string SiteCode, string SiteName,
    int TopicId, string TopicCode, string TopicName, string TopicCategory,
    int VersionNo, int? SubmissionId, SubmissionStatus? Status,
    IReadOnlyList<RoleName> Roles, NextAction Next)
{
    public bool NeedsMyAction => Next != NextAction.None;
}

// Period null bila tenant belum punya periode pelaporan
public sealed record WorkspaceView(
    IReadOnlyList<ReportingPeriodView> Periods, ReportingPeriodView? Period, IReadOnlyList<WorkspaceRow> Rows);

public sealed record TransitionOption(SubmissionStatus To, RoleName As, bool RequiresComment);

public sealed record SubmissionHistoryEntry(DateTime At, string Who, string From, string To, string? Comment);

// Submission yang dibuka beserta hak pengguna ini di site-nya
public sealed record SubmissionWorkspace(
    SubmissionView Submission, string TopicCode, string TopicName, int VersionNo,
    IReadOnlyList<RoleName> Roles, bool CanEdit,
    IReadOnlyList<TransitionOption> Transitions, IReadOnlyList<SubmissionHistoryEntry> History);
