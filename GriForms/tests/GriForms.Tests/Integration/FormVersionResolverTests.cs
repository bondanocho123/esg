using GriForms.Domain.Entities;
using GriForms.Infrastructure.Forms;
using GriForms.Tests.Support;

namespace GriForms.Tests.Integration;

public class FormVersionResolverTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly int _topicId;

    public FormVersionResolverTests()
    {
        using var db = _db.CreateContext();
        var topic = new Topic { Code = "403", Category = "Soc", Name = "K3" };
        db.Add(topic);
        db.SaveChanges();
        _topicId = topic.Id;

        db.AddRange(
            Version(1, from: 2025, to: 2025, published: true),
            Version(2, from: 2026, to: null, published: true),
            Version(3, from: 2026, to: null, published: false),   // draft, tidak boleh terpilih
            Version(4, from: 2028, to: null, published: true));
        db.SaveChanges();
    }

    private FormVersion Version(int no, int from, int? to, bool published) => new()
    {
        TopicId = _topicId, VersionNo = no, StandardRef = "GRI 403",
        EffectiveFromYear = from, EffectiveToYear = to, IsPublished = published,
        DefinitionJson = SampleDefinitions.Topic403,
    };

    [Theory]
    [InlineData(2025, 1)]
    [InlineData(2026, 2)]
    [InlineData(2027, 2)]
    [InlineData(2028, 4)]
    public async Task Picks_highest_published_version_effective_for_year(int year, int expectedVersionNo)
    {
        using var db = _db.CreateContext();

        var version = await new FormVersionResolver(db).ResolveAsync(_topicId, year, CancellationToken.None);

        Assert.Equal(expectedVersionNo, version.VersionNo);
    }

    [Fact]
    public async Task Throws_when_no_published_version_exists()
    {
        using var db = _db.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new FormVersionResolver(db).ResolveAsync(_topicId, 2024, CancellationToken.None));
    }

    public void Dispose() => _db.Dispose();
}
