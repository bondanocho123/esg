using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GriForms.Infrastructure.Persistence;

// SQLite tidak punya rowversion otomatis: naikkan kolom RowVersion (concurrency token) setiap update
public class RowVersionInterceptor : SaveChangesInterceptor
{
    private const string Column = "RowVersion";

    public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> result)
    {
        Bump(e.Context);
        return base.SavingChanges(e, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        Bump(e.Context);
        return base.SavingChangesAsync(e, result, ct);
    }

    private static void Bump(DbContext? db)
    {
        if (db is null) return;

        foreach (var entry in db.ChangeTracker.Entries().Where(x => x.State == EntityState.Modified).ToList())
        {
            if (entry.Metadata.FindProperty(Column) is null) continue;
            var property = entry.Property(Column);
            property.CurrentValue = (int)(property.OriginalValue ?? 0) + 1;
        }
    }
}
