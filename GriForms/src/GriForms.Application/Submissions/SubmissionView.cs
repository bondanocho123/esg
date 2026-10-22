using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Submissions;

public record SubmissionView(
    int Id, int SiteId, string SiteName, int Year, int TopicId,
    SubmissionStatus Status, PeriodStatus PeriodStatus,
    FormDefinition Definition, IReadOnlyDictionary<string, AnswerValue> Cells);
