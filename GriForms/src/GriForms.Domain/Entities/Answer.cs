using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class Answer
{
    public long Id { get; set; }
    public int SubmissionId { get; set; }
    public string DisclosureNo { get; set; } = "";
    public string QuestionKey { get; set; } = "";
    public string? PeriodKey { get; set; }
    public decimal? NumberValue { get; set; }
    public string? StringValue { get; set; }
    public string? Unit { get; set; }
    public ValueState ValueState { get; set; } = ValueState.Filled;
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RowVersion { get; set; }
}
