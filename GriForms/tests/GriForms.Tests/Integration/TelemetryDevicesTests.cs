using GriForms.Application.Abstractions;
using GriForms.Application.Telemetry;
using GriForms.Infrastructure.Telemetry;
using GriForms.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Tests.Integration;

public class TelemetryDevicesTests : IDisposable
{
    private static readonly TelemetryAccess Admin = new(true, null);

    private readonly TestDb _db = new();

    private async Task<string> RegisterAsync(TelemetryAccess access, string code, string name = "Sensor", int siteId = 1)
    {
        await using var ctx = _db.CreateContext();
        return await new TelemetryDevices(ctx, ctx, _db.ActiveTenant).RegisterAsync(access, code, name, siteId, CancellationToken.None);
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
    public async Task Save_failure_other_than_duplicate_is_not_reported_as_duplicate_code()
    {
        await using var ctx = _db.CreateContext();
        var devices = new TelemetryDevices(ctx, new FailingUnitOfWork(ctx));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            devices.RegisterAsync(Admin, "esp32-ho-01", "Sensor", 1, CancellationToken.None));
    }

    // Meniru SQLite 'database is locked': simpan gagal tanpa ada baris dengan kode yang sama
    private sealed class FailingUnitOfWork(IUnitOfWork inner) : IUnitOfWork
    {
        public void Add<T>(T entity) where T : class => inner.Add(entity);
        public void Remove<T>(T entity) where T : class => inner.Remove(entity);
        public Task<int> SaveChangesAsync(CancellationToken ct) => throw new DbUpdateException("database is locked");
    }

    [Fact]
    public async Task Unknown_site_is_rejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterAsync(Admin, "esp32-x-01", siteId: 999));
    }

    public void Dispose() => _db.Dispose();
}
