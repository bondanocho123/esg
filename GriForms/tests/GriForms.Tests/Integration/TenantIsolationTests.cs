using ClosedXML.Excel;
using GriForms.Application;
using GriForms.Application.Forms;
using GriForms.Application.Telemetry;
using GriForms.Application.Validation;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Consolidation;
using GriForms.Infrastructure.Excel;
using GriForms.Infrastructure.Submissions;
using GriForms.Infrastructure.Telemetry;
using GriForms.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

// Dua tenant: A (Z Resource dari seed) dan B. Keduanya punya site ber-Code "1", periode 2026,
// dan device telemetry ber-Code sama.
public class TenantIsolationTests : IDisposable
{
    private const int A = Tenant.LegacyId;
    private const int SiteA = 1;   // seed: Head Office, Code "1"
    private const string DeviceCode = "esp32-01";

    private static readonly TelemetryAccess Admin = new(true, null);

    private readonly TestDb _db = new();
    private readonly int _b;
    private readonly int _siteB;
    private readonly (int TopicId, int PeriodId, int FormVersionId) _seedA;
    private readonly (int TopicId, int PeriodId, int FormVersionId) _seedB;

    public TenantIsolationTests()
    {
        _seedA = _db.SeedTopic();

        using (var db = _db.CreateContext())
        {
            var tenant = new Tenant { Code = "ACME", Name = "Acme Mining" };
            db.Add(tenant);
            db.SaveChanges();
            _b = tenant.Id;
        }

        using (var db = _db.CreateContext(_b))
        {
            var site = new Site { Code = "1", Name = "Acme Head Office" };
            var period = new ReportingPeriod { Year = 2026 };
            db.AddRange(site, period);
            db.SaveChanges();
            _siteB = site.Id;
            _seedB = (_seedA.TopicId, period.Id, _seedA.FormVersionId);
        }
    }

    private static Answer Narrative(string text) => new() { DisclosureNo = "403-5", QuestionKey = "403-5-A", StringValue = text };

    private int SeedSubmission(int tenantId, int siteId, (int TopicId, int PeriodId, int FormVersionId) seed,
        SubmissionStatus status, params Answer[] answers)
    {
        using var db = _db.CreateContext(tenantId);
        var submission = new Submission
        {
            SiteId = siteId, PeriodId = seed.PeriodId, TopicId = seed.TopicId,
            FormVersionId = seed.FormVersionId, Status = status, Answers = answers.ToList(),
        };
        db.Add(submission);
        db.SaveChanges();
        return submission.Id;
    }

    private int SeedDevice(int tenantId, int siteId, string key)
    {
        using var db = _db.CreateContext(tenantId);
        var device = new TelemetryDevice { Code = DeviceCode, Name = DeviceCode, SiteId = siteId, ApiKeyHash = DeviceApiKey.Hash(key) };
        db.Add(device);
        db.SaveChanges();
        return device.Id;
    }

    [Fact]
    public void Empty_active_tenant_returns_no_rows()
    {
        SeedSubmission(A, SiteA, _seedA, SubmissionStatus.Draft, Narrative("A"));
        SeedDevice(A, SiteA, "kunci-a");

        using var db = _db.CreateContext(new FakeTenant(null));

        Assert.Empty(db.Sites);
        Assert.Empty(db.ReportingPeriods);
        Assert.Empty(db.Submissions);
        Assert.Empty(db.Answers);
        Assert.Empty(db.TelemetryDevices);
        Assert.Empty(db.AuditEntries);
    }

    [Fact]
    public void Each_tenant_sees_only_its_own_sites_and_periods()
    {
        using var a = _db.CreateContext();
        using var b = _db.CreateContext(_b);

        Assert.Equal(6, a.Sites.Count());
        Assert.Equal(new[] { "1" }, b.Sites.Select(s => s.Code).ToArray());
        Assert.Equal(2026, Assert.Single(a.ReportingPeriods).Year);
        Assert.Equal(2026, Assert.Single(b.ReportingPeriods).Year);   // tahun sama di dua tenant
    }

    [Fact]
    public void New_root_entity_takes_active_tenant()
    {
        using (var db = _db.CreateContext(_b))
        {
            db.Add(new ReportingPeriod { Year = 2027 });
            db.SaveChanges();
        }

        using var check = _db.CreateContext(_b);
        Assert.Equal(_b, check.ReportingPeriods.Single(p => p.Year == 2027).TenantId);
    }

    [Fact]
    public void Writes_to_other_tenant_or_without_tenant_are_rejected()
    {
        using (var db = _db.CreateContext(_b))
        {
            db.Sites.IgnoreQueryFilters().Single(s => s.Id == SiteA).Name = "Diubah dari B";
            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }
        using (var db = _db.CreateContext(_b))
        {
            db.Add(new Site { TenantId = A, Code = "99", Name = "Disusupkan" });
            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }
        using (var db = _db.CreateContext(new FakeTenant(null)))
        {
            db.Add(new Site { Code = "98", Name = "Tanpa tenant" });
            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }

        using var check = _db.CreateContext();
        Assert.Equal("Head Office", check.Sites.Single(s => s.Id == SiteA).Name);
        Assert.DoesNotContain(check.Sites.IgnoreQueryFilters().ToList(), s => s.Code == "99" || s.Code == "98");
    }

    [Fact]
    public void Submission_and_device_must_belong_to_tenant_of_their_site_and_period()
    {
        using (var db = _db.CreateContext(_b))
        {
            db.Add(new Submission { SiteId = SiteA, PeriodId = _seedB.PeriodId, TopicId = _seedB.TopicId, FormVersionId = _seedB.FormVersionId });
            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }
        using (var db = _db.CreateContext())
        {
            db.Add(new Submission { SiteId = SiteA, PeriodId = _seedB.PeriodId, TopicId = _seedB.TopicId, FormVersionId = _seedB.FormVersionId });
            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }
        using (var db = _db.CreateContext())
        {
            db.Add(new TelemetryDevice { Code = "x-01", Name = "x", SiteId = _siteB, ApiKeyHash = DeviceApiKey.Hash("x") });
            Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        }

        using var a = _db.CreateContext();
        using var b = _db.CreateContext(_b);
        Assert.Empty(a.Submissions);
        Assert.Empty(b.Submissions);
        Assert.Empty(a.TelemetryDevices);
    }

    [Fact]
    public async Task Submission_of_other_tenant_is_not_found_and_stays_unchanged()
    {
        var idB = SeedSubmission(_b, _siteB, _seedB, SubmissionStatus.Draft, Narrative("Milik B"));
        var reader = new JsonFormDefinitionReader();
        var validation = new ServiceCollection().AddApplication().BuildServiceProvider().GetRequiredService<IValidationEngine>();

        await using (var ctx = _db.CreateContext())   // tenant A
        {
            Assert.Null(await new SubmissionQueries(ctx, reader).GetAsync(idB, CancellationToken.None));

            var write = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new AnswerWriter(ctx, ctx, reader, _db.User, _db.Clock).SaveCellsAsync(
                    idB, [new CellChange("403-5-A", new AnswerValue("Disusupkan", null))], CancellationToken.None));
            Assert.Contains("tidak ditemukan", write.Message);

            var transition = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new SubmissionWorkflow(ctx, ctx, reader, validation, _db.User, _db.Clock).TransitionAsync(
                    idB, SubmissionStatus.Submitted, RoleName.Contributor, null, CancellationToken.None));
            Assert.Contains("tidak ditemukan", transition.Message);
        }

        using var check = _db.CreateContext(_b);
        var submission = check.Submissions.Include(s => s.Answers).Single(s => s.Id == idB);
        Assert.Equal(SubmissionStatus.Draft, submission.Status);
        Assert.Equal("Milik B", Assert.Single(submission.Answers).StringValue);
        Assert.Empty(check.Signoffs);
        Assert.DoesNotContain(check.AuditEntries.ToList(), e => e.EntityType == nameof(Submission));
    }

    [Fact]
    public async Task Consolidation_only_uses_answers_and_sites_of_active_tenant()
    {
        SeedSubmission(A, SiteA, _seedA, SubmissionStatus.Approved, Narrative("Jawaban A"));
        SeedSubmission(_b, _siteB, _seedB, SubmissionStatus.Approved, Narrative("Jawaban B"));

        await using var a = _db.CreateContext();
        await using var b = _db.CreateContext(_b);
        var resultA = await new ConsolidationQuery(a).BuildAsync(_seedA.TopicId, 2026, CancellationToken.None);
        var resultB = await new ConsolidationQuery(b).BuildAsync(_seedA.TopicId, 2026, CancellationToken.None);

        Assert.Equal("Jawaban A", Assert.Single(resultA.Rows).Text);
        Assert.Equal(new[] { SiteA }, resultA.Coverage.ApprovedSiteIds);
        Assert.Equal(new[] { 2, 3, 4, 5, 6 }, resultA.Coverage.MissingSiteIds);

        Assert.Equal("Jawaban B", Assert.Single(resultB.Rows).Text);
        Assert.Equal(new[] { _siteB }, resultB.Coverage.ApprovedSiteIds);
        Assert.Empty(resultB.Coverage.MissingSiteIds);
    }

    [Fact]
    public async Task Telemetry_queries_are_limited_to_active_tenant_even_for_admin()
    {
        var deviceA = SeedDevice(A, SiteA, "kunci-a");
        var deviceB = SeedDevice(_b, _siteB, "kunci-b");
        using (var db = _db.CreateContext(_b))
        {
            db.Add(new TelemetryReading
            {
                DeviceId = deviceB, Metric = "temperature", Value = 30, Unit = "C",
                RecordedAt = _db.Clock.UtcNow, ReceivedAt = _db.Clock.UtcNow,
            });
            db.SaveChanges();
        }

        await using var ctx = _db.CreateContext();
        var queries = new TelemetryQueries(ctx);

        Assert.Equal(deviceA, Assert.Single(await queries.GetDevicesAsync(Admin, CancellationToken.None)).DeviceId);
        Assert.DoesNotContain(await queries.GetSitesAsync(Admin, CancellationToken.None), s => s.Id == _siteB);
        Assert.Empty(await queries.GetSeriesAsync(Admin, deviceB, _db.Clock.UtcNow.AddDays(-1), CancellationToken.None));
        Assert.Empty(await queries.GetMonthlyAggregateAsync(deviceB, "temperature", 2026, CancellationToken.None));
    }

    [Fact]
    public async Task Device_code_is_unique_per_tenant_and_site_must_be_in_active_tenant()
    {
        await using (var a = _db.CreateContext())
            await new TelemetryDevices(a, a, _db.ActiveTenant).RegisterAsync(Admin, DeviceCode, "Sensor A", SiteA, CancellationToken.None);

        var tenantB = new FakeTenant(_b);
        await using (var b = _db.CreateContext(tenantB))
        {
            // Code sama di tenant lain diterima
            await new TelemetryDevices(b, b, tenantB).RegisterAsync(Admin, DeviceCode, "Sensor B", _siteB, CancellationToken.None);
        }
        await using (var b = _db.CreateContext(tenantB))
        {
            // Code ganda di tenant yang sama ditolak dengan pesan validasi
            var duplicate = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new TelemetryDevices(b, b, tenantB).RegisterAsync(Admin, DeviceCode, "Lagi", _siteB, CancellationToken.None));
            Assert.Contains("sudah dipakai", duplicate.Message);

            // Site tenant lain tidak dikenal
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new TelemetryDevices(b, b, tenantB).RegisterAsync(Admin, "esp32-02", "Lain", SiteA, CancellationToken.None));
        }

        using var checkB = _db.CreateContext(_b);
        var device = Assert.Single(checkB.TelemetryDevices);
        Assert.Equal(_b, device.TenantId);
        Assert.Equal(_siteB, device.SiteId);
    }

    [Fact]
    public async Task Ingest_without_login_stores_reading_under_tenant_of_device()
    {
        SeedDevice(A, SiteA, "kunci-a");
        var deviceB = SeedDevice(_b, _siteB, "kunci-b");

        await using (var anonymous = _db.CreateContext(new FakeTenant(null)))
        {
            var ingest = new TelemetryIngest(anonymous, new TelemetryNotifier(), _db.Clock, NullLogger<TelemetryIngest>.Instance);
            var result = await ingest.IngestAsync("kunci-b",
                new TelemetryPayload(null, [new TelemetryReadingInput("temperature", 27.5, "C")]), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        using var a = _db.CreateContext();
        using var b = _db.CreateContext(_b);
        Assert.Empty(a.TelemetryReadings);
        Assert.Equal(deviceB, Assert.Single(b.TelemetryReadings).DeviceId);
        Assert.Equal(_db.Clock.UtcNow, b.TelemetryDevices.Single().LastSeenAt);
    }

    [Fact]
    public async Task Import_maps_site_code_only_within_active_tenant()
    {
        var dir = Directory.CreateTempSubdirectory("griforms-import-");
        var path = Path.Combine(dir.FullName, "Data-Konsolidasi.xlsx");
        try
        {
            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Data-Konsolidasi");
                ws.Cell(1, 1).Value = "Site_ID";
                ws.Cell(1, 7).Value = "No_Qz";
                ws.Cell(2, 1).Value = 1;            // file lama: Site_ID angka
                ws.Cell(2, 7).Value = "403-5";
                ws.Cell(2, 11).Value = "Pelatihan rutin";
                ws.Cell(3, 1).Value = 7;            // tidak ada di tenant mana pun
                ws.Cell(3, 7).Value = "403-5";
                ws.Cell(3, 11).Value = "Tidak terpetakan";
                wb.SaveAs(path);
            }

            var rows = new DataKonsolidasiImporter().Read(path).ToList();
            Assert.Equal(new[] { "1", "7" }, rows.Select(r => r.SiteCode).ToArray());

            await using var a = _db.CreateContext();
            await using var b = _db.CreateContext(_b);
            var mappedA = await new LegacySiteMapper(a, _db.ActiveTenant, NullLogger<LegacySiteMapper>.Instance)
                .MapAsync(rows, CancellationToken.None);
            var mappedB = await new LegacySiteMapper(b, new FakeTenant(_b), NullLogger<LegacySiteMapper>.Instance)
                .MapAsync(rows, CancellationToken.None);

            Assert.Equal(SiteA, Assert.Single(mappedA.Mapped).SiteId);
            Assert.Equal(_siteB, Assert.Single(mappedB.Mapped).SiteId);
            Assert.Equal("7", Assert.Single(mappedA.Rejected).SiteCode);
            Assert.Equal("7", Assert.Single(mappedB.Rejected).SiteCode);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    public void Dispose() => _db.Dispose();
}
