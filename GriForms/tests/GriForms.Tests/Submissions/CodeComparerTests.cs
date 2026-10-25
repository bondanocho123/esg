using GriForms.Application.Submissions;

namespace GriForms.Tests.Submissions;

public class CodeComparerTests
{
    [Fact]
    public void Numbers_inside_codes_are_ordered_by_value()
    {
        var codes = new[] { "10", "2", "1", "GRI-14", "gri-2", "302", "1001", "103", "002" };

        var ordered = codes.Order(CodeComparer.Instance).ToArray();

        Assert.Equal(new[] { "1", "002", "2", "10", "103", "302", "1001", "gri-2", "GRI-14" }, ordered);
    }

    [Fact]
    public void Comparison_is_consistent_for_equal_and_null_values()
    {
        Assert.Equal(0, CodeComparer.Instance.Compare("403", "403"));
        Assert.True(CodeComparer.Instance.Compare(null, "1") < 0);
        Assert.True(CodeComparer.Instance.Compare("Env", "env") != 0);   // tetap total order untuk pengurutan yang stabil
    }
}
