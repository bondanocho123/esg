using Bunit;
using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using GriForms.Application.Tenancy;
using GriForms.Domain.Enums;
using GriForms.Tests.Support;
using GriForms.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace GriForms.Tests.Forms;

// Halaman Form pelaporan: filter Site untuk Admin, filter Kategori untuk Contributor/Reviewer/Approver
public class ReportingPageTests : MudBunitContext
{
    private static readonly ReportingPeriodView Period = new(1, 2026, PeriodStatus.Open, 0);

    public ReportingPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddLogging();
        Services.AddSingleton<IReportingWorkspace>(new FakeWorkspace());
    }

    private static WorkspaceRow Row(int siteId, string siteCode, string topicCode, string category, NextAction next = NextAction.None) =>
        new(siteId, siteCode, $"Site {siteCode}", topicCode.GetHashCode(), topicCode, $"Topik {topicCode}", category,
            1, null, null, [RoleName.Admin, RoleName.Contributor], next);

    private IRenderedComponent<Reporting> RenderAs(params RoleName[] roles)
    {
        Services.AddSingleton<ITenantSession>(new FakeSession(roles));
        return Render<Reporting>();
    }

    private static IReadOnlyList<string> CardTitles(IRenderedComponent<Reporting> cut) =>
        cut.FindAll("section h2").Select(h => h.TextContent.Trim()).ToList();

    [Fact]
    public void Admin_filters_by_site_and_sites_follow_code_order()
    {
        var cut = RenderAs(RoleName.Admin);

        Assert.Empty(cut.FindAll("#category-filter"));
        Assert.Equal(new[] { "", "1", "2", "10" }, cut.FindAll("#site-filter option").Select(o => o.GetAttribute("value")));
        Assert.Equal(new[] { "Site 1 (1)", "Site 2 (2)", "Site 10 (10)" }, CardTitles(cut));

        cut.Find("#site-filter").Change("10");

        Assert.Equal(new[] { "Site 10 (10)" }, CardTitles(cut));
    }

    [Fact]
    public void Contributor_filters_by_category_and_action_counts_follow_the_filter()
    {
        var cut = RenderAs(RoleName.Contributor);

        Assert.Empty(cut.FindAll("#site-filter"));
        Assert.Equal(new[] { "", "Env", "Soc" }, cut.FindAll("#category-filter option").Select(o => o.GetAttribute("value")));
        Assert.Contains("Perlu diisi (2)", cut.Markup);

        cut.Find("#category-filter").Change("Env");

        Assert.Contains("Perlu diisi (1)", cut.Markup);
        Assert.DoesNotContain("Topik 403", cut.Markup);
        Assert.Contains("Topik 302", cut.Markup);
    }

    private sealed class FakeWorkspace : IReportingWorkspace
    {
        // Sudah urut seperti dari ReportingWorkspace: kode site lalu kode topik
        public Task<WorkspaceView> GetAsync(int? periodId, CancellationToken ct) => Task.FromResult(new WorkspaceView(
            [Period], Period,
            [
                Row(1, "1", "302", "Env", NextAction.Start),
                Row(1, "1", "403", "Soc", NextAction.Start),
                Row(2, "2", "403", "Soc"),
                Row(10, "10", "403", "Soc"),
            ]));

        public Task<int> OpenAsync(int siteId, int periodId, int topicId, CancellationToken ct) => throw new NotSupportedException();
        public Task<SubmissionWorkspace> GetSubmissionAsync(int submissionId, CancellationToken ct) => throw new NotSupportedException();
        public Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct) => throw new NotSupportedException();
        public Task TransitionAsync(int submissionId, SubmissionStatus to, string? comment, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeSession(RoleName[] roles) : ITenantSession
    {
        public SessionInfo Current { get; } = new SessionInfo("u1", new TenantOption(1, "ZRES", "Z Resource"), [], false,
            roles.Contains(RoleName.Admin), false) { Roles = roles.ToHashSet() };

        public Task<SessionInfo> StartAsync(string? userId, CancellationToken ct) => Task.FromResult(Current);
        public Task<bool> SwitchAsync(int tenantId, CancellationToken ct) => Task.FromResult(false);
        public Task<bool> IsPlatformAdminAsync(string userId, CancellationToken ct) => Task.FromResult(false);
    }
}
