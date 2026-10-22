using ClosedXML.Excel;
using GriForms.Application.Abstractions;
using GriForms.Application.Imports;

namespace GriForms.Infrastructure.Excel;

// Membaca kolom A sampai L sheet Data-Konsolidasi dari file Excel lama (bab 13)
public sealed class DataKonsolidasiImporter : IExcelImporter
{
    public IEnumerable<LegacyRow> Read(string path)
    {
        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheet("Data-Konsolidasi");
        var used = ws.RangeUsed();
        if (used is null) yield break;

        foreach (var r in used.RowsUsed().Skip(1))   // lewati baris header
        {
            var number = r.Cell(10).TryGetValue<decimal>(out var n) ? n : (decimal?)null;
            yield return new LegacyRow(
                SiteId: r.Cell(1).TryGetValue<int>(out var sid) ? sid : null,
                NoQz:   r.Cell(7).GetString().Trim(),
                Qz:     r.Cell(8).GetString().Trim(),
                Period: NullIfEmpty(r.Cell(9).GetString()),
                Number: number,
                Text:   NullIfEmpty(r.Cell(11).GetString()),
                Unit:   NullIfEmpty(r.Cell(12).GetString()));
        }
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
