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

    // Kolom Topik di tabel kartu site (tree juga memuat nama topik, jadi Markup tidak cukup)
    private static IReadOnlyList<string> TableTopics(IRenderedComponent<Reporting> cut) =>
        cut.FindAll("table tbody tr td:first-child").Select(td => td.TextContent.Trim()).ToList();

    private static IReadOnlyList<string> CardTitles(IRenderedComponent<Reporting> cut) =>
        cut.FindAll("section h2").Select(h => h.TextContent.Trim()).ToList();

    [Fact]
    public void Admin_filters_by_site_and_sites_follow_code_order()
    {
        var cut = RenderAs(RoleName.Admin);

        Assert.Empty(cut.FindAll("#tree-all"));   // tree hanya untuk Contributor/Reviewer/Approver
        Assert.Equal(new[] { "", "1", "2", "10" }, cut.FindAll("#site-filter option").Select(o => o.GetAttribute("value")));
        Assert.Equal(new[] { "Site 1 (1)", "Site 2 (2)", "Site 10 (10)" }, CardTitles(cut));

        cut.Find("#site-filter").Change("10");

        Assert.Equal(new[] { "Site 10 (10)" }, CardTitles(cut));
    }

    [Fact]
    public void Contributor_tree_lists_categories_with_topic_codes_in_code_order()
    {
        var cut = RenderAs(RoleName.Contributor);

        Assert.Empty(cut.FindAll("#site-filter"));
        Assert.Equal(new[] { "Env", "Soc" }, cut.FindAll("[data-category]").Select(b => b.GetAttribute("data-category")));
        Assert.Equal(new[] { "302", "403", "1001" }, cut.FindAll("[data-topic]").Select(b => b.GetAttribute("data-topic")));
        Assert.Equal("true", cut.Find("#tree-all").GetAttribute("aria-current"));
    }

    [Fact]
    public void Selecting_category_or_code_filters_forms_and_action_counts()
    {
        var cut = RenderAs(RoleName.Contributor);
        Assert.Contains("Perlu diisi (3)", cut.Markup);

        cut.Find("[data-category='Env']").Click();

        Assert.Contains("Perlu diisi (1)", cut.Markup);
        Assert.Equal(new[] { "302 Topik 302" }, TableTopics(cut));

        cut.Find("[data-topic='1001']").Click();

        Assert.Equal("true", cut.Find("[data-topic='1001']").GetAttribute("aria-current"));
        Assert.Equal(new[] { "1001 Topik 1001" }, TableTopics(cut));
        Assert.Contains("Perlu diisi (1)", cut.Markup);

        cut.Find("#tree-all").Click();
        Assert.Contains("Perlu diisi (3)", cut.Markup);
    }

    [Fact]
    public void Category_menu_collapses_and_expands_its_codes()
    {
        var cut = RenderAs(RoleName.Reviewer);
        var toggle = cut.Find("button[aria-label='Tutup Soc']");

        toggle.Click();

        Assert.Equal(new[] { "302" }, cut.FindAll("[data-topic]").Select(b => b.GetAttribute("data-topic")));
        cut.Find("button[aria-label='Buka Soc']").Click();
        Assert.Equal(3, cut.FindAll("[data-topic]").Count);
    }

    private sealed class FakeWorkspace : IReportingWorkspace
    {
        // Sudah urut seperti dari ReportingWorkspace: kode site lalu kode topik
        public Task<WorkspaceView> GetAsync(int? periodId, CancellationToken ct) => Task.FromResult(new WorkspaceView(
            [Period], Period,
            [
                Row(1, "1", "302", "Env", NextAction.Start),
                Row(1, "1", "403", "Soc", NextAction.Start),
                Row(1, "1", "1001", "Soc", NextAction.Start),
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
