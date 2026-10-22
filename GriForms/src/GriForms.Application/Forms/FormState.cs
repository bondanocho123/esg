using GriForms.Domain.Enums;

namespace GriForms.Application.Forms;

public record AnswerValue(string? Text, decimal? Number, ValueState State = ValueState.Filled);
public record CellChange(string CellKey, AnswerValue? Value);

public class FormState
{
    private readonly Dictionary<string, AnswerValue> _cells = new();
    private readonly HashSet<string> _dirty = new();
    private readonly object _gate = new();

    public event Action? Changed;

    public IReadOnlyDictionary<string, AnswerValue> Cells => _cells;

    public static string CellKey(string questionKey, string? periodKey = null)
        => periodKey is null ? questionKey : $"{questionKey}|{periodKey}";

    public static (string QuestionKey, string? PeriodKey) SplitCellKey(string cellKey)
    {
        var i = cellKey.IndexOf('|');
        return i < 0 ? (cellKey, null) : (cellKey[..i], cellKey[(i + 1)..]);
    }

    // "403-5-B/k3-dasar" milik pertanyaan "403-5-B"
    public static string QuestionOf(string questionKey)
    {
        var i = questionKey.IndexOf('/');
        return i < 0 ? questionKey : questionKey[..i];
    }

    public AnswerValue? Get(string questionKey, string? periodKey = null)
        => _cells.GetValueOrDefault(CellKey(questionKey, periodKey));

    // Mengisi jawaban yang sudah tersimpan tanpa menandainya berubah
    public void Load(IReadOnlyDictionary<string, AnswerValue> cells)
    {
        lock (_gate)
        {
            _cells.Clear();
            _dirty.Clear();
            foreach (var (key, value) in cells) _cells[key] = value;
        }
        Changed?.Invoke();
    }

    public void Set(string questionKey, string? periodKey, AnswerValue? value)
    {
        var key = CellKey(questionKey, periodKey);
        lock (_gate)
        {
            if (value is null) _cells.Remove(key); else _cells[key] = value;
            _dirty.Add(key);
        }
        Changed?.Invoke();
    }

    public IReadOnlyList<CellChange> TakeDirtyChanges()
    {
        lock (_gate)
        {
            var changes = _dirty
                .Select(k => new CellChange(k, _cells.GetValueOrDefault(k)))
                .ToList();
            _dirty.Clear();
            return changes;
        }
    }

    // Menandai ulang sel yang gagal disimpan supaya ikut di simpan berikutnya
    public void MarkDirty(IEnumerable<CellChange> changes)
    {
        lock (_gate)
        {
            foreach (var c in changes) _dirty.Add(c.CellKey);
        }
    }
}
