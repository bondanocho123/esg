using GriForms.Application.Abstractions;
using GriForms.Application.Forms;

namespace GriForms.Tests.Support;

public sealed class FakeUser(string? userId) : ICurrentUser
{
    public string? UserId { get; } = userId;
}

public sealed class FixedClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; } = utcNow;
}

public sealed class FakeFormContext(int year) : IFormContext
{
    public int SiteId => 1;
    public int Year { get; } = year;
}

// Penulis palsu untuk AutosaveCoordinator
public sealed class FakeAnswerWriter : IAnswerWriter
{
    private readonly object _gate = new();
    private readonly List<List<CellChange>> _calls = new();

    public Exception? ThrowWith { get; set; }
    public TaskCompletionSource Called { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IReadOnlyList<List<CellChange>> Calls
    {
        get { lock (_gate) return _calls.ToList(); }
    }

    public Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct)
    {
        lock (_gate) _calls.Add(changes.ToList());
        Called.TrySetResult();
        return ThrowWith is null ? Task.CompletedTask : Task.FromException(ThrowWith);
    }
}
