using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using GriForms.Domain.Enums;

namespace GriForms.Application.Abstractions;

// Pengisian form terbit di tenant aktif. Hak dibaca dari UserSiteRole pengguna di site submission:
// Contributor mengisi dan mengajukan, Reviewer meninjau, Approver menyetujui, Admin membuka kembali,
// ReportViewer hanya membaca. Admin dan ReportViewer berlaku di semua site tenant.
// Tanpa tenant aktif atau tanpa peran di site itu: UnauthorizedAccessException. Status tidak cocok: InvalidOperationException.
public interface IReportingWorkspace
{
    // periodId null: periode terbuka terbaru, atau periode terbaru bila semuanya sudah ditutup
    Task<WorkspaceView> GetAsync(int? periodId, CancellationToken ct);

    // Submission yang sudah ada dibuka oleh siapa pun yang punya peran di site-nya.
    // Submission baru hanya dibuat Contributor site itu, dengan versi form terbit yang berlaku untuk tahun periode.
    Task<int> OpenAsync(int siteId, int periodId, int topicId, CancellationToken ct);

    Task<SubmissionWorkspace> GetSubmissionAsync(int submissionId, CancellationToken ct);

    // Hanya Contributor site, selama submission Draft/Rejected dan periode terbuka
    Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct);

    // Peran pelaku ditentukan dari keanggotaan pengguna, bukan dari pemanggil
    Task TransitionAsync(int submissionId, SubmissionStatus to, string? comment, CancellationToken ct);
}

// Halaman Periode pelaporan, khusus Admin tenant aktif
public interface IReportingPeriods
{
    Task<IReadOnlyList<ReportingPeriodView>> GetPeriodsAsync(CancellationToken ct);

    Task<int> CreatePeriodAsync(int year, CancellationToken ct);

    // Periode tertutup: submission hanya bisa dibaca
    Task SetStatusAsync(int periodId, PeriodStatus status, CancellationToken ct);
}
