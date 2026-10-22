using GriForms.Application.Forms;

namespace GriForms.Application.Validation.Rules;

public sealed class MaxLengthRule : IValidationRule
{
    public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
    {
        if (question.Validation?.MaxLength is not { } max) yield break;

        foreach (var c in cells)
        {
            if (c.Value.Text is { } text && text.Length > max)
                yield return new(question.Key, c.PeriodKey, $"Maksimal {max} karakter.", Severity.Error);
        }
    }
}
