using GriForms.Application.Telemetry;
using GriForms.Domain.Entities;
using GriForms.Infrastructure.Telemetry;
using GriForms.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace GriForms.Tests.Integration;

public class TelemetryIngestTests : IDisposable
{
    private const string Key = "kunci-benar";

    private readonly TestDb _db = new();
    private readonly TelemetryNotifier _notifier = new();

    private int SeedDevice(bool isActive = true)
    {
        using var db = _db.CreateContext();
        var device = new TelemetryDevice
        {
            Code = "esp32-ho-01", Name = "Sensor HO", SiteId = 1,
            ApiKeyHash = DeviceApiKey.Hash(Key), IsActive = isActive,
        };
        db.Add(device);
        db.SaveChanges();
        return device.Id;
    }

    private async Task<IngestResult> IngestAsync(string key, TelemetryPayload payload)
    {
        await using var ctx = _db.CreateContext();
        var ingest = new TelemetryIngest(ctx, _notifier, _db.Clock, NullLogger<TelemetryIngest>.Instance);
        return await ingest.IngestAsync(key, payload, CancellationToken.None);
    }

    private static TelemetryPayload Payload(double temperature = 28.4, double humidity = 71.0, DateTime? recordedAt = null) =>
        new(recordedAt, [new("temperature", temperature, "C"), new("humidity", humidity, "%")]);

    [Fact]
    public async Task Wrong_key_is_rejected()
    {
        SeedDevice();

        var result = await IngestAsync("kunci-salah", Payload());

        Assert.False(result.IsSuccess);
        Assert.Equal(401, result.StatusCode);
        using var db = _db.CreateContext();
        Assert.Empty(db.TelemetryReadings);
    }

    [Fact]
    public async Task Inactive_device_is_rejected()
    {
        SeedDevice(isActive: false);

        var result = await IngestAsync(Key, Payload());

        Assert.Equal(403, result.StatusCode);
        using var db = _db.CreateContext();
        Assert.Empty(db.TelemetryReadings);
    }

    [Theory]
    [InlineData(51.0, 71.0)]     // suhu di atas rentang DHT11
    [InlineData(-1.0, 71.0)]
    [InlineData(28.0, 95.0)]     // kelembapan di atas rentang DHT11
    public async Task Out_of_range_value_rejects_whole_payload(double temperature, double humidity)
    {
        var deviceId = SeedDevice();

        var result = await IngestAsync(Key, Payload(temperature, humidity));

        Assert.Equal(422, result.StatusCode);
        using var db = _db.CreateContext();
        Assert.Empty(db.TelemetryReadings);
        Assert.Null(db.TelemetryDevices.Single(d => d.Id == deviceId).LastSeenAt);
    }

    [Fact]
    public async Task Unknown_metric_or_wrong_unit_is_rejected()
    {
        SeedDevice();

        var unknown = await IngestAsync(Key, new TelemetryPayload(null, [new("pressure", 1000, "hPa")]));
        var wrongUnit = await IngestAsync(Key, new TelemetryPayload(null, [new("temperature", 28, "F")]));

        Assert.Equal(422, unknown.StatusCode);
        Assert.Equal(422, wrongUnit.StatusCode);
    }

    [Fact]
    public async Task Empty_payload_is_rejected()
    {
        SeedDevice();

        var result = await IngestAsync(Key, new TelemetryPayload(null, []));

        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task Valid_payload_is_stored_and_updates_last_seen()
    {
        var deviceId = SeedDevice();
        var published = new List<int>();
        _notifier.ReadingsReceived += published.Add;
        var recordedAt = _db.Clock.UtcNow.AddMinutes(-1);

        var result = await IngestAsync(Key, Payload(28.4, 71.0, recordedAt));

        Assert.True(result.IsSuccess);
        Assert.Equal(202, result.StatusCode);
        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { deviceId }, published);

        using var db = _db.CreateContext();
        var readings = db.TelemetryReadings.OrderBy(r => r.Metric).ToList();
        Assert.Collection(readings,
            r => { Assert.Equal("humidity", r.Metric); Assert.Equal(71.0, r.Value); Assert.Equal("%", r.Unit); },
            r => { Assert.Equal("temperature", r.Metric); Assert.Equal(28.4, r.Value); Assert.Equal("C", r.Unit); });
        Assert.All(readings, r =>
        {
            Assert.Equal(deviceId, r.DeviceId);
            Assert.Equal(recordedAt, r.RecordedAt);
            Assert.Equal(_db.Clock.UtcNow, r.ReceivedAt);
        });
        Assert.Equal(_db.Clock.UtcNow, db.TelemetryDevices.Single(d => d.Id == deviceId).LastSeenAt);
    }

    [Fact]
    public async Task Unsynced_device_clock_falls_back_to_received_at()
    {
        SeedDevice();

        // ESP32 tanpa NTP mulai dari 1970
        await IngestAsync(Key, Payload(recordedAt: new DateTime(1970, 1, 1, 0, 1, 0, DateTimeKind.Utc)));

        using var db = _db.CreateContext();
        Assert.All(db.TelemetryReadings, r => Assert.Equal(_db.Clock.UtcNow, r.RecordedAt));
    }

    [Fact]
    public async Task Missing_or_blank_key_is_rejected()
    {
        SeedDevice();

        Assert.Equal(401, (await IngestAsync("", Payload())).StatusCode);
        Assert.Equal(401, (await IngestAsync("   ", Payload())).StatusCode);
    }

    [Fact]
    public async Task Null_payload_and_null_readings_are_rejected()
    {
        SeedDevice();

        Assert.Equal(400, (await IngestAsync(Key, null!)).StatusCode);
        Assert.Equal(400, (await IngestAsync(Key, new TelemetryPayload(null, null))).StatusCode);
    }

    [Fact]
    public async Task Too_many_readings_are_rejected()
    {
        SeedDevice();
        var readings = Enumerable.Range(0, 51).Select(_ => new TelemetryReadingInput("temperature", 25, "C")).ToList();

        var result = await IngestAsync(Key, new TelemetryPayload(null, readings));

        Assert.Equal(400, result.StatusCode);
        using var db = _db.CreateContext();
        Assert.Empty(db.TelemetryReadings);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task Non_finite_value_is_rejected(double value)
    {
        SeedDevice();

        var result = await IngestAsync(Key, new TelemetryPayload(null, [new("temperature", value, "C")]));

        Assert.Equal(422, result.StatusCode);
    }

    [Fact]
    public async Task Missing_metric_or_value_is_rejected()
    {
        SeedDevice();

        Assert.Equal(422, (await IngestAsync(Key, new TelemetryPayload(null, [new(null, 25, "C")]))).StatusCode);
        Assert.Equal(422, (await IngestAsync(Key, new TelemetryPayload(null, [new("temperature", null, "C")]))).StatusCode);
    }

    [Fact]
    public async Task Throwing_dashboard_handler_does_not_fail_ingest()
    {
        SeedDevice();
        var reached = false;
        _notifier.ReadingsReceived += _ => throw new InvalidOperationException("dashboard rusak");
        _notifier.ReadingsReceived += _ => reached = true;

        var result = await IngestAsync(Key, Payload());

        Assert.True(result.IsSuccess);
        Assert.True(reached);
        using var db = _db.CreateContext();
        Assert.Equal(2, db.TelemetryReadings.Count());
    }

    public void Dispose() => _db.Dispose();
}
