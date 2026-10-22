using GriForms.Application;
using GriForms.Application.Forms;
using GriForms.Application.Validation;
using GriForms.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Tests.Validation;

public class ValidationEngineTests
{
    private static readonly FormDefinition Definition = new JsonFormDefinitionReader().Read(SampleDefinitions.Topic403);

    private static IValidationEngine Engine(Action<IServiceCollection>? extra = null)
    {
        var services = new ServiceCollection().AddApplication();
        extra?.Invoke(services);
        return services.BuildServiceProvider().GetRequiredService<IValidationEngine>();
    }

    [Fact]
    public void Draft_save_does_not_report_required()
    {
        var result = Engine().Validate(Definition, new Dictionary<string, AnswerValue>(), forSubmit: false);

        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Submit_reports_required_on_question_key()
    {
        var result = Engine().Validate(Definition, new Dictionary<string, AnswerValue>(), forSubmit: true);

        Assert.True(result.HasErrors);
        Assert.NotEmpty(result.For("403-5-A"));
    }

    [Fact]
    public void Monthly_cell_issue_is_reported_on_parent_question()
    {
        var cells = new Dictionary<string, AnswerValue>
        {
            [FormState.CellKey("403-5-B/k3-dasar", "2026-01")] = new(null, -3),
        };

        var result = Engine().Validate(Definition, cells, forSubmit: false);

        var issue = Assert.Single(result.Issues);
        Assert.Equal("403-5-B", issue.QuestionKey);
        Assert.Equal("2026-01", issue.PeriodKey);
    }

    [Fact]
    public void HasErrors_is_false_when_only_warnings()
    {
        var result = new ValidationResult(new[] { new ValidationIssue("q", null, "awas", Severity.Warning) });

        Assert.False(result.HasErrors);
        Assert.Equal(new[] { "awas" }, result.For("q"));
    }

    // Open/Closed: aturan baru cukup kelas baru dan satu registrasi DI
    [Fact]
    public void New_rule_is_picked_up_through_registration_only()
    {
        var engine = Engine(s => s.AddValidationRule<WarnOnEverythingRule>());

        var result = engine.Validate(Definition, new Dictionary<string, AnswerValue>(), forSubmit: false);

        Assert.Contains(result.Issues, i => i.Message == "uji" && i.Severity == Severity.Warning);
        Assert.False(result.HasErrors);
    }

    private sealed class WarnOnEverythingRule : IValidationRule
    {
        public IEnumerable<ValidationIssue> Check(QuestionDef question, IReadOnlyList<QuestionCell> cells, bool forSubmit)
        {
            yield return new(question.Key, null, "uji", Severity.Warning);
        }
    }
}
