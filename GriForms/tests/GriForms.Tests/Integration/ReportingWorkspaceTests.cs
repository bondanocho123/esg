using GriForms.Application;
using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using GriForms.Application.Validation;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Forms;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Submissions;
using GriForms.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

// Pengisian form: hak per site dari UserSiteRole, alur Draft -> Submitted -> Reviewed -> Approved -> Draft
public class ReportingWorkspaceTests : IDisposable
{
    private const int A = Tenant.LegacyId;
    private const int Site1 = 1;
    private const int Site2 = 2;

    private static readonly IValidationEngine Validation =
        new ServiceCollection().AddApplication().BuildServiceProvider().GetRequiredService<IValidationEngine>();

    private readonly TestDb _db = new();
    private readonly FakeUserAccounts _accounts = new();
    private readonly (int TopicId, int PeriodId, int FormVersionId) _seed;

    public ReportingWorkspaceTests()
    {
        _seed = _db.SeedTopic(year: 2026);
        using var db = _db.CreateContext();
        db.AddRange(
            new UserSiteRole { UserId = "contrib", Role = RoleName.Contributor, SiteId = Site1 },
            new UserSiteRole { UserId = "reviewer", Role = RoleName.Reviewer, SiteId = Site1 },
            new UserSiteRole { UserId = "approver", Role = RoleName.Approver, SiteId = Site1 },
            new UserSiteRole { UserId = "admin", Role = RoleName.Admin },
            new UserSiteRole { UserId = "viewer", Role = RoleName.ReportViewer },
            new UserSiteRole { UserId = "contrib-2", Role = RoleName.Contributor, SiteId = Site2 });
        db.SaveChanges();
    }

    private ReportingWorkspace Workspace(string userId, out AppDbContext ctx, int? tenantId = A)
    {
        ctx = _db.CreateContext(new FakeTenant(tenantId));
        var user = new FakeUser(userId);
        var reader = new JsonFormDefinitionReader();
        return new ReportingWorkspace(ctx, user, new FakeTenant(tenantId), new FormVersionResolver(ctx),
            new SubmissionQueries(ctx, reader), new AnswerWriter(ctx, ctx, reader, user, _db.Clock),
            new SubmissionWorkflow(ctx, ctx, reader, Validation, user, _db.Clock), _accounts,
            NullLogger<ReportingWorkspace>.Instance);
    }

    private async Task<int> StartAsync()
    {
        var workspace = Workspace("contrib", out var ctx);
        await using var _ = ctx;
        return await workspace.OpenAsync(Site1, _seed.PeriodId, _seed.TopicId, CancellationToken.None);
    }

    private async Task AsAsync(string userId, Func<ReportingWorkspace, Task> action)
    {
        var workspace = Workspace(userId, out var ctx);
        await using var _ = ctx;
        await action(workspace);
    }

    [Fact]
    public async Task Workspace_lists_sites_by_role_with_next_action()
    {
        WorkspaceView contrib = null!, reviewer = null!, admin = null!;
        await AsAsync("contrib", async w => contrib = await w.GetAsync(null, CancellationToken.None));
        await AsAsync("reviewer", async w => reviewer = await w.GetAsync(null, CancellationToken.None));
        await AsAsync("admin", async w => admin = await w.GetAsync(null, CancellationToken.None));

        var row = Assert.Single(contrib.Rows);
        Assert.Equal((Site1, "403", 1, NextAction.Start, (int?)null), (row.SiteId, row.TopicCode, row.VersionNo, row.Next, row.SubmissionId));
        Assert.Equal(2026, contrib.Period!.Year);
        Assert.Equal(NextAction.None, Assert.Single(reviewer.Rows).Next);
        Assert.Equal(6, admin.Rows.Count);
        Assert.All(admin.Rows, r => Assert.Equal(NextAction.None, r.Next));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            AsAsync("tanpa-peran", w => w.GetAsync(null, CancellationToken.None)));

        // Tanpa tenant aktif (mis. PlatformAdmin tanpa keanggotaan)
        var noTenant = Workspace("contrib", out var ctx, tenantId: null);
        await using var _ = ctx;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => noTenant.GetAsync(null, CancellationToken.None));
    }

    [Fact]
    public async Task Only_site_contributor_can_start_and_others_reuse_existing_submission()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AsAsync("reviewer", w => w.OpenAsync(Site1, _seed.PeriodId, _seed.TopicId, CancellationToken.None)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            AsAsync("contrib-2", w => w.OpenAsync(Site1, _seed.PeriodId, _seed.TopicId, CancellationToken.None)));

        var id = await StartAsync();
        Assert.Equal(id, await StartAsync());
        int reviewerId = 0;
        await AsAsync("reviewer", async w => reviewerId = await w.OpenAsync(Site1, _seed.PeriodId, _seed.TopicId, CancellationToken.None));
        Assert.Equal(id, reviewerId);

        using var check = _db.CreateContext();
        var submission = Assert.Single(check.Submissions);
        Assert.Equal((SubmissionStatus.Draft, _seed.FormVersionId, A), (submission.Status, submission.FormVersionId, submission.TenantId));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            AsAsync("contrib-2", w => w.GetSubmissionAsync(id, CancellationToken.None)));
    }

    [Fact]
    public async Task Submission_moves_through_each_role_and_records_history()
    {
        var id = await StartAsync();
        var none = CancellationToken.None;

        // Reviewer tidak bisa mengisi; Contributor gagal mengajukan selama isian wajib kosong
        await Assert.ThrowsAsync<InvalidOperationException>(() => AsAsync("reviewer", w =>
            w.SaveCellsAsync(id, [new CellChange("403-5-A", new AnswerValue("x", null))], none)));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AsAsync("contrib", w => w.TransitionAsync(id, SubmissionStatus.Submitted, null, none)));

        await AsAsync("contrib", async w =>
        {
            var opened = await w.GetSubmissionAsync(id, none);
            Assert.True(opened.CanEdit);
            Assert.Equal([SubmissionStatus.Submitted], opened.Transitions.Select(t => t.To));
            await w.SaveCellsAsync(id, [new CellChange("403-5-A", new AnswerValue("Pelatihan K3 dasar", null))], none);
            await w.TransitionAsync(id, SubmissionStatus.Submitted, null, none);
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AsAsync("contrib", w => w.TransitionAsync(id, SubmissionStatus.Reviewed, null, none)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => AsAsync("contrib", w =>
            w.SaveCellsAsync(id, [new CellChange("403-5-A", new AnswerValue("ubah", null))], none)));

        await AsAsync("reviewer", async w =>
        {
            var opened = await w.GetSubmissionAsync(id, none);
            Assert.False(opened.CanEdit);
            Assert.Equal(new[] { SubmissionStatus.Reviewed, SubmissionStatus.Rejected }.Order(), opened.Transitions.Select(t => t.To).Order());
            Assert.True(opened.Transitions.Single(t => t.To == SubmissionStatus.Rejected).RequiresComment);
            await w.TransitionAsync(id, SubmissionStatus.Reviewed, null, none);
        });
        await AsAsync("approver", w => w.TransitionAsync(id, SubmissionStatus.Approved, null, none));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AsAsync("admin", w => w.TransitionAsync(id, SubmissionStatus.Draft, comment: null, none)));
        await AsAsync("admin", w => w.TransitionAsync(id, SubmissionStatus.Draft, "Angka perlu dikoreksi", none));

        _accounts.Add("reviewer@jre.com");
        SubmissionWorkspace final = null!;
        await AsAsync("viewer", async w => final = await w.GetSubmissionAsync(id, none));
        Assert.Equal(SubmissionStatus.Draft, final.Submission.Status);
        Assert.False(final.CanEdit);
        Assert.Empty(final.Transitions);
        Assert.Equal(
            new[] { ("Draft", "Submitted"), ("Submitted", "Reviewed"), ("Reviewed", "Approved"), ("Approved", "Draft") },
            final.History.Select(h => (h.From, h.To)));
        Assert.Equal("Angka perlu dikoreksi", final.History[^1].Comment);
        Assert.Equal("Pelatihan K3 dasar", final.Submission.Cells["403-5-A"].Text);
    }

    [Fact]
    public async Task Closed_period_is_read_only()
    {
        var id = await StartAsync();
        using (var db = _db.CreateContext())
        {
            db.ReportingPeriods.Single().Status = PeriodStatus.Closed;
            db.SaveChanges();
        }

        await AsAsync("contrib", async w =>
        {
            var opened = await w.GetSubmissionAsync(id, CancellationToken.None);
            Assert.False(opened.CanEdit);
            Assert.Empty(opened.Transitions);
            Assert.Equal(NextAction.None, Assert.Single((await w.GetAsync(null, CancellationToken.None)).Rows).Next);
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => AsAsync("contrib", w =>
            w.SaveCellsAsync(id, [new CellChange("403-5-A", new AnswerValue("x", null))], CancellationToken.None)));
    }

    [Fact]
    public async Task Tenant_admin_manages_periods()
    {
        ReportingPeriods Periods(string userId, out AppDbContext ctx)
        {
            ctx = _db.CreateContext(new FakeTenant(A));
            return new ReportingPeriods(ctx, new FakeUser(userId), new FakeTenant(A), _db.Clock, NullLogger<ReportingPeriods>.Instance);
        }

        var periods = Periods("admin", out var ctx);
        await using (ctx)
        {
            var id = await periods.CreatePeriodAsync(2027, CancellationToken.None);
            await Assert.ThrowsAsync<InvalidOperationException>(() => periods.CreatePeriodAsync(2027, CancellationToken.None));
            await Assert.ThrowsAsync<InvalidOperationException>(() => periods.CreatePeriodAsync(1999, CancellationToken.None));
            await periods.SetStatusAsync(id, PeriodStatus.Closed, CancellationToken.None);

            var list = await periods.GetPeriodsAsync(CancellationToken.None);
            Assert.Equal(new[] { (2027, PeriodStatus.Closed), (2026, PeriodStatus.Open) }, list.Select(p => (p.Year, p.Status)));
        }

        var contrib = Periods("contrib", out var ctx2);
        await using (ctx2)
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => contrib.CreatePeriodAsync(2028, CancellationToken.None));

        using var check = _db.CreateContext();
        Assert.Equal(2, check.AuditEntries.Count(e => e.EntityType == nameof(ReportingPeriod)));
    }

    public void Dispose() => _db.Dispose();
}
