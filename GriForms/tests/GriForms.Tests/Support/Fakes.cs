using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Application.Tenancy;

namespace GriForms.Tests.Support;

// Akun Identity di memori. UserId = "u-" + email agar mudah dibaca di tes.
public sealed class FakeUserAccounts : IUserAccounts
{
    private readonly Dictionary<string, AccountInfo> _byEmail = new(StringComparer.OrdinalIgnoreCase);

    public List<(string Email, string Password)> Created { get; } = new();
    public List<string> Deleted { get; } = new();

    // Mensimulasikan kegagalan di tengah bootstrap atau pembuatan akun
    public string? FailFindFor { get; set; }

    public AccountInfo Add(string email)
    {
        var account = new AccountInfo($"u-{email}", email);
        _byEmail[email] = account;
        return account;
    }

    public Task<AccountInfo?> FindByEmailAsync(string email, CancellationToken ct)
    {
        if (string.Equals(email, FailFindFor, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Gagal membaca akun {email}.");
        return Task.FromResult(_byEmail.GetValueOrDefault(email));
    }

    public Task<AccountInfo> CreateAsync(string email, string initialPassword, CancellationToken ct)
    {
        Created.Add((email, initialPassword));
        return Task.FromResult(Add(email));
    }

    // Mensimulasikan kompensasi yang gagal menghapus akun
    public bool FailDelete { get; set; }

    public Task DeleteAsync(string userId, CancellationToken ct)
    {
        if (FailDelete) throw new InvalidOperationException("Gagal menghapus akun.");
        Deleted.Add(userId);
        foreach (var email in _byEmail.Where(x => x.Value.UserId == userId).Select(x => x.Key).ToList())
            _byEmail.Remove(email);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<string, string>> GetEmailsAsync(IReadOnlyCollection<string> userIds, CancellationToken ct) =>
        Task.FromResult<IReadOnlyDictionary<string, string>>(
            _byEmail.Values.Where(a => userIds.Contains(a.UserId)).ToDictionary(a => a.UserId, a => a.Email));
}

public sealed class FakeUser(string? userId) : ICurrentUser
{
    public string? UserId { get; set; } = userId;
}

public sealed class FakeTenant(int? tenantId) : ITenantContext
{
    public int? TenantId { get; set; } = tenantId;
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
