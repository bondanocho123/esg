using ClosedXML.Excel;
using GriForms.Application.Consolidation;

namespace GriForms.Infrastructure.Excel;

public sealed class QuantitativeCardWriter : CardWriter<QuantitativeCard>
{
    private static readonly string[] Header =
        { "Site", "Baris", "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des", "Total" };

    protected override int WriteCard(IXLWorksheet ws, int row, QuantitativeCard card)
    {
        WriteTitle(ws, row++,
            string.IsNullOrEmpty(card.Unit)
                ? $"{card.DisclosureNo}  {card.Title}"
                : $"{card.DisclosureNo}  {card.Title} ({card.Unit})");

        for (var c = 0; c < Header.Length; c++)
            ws.Cell(row, c + 2).Value = Header[c];
        StyleHeader(ws, row++, Header.Length + 1);

        const int lastCol = 16;   // kolom P, Total
        var i = 0;
        foreach (var r in card.Rows)
        {
            ws.Cell(row, 2).Value = r.SiteName;
            ws.Cell(row, 3).Value = r.RowLabel;
            if (r.NotApplicable)
            {
                ws.Cell(row, 4).Value = "Tidak berlaku";
            }
            else
            {
                for (var m = 0; m < 12; m++)
                    if (r.Months[m] is { } v) ws.Cell(row, 4 + m).Value = v;
                if (r.Total is { } t) ws.Cell(row, lastCol).Value = t;
            }

            StyleDataRow(ws, row++, lastCol, banded: i++ % 2 == 1);
        }
        return row + 1;   // baris kosong antar kartu
    }
}
