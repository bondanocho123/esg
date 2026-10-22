using GriForms.Application.Forms;

namespace GriForms.Application.Validation.Rules;

// Wajib diisi atau ditandai Tidak berlaku; hanya memblokir saat pengajuan
public sealed class RequiredRule : IValidationRule
{
    public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
    {
        if (forSubmit && question.Required && question.CalcRef is null && cells.Count == 0)
            yield return new(question.Key, null, "Wajib diisi atau tandai Tidak berlaku.", Severity.Error);
    }
}
