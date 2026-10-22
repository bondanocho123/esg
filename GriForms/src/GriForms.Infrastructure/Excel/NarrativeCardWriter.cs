using ClosedXML.Excel;
using GriForms.Application.Consolidation;

namespace GriForms.Infrastructure.Excel;

public sealed class NarrativeCardWriter : CardWriter<NarrativeCard>
{
    protected override int WriteCard(IXLWorksheet ws, int row, NarrativeCard card)
    {
        WriteTitle(ws, row++, $"{card.DisclosureNo}  {card.Title}");

        ws.Cell(row, 2).Value = "Site";
        ws.Cell(row, 3).Value = "Uraian";
        StyleHeader(ws, row++, 3);

        var i = 0;
        foreach (var r in card.Rows)
        {
            ws.Cell(row, 2).Value = r.SiteName;
            // Tidak berlaku tampil sebagai teks, bukan "0" atau "-"
            ws.Cell(row, 3).Value = r.NotApplicable ? "Tidak berlaku" : r.Missing ? "Belum diisi" : r.Text ?? "";

            StyleDataRow(ws, row++, 3, banded: i++ % 2 == 1);
        }
        return row + 1;   // baris kosong antar kartu
    }
}
