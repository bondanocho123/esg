using System.Security.Claims;
using GriForms.Application.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace GriForms.Web.Services;

// Tenant aktif circuit Blazor ditentukan saat circuit dibuka, sebelum komponen apa pun dirender.
// Pergantian tenant memuat ulang halaman (circuit baru), jadi tenant satu circuit tidak berubah di tengah jalan.
public sealed class TenantCircuitHandler(
    AuthenticationStateProvider auth, ITenantSession session, ILogger<TenantCircuitHandler> logger) : CircuitHandler
{
    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var user = (await auth.GetAuthenticationStateAsync()).User;
        var userId = user.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.NameIdentifier) : null;
        await session.StartAsync(userId, cancellationToken);
    }

    // Properti UserId dan TenantId untuk log selama event circuit (Serilog Enrich.FromLogContext)
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            using var scope = logger.BeginScope(new Dictionary<string, object?>
            {
                ["UserId"] = session.Current.UserId,
                ["TenantId"] = session.Current.TenantId,
            });
            await next(context);
        };
}
