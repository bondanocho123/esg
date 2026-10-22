using GriForms.Application.Forms;
using GriForms.Tests.Support;

namespace GriForms.Tests.Forms;

public class AutosaveCoordinatorTests
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan WaitLimit =TimeSpan.FromSeconds(5);

    [Fact]
    public async Task Burst_of_changes_is_saved_in_one_call()
    {
        var state = new FormState();
        var writer = new FakeAnswerWriter();
        using var autosave = new AutosaveCoordinator(state, writer, submissionId: 7, Delay);
        var saved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        autosave.Saved += () => saved.TrySetResult();

        state.Set("a", null, new AnswerValue("x", null));
        state.Set("b", null, new AnswerValue(null, 1));
        state.Set("c/r1", "2026-01", new AnswerValue(null, 2));

        await saved.Task.WaitAsync(WaitLimit);
        await Task.Delay(Delay * 4);

        var call = Assert.Single(writer.Calls);
        Assert.Equal(new[] { "a", "b", "c/r1|2026-01" }, call.Select(c => c.CellKey).Order());
        Assert.NotNull(autosave.SavedAt);
    }

    [Fact]
    public async Task Cleared_cell_is_sent_as_null_value()
    {
        var state = new FormState();
        var writer = new FakeAnswerWriter();
        using var autosave = new AutosaveCoordinator(state, writer, 7, Delay);

        state.Set("a", null, new AnswerValue("x", null));
        state.Set("a", null, null);

        await writer.Called.Task.WaitAsync(WaitLimit);

        var change = Assert.Single(Assert.Single(writer.Calls));
        Assert.Null(change.Value);
    }

    [Fact]
    public async Task Failed_is_raised_when_writer_throws_and_cells_stay_dirty()
    {
        var state = new FormState();
        var writer = new FakeAnswerWriter { ThrowWith = new InvalidOperationException("putus") };
        using var autosave = new AutosaveCoordinator(state, writer, 7, Delay);
        var failed = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        autosave.Failed += ex => failed.TrySetResult(ex);

        state.Set("a", null, new AnswerValue("x", null));

        var error = await failed.Task.WaitAsync(WaitLimit);
        Assert.Equal("putus", error.Message);
        Assert.Null(autosave.SavedAt);
        Assert.Single(state.TakeDirtyChanges());   // akan ikut di simpan berikutnya
    }

    [Fact]
    public async Task Saves_never_overlap_when_writer_is_slow()
    {
        var state = new FormState();
        var writer = new SlowWriter();
        using var autosave = new AutosaveCoordinator(state, writer, 7, TimeSpan.FromMilliseconds(10));

        state.Set("a", null, new AnswerValue("x", null));
        await Task.Delay(100);                 // simpan pertama sedang berjalan
        state.Set("b", null, new AnswerValue("y", null));
        await Task.Delay(100);                 // jeda kedua habis saat simpan pertama belum selesai
        writer.Release();

        await writer.TwoCalls.Task.WaitAsync(WaitLimit);
        Assert.Equal(1, writer.MaxConcurrent);
    }

    [Fact]
    public async Task Throwing_saved_handler_does_not_raise_failed_or_requeue_cells()
    {
        var state = new FormState();
        var writer = new FakeAnswerWriter();
        using var autosave = new AutosaveCoordinator(state, writer, 7, Delay);
        var failed = false;
        autosave.Failed += _ => failed = true;
        autosave.Saved += () => throw new InvalidOperationException("handler rusak");

        state.Set("a", null, new AnswerValue("x", null));
        await writer.Called.Task.WaitAsync(WaitLimit);
        await Task.Delay(Delay * 4);

        Assert.False(failed);
        Assert.Empty(state.TakeDirtyChanges());
    }

    private sealed class SlowWriter : GriForms.Application.Abstractions.IAnswerWriter
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _running, _calls;

        public int MaxConcurrent { get; private set; }
        public TaskCompletionSource TwoCalls { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Release() => _release.TrySetResult();

        public async Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct)
        {
            var now = Interlocked.Increment(ref _running);
            lock (this) MaxConcurrent = Math.Max(MaxConcurrent, now);
            await _release.Task;
            Interlocked.Decrement(ref _running);
            if (Interlocked.Increment(ref _calls) == 2) TwoCalls.TrySetResult();
        }
    }

    [Fact]
    public async Task No_save_after_dispose()
    {
        var state = new FormState();
        var writer = new FakeAnswerWriter();
        var autosave = new AutosaveCoordinator(state, writer, 7, Delay);

        autosave.Dispose();
        state.Set("a", null, new AnswerValue("x", null));
        await Task.Delay(Delay * 4);

        Assert.Empty(writer.Calls);
    }
}
