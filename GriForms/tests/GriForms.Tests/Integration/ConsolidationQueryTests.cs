using GriForms.Application;
using GriForms.Application.Consolidation;
using GriForms.Application.Forms;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Consolidation;
using GriForms.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Tests.Integration;

public class ConsolidationQueryTests : IDisposable
{
    private readonly TestDb _db = new();

    private static Answer Monthly(string month, decimal value) => new()
    {
        DisclosureNo = "403-5", QuestionKey = "403-5-B/k3-dasar", PeriodKey = $"2026-{month}",
        NumberValue = value, Unit = "orang",
    };

    private static Answer Narrative(string text) => new()
    {
        DisclosureNo = "403-5", QuestionKey = "403-5-A", StringValue = text,
    };

    [Fact]
    public async Task Only_approved_answers_are_included_and_missing_sites_are_reported()
    {
        var seed = _db.SeedTopic();
        _db.SeedSubmission(1, seed, SubmissionStatus.Approved, Narrative("Pelatihan rutin"), Monthly("01", 10), Monthly("02", 20.5m));
        _db.SeedSubmission(2, seed, SubmissionStatus.Reviewed, Narrative("Belum disetujui"), Monthly("01", 999));
        using (var db = _db.CreateContext())
        {
            db.Sites.Single(s => s.Id == 6).IsActive = false;   // site nonaktif tidak dihitung kurang
            db.SaveChanges();
        }

        using var ctx = _db.CreateContext();
        var result = await new ConsolidationQuery(ctx).BuildAsync(seed.TopicId, 2026, CancellationToken.None);

        Assert.All(result.Rows, r => Assert.Equal(1, r.SiteId));
        Assert.Equal(3, result.Rows.Count);
        Assert.Equal(new[] { 1 }, result.Coverage.ApprovedSiteIds);
        Assert.Equal(new[] { 2, 3, 4, 5 }, result.Coverage.MissingSiteIds);
    }

    [Fact]
    public async Task Cards_total_is_computed_in_memory_from_filled_cells()
    {
        var seed = _db.SeedTopic();
        _db.SeedSubmission(1, seed, SubmissionStatus.Approved, Narrative("Pelatihan rutin"), Monthly("01", 10), Monthly("02", 20.5m));

        using var ctx = _db.CreateContext();
        var result = await new ConsolidationQuery(ctx).BuildAsync(seed.TopicId, 2026, CancellationToken.None);
        var builder = new ServiceCollection().AddApplication().BuildServiceProvider()
            .GetRequiredService<IConsolidationCardBuilder>();

        var cards = builder.Build(result, new JsonFormDefinitionReader().Read(SampleDefinitions.Topic403));

        var narrative = Assert.Single(cards.OfType<NarrativeCard>());
        Assert.Equal("Pelatihan rutin", Assert.Single(narrative.Rows).Text);

        var row = Assert.Single(Assert.Single(cards.OfType<QuantitativeCard>()).Rows);
        Assert.Equal("Pelatihan K3 Dasar", row.RowLabel);
        Assert.Equal(30.5m, row.Total);
        Assert.Equal(10m, row.Months[0]);
        Assert.Null(row.Months[2]);
    }

    [Fact]
    public async Task Cards_fail_loudly_when_an_answered_question_has_no_card_factory()
    {
        var seed = _db.SeedTopic();
        _db.SeedSubmission(1, seed, SubmissionStatus.Approved, new Answer
        {
            DisclosureNo = "403-9", QuestionKey = "403-9-A", NumberValue = 100, Unit = "jam",
        });

        using var ctx = _db.CreateContext();
        var result = await new ConsolidationQuery(ctx).BuildAsync(seed.TopicId, 2026, CancellationToken.None);
        var builder = new ServiceCollection().AddApplication().BuildServiceProvider()
            .GetRequiredService<IConsolidationCardBuilder>();

        var ex = Assert.Throws<NotSupportedException>(() =>
            builder.Build(result, new JsonFormDefinitionReader().Read(SampleDefinitions.Topic403)));
        Assert.Contains("403-9-A", ex.Message);
    }

    public void Dispose() => _db.Dispose();
}
