using GriForms.Domain.Enums;

namespace GriForms.Application.Consolidation;

// Total dihitung di memori dari sel yang terisi saja (SQLite menyimpan decimal sebagai TEXT)
public sealed class MonthlyCardFactory : ICardFactory
{
    public InputType Type => InputType.MonthlyTable;

    public ICard Build(CardInput input)
    {
        var q = input.Question;
        var labels = (q.Rows ?? new()).ToDictionary(r => $"{q.Key}/{r.Key}", r => r.Label);

        var rows = new List<QuantRow>();
        foreach (var site in input.SiteNames)
        {
            var siteRows = input.Rows.Where(r => r.SiteName == site).GroupBy(r => r.QuestionKey);
            foreach (var g in siteRows)
            {
                var months = new decimal?[12];
                foreach (var cell in g.Where(c => c.State == ValueState.Filled))
                {
                    if (MonthIndex(cell.PeriodKey) is { } m) months[m] = cell.Number;
                }

                var filled = months.Where(v => v.HasValue).Select(v => v!.Value).ToList();
                rows.Add(new QuantRow(
                    site,
                    labels.GetValueOrDefault(g.Key) ?? g.Key[(g.Key.IndexOf('/') + 1)..],
                    months,
                    filled.Count > 0 ? filled.Sum() : (decimal?)null,
                    NotApplicable: g.All(c => c.State == ValueState.NotApplicable)));
            }
        }

        return new QuantitativeCard(input.Disclosure.No, q.Label, q.Unit, rows);
    }

    // "2026-01" -> 0
    private static int? MonthIndex(string? periodKey) =>
        periodKey is { Length: 7 } && int.TryParse(periodKey[5..], out var m) && m is >= 1 and <= 12
            ? m - 1
            : null;
}
