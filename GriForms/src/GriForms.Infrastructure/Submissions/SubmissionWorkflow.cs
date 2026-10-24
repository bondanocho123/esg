using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Application.Submissions;
using GriForms.Application.Validation;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Submissions;

// Menerapkan transisi status. Aturan transisi sendiri ada di Submission.Transition (Domain).
// Submission tenant lain tidak terlihat (filter tenant di AppDbContext), jadi dianggap tidak ditemukan.
public sealed class SubmissionWorkflow(
    IAppReadDb db, IUnitOfWork uow, IFormDefinitionReader reader, IValidationEngine validation,
    ICurrentUser user, IClock clock) : ISubmissionWorkflow
{
    public async Task TransitionAsync(int submissionId, SubmissionStatus to, RoleName actorRole, string? comment, CancellationToken ct)
    {
        var submission = await db.Submissions
            .Include(s => s.Period)
            .Include(s => s.FormVersion)
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == submissionId, ct)
            ?? throw new InvalidOperationException($"Submission {submissionId} tidak ditemukan.");

        if (submission.Period.Status == PeriodStatus.Closed)
            throw new InvalidOperationException("Periode sudah ditutup, submission hanya bisa dibaca.");

        if (to == SubmissionStatus.Submitted)
        {
            var definition = reader.Read(submission.FormVersion.DefinitionJson);
            var result = validation.Validate(definition, AnswerCells.ToCells(submission.Answers), forSubmit: true);
            if (result.HasErrors)
                throw new InvalidOperationException(
                    $"Submission belum bisa diajukan, masih ada {result.Issues.Count(i => i.Severity == Severity.Error)} error validasi.");
        }

        var from = submission.Status;
        submission.Transition(to, actorRole, comment);

        var userId = user.UserId ?? "system";
        var now = clock.UtcNow;

        if (to is SubmissionStatus.Reviewed or SubmissionStatus.Approved)
        {
            uow.Add(new Signoff
            {
                SubmissionId = submission.Id,
                Role = actorRole,
                UserId = userId,
                DisplayName = userId,   // nama dan jabatan dari profil menyusul bersama halaman Admin (bab 11)
                SignedAt = now,
            });
        }

        uow.Add(new AuditEntry
        {
            TenantId = submission.TenantId,
            EntityType = nameof(Submission),
            EntityId = submission.Id.ToString(),
            Action = "Transition",
            OldValue = from.ToString(),
            NewValue = string.IsNullOrWhiteSpace(comment) ? to.ToString() : $"{to}: {comment}",
            UserId = userId,
            At = now,
        });

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "Submission sudah diubah pengguna lain. Muat ulang lalu ulangi.", ex);
        }
    }
}
