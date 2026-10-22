using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Validation.Rules;

// Berlaku untuk setiap pertanyaan yang punya daftar opsi
public sealed class SelectOptionRule : IValidationRule
{
    public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
    {
        if (question.Options is not { Count: > 0 } options) yield break;

        foreach (var c in cells)
        {
            if (c.Value.State != ValueState.Filled || string.IsNullOrEmpty(c.Value.Text)) continue;
            if (!options.Any(o => o.Value == c.Value.Text))
                yield return new(question.Key, c.PeriodKey, "Pilihan tidak ada di daftar.", Severity.Error);
        }
    }
}
