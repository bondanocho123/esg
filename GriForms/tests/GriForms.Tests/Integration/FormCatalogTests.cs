using System.Text.Json.Nodes;
using GriForms.Application.Calculations;
using GriForms.Application.Forms;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Forms;
using GriForms.Infrastructure.Persistence;
using GriForms.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

// Halaman Definisi form: siklus Draft -> Published -> Retired, khusus PlatformAdmin tanpa tenant aktif
public class FormCatalogTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly FakeUserAccounts _accounts = new();

    public FormCatalogTests()
    {
        using var db = _db.CreateContext();
        db.Add(new PlatformAdmin { UserId = "u-platform@griforms.id", GrantedAt = _db.Clock.UtcNow });
        db.SaveChanges();
        _accounts.Add("platform@griforms.id");
    }

    private FormCatalog Catalog(out AppDbContext ctx, string userId = "u-platform@griforms.id")
    {
        ctx = _db.CreateContext(new FakeTenant(null));
        return Create(ctx, userId, _accounts, _db.Clock);
    }

    internal static FormCatalog Create(AppDbContext ctx, string userId, FakeUserAccounts accounts, FixedClock clock) => new(
        ctx, new FakeUser(userId), accounts, clock,
        new FormDefinitionValidator(new JsonFormDefinitionReader(), new CalcRegistry([new RowTotalCalculation()])),
        NullLogger<FormCatalog>.Instance);

    private static string WithVersionNo(string json, int versionNo)
    {
        var root = JsonNode.Parse(json)!.AsObject();
        root["versionNo"] = versionNo;
        return root.ToJsonString();
    }

    private async Task<(FormCatalog Catalog, AppDbContext Ctx, int TopicId)> TopicAsync()
    {
        var catalog = Catalog(out var ctx);
        var topicId = await catalog.CreateTopicAsync(new TopicInput("403", "Soc", "Keselamatan dan Kesehatan Kerja"), CancellationToken.None);
        return (catalog, ctx, topicId);
    }

    [Fact]
    public async Task Topic_codes_are_unique_and_locked_once_versions_exist()
    {
        var (catalog, ctx, topicId) = await TopicAsync();
        await using var _ = ctx;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            catalog.CreateTopicAsync(new TopicInput("403", "Soc", "Ganda"), CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            catalog.CreateTopicAsync(new TopicInput("", "Soc", "Tanpa kode"), CancellationToken.None));

        await catalog.UpdateTopicAsync(topicId, new TopicInput("403", "GRI 14", "K3"), CancellationToken.None);
        await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            catalog.UpdateTopicAsync(topicId, new TopicInput("404", "Soc", "K3"), CancellationToken.None));

        var topic = Assert.Single(await catalog.GetTopicsAsync(CancellationToken.None));
        Assert.Equal(("403", "GRI 14", "K3", 1), (topic.Code, topic.Category, topic.Name, topic.VersionCount));
    }

    [Fact]
    public async Task Blank_draft_is_valid_and_numbered_from_one()
    {
        var (catalog, ctx, topicId) = await TopicAsync();
        await using var _ = ctx;

        var id = await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);

        var detail = await catalog.GetVersionAsync(id, CancellationToken.None);
        Assert.Equal((1, FormVersionStatus.Draft, 2026), (detail.Summary.VersionNo, detail.Summary.Status, detail.Summary.EffectiveFromYear));
        Assert.Equal("platform@griforms.id", detail.CreatedBy);
        Assert.Equal(_db.Clock.UtcNow, detail.CreatedAt);
        var check = new FormDefinitionValidator(new JsonFormDefinitionReader(), new CalcRegistry([]))
            .Check(detail.DefinitionJson, "403", 1);
        Assert.True(check.IsValid, string.Join("; ", check.Issues.Select(i => i.Message)));
    }

    [Fact]
    public async Task Draft_save_syncs_header_and_reports_issues_without_blocking()
    {
        var (catalog, ctx, topicId) = await TopicAsync();
        await using var _ = ctx;
        var id = await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);

        var saved = await catalog.SaveDraftAsync(id, SampleDefinitions.Topic403, CancellationToken.None);
        Assert.True(saved.IsValid);

        var withIssue = JsonNode.Parse(SampleDefinitions.Topic403)!.AsObject();
        withIssue["standardRef"] = "GRI 403: 2018";
        withIssue["effectiveToYear"] = 2027;
        withIssue["disclosures"]![1]!["questions"]![0]!["key"] = "403-5-A";   // key ganda
        var check = await catalog.SaveDraftAsync(id, withIssue.ToJsonString(), CancellationToken.None);
        Assert.False(check.IsValid);

        var summary = (await catalog.GetVersionAsync(id, CancellationToken.None)).Summary;
        Assert.Equal(("GRI 403: 2018", 2025, (int?)2027), (summary.StandardRef, summary.EffectiveFromYear, summary.EffectiveToYear));

        // JSON yang tidak bisa dibaca tidak disimpan sama sekali
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.SaveDraftAsync(id, "{ rusak", CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.PublishAsync(id, CancellationToken.None));
        Assert.Equal(FormVersionStatus.Draft, (await catalog.GetVersionAsync(id, CancellationToken.None)).Summary.Status);
    }

    [Fact]
    public async Task Published_version_is_locked_and_changes_go_through_a_copied_draft()
    {
        var (catalog, ctx, topicId) = await TopicAsync();
        await using var _ = ctx;
        var v1 = await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);
        await catalog.SaveDraftAsync(v1, SampleDefinitions.Topic403, CancellationToken.None);

        await catalog.PublishAsync(v1, CancellationToken.None);

        var published = await catalog.GetVersionAsync(v1, CancellationToken.None);
        Assert.Equal((FormVersionStatus.Published, _db.Clock.UtcNow, "platform@griforms.id"),
            (published.Summary.Status, published.PublishedAt, published.PublishedBy));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            catalog.SaveDraftAsync(v1, SampleDefinitions.Topic403, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.PublishAsync(v1, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.DeleteDraftAsync(v1, CancellationToken.None));

        var v2 = await catalog.CreateDraftAsync(topicId, v1, CancellationToken.None);

        var copy = await catalog.GetVersionAsync(v2, CancellationToken.None);
        Assert.Equal((2, FormVersionStatus.Draft), (copy.Summary.VersionNo, copy.Summary.Status));
        Assert.Equal(WithVersionNo(SampleDefinitions.Topic403, 2), JsonNode.Parse(copy.DefinitionJson)!.ToJsonString());
        Assert.Equal(SampleDefinitions.Topic403, published.DefinitionJson);
    }

    [Fact]
    public async Task Retired_version_is_no_longer_resolved_for_new_submissions()
    {
        var (catalog, ctx, topicId) = await TopicAsync();
        await using var _ = ctx;
        var v1 = await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);
        await catalog.SaveDraftAsync(v1, SampleDefinitions.Topic403, CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.RetireAsync(v1, CancellationToken.None));
        await catalog.PublishAsync(v1, CancellationToken.None);
        Assert.Equal(1, (await new FormVersionResolver(ctx).ResolveAsync(topicId, 2026, CancellationToken.None)).VersionNo);

        await catalog.RetireAsync(v1, CancellationToken.None);

        var retired = await catalog.GetVersionAsync(v1, CancellationToken.None);
        Assert.Equal((FormVersionStatus.Retired, "platform@griforms.id"), (retired.Summary.Status, retired.RetiredBy));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FormVersionResolver(ctx).ResolveAsync(topicId, 2026, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.RetireAsync(v1, CancellationToken.None));
    }

    [Fact]
    public async Task Only_unused_drafts_can_be_deleted()
    {
        var (catalog, ctx, topicId) = await TopicAsync();
        await using var _ = ctx;
        var unused = await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);
        var used = await catalog.CreateDraftAsync(topicId, null, CancellationToken.None);

        // Submission tenant lain tetap menghalangi, padahal PlatformAdmin tidak punya tenant aktif.
        // Draft biasanya tidak dipakai submission; kasus ini menjaga FK cascade Submission -> FormVersion.
        int periodId;
        using (var tenantDb = _db.CreateContext())
        {
            var period = new ReportingPeriod { Year = 2026, Status = PeriodStatus.Open };
            tenantDb.Add(period);
            tenantDb.SaveChanges();
            periodId = period.Id;
        }
        _db.SeedSubmission(siteId: 1, (topicId, periodId, used));

        await catalog.DeleteDraftAsync(unused, CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.DeleteDraftAsync(used, CancellationToken.None));

        var versions = await catalog.GetVersionsAsync(CancellationToken.None);
        var remaining = Assert.Single(versions);
        Assert.Equal((used, 1), (remaining.Id, remaining.SubmissionCount));
        using var check = _db.CreateContext();
        Assert.Single(check.Submissions);
    }

    [Fact]
    public async Task Non_platform_admin_is_rejected_everywhere()
    {
        int topicId;
        int versionId;
        using (var db = _db.CreateContext())
        {
            var topic = new Topic { Code = "403", Category = "Soc", Name = "K3" };
            var version = new FormVersion { Topic = topic, VersionNo = 1, DefinitionJson = SampleDefinitions.Topic403 };
            db.AddRange(topic, version);
            db.SaveChanges();
            (topicId, versionId) = (topic.Id, version.Id);
        }
        var catalog = Catalog(out var ctx, userId: "admin-a");
        await using var _ = ctx;
        var none = CancellationToken.None;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.GetTopicsAsync(none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.CreateTopicAsync(new TopicInput("1", "Env", "X"), none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.UpdateTopicAsync(topicId, new TopicInput("403", "Env", "X"), none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.GetVersionsAsync(none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.GetVersionAsync(versionId, none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.CreateDraftAsync(topicId, null, none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.SaveDraftAsync(versionId, SampleDefinitions.Topic403, none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.PublishAsync(versionId, none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.RetireAsync(versionId, none));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => catalog.DeleteDraftAsync(versionId, none));

        using var check = _db.CreateContext();
        Assert.False(check.FormVersions.Single().IsPublished);
    }

    public void Dispose() => _db.Dispose();
}
