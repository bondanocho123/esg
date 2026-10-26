using System.Globalization;
using System.Text.RegularExpressions;

namespace GriForms.Application.Forms;

// Evaluator kecil untuk VisibleWhen dengan sintaks terbatas: kunci == 'nilai' atau kunci != 'nilai'.
// Sengaja tidak mengevaluasi string dari JSON sebagai kode.
public static partial class VisibleRules
{
    public static Func<QuestionDef, bool> IsVisible(FormState state) => q => Evaluate(q.VisibleWhen, state);

    public static bool Evaluate(string? expression, FormState state)
    {
        if (string.IsNullOrWhiteSpace(expression)) return true;

        var match = Expression().Match(expression);
        if (!match.Success) return true;   // ekspresi tidak dikenal ditolak saat publish, bukan saat render

        var value = state.Get(match.Groups["key"].Value);
        var actual = value?.Text ?? value?.Number?.ToString(CultureInfo.InvariantCulture);
        var equal = string.Equals(actual, match.Groups["value"].Value, StringComparison.OrdinalIgnoreCase);

        return match.Groups["op"].Value == "==" ? equal : !equal;
    }

    // Dipakai FormDefinitionValidator sebelum publish: sintaks dikenal dan kunci yang dirujuk
    public static bool TryParse(string expression, out string key)
    {
        var match = Expression().Match(expression);
        key = match.Success ? match.Groups["key"].Value : "";
        return match.Success;
    }

    [GeneratedRegex(@"^\s*(?<key>[\w\-/]+)\s*(?<op>==|!=)\s*'(?<value>[^']*)'\s*$")]
    private static partial Regex Expression();
}
