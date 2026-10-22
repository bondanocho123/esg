using GriForms.Application.Forms;

namespace GriForms.Application.Validation;

// Satu sel jawaban milik satu pertanyaan. QuestionKey bisa memuat kunci baris, mis. "403-5-B/k3-dasar".
public record QuestionCell(string QuestionKey, string? PeriodKey, AnswerValue Value);

// Kontrak (Liskov): hanya mengembalikan issue, tidak pernah melempar untuk input valid maupun kosong.
public interface IValidationRule
{
    IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit);
}
