using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Forms;

// Topic, FormVersion, dan QuestionMapping adalah katalog GRI global (tanpa tenant), dikelola PlatformAdmin saja
public sealed class FormCatalog(AppDbContext db, ICurrentUser user) : IFormCatalog
{
    public async Task<IReadOnlyList<FormVersionSummary>> GetVersionsAsync(CancellationToken ct)
    {
        var userId = user.UserId;
        if (userId is null || !await db.PlatformAdmins.AnyAsync(p => p.UserId == userId, ct))
            throw new UnauthorizedAccessException("Hanya PlatformAdmin yang boleh mengelola definisi form.");

        return await db.FormVersions.AsNoTracking()
            .OrderBy(v => v.Topic.Code).ThenBy(v => v.VersionNo)
            .Select(v => new FormVersionSummary(
                v.Id, v.Topic.Code, v.Topic.Name, v.VersionNo, v.StandardRef,
                v.EffectiveFromYear, v.EffectiveToYear, v.IsPublished,
                db.QuestionMappings.Count(m => m.FromFormVersionId == v.Id || m.ToFormVersionId == v.Id)))
            .ToListAsync(ct);
    }
}
