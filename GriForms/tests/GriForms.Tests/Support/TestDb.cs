using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Tests.Support;

// Satu koneksi SQLite in-memory per tes; koneksi tetap terbuka selama tes berjalan
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();   // seed 6 site ikut dibuat lewat HasData
    }

    public FakeUser User { get; } = new("user-1");
    public FixedClock Clock { get; } = new(new DateTime(2026, 10, 22, 0, 0, 0, DateTimeKind.Utc));

    public AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(_connection)
        .AddInterceptors(new RowVersionInterceptor(), new AuditInterceptor(User, Clock))
        .Options);

    // Data minimal: satu topik, satu periode, satu versi form terbit
    public (int TopicId, int PeriodId, int FormVersionId) SeedTopic(int year = 2026, PeriodStatus status = PeriodStatus.Open)
    {
        using var db = CreateContext();
        var topic = new Topic { Code = "403", Category = "Soc", Name = "Keselamatan dan Kesehatan Kerja" };
        var period = new ReportingPeriod { Year = year, Status = status };
        var version = new FormVersion
        {
            Topic = topic, VersionNo = 1, StandardRef = "GRI 403", EffectiveFromYear = 2025,
            DefinitionJson = SampleDefinitions.Topic403, IsPublished = true,
        };
        db.AddRange(topic, period, version);
        db.SaveChanges();
        return (topic.Id, period.Id, version.Id);
    }

    public int SeedSubmission(int siteId, (int TopicId, int PeriodId, int FormVersionId) seed,
        SubmissionStatus status = SubmissionStatus.Draft, params Answer[] answers)
    {
        using var db = CreateContext();
        var submission = new Submission
        {
            SiteId = siteId, PeriodId = seed.PeriodId, TopicId = seed.TopicId,
            FormVersionId = seed.FormVersionId, Status = status, Answers = answers.ToList(),
        };
        db.Add(submission);
        db.SaveChanges();
        return submission.Id;
    }

    public void Dispose() => _connection.Dispose();
}
