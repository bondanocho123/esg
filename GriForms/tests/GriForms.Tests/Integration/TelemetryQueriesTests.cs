using GriForms.Application.Telemetry;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Telemetry;
using GriForms.Tests.Support;

namespace GriForms.Tests.Integration;

public class TelemetryQueriesTests : IDisposable
{
    private readonly TestDb _db = new();

    private static DateTime Utc(int month, int day, int hour = 0) => new(2026, month, day, hour, 0, 0, DateTimeKind.Utc);

    private int SeedDevice(string code, int siteId, params (string Metric, double Value, DateTime At)[] readings)
    {
        using var db = _db.CreateContext();
        var device = new TelemetryDevice { Code = code, Name = code, SiteId = siteId, ApiKeyHash = DeviceApiKey.Hash(code) };
        db.Add(device);
        db.SaveChanges();
        db.AddRange(readings.Select(r => new TelemetryReading
        {
            DeviceId = device.Id, Metric = r.Metric, Value = r.Value, Unit = TelemetryMetrics.Find(r.Metric)!.Unit,
            RecordedAt = r.At, ReceivedAt = r.At,
        }));
        db.SaveChanges();
        return device.Id;
    }

    [Fact]
    public async Task Devices_show_latest_value_per_metric_within_user_sites()
    {
        var ho = SeedDevice("esp32-ho-01", 1,
            ("temperature", 25, Utc(10, 21, 8)),
            ("temperature", 27, Utc(10, 21, 9)),
            ("humidity", 60, Utc(10, 21, 9)));
        SeedDevice("esp32-bakan-01", 2, ("temperature", 30, Utc(10, 21, 9)));

        await using var ctx = _db.CreateContext();
        var queries = new TelemetryQueries(ctx);

        var all = await queries.GetDevicesAsync(new TelemetryAccess(true, null), CancellationToken.None);
        var headOffice = await queries.GetDevicesAsync(new TelemetryAccess(false, [1]), CancellationToken.None);

        Assert.Equal(2, all.Count);
        var device = Assert.Single(headOffice);
        Assert.Equal(ho, device.DeviceId);
        Assert.Equal(new[] { ("humidity", 60.0), ("temperature", 27.0) },
            device.Latest.Select(r => (r.Metric, r.Value)).ToArray());
    }

    [Fact]
    public async Task Access_follows_user_site_roles()
    {
        using (var db = _db.CreateContext())
        {
            db.AddRange(
                new UserSiteRole { UserId = "admin", SiteId = null, Role = RoleName.Admin },
                new UserSiteRole { UserId = "kontributor", SiteId = 3, Role = RoleName.Contributor });
            db.SaveChanges();
        }

        await using var ctx = _db.CreateContext();
        var queries = new TelemetryQueries(ctx);

        var admin = await queries.GetAccessAsync("admin", CancellationToken.None);
        var contributor = await queries.GetAccessAsync("kontributor", CancellationToken.None);
        var stranger = await queries.GetAccessAsync("tamu", CancellationToken.None);

        Assert.True(admin.IsAdmin);
        Assert.Null(admin.SiteIds);
        Assert.False(contributor.IsAdmin);
        Assert.Equal(new[] { 3 }, contributor.SiteIds!);
        Assert.Empty(stranger.SiteIds!);
    }

    [Fact]
    public async Task Monthly_aggregate_uses_period_key_like_monthly_table()
    {
        var id = SeedDevice("esp32-ho-01", 1,
            ("temperature", 20, Utc(1, 5)),
            ("temperature", 30, Utc(1, 20)),
            ("temperature", 25, Utc(2, 1)),
            ("humidity", 80, Utc(1, 5)));

        await using var ctx = _db.CreateContext();
        var rows = await new TelemetryQueries(ctx).GetMonthlyAggregateAsync(id, "temperature", 2026, CancellationToken.None);

        Assert.Collection(rows,
            m => Assert.Equal(new MonthlyAggregate("2026-01", 25, 20, 30, 2), m),
            m => Assert.Equal(new MonthlyAggregate("2026-02", 25, 25, 25, 1), m));
    }

    [Fact]
    public async Task Series_is_empty_for_device_outside_user_sites()
    {
        var bakan = SeedDevice("esp32-bakan-01", 2, ("temperature", 30, Utc(10, 21, 9)));

        await using var ctx = _db.CreateContext();
        var queries = new TelemetryQueries(ctx);

        var denied = await queries.GetSeriesAsync(new TelemetryAccess(false, [1]), bakan, Utc(10, 21), CancellationToken.None);
        var allowed = await queries.GetSeriesAsync(new TelemetryAccess(false, [2]), bakan, Utc(10, 21), CancellationToken.None);
        var admin = await queries.GetSeriesAsync(new TelemetryAccess(true, null), bakan, Utc(10, 21), CancellationToken.None);

        Assert.Empty(denied);
        Assert.Single(allowed);
        Assert.Single(admin);
    }

    [Fact]
    public async Task Series_for_unknown_device_is_empty()
    {
        await using var ctx = _db.CreateContext();

        var rows = await new TelemetryQueries(ctx).GetSeriesAsync(new TelemetryAccess(true, null), 9999, Utc(1, 1), CancellationToken.None);

        Assert.Empty(rows);
    }

    public void Dispose() => _db.Dispose();
}
