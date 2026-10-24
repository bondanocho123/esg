using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Submissions;

// Hanya menyimpan sel. Audit ditangani AuditInterceptor, kenaikan RowVersion oleh RowVersionInterceptor.
// Submission tenant lain tidak terlihat (filter tenant di AppDbContext), jadi dianggap tidak ditemukan.
public sealed class AnswerWriter(
    IAppReadDb db, IUnitOfWork uow, IFormDefinitionReader reader, ICurrentUser user, IClock clock) : IAnswerWriter
{
    public async Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(changes);
        var changeList = changes.ToList();
        if (changeList.Any(c => c is null || string.IsNullOrWhiteSpace(c.CellKey)))
            throw new ArgumentException("Perubahan sel tidak boleh kosong dan harus punya kunci sel.", nameof(changes));
        if (changeList.Count == 0) return;

        var submission = await db.Submissions
            .AsNoTracking()
            .Include(s => s.Period)
            .Include(s => s.FormVersion)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct)
            ?? throw new InvalidOperationException($"Submission {submissionId} tidak ditemukan.");

        if (submission.Period.Status == PeriodStatus.Closed || !submission.IsEditableBy(RoleName.Contributor))
            throw new InvalidOperationException($"Submission {submissionId} berstatus {submission.Status} dan tidak bisa diubah.");

        var questions = reader.Read(submission.FormVersion.DefinitionJson).Disclosures
            .SelectMany(d => d.Questions.Select(q => (DisclosureNo: d.No, Question: q)))
            .ToDictionary(x => x.Question.Key);

        var existing = (await db.Answers
                .Where(a => a.SubmissionId == submissionId)
                .ToListAsync(ct))
            .ToDictionary(a => FormState.CellKey(a.QuestionKey, a.PeriodKey));

        // batas keras di server walau UI sudah membatasi: teks berukuran tak wajar ditolak
        const int HardTextLimit = 20_000;

        foreach (var change in changeList)
        {
            if (change.Value?.Text is { Length: > HardTextLimit })
                throw new InvalidOperationException($"Isian {change.CellKey} melebihi {HardTextLimit} karakter.");

            var (questionKey, periodKey) = FormState.SplitCellKey(change.CellKey);
            if (!questions.TryGetValue(FormState.QuestionOf(questionKey), out var def))
                throw new InvalidOperationException($"Pertanyaan {questionKey} tidak ada di definisi form.");

            existing.TryGetValue(change.CellKey, out var answer);

            if (change.Value is null)
            {
                if (answer is not null) uow.Remove(answer);
                continue;
            }

            if (answer is null)
            {
                answer = new Answer { SubmissionId = submissionId, QuestionKey = questionKey, PeriodKey = periodKey };
                uow.Add(answer);
                existing[change.CellKey] = answer;
            }

            answer.DisclosureNo = def.DisclosureNo;
            answer.NumberValue = change.Value.Number;
            answer.StringValue = change.Value.Text;
            answer.ValueState = change.Value.State;
            answer.Unit = def.Question.Unit;   // unit dari definisi, tidak diketik pengguna
            answer.UpdatedBy = user.UserId;
            answer.UpdatedAt = clock.UtcNow;
        }

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "Jawaban sudah diubah pengguna lain. Muat ulang form lalu ulangi perubahan.", ex);
        }
    }
}
