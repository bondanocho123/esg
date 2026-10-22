using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Validation.Rules;

// Pertanyaan bertanda Tidak berlaku tidak boleh membawa nilai
public sealed class NotApplicableRule : IValidationRule
{
    public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
    {
        foreach (var c in cells)
        {
            if (c.Value.State == ValueState.NotApplicable
                && (c.Value.Number is not null || !string.IsNullOrEmpty(c.Value.Text)))
                yield return new(question.Key, c.PeriodKey, "Tidak berlaku tidak boleh berisi nilai.", Severity.Error);
        }
    }
}
