using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Validation.Rules;

public sealed class RangeRule : IValidationRule
{
    public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
    {
        if (question.Validation is not { } rule) yield break;

        foreach (var c in cells)
        {
            if (c.Value.State != ValueState.Filled || c.Value.Number is not { } n) continue;

            if (rule.Min is { } min && n < min)
                yield return new(question.Key, c.PeriodKey, $"Nilai minimal {min}.", Severity.Error);
            if (rule.Max is { } max && n > max)
                yield return new(question.Key, c.PeriodKey, $"Nilai maksimal {max}.", Severity.Error);
        }
    }
}
