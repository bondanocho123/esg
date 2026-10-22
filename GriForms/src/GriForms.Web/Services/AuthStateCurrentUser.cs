using System.Security.Claims;
using GriForms.Application.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;

namespace GriForms.Web.Services;

// Pengguna yang login pada circuit Blazor saat ini
public sealed class AuthStateCurrentUser(AuthenticationStateProvider auth) : ICurrentUser
{
    public string? UserId
    {
        get
        {
            var state = auth.GetAuthenticationStateAsync();
            return state.IsCompletedSuccessfully
                ? state.Result.User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
        }
    }
}
