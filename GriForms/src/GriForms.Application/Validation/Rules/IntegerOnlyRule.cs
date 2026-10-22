using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Validation.Rules;

public sealed class IntegerOnlyRule : IValidationRule
{
    public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
    {
        if (question.Validation is not { IntegerOnly: true }) yield break;

        foreach (var c in cells)
        {
            if (c.Value.State == ValueState.Filled && c.Value.Number is { } n && n != decimal.Truncate(n))
                yield return new(question.Key, c.PeriodKey, "Harus bilangan bulat.", Severity.Error);
        }
    }
}
