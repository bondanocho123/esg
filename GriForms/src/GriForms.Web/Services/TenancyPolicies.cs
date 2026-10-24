using System.Security.Claims;
using GriForms.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace GriForms.Web.Services;

public static class TenancyPolicies
{
    // Tabel PlatformAdmin: halaman Organisasi dan kelola definisi form
    public const string PlatformAdmin = "PlatformAdmin";

    // Admin (tanpa site) di tenant aktif: halaman Pengaturan Organisasi
    public const string TenantAdmin = "TenantAdmin";
}

public sealed class PlatformAdminRequirement : IAuthorizationRequirement
{
}

public sealed class TenantAdminRequirement : IAuthorizationRequirement
{
}

// Policy hanya menyaring halaman; service terkait tetap memeriksa ulang hak di server
public sealed class TenancyAuthorizationHandler(ITenantSession session) : IAuthorizationHandler
{
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return;

        foreach (var requirement in context.PendingRequirements.ToList())
        {
            if (requirement is PlatformAdminRequirement
                && await session.IsPlatformAdminAsync(userId, CancellationToken.None))
                context.Succeed(requirement);

            // StartAsync memakai hasil yang ada bila sesi pengguna ini sudah dimulai (middleware atau circuit)
            if (requirement is TenantAdminRequirement
                && (await session.StartAsync(userId, CancellationToken.None)).IsTenantAdmin)
                context.Succeed(requirement);
        }
    }
}
