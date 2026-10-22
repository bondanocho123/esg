using GriForms.Application.Forms;

namespace GriForms.Application.Validation;

// Engine hanya menelusuri definisi dan menyerahkan tiap pertanyaan ke semua aturan terdaftar.
// Aturan baru cukup berupa kelas baru dan satu registrasi DI (AddValidationRule).
public sealed class ValidationEngine(IEnumerable<IValidationRule> rules) : IValidationEngine
{
    private readonly IReadOnlyList<IValidationRule> _rules = rules.ToList();

    public ValidationResult Validate(FormDefinition definition, IReadOnlyDictionary<string, AnswerValue> cells, bool forSubmit)
    {
        var byQuestion = cells
            .Select(c =>
            {
                var (questionKey, periodKey) = FormState.SplitCellKey(c.Key);
                return new QuestionCell(questionKey, periodKey, c.Value);
            })
            .ToLookup(c => FormState.QuestionOf(c.QuestionKey));

        var issues = new List<ValidationIssue>();
        foreach (var q in definition.Disclosures.SelectMany(d => d.Questions))
        {
            var own = byQuestion[q.Key].ToList();
            foreach (var rule in _rules)
                issues.AddRange(rule.Check(q, own, forSubmit));
        }
        return new ValidationResult(issues);
    }
}
