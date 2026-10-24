using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Submissions;

// Null juga untuk submission tenant lain: filter tenant di AppDbContext menyembunyikannya
public sealed class SubmissionQueries(IAppReadDb db, IFormDefinitionReader reader) : ISubmissionQueries
{
    public async Task<SubmissionView?> GetAsync(int submissionId, CancellationToken ct)
    {
        var s = await db.Submissions
            .AsNoTracking()
            .Include(x => x.Site)
            .Include(x => x.Period)
            .Include(x => x.FormVersion)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == submissionId, ct);

        if (s is null) return null;

        return new SubmissionView(
            s.Id, s.SiteId, s.Site.Name, s.Period.Year, s.TopicId,
            s.Status, s.Period.Status,
            reader.Read(s.FormVersion.DefinitionJson),
            AnswerCells.ToCells(s.Answers));
    }
}
