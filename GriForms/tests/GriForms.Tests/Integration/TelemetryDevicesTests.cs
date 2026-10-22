using GriForms.Application.Telemetry;
using GriForms.Infrastructure.Telemetry;
using GriForms.Tests.Support;

namespace GriForms.Tests.Integration;

public class TelemetryDevicesTests : IDisposable
{
    private static readonly TelemetryAccess Admin = new(true, null);

    private readonly TestDb _db = new();

    private async Task<string> RegisterAsync(TelemetryAccess access, string code, string name = "Sensor", int siteId = 1)
    {
        await using var ctx = _db.CreateContext();
        return await new TelemetryDevices(ctx, ctx).RegisterAsync(access, code, name, siteId, CancellationToken.None);
    }

    [Fact]
    public async Task Admin_registers_device_and_only_hash_is_stored()
    {
        var key = await RegisterAsync(Admin, " esp32-ho-01 ", "");

        using var db = _db.CreateContext();
        var device = Assert.Single(db.TelemetryDevices);
        Assert.Equal("esp32-ho-01", device.Code);
        Assert.Equal("esp32-ho-01", device.Name);
        Assert.Equal(DeviceApiKey.Hash(key), device.ApiKeyHash);
        Assert.NotEqual(key, device.ApiKeyHash);
    }

    [Fact]
    public async Task Non_admin_cannot_register()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RegisterAsync(new TelemetryAccess(false, [1]), "esp32-ho-01"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RegisterAsync(TelemetryAccess.None, "esp32-ho-01"));

        using var db = _db.CreateContext();
        Assert.Empty(db.TelemetryDevices);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("kode dengan spasi")]
    [InlineData("<script>")]
    [InlineData("-diawali-strip")]
    public async Task Invalid_code_is_rejected(string code)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterAsync(Admin, code));
    }

    [Fact]
    public async Task Overlong_code_or_name_is_rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterAsync(Admin, new string('a', 51)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterAsync(Admin, "esp32-ho-01", new string('n', 101)));
    }

    [Fact]
    public async Task Duplicate_code_is_rejected()
    {
        await RegisterAsync(Admin, "esp32-ho-01");

        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterAsync(Admin, "esp32-ho-01"));

        using var db = _db.CreateContext();
        Assert.Single(db.TelemetryDevices);
    }

    [Fact]
    public async Task Unknown_site_is_rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterAsync(Admin, "esp32-x-01", siteId: 999));
    }

    public void Dispose() => _db.Dispose();
}
