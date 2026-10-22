using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Consolidation;

public record CardInput(DisclosureDef Disclosure, QuestionDef Question, IReadOnlyList<string> SiteNames, IReadOnlyList<ConsolidatedRow> Rows);

// Satu pabrik kartu per tipe input. Tipe baru cukup menambah kelas dan satu registrasi DI.
public interface ICardFactory
{
    InputType Type { get; }
    ICard Build(CardInput input);
}

public interface IConsolidationCardBuilder
{
    IReadOnlyList<ICard> Build(ConsolidationResult result, FormDefinition definition);
}

public sealed class ConsolidationCardBuilder(IEnumerable<ICardFactory> factories) : IConsolidationCardBuilder
{
    private readonly IReadOnlyDictionary<InputType, ICardFactory> _byType = factories.ToDictionary(f => f.Type);

    public IReadOnlyList<ICard> Build(ConsolidationResult result, FormDefinition definition)
    {
        var siteNames = result.Rows
            .GroupBy(r => r.SiteId)
            .OrderBy(g => g.Key)
            .Select(g => g.First().SiteName)
            .ToList();
        var byQuestion = result.Rows.ToLookup(r => FormState.QuestionOf(r.QuestionKey));

        var cards = new List<ICard>();
        foreach (var d in definition.Disclosures)
        foreach (var q in d.Questions)
        {
            var rows = byQuestion[q.Key].ToList();
            if (_byType.TryGetValue(q.Type, out var factory))
                cards.Add(factory.Build(new CardInput(d, q, siteNames, rows)));
            else if (rows.Count > 0)
                // jangan membuang jawaban Approved secara diam-diam dari export
                throw new NotSupportedException(
                    $"Pertanyaan {q.Key} bertipe {q.Type} punya {rows.Count} jawaban tetapi belum punya ICardFactory.");
        }
        return cards;
    }
}
