using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Submissions;

// Pengisian form di tenant aktif. Semua query memakai filter tenant di AppDbContext, jadi site, periode,
// dan submission tenant lain tidak terlihat. Hak per site diperiksa ulang di setiap operasi.
// Menyimpan jawaban dan transisi diteruskan ke IAnswerWriter dan ISubmissionWorkflow setelah hak diperiksa.
public sealed class ReportingWorkspace(
    AppDbContext db, ICurrentUser user, ITenantContext tenant, IFormVersionResolver resolver,
    ISubmissionQueries queries, IAnswerWriter writer, ISubmissionWorkflow workflow, IUserAccounts accounts,
    ILogger<ReportingWorkspace> logger) : IReportingWorkspace
{
    public async Task<WorkspaceView> GetAsync(int? periodId, CancellationToken ct)
    {
        var (_, roles) = await MemberAsync(ct);

        var periods = await PeriodViewsAsync(db, ct);
        var period = periodId is { } id
            ? periods.FirstOrDefault(p => p.Id == id) ?? throw new InvalidOperationException($"Periode {id} tidak ditemukan.")
            : periods.FirstOrDefault(p => p.Status == PeriodStatus.Open) ?? periods.FirstOrDefault();
        if (period is null) return new WorkspaceView(periods, null, []);

        var sites = (await db.Sites.AsNoTracking().Where(s => s.IsActive).ToListAsync(ct))
            .Where(s => roles.For(s.Id).Count > 0)
            .OrderBy(s => s.Code, CodeComparer.Instance)
            .ToList();

        // Versi terbit yang berlaku untuk tahun periode, satu per topik (aturan FormVersionResolver)
        var year = period.Year;
        var current = (await db.FormVersions.AsNoTracking()
                .Where(v => v.IsPublished && v.RetiredAt == null && v.EffectiveFromYear <= year
                         && (v.EffectiveToYear == null || v.EffectiveToYear >= year))
                .Select(v => new { v.TopicId, v.VersionNo })
                .ToListAsync(ct))
            .GroupBy(v => v.TopicId)
            .ToDictionary(g => g.Key, g => g.Max(v => v.VersionNo));

        var submissions = await db.Submissions.AsNoTracking()
            .Where(s => s.PeriodId == period.Id)
            .Select(s => new { s.Id, s.SiteId, s.TopicId, s.Status, s.FormVersion.VersionNo })
            .ToListAsync(ct);

        // Topik tanpa versi berlaku tetap tampil bila sudah ada submission (mis. versinya dipensiunkan kemudian)
        var topicIds = current.Keys.Union(submissions.Select(s => s.TopicId)).ToHashSet();
        var topics = (await db.Topics.AsNoTracking().Where(t => topicIds.Contains(t.Id)).ToListAsync(ct))
            .OrderBy(t => t.Code, CodeComparer.Instance)
            .ToList();

        var rows = new List<WorkspaceRow>();
        foreach (var site in sites)
        {
            var siteRoles = roles.For(site.Id);
            foreach (var topic in topics)
            {
                var submission = submissions.FirstOrDefault(s => s.SiteId == site.Id && s.TopicId == topic.Id);
                if (submission is null && !current.ContainsKey(topic.Id)) continue;

                rows.Add(new WorkspaceRow(
                    site.Id, site.Code, site.Name, topic.Id, topic.Code, topic.Name, topic.Category,
                    submission?.VersionNo ?? current[topic.Id], submission?.Id, submission?.Status, siteRoles,
                    NextFor(siteRoles, submission?.Status, period.Status)));
            }
        }

        return new WorkspaceView(periods, period, rows);
    }

    public async Task<int> OpenAsync(int siteId, int periodId, int topicId, CancellationToken ct)
    {
        var (userId, roles) = await MemberAsync(ct);
        if (roles.For(siteId).Count == 0)
            throw new UnauthorizedAccessException("Anda tidak punya peran di site ini.");

        var existing = await db.Submissions.AsNoTracking()
            .Where(s => s.SiteId == siteId && s.PeriodId == periodId && s.TopicId == topicId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);
        if (existing is { } id) return id;

        if (!roles.For(siteId).Contains(RoleName.Contributor))
            throw new InvalidOperationException("Form ini belum dimulai. Hanya Contributor site ini yang bisa memulainya.");

        var period = await db.ReportingPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.Id == periodId, ct)
            ?? throw new InvalidOperationException($"Periode {periodId} tidak ditemukan.");
        if (period.Status == PeriodStatus.Closed)
            throw new InvalidOperationException($"Periode {period.Year} sudah ditutup.");
        if (!await db.Sites.AnyAsync(s => s.Id == siteId && s.IsActive, ct))
            throw new InvalidOperationException("Site tidak ditemukan atau sudah nonaktif.");

        // Versi dikunci sekali saat submission dibuat (bab 7)
        var version = await resolver.ResolveAsync(topicId, period.Year, ct);
        var submission = new Submission
        {
            SiteId = siteId, PeriodId = periodId, TopicId = topicId, FormVersionId = version.Id,
            Status = SubmissionStatus.Draft,
        };
        db.Submissions.Add(submission);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Contributor lain baru saja memulai form yang sama: pakai submission itu
            db.ChangeTracker.Clear();
            return await db.Submissions.AsNoTracking()
                .Where(s => s.SiteId == siteId && s.PeriodId == periodId && s.TopicId == topicId)
                .Select(s => s.Id)
                .FirstAsync(ct);
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
        db.Entry(submission).State = EntityState.Detached;

        using (logger.BeginScope(new Dictionary<string, object?> { ["TenantId"] = tenant.TenantId, ["UserId"] = userId }))
            logger.LogInformation("Submission {SubmissionId} dimulai: site {SiteId}, topik {TopicId}, periode {Year}, versi {VersionNo}",
                submission.Id, siteId, topicId, period.Year, version.VersionNo);
        return submission.Id;
    }

    public async Task<SubmissionWorkspace> GetSubmissionAsync(int submissionId, CancellationToken ct)
    {
        var (_, roles) = await MemberAsync(ct);
        var view = await queries.GetAsync(submissionId, ct)
            ?? throw new InvalidOperationException($"Submission {submissionId} tidak ditemukan.");
        var siteRoles = roles.For(view.SiteId);
        if (siteRoles.Count == 0)
            throw new UnauthorizedAccessException("Anda tidak punya peran di site submission ini.");

        var info = await db.Submissions.AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => new { s.Topic.Code, s.Topic.Name, s.FormVersion.VersionNo })
            .FirstAsync(ct);

        var open = view.PeriodStatus == PeriodStatus.Open;
        var transitions = open
            ? Submission.TransitionsFrom(view.Status)
                .Where(t => siteRoles.Contains(t.Role))
                .Select(t => new TransitionOption(t.To, t.Role, Submission.RequiresComment(t.To)))
                .ToList()
            : [];

        return new SubmissionWorkspace(view, info.Code, info.Name, info.VersionNo, siteRoles,
            CanEdit(siteRoles, view.Status, view.PeriodStatus), transitions, await HistoryAsync(submissionId, ct));
    }

    public async Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct)
    {
        await RequireSubmissionRolesAsync(submissionId, ct, (siteRoles, status, period) =>
            CanEdit(siteRoles, status, period)
                ? null
                : "Anda tidak bisa mengubah jawaban submission ini.");
        await writer.SaveCellsAsync(submissionId, changes, ct);
    }

    public async Task TransitionAsync(int submissionId, SubmissionStatus to, string? comment, CancellationToken ct)
    {
        RoleName? actor = null;
        await RequireSubmissionRolesAsync(submissionId, ct, (siteRoles, status, _) =>
        {
            actor = Submission.TransitionsFrom(status)
                .Where(t => t.To == to && siteRoles.Contains(t.Role))
                .Select(t => (RoleName?)t.Role)
                .FirstOrDefault();
            return actor is null ? $"Peran Anda tidak bisa mengubah status {status} menjadi {to}." : null;
        });
        await workflow.TransitionAsync(submissionId, to, actor!.Value, comment, ct);
    }

    // Periode terbaru lebih dulu
    internal static async Task<List<ReportingPeriodView>> PeriodViewsAsync(AppDbContext db, CancellationToken ct) =>
        await db.ReportingPeriods.AsNoTracking()
            .OrderByDescending(p => p.Year)
            .Select(p => new ReportingPeriodView(p.Id, p.Year, p.Status, db.Submissions.Count(s => s.PeriodId == p.Id)))
            .ToListAsync(ct);

    private static bool CanEdit(IReadOnlyList<RoleName> siteRoles, SubmissionStatus status, PeriodStatus period) =>
        period == PeriodStatus.Open
        && siteRoles.Contains(RoleName.Contributor)
        && status is SubmissionStatus.Draft or SubmissionStatus.Rejected;

    private static NextAction NextFor(IReadOnlyList<RoleName> roles, SubmissionStatus? status, PeriodStatus period)
    {
        if (period == PeriodStatus.Closed) return NextAction.None;
        return status switch
        {
            null when roles.Contains(RoleName.Contributor) => NextAction.Start,
            SubmissionStatus.Draft or SubmissionStatus.Rejected when roles.Contains(RoleName.Contributor) => NextAction.Fill,
            SubmissionStatus.Submitted when roles.Contains(RoleName.Reviewer) => NextAction.Review,
            SubmissionStatus.Reviewed when roles.Contains(RoleName.Approver) => NextAction.Approve,
            _ => NextAction.None,
        };
    }

    private async Task RequireSubmissionRolesAsync(int submissionId, CancellationToken ct,
        Func<IReadOnlyList<RoleName>, SubmissionStatus, PeriodStatus, string?> check)
    {
        var (_, roles) = await MemberAsync(ct);
        var s = await db.Submissions.AsNoTracking()
            .Where(x => x.Id == submissionId)
            .Select(x => new { x.SiteId, x.Status, PeriodStatus = x.Period.Status })
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"Submission {submissionId} tidak ditemukan.");

        var siteRoles = roles.For(s.SiteId);
        if (siteRoles.Count == 0)
            throw new UnauthorizedAccessException("Anda tidak punya peran di site submission ini.");
        if (check(siteRoles, s.Status, s.PeriodStatus) is { } message)
            throw new InvalidOperationException(message);
    }

    // Riwayat transisi dari AuditEntry yang ditulis SubmissionWorkflow; NewValue "Status" atau "Status: komentar"
    private async Task<IReadOnlyList<SubmissionHistoryEntry>> HistoryAsync(int submissionId, CancellationToken ct)
    {
        var entityId = submissionId.ToString();
        var entries = await db.AuditEntries.AsNoTracking()
            .Where(e => e.EntityType == nameof(Submission) && e.EntityId == entityId && e.Action == "Transition")
            .OrderBy(e => e.At).ThenBy(e => e.Id)
            .ToListAsync(ct);
        var emails = await accounts.GetEmailsAsync(entries.Select(e => e.UserId).Distinct().ToList(), ct);

        return entries.Select(e =>
        {
            var parts = (e.NewValue ?? "").Split(": ", 2);
            return new SubmissionHistoryEntry(e.At, emails.GetValueOrDefault(e.UserId) ?? e.UserId,
                e.OldValue ?? "", parts[0], parts.Length > 1 ? parts[1] : null);
        }).ToList();
    }

    private async Task<(string UserId, MemberRoles Roles)> MemberAsync(CancellationToken ct)
    {
        if (user.UserId is not { } userId || tenant.TenantId is null)
            throw new UnauthorizedAccessException("Pilih organisasi aktif lebih dulu.");

        // db.UserSiteRoles sudah dibatasi ke tenant aktif
        var roles = await db.UserSiteRoles.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => new MemberRole(r.Role, r.SiteId))
            .ToListAsync(ct);
        if (roles.Count == 0)
            throw new UnauthorizedAccessException("Anda belum punya peran di organisasi ini.");
        return (userId, new MemberRoles(roles));
    }

    private sealed record MemberRole(RoleName Role, int? SiteId);

    // Peran tanpa site (Admin, ReportViewer) berlaku untuk semua site tenant
    private sealed class MemberRoles(IReadOnlyList<MemberRole> roles)
    {
        public IReadOnlyList<RoleName> For(int siteId) =>
            roles.Where(r => r.SiteId == siteId || r.SiteId == null).Select(r => r.Role).Distinct().Order().ToList();
    }
}
