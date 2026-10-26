using System.Text.Json.Nodes;
using Bunit;
using GriForms.Application;
using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Domain.Enums;
using GriForms.Tests.Support;
using GriForms.Web.Components.Forms;
using GriForms.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace GriForms.Tests.Forms;

// Editor Definisi form: ringkasan Kesiapan telemetry dan tombol Terbitkan mengikuti klasifikasi telemetry
public class FormDefinitionEditorTests : MudBunitContext
{
    private readonly FakeCatalog _catalog = new();

    public FormDefinitionEditorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddApplication();
        Services.AddFormFields();
        Services.AddLogging();
        Services.AddSingleton<IFormCatalog>(_catalog);
        Render<MudBlazor.MudPopoverProvider>();
    }

    private IRenderedComponent<FormDefinitionEditor> RenderWith(string json)
    {
        _catalog.Json = json;
        return Render<FormDefinitionEditor>(p => p.Add(x => x.VersionId, 1));
    }

    private static string WithMeasuredHours()
    {
        var root = JsonNode.Parse(SampleDefinitions.Topic403)!.AsObject();
        root["disclosures"]![1]!["questions"]![0]!["telemetry"] = new JsonObject
        {
            ["feasibility"] = "measured", ["metric"] = "hours_worked", ["unit"] = "jam", ["aggregation"] = "sum",
        };
        return root.ToJsonString();
    }

    [Fact]
    public void Shows_telemetry_readiness_counts_and_measured_items()
    {
        var cut = RenderWith(WithMeasuredHours());

        cut.WaitForAssertion(() => Assert.Contains("Kesiapan telemetry", cut.Markup));
        Assert.Contains("Manual: 2", cut.Markup);
        Assert.Contains("Measured: 1", cut.Markup);
        Assert.Contains("hours_worked", cut.Markup);
        Assert.DoesNotContain("Belum diklasifikasikan", cut.Markup);
        Assert.False(cut.FindAll("button").Single(b => b.TextContent.Contains("Terbitkan")).HasAttribute("disabled"));
    }

    [Fact]
    public void Unclassified_numeric_question_blocks_publishing()
    {
        var root = JsonNode.Parse(SampleDefinitions.Topic403)!.AsObject();
        root["disclosures"]![1]!["questions"]![0]!.AsObject().Remove("telemetry");

        var cut = RenderWith(root.ToJsonString());

        cut.WaitForAssertion(() => Assert.Contains("Belum diklasifikasikan: 1", cut.Markup));
        Assert.Contains("disclosures[1].questions[0].telemetry", cut.Markup);
        Assert.True(cut.FindAll("button").Single(b => b.TextContent.Contains("Terbitkan")).HasAttribute("disabled"));
    }

    private sealed class FakeCatalog : IFormCatalog
    {
        public string Json { get; set; } = "";

        public Task<FormVersionDetail> GetVersionAsync(int versionId, CancellationToken ct) => Task.FromResult(new FormVersionDetail(
            new FormVersionSummary(versionId, 1, "403", "Keselamatan dan Kesehatan Kerja", 1, "GRI 403", 2025, null,
                FormVersionStatus.Draft, 0, 0),
            Json, new DateTime(2026, 10, 25, 0, 0, 0, DateTimeKind.Utc), "platform@griforms.id", null, null, null, null));

        public Task<IReadOnlyList<TopicView>> GetTopicsAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<int> CreateTopicAsync(TopicInput input, CancellationToken ct) => throw new NotSupportedException();
        public Task UpdateTopicAsync(int topicId, TopicInput input, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<FormVersionSummary>> GetVersionsAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<int> CreateDraftAsync(int topicId, int? copyFromVersionId, CancellationToken ct) => throw new NotSupportedException();
        public Task<DefinitionCheck> SaveDraftAsync(int versionId, string definitionJson, CancellationToken ct) => throw new NotSupportedException();
        public Task PublishAsync(int versionId, CancellationToken ct) => throw new NotSupportedException();
        public Task RetireAsync(int versionId, CancellationToken ct) => throw new NotSupportedException();
        public Task DeleteDraftAsync(int versionId, CancellationToken ct) => throw new NotSupportedException();
    }
}
