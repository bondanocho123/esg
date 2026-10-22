namespace GriForms.Application.Validation;

public enum Severity { Error, Warning }

public record ValidationIssue(string QuestionKey, string? PeriodKey, string Message, Severity Severity);

public class ValidationResult
{
    public static readonly ValidationResult Empty = new(Array.Empty<ValidationIssue>());
    private readonly ILookup<string, ValidationIssue> _byKey;

    public ValidationResult(IEnumerable<ValidationIssue> issues)
    {
        Issues = issues.ToList();
        _byKey = Issues.ToLookup(i => i.QuestionKey);
    }

    public IReadOnlyList<ValidationIssue> Issues { get; }
    public bool HasErrors => Issues.Any(i => i.Severity == Severity.Error);
    public IReadOnlyList<string> For(string key) => _byKey[key].Select(i => i.Message).ToList();
}
