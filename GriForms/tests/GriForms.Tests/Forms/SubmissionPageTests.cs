using Bunit;
using GriForms.Application;
using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using GriForms.Domain.Enums;
using GriForms.Tests.Support;
using GriForms.Web.Components.Forms;
using GriForms.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace GriForms.Tests.Forms;

// Halaman pengisian: simpan otomatis lewat IReportingWorkspace, validasi sebelum diajukan, dan mode baca
public class SubmissionPageTests : MudBunitContext
{
    private static readonly FormDefinition Definition = new JsonFormDefinitionReader().Read(SampleDefinitions.Topic403);

    private readonly FakeWorkspace _workspace = new();

    public SubmissionPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddApplication();
        Services.AddFormFields();
        Services.AddLogging();
        Services.AddSingleton<IReportingWorkspace>(_workspace);
        Render<MudBlazor.MudPopoverProvider>();
    }

    private IRenderedComponent<SubmissionPage> RenderPage() =>
        Render<SubmissionPage>(p => p.Add(x => x.SubmissionId, 7));

    [Fact]
    public void Contributor_edits_are_saved_automatically()
    {
        var cut = RenderPage();
        Assert.Contains("Perubahan tersimpan otomatis", cut.Markup);

        cut.Find("textarea").Change("Pelatihan K3 dasar untuk semua pekerja");

        cut.WaitForAssertion(() =>
        {
            var saved = Assert.Single(_workspace.Saved);
            Assert.Equal("403-5-A", saved.CellKey);
            Assert.Equal("Pelatihan K3 dasar untuk semua pekerja", saved.Value!.Text);
        }, TimeSpan.FromSeconds(5));
        cut.WaitForAssertion(() => Assert.Contains("Tersimpan otomatis", cut.Markup));
    }

    [Fact]
    public void Submit_is_blocked_until_required_answers_are_filled()
    {
        var cut = RenderPage();

        cut.FindAll("button").Single(b => b.TextContent.Contains("Ajukan untuk ditinjau")).Click();

        Assert.Empty(_workspace.Transitions);
        Assert.Contains("perlu diperbaiki sebelum diajukan", cut.Markup);

        cut.Find("textarea").Change("Pelatihan K3 dasar");
        cut.FindAll("button").Single(b => b.TextContent.Contains("Ajukan untuk ditinjau")).Click();

        cut.WaitForAssertion(() => Assert.Equal([SubmissionStatus.Submitted], _workspace.Transitions));
        Assert.Contains(_workspace.Saved, c => c.CellKey == "403-5-A");   // isian dikirim lebih dulu sebelum diajukan
    }

    [Fact]
    public void Reviewer_sees_read_only_form_and_must_comment_to_reject()
    {
        _workspace.Roles = [RoleName.Reviewer];
        _workspace.Status = SubmissionStatus.Submitted;
        _workspace.CanEdit = false;
        _workspace.Options = [new(SubmissionStatus.Reviewed, RoleName.Reviewer, false), new(SubmissionStatus.Rejected, RoleName.Reviewer, true)];

        var cut = RenderPage();

        Assert.Contains("Hanya baca", cut.Markup);
        Assert.All(cut.FindAll("textarea:not(#comment)"), t => Assert.True(t.HasAttribute("disabled")));

        cut.FindAll("button").Single(b => b.TextContent.Contains("Tolak")).Click();
        Assert.Empty(_workspace.Transitions);
        Assert.Contains("Isi komentar", cut.Markup);

        cut.Find("#comment").Change("Data bulan Maret belum lengkap");
        cut.FindAll("button").Single(b => b.TextContent.Contains("Tolak")).Click();
        cut.WaitForAssertion(() => Assert.Equal([SubmissionStatus.Rejected], _workspace.Transitions));
        Assert.Equal("Data bulan Maret belum lengkap", _workspace.LastComment);
    }

    private sealed class FakeWorkspace : IReportingWorkspace
    {
        public IReadOnlyList<RoleName> Roles { get; set; } = [RoleName.Contributor];
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
        public bool CanEdit { get; set; } = true;
        public IReadOnlyList<TransitionOption> Options { get; set; } =
            [new(SubmissionStatus.Submitted, RoleName.Contributor, false)];

        public List<CellChange> Saved { get; } = [];
        public List<SubmissionStatus> Transitions { get; } = [];
        public string? LastComment { get; private set; }

        public Task<SubmissionWorkspace> GetSubmissionAsync(int submissionId, CancellationToken ct) =>
            Task.FromResult(new SubmissionWorkspace(
                new SubmissionView(submissionId, 1, "Head Office", 2026, 1, Status, PeriodStatus.Open, Definition,
                    new Dictionary<string, AnswerValue>()),
                "403", "Keselamatan dan Kesehatan Kerja", 1, Roles, CanEdit, Options, []));

        public Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct)
        {
            lock (Saved) Saved.AddRange(changes);
            return Task.CompletedTask;
        }

        public Task TransitionAsync(int submissionId, SubmissionStatus to, string? comment, CancellationToken ct)
        {
            Transitions.Add(to);
            LastComment = comment;
            Status = to;
            CanEdit = false;
            Options = [];
            return Task.CompletedTask;
        }

        public Task<WorkspaceView> GetAsync(int? periodId, CancellationToken ct) => throw new NotSupportedException();
        public Task<int> OpenAsync(int siteId, int periodId, int topicId, CancellationToken ct) => throw new NotSupportedException();
    }
}
