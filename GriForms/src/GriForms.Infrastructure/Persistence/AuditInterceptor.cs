using System.Text.Json;
using GriForms.Application.Abstractions;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GriForms.Infrastructure.Persistence;

// Mencatat perubahan Answer ke AuditEntry, terpisah dari kode yang menyimpan jawaban
public class AuditInterceptor(ICurrentUser user, IClock clock, ITenantContext tenant) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> result)
    {
        AddAuditEntries(e.Context);
        return base.SavingChanges(e, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        AddAuditEntries(e.Context);
        return base.SavingChangesAsync(e, result, ct);
    }

    private void AddAuditEntries(DbContext? db)
    {
        if (db is null) return;

        var entries = db.ChangeTracker.Entries<Answer>()
            .Where(x => x.State is EntityState.Modified or EntityState.Added or EntityState.Deleted)
            .ToList();

        foreach (var x in entries)
        {
            db.Add(new AuditEntry
            {
                // Answer hanya bisa diubah di tenant aktif (TenantGuardInterceptor); 0 ditolak di sana bila tenant kosong
                TenantId = tenant.TenantId ?? 0,
                EntityType = nameof(Answer),
                EntityId = $"{x.Entity.SubmissionId}:{x.Entity.QuestionKey}|{x.Entity.PeriodKey}",
                Action = x.State.ToString(),
                OldValue = x.State == EntityState.Added ? null : Describe(x.OriginalValues),
                NewValue = x.State == EntityState.Deleted ? null : Describe(x.CurrentValues),
                UserId = user.UserId ?? "system",
                At = clock.UtcNow,
            });
        }
    }

    private static string Describe(PropertyValues v) =>
        JsonSerializer.Serialize(new
        {
            Number = v[nameof(Answer.NumberValue)],
            Text = v[nameof(Answer.StringValue)],
            Unit = v[nameof(Answer.Unit)],
            State = v[nameof(Answer.ValueState)]?.ToString(),
        });
}
