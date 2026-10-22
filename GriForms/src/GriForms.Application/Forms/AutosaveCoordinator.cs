using GriForms.Application.Abstractions;

namespace GriForms.Application.Forms;

public sealed class AutosaveCoordinator : IDisposable
{
    private readonly FormState _state;
    private readonly IAnswerWriter _writer;
    private readonly int _submissionId;
    private readonly TimeSpan _delay;
    private readonly SemaphoreSlim _saveGate = new(1, 1);   // satu simpan pada satu waktu
    private CancellationTokenSource? _debounce;
    private volatile bool _disposed;

    public AutosaveCoordinator(FormState state, IAnswerWriter writer, int submissionId, TimeSpan? delay = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(writer);
        _state = state;
        _writer = writer;
        _submissionId = submissionId;
        _delay = delay is { } d && d >= TimeSpan.Zero ? d : TimeSpan.FromSeconds(1);
        _state.Changed += OnChanged;
    }

    public DateTime? SavedAt { get; private set; }
    public event Action? Saved;
    public event Action<Exception>? Failed;

    private void OnChanged()
    {
        if (_disposed) return;
        var next = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _debounce, next);
        previous?.Cancel();
        previous?.Dispose();
        _ = SaveLaterAsync(next.Token);
    }

    private async Task SaveLaterAsync(CancellationToken delayToken)
    {
        IReadOnlyList<CellChange> changes = Array.Empty<CellChange>();
        try
        {
            await Task.Delay(_delay, delayToken);          // hanya jeda yang bisa dibatalkan

            // simpan berikutnya menunggu simpan yang masih berjalan, supaya tidak tumpang tindih
            await _saveGate.WaitAsync(CancellationToken.None);
            try
            {
                if (_disposed) return;
                changes = _state.TakeDirtyChanges();
                if (changes.Count == 0) return;

                await _writer.SaveCellsAsync(_submissionId, changes, CancellationToken.None);
                SavedAt = DateTime.Now;
            }
            finally
            {
                _saveGate.Release();
            }

            RaiseSafely(Saved);
        }
        catch (OperationCanceledException) when (delayToken.IsCancellationRequested) { /* perubahan baru menggantikan */ }
        catch (Exception ex)
        {
            _state.MarkDirty(changes);   // ikut tersimpan pada perubahan berikutnya
            try { Failed?.Invoke(ex); } catch { /* handler UI tidak boleh membuat task ini tak teramati */ }
        }
    }

    private static void RaiseSafely(Action? handler)
    {
        try { handler?.Invoke(); } catch { /* handler UI tidak boleh dianggap kegagalan simpan */ }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _state.Changed -= OnChanged;
        var current = Interlocked.Exchange(ref _debounce, null);
        current?.Cancel();
        current?.Dispose();
    }
}
