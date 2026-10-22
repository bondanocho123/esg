using ClosedXML.Excel;
using GriForms.Application.Consolidation;
using GriForms.Infrastructure.Excel;

namespace GriForms.Tests.Excel;

public class ConsolidationExporterTests
{
    [Fact]
    public void Writes_narrative_and_quantitative_cards()
    {
        var exporter = new ClosedXmlConsolidationExporter(new ICardWriter[] { new NarrativeCardWriter(), new QuantitativeCardWriter() });
        var months = new decimal?[12];
        months[0] = 10;

        using var stream = exporter.Export(new ICard[]
        {
            new NarrativeCard("403-5", "Pelatihan", new[] { new NarrativeRow("Head Office", null, NotApplicable: true, Missing: false) }),
            new QuantitativeCard("403-5", "Peserta", "orang", new[] { new QuantRow("Head Office", "K3", months, 10, false) }),
        });

        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheet("Konsolidasi");
        Assert.Equal("Tidak berlaku", ws.Cell(3, 3).GetString());
        // kartu naratif memakai baris 1-3 dan satu baris kosong; kartu kuantitatif mulai di baris 5
        Assert.Equal(10m, ws.Cell(7, 16).GetValue<decimal>());
    }

    // Open/Closed: jenis kartu baru cukup satu penulis baru, exporter tidak diubah
    [Fact]
    public void New_card_type_needs_only_a_new_writer()
    {
        var exporter = new ClosedXmlConsolidationExporter(new ICardWriter[] { new NoteCardWriter() });

        using var stream = exporter.Export(new ICard[] { new NoteCard("403-5", "Catatan") });

        using var wb = new XLWorkbook(stream);
        Assert.Equal("Catatan", wb.Worksheet("Konsolidasi").Cell(1, 2).GetString());
    }

    [Fact]
    public void Card_without_writer_is_reported()
    {
        var exporter = new ClosedXmlConsolidationExporter(Array.Empty<ICardWriter>());

        Assert.Throws<NotSupportedException>(() => exporter.Export(new ICard[] { new NoteCard("403-5", "Catatan") }));
    }

    private sealed record NoteCard(string DisclosureNo, string Title) : ICard;

    private sealed class NoteCardWriter : CardWriter<NoteCard>
    {
        protected override int WriteCard(IXLWorksheet ws, int row, NoteCard card)
        {
            ws.Cell(row, 2).Value = card.Title;
            return row + 2;
        }
    }
}
