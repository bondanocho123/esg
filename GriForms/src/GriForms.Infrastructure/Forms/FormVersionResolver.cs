using GriForms.Application.Abstractions;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Forms;

public sealed class FormVersionResolver(IAppReadDb db) : IFormVersionResolver
{
    public async Task<FormVersion> ResolveAsync(int topicId, int year, CancellationToken ct)
    {
        var version = await db.FormVersions
            .AsNoTracking()
            .Where(v => v.TopicId == topicId
                     && v.IsPublished
                     && v.EffectiveFromYear <= year
                     && (v.EffectiveToYear == null || v.EffectiveToYear >= year))
            .OrderByDescending(v => v.VersionNo)
            .FirstOrDefaultAsync(ct);

        return version ?? throw new InvalidOperationException(
            $"Tidak ada versi form terbit untuk topik {topicId} tahun {year}.");
    }
}
