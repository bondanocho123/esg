using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GriForms.Application;
using GriForms.Application.Abstractions;
using GriForms.Application.Telemetry;
using GriForms.Infrastructure;
using GriForms.Web.Components;
using GriForms.Web.Components.Account;
using GriForms.Web.Components.Forms;
using GriForms.Web.Data;
using GriForms.Web.Services;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFormFields();
builder.Services.AddScoped<ICurrentUser, AuthStateCurrentUser>();
builder.Services.AddScoped<IUserAccounts, IdentityUserAccounts>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Multitenant: tenant aktif circuit ditentukan saat circuit dibuka; request HTTP lewat TenantMiddleware
builder.Services.AddScoped<CircuitHandler, TenantCircuitHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TenancyAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(TenancyPolicies.PlatformAdmin, p => p.RequireAuthenticatedUser().AddRequirements(new PlatformAdminRequirement()))
    .AddPolicy(TenancyPolicies.TenantAdmin, p => p.RequireAuthenticatedUser().AddRequirements(new TenantAdminRequirement()));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Telemetry: batasi tiap device (dikenali dari X-Device-Key) 10 request per menit
builder.Services.AddRateLimiter(options =>
{
    // Header kosong atau tidak masuk akal panjangnya dipartisi per IP, bukan per nilai mentah:
    // satu klien tidak bisa membuat partisi baru tanpa batas dengan key acak, dan tidak berbagi partisi dengan device sah
    options.AddPolicy("telemetry", http =>
    {
        var key = http.Request.Headers["X-Device-Key"].ToString();
        var partition = key.Length is > 0 and <= 128
            ? $"key:{key}"
            : $"ip:{http.Connection.RemoteIpAddress}";
        return RateLimitPartition.GetFixedWindowLimiter(
            partition,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) });
    });
    // Respons berisi body agar tidak di-re-execute oleh UseStatusCodePagesWithReExecute
    options.OnRejected = (ctx, _) => new ValueTask(
        Results.Problem("Terlalu banyak request dari device ini.", statusCode: StatusCodes.Status429TooManyRequests)
            .ExecuteAsync(ctx.HttpContext));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAntiforgery();

// Setelah autentikasi (ditambahkan otomatis di awal pipeline): tenant aktif dan wajib ganti kata sandi
app.UseMiddleware<TenantMiddleware>();

app.MapStaticAssets();

// Ingest telemetry dari device (ESP32). Endpoint JSON tidak terkena antiforgery.
// Saat development jalankan profile http: POST ke http:// yang di-redirect ke https akan gagal di device.
app.MapPost("/api/telemetry", async (
        [FromHeader(Name = "X-Device-Key")] string? key,
        TelemetryPayload payload, ITelemetryIngest ingest, CancellationToken ct) =>
    {
        // Header kosong ditolak di IngestAsync, jadi tidak perlu dicek di sini
        var result = await ingest.IngestAsync(key ?? "", payload, ct);
        return result.IsSuccess ? Results.Accepted() : Results.Problem(result.Error, statusCode: result.StatusCode);
    })
    .RequireRateLimiting("telemetry");

// Pemilih tenant di navbar (form POST biasa, berfungsi juga di halaman Account yang statis).
// Tenant divalidasi terhadap keanggotaan di server; halaman dimuat ulang agar semua data mengikuti tenant baru.
app.MapPost("/tenant/switch", async (
        [FromForm] int tenantId, [FromForm] string? returnUrl, ITenantSession session, CancellationToken ct) =>
    {
        await session.SwitchAsync(tenantId, ct);
        return TypedResults.LocalRedirect($"~/{returnUrl}");
    })
    .RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

await app.RunPlatformBootstrapAsync();

app.Run();
