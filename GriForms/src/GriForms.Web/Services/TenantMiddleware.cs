using System.Security.Claims;
using GriForms.Application.Abstractions;

namespace GriForms.Web.Services;

// Tiap request terautentikasi: tenant aktif ditentukan di server (ITenantSession), lalu
// - selama UserProfile.MustChangePassword aktif, semua halaman, endpoint, dan circuit Blazor (/_blazor) diblokir
//   kecuali halaman ganti kata sandi, logout, dan aset statis;
// - pengguna tanpa organisasi aktif yang membuka halaman utama diarahkan ke halaman Organisasi (PlatformAdmin)
//   atau ke halaman "Akun belum terhubung ke organisasi aktif".
// Circuit Blazor menentukan tenantnya sendiri di TenantCircuitHandler.
public sealed class TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
{
    public const string ChangePasswordPath = "/Account/Manage/ChangePassword";

    public async Task InvokeAsync(HttpContext context, ITenantSession session)
    {
        var path = context.Request.Path;
        if (IsStaticAsset(path))
        {
            await next(context);
            return;
        }

        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var info = await session.StartAsync(userId, context.RequestAborted);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["UserId"] = info.UserId,
            ["TenantId"] = info.TenantId,
        });

        if (info.MustChangePassword && !IsAllowedWhilePasswordChangePending(path))
        {
            if (path.StartsWithSegments("/_blazor") || path.StartsWithSegments("/api") || !HttpMethods.IsGet(context.Request.Method))
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            else
                context.Response.Redirect($"{context.Request.PathBase}{ChangePasswordPath}");
            return;
        }

        if (info.UserId is not null && info.TenantId is null && IsHome(path))
        {
            context.Response.Redirect($"{context.Request.PathBase}/{(info.IsPlatformAdmin ? "organisasi" : "tanpa-organisasi")}");
            return;
        }

        await next(context);
    }

    private static bool IsAllowedWhilePasswordChangePending(PathString path) =>
        path.StartsWithSegments(ChangePasswordPath, StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase);

    private static bool IsHome(PathString path) => !path.HasValue || path.Value == "/";

    private static readonly HashSet<string> StaticExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".map", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".webp", ".woff", ".woff2", ".ttf", ".eot",
    };

    // Aset statis (MapStaticAssets, _framework, _content) tidak butuh tenant dan tetap dilayani selama ganti kata sandi wajib.
    // Ekstensi dibatasi pada daftar aset: endpoint ber-ekstensi lain (mis. export .xlsx) tetap melewati pemeriksaan tenant.
    private static bool IsStaticAsset(PathString path) =>
        path.StartsWithSegments("/_framework")
        || path.StartsWithSegments("/_content")
        || (path.HasValue && StaticExtensions.Contains(Path.GetExtension(path.Value)));
}
