using GriForms.Application.Forms;
using GriForms.Application.Validation;
using GriForms.Application.Validation.Rules;
using GriForms.Domain.Enums;

namespace GriForms.Tests.Validation;

public class ValidationRuleTests
{
    private static QuestionCell Cell(AnswerValue v, string key = "q", string? period = null) => new(key, period, v);
    private static QuestionCell Num(decimal n) => Cell(new AnswerValue(null, n));
    private static QuestionCell Txt(string t) => Cell(new AnswerValue(t, null));

    private static List<ValidationIssue> Run(IValidationRule rule, QuestionDef q, bool forSubmit, params QuestionCell[] cells) =>
        rule.Check(q, cells, forSubmit).ToList();

    [Fact]
    public void Required_fails_on_submit_when_empty()
    {
        var q = new QuestionDef("q", InputType.Text, "Q", Required: true);
        Assert.Single(Run(new RequiredRule(), q, forSubmit: true));
    }

    [Fact]
    public void Required_passes_when_filled_or_not_applicable()
    {
        var q = new QuestionDef("q", InputType.Text, "Q", Required: true);
        Assert.Empty(Run(new RequiredRule(), q, true, Txt("isi")));
        Assert.Empty(Run(new RequiredRule(), q, true, Cell(new AnswerValue(null, null, ValueState.NotApplicable))));
    }

    [Fact]
    public void Required_is_not_checked_when_saving_draft()
    {
        var q = new QuestionDef("q", InputType.Text, "Q", Required: true);
        Assert.Empty(Run(new RequiredRule(), q, forSubmit: false));
    }

    [Fact]
    public void Range_fails_outside_bounds_and_passes_inside()
    {
        var q = new QuestionDef("q", InputType.Number, "Q", Validation: new ValidationDef(Min: 0, Max: 100));
        Assert.Single(Run(new RangeRule(), q, false, Num(-1)));
        Assert.Single(Run(new RangeRule(), q, false, Num(101)));
        Assert.Empty(Run(new RangeRule(), q, false, Num(0), Num(100)));
    }

    [Fact]
    public void IntegerOnly_fails_on_fraction_and_passes_on_whole_number()
    {
        var q = new QuestionDef("q", InputType.Number, "Q", Validation: new ValidationDef(IntegerOnly: true));
        Assert.Single(Run(new IntegerOnlyRule(), q, false, Num(1.5m)));
        Assert.Empty(Run(new IntegerOnlyRule(), q, false, Num(2m)));
    }

    [Fact]
    public void MaxLength_fails_on_long_text_and_passes_on_short_text()
    {
        var q = new QuestionDef("q", InputType.Text, "Q", Validation: new ValidationDef(MaxLength: 5));
        Assert.Single(Run(new MaxLengthRule(), q, false, Txt("123456")));
        Assert.Empty(Run(new MaxLengthRule(), q, false, Txt("12345")));
    }

    [Fact]
    public void Select_fails_on_unknown_option_and_passes_on_known_option()
    {
        var q = new QuestionDef("q", InputType.Select, "Q", Options: new() { new OptionDef("a", "A") });
        Assert.Single(Run(new SelectOptionRule(), q, false, Txt("x")));
        Assert.Empty(Run(new SelectOptionRule(), q, false, Txt("a")));
    }

    [Fact]
    public void NotApplicable_with_value_fails_and_without_value_passes()
    {
        var q = new QuestionDef("q", InputType.Number, "Q");
        Assert.Single(Run(new NotApplicableRule(), q, false, Cell(new AnswerValue(null, 5, ValueState.NotApplicable))));
        Assert.Empty(Run(new NotApplicableRule(), q, false, Cell(new AnswerValue(null, null, ValueState.NotApplicable))));
    }

    public static TheoryData<IValidationRule> AllRules => new()
    {
        new RequiredRule(), new RangeRule(), new IntegerOnlyRule(),
        new MaxLengthRule(), new SelectOptionRule(), new NotApplicableRule(),
    };

    // Kontrak Liskov: aturan tidak melempar dan tidak mengeluarkan issue untuk input valid
    [Theory]
    [MemberData(nameof(AllRules))]
    public void Rule_returns_no_issue_and_does_not_throw_for_valid_input(IValidationRule rule)
    {
        var q = new QuestionDef("q", InputType.Number, "Q", Required: true,
            Validation: new ValidationDef(Min: 0, Max: 10, IntegerOnly: true, MaxLength: 10));

        Assert.Empty(Run(rule, q, true, Num(5)));
        Assert.Empty(Run(rule, new QuestionDef("q", InputType.Text, "Q"), true));
    }
}
