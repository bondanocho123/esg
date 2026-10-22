using ClosedXML.Excel;
using GriForms.Application.Abstractions;
using GriForms.Application.Consolidation;

namespace GriForms.Infrastructure.Excel;

public sealed class ClosedXmlConsolidationExporter(IEnumerable<ICardWriter> writers) : IConsolidationExporter
{
    private readonly IReadOnlyDictionary<Type, ICardWriter> _byType = writers.ToDictionary(w => w.CardType);

    public Stream Export(IEnumerable<ICard> cards)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Konsolidasi");

        var row = 1;
        foreach (var card in cards)
        {
            if (!_byType.TryGetValue(card.GetType(), out var writer))
                throw new NotSupportedException($"Kartu {card.GetType().Name} belum punya penulis.");
            row = writer.Write(ws, row, card);
        }

        ws.Column(1).Width = 2;
        ws.Columns(2, 16).AdjustToContents();

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
