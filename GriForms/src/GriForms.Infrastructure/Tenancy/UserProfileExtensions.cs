using GriForms.Domain.Entities;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Tenancy;

internal static class UserProfileExtensions
{
    // Profil yang sudah ada, atau profil baru yang ditambahkan ke db (disimpan pemanggil)
    public static async Task<UserProfile> FindOrAddProfileAsync(this AppDbContext db, string userId, CancellationToken ct)
    {
        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(profile);
        }
        return profile;
    }
}
