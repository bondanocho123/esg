using GriForms.Application.Abstractions;
using GriForms.Application.Consolidation;
using GriForms.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Consolidation;

// Hanya data Approved. Tidak ada Sum() di query karena SQLite menyimpan decimal sebagai TEXT.
// Submission, jawaban, dan site dibatasi ke tenant aktif oleh filter tenant di AppDbContext.
public sealed class ConsolidationQuery(IAppReadDb db) : IConsolidationQuery
{
    public async Task<ConsolidationResult> BuildAsync(int topicId, int year, CancellationToken ct)
    {
        var approved = db.Submissions.AsNoTracking()
            .Where(s => s.TopicId == topicId && s.Period.Year == year && s.Status == SubmissionStatus.Approved);

        // Dimulai dari Answer, bukan SelectMany dari Submission: koleksi Answers yang ikut difilter tenant
        // menjadi subquery berkorelasi (APPLY) yang tidak didukung SQLite
        var rows = await db.Answers.AsNoTracking()
            .Where(a => a.Submission.TopicId == topicId && a.Submission.Period.Year == year
                     && a.Submission.Status == SubmissionStatus.Approved)
            .Select(a => new ConsolidatedRow(
                a.Submission.SiteId, a.Submission.Site.Name, a.DisclosureNo, a.QuestionKey,
                a.PeriodKey, a.NumberValue, a.StringValue, a.Unit, a.ValueState))
            .ToListAsync(ct);

        var approvedIds = await approved.Select(s => s.SiteId).Distinct().ToListAsync(ct);
        var activeIds = await db.Sites.AsNoTracking().Where(s => s.IsActive).Select(s => s.Id).ToListAsync(ct);

        return new ConsolidationResult(topicId, year,
            new Coverage(approvedIds.Order().ToList(), activeIds.Except(approvedIds).Order().ToList()), rows);
    }
}
