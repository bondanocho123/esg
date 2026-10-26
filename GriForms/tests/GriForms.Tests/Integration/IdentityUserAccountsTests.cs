using GriForms.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Tests.Integration;

// IUserAccounts di atas Identity sungguhan (SQLite in-memory), dengan opsi Identity yang sama seperti Program.cs
public class IdentityUserAccountsTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private ServiceProvider _services = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _services = new ServiceCollection()
            .AddLogging()
            .AddDataProtection().Services   // di aplikasi didaftarkan host web; token reset kata sandi membutuhkannya
            .AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection))
            .AddIdentityCore<ApplicationUser>(o => o.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .Services
            .BuildServiceProvider();
        await _services.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
    }

    private (IdentityUserAccounts Accounts, UserManager<ApplicationUser> Users) Create()
    {
        var users = _services.GetRequiredService<UserManager<ApplicationUser>>();
        return (new IdentityUserAccounts(users), users);
    }

    [Fact]
    public async Task Reset_replaces_password_unlocks_account_and_renews_security_stamp()
    {
        var (accounts, users) = Create();
        var account = await accounts.CreateAsync("reviewer@jre.com", "Awal-12345!", CancellationToken.None);
        var user = (await users.FindByIdAsync(account.UserId))!;
        var stampBefore = user.SecurityStamp;
        await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(1));

        await accounts.ResetPasswordAsync(account.UserId, "Sementara-123!", CancellationToken.None);

        user = (await users.FindByIdAsync(account.UserId))!;
        Assert.True(await users.CheckPasswordAsync(user, "Sementara-123!"));
        Assert.False(await users.CheckPasswordAsync(user, "Awal-12345!"));
        Assert.False(await users.IsLockedOutAsync(user));
        Assert.NotEqual(stampBefore, user.SecurityStamp);   // sesi login lain berakhir saat divalidasi ulang
    }

    [Fact]
    public async Task Weak_password_or_unknown_account_is_rejected_without_changes()
    {
        var (accounts, users) = Create();
        var account = await accounts.CreateAsync("reviewer@jre.com", "Awal-12345!", CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            accounts.ResetPasswordAsync(account.UserId, "123", CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            accounts.ResetPasswordAsync("tidak-ada", "Sementara-123!", CancellationToken.None));

        Assert.True(await users.CheckPasswordAsync((await users.FindByIdAsync(account.UserId))!, "Awal-12345!"));
    }

    public async Task DisposeAsync()
    {
        await _services.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
