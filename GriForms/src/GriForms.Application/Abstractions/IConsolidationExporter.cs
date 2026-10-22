using GriForms.Application.Consolidation;

namespace GriForms.Application.Abstractions;

public interface IConsolidationExporter
{
    Stream Export(IEnumerable<ICard> cards);
}
