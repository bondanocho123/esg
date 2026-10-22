using ClosedXML.Excel;
using GriForms.Application.Consolidation;

namespace GriForms.Infrastructure.Excel;

// Satu penulis per jenis kartu. Kartu baru cukup menambah penulis dan satu AddCardWriter, tanpa mengubah exporter.
public interface ICardWriter
{
    Type CardType { get; }

    // Mengembalikan baris berikutnya yang kosong
    int Write(IXLWorksheet ws, int row, ICard card);
}

public abstract class CardWriter<TCard> : ICardWriter where TCard : ICard
{
    public Type CardType => typeof(TCard);

    public int Write(IXLWorksheet ws, int row, ICard card) => WriteCard(ws, row, (TCard)card);

    protected abstract int WriteCard(IXLWorksheet ws, int row, TCard card);

    // Judul kartu dengan spine di kolom A
    protected static void WriteTitle(IXLWorksheet ws, int row, string title)
    {
        ws.Cell(row, 2).Value = title;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = Palette.Bronze;
    }

    // Header tabel dari kolom B sampai lastCol
    protected static void StyleHeader(IXLWorksheet ws, int row, int lastCol)
    {
        var range = ws.Range(row, 2, row, lastCol);
        range.Style.Fill.BackgroundColor = Palette.Teal;
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Font.Bold = true;
    }

    // Spine dan, untuk baris ganjil, warna selang-seling dari kolom B sampai lastCol
    protected static void StyleDataRow(IXLWorksheet ws, int row, int lastCol, bool banded)
    {
        ws.Cell(row, 1).Style.Fill.BackgroundColor = Palette.Bronze;
        if (banded) ws.Range(row, 2, row, lastCol).Style.Fill.BackgroundColor = Palette.Band;
    }
}

internal static class Palette
{
    public static readonly XLColor Teal = XLColor.FromHtml("#2FA394");     // header tabel
    public static readonly XLColor Bronze = XLColor.FromHtml("#A9895D");   // spine kartu
    public static readonly XLColor Band = XLColor.FromHtml("#EAF4F2");     // baris selang-seling
}
