using GriForms.Domain.Enums;

namespace GriForms.Application.Consolidation;

public sealed class NarrativeCardFactory : ICardFactory
{
    public InputType Type => InputType.Text;

    public ICard Build(CardInput input)
    {
        var rows = input.SiteNames
            .Select(site =>
            {
                var answer = input.Rows.FirstOrDefault(r => r.SiteName == site);
                return new NarrativeRow(
                    site,
                    answer?.Text,
                    NotApplicable: answer?.State == ValueState.NotApplicable,
                    Missing: answer is null);
            })
            .ToList();

        return new NarrativeCard(input.Disclosure.No, input.Question.Label, rows);
    }
}
