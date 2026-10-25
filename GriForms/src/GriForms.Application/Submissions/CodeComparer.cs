namespace GriForms.Application.Submissions;

// Urutan kode site dan topik seperti dibaca orang: angka dibandingkan sebagai angka ("2" sebelum "10",
// "GRI-2" sebelum "GRI-14"), sisanya tanpa membedakan huruf besar kecil. Urutan string biasa di SQL
// menaruh "10" sebelum "2".
public sealed class CodeComparer : IComparer<string?>
{
    public static readonly CodeComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        int i = 0, j = 0;
        while (i < x.Length && j < y.Length)
        {
            if (char.IsAsciiDigit(x[i]) && char.IsAsciiDigit(y[j]))
            {
                var (a, nextI) = Digits(x, i);
                var (b, nextJ) = Digits(y, j);
                // Tanpa nol di depan, angka yang lebih panjang lebih besar; panjang sama dibandingkan per digit
                var byValue = a.Length != b.Length ? a.Length.CompareTo(b.Length) : string.CompareOrdinal(a, b);
                if (byValue != 0) return byValue;
                (i, j) = (nextI, nextJ);
            }
            else
            {
                var byChar = char.ToUpperInvariant(x[i]).CompareTo(char.ToUpperInvariant(y[j]));
                if (byChar != 0) return byChar;
                (i, j) = (i + 1, j + 1);
            }
        }

        var byLength = (x.Length - i).CompareTo(y.Length - j);
        return byLength != 0 ? byLength : string.CompareOrdinal(x, y);
    }

    private static (string Value, int Next) Digits(string s, int start)
    {
        var end = start;
        while (end < s.Length && char.IsAsciiDigit(s[end])) end++;
        var value = s[start..end].TrimStart('0');
        return (value, end);
    }
}
