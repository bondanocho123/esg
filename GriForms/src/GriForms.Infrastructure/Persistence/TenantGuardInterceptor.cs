using System.Linq.Expressions;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GriForms.Infrastructure.Persistence;

// Pemeriksaan tenant saat SaveChanges. Dipasang terakhir (AppDbContext.OnConfiguring), jadi AuditEntry
// yang ditambahkan AuditInterceptor ikut diperiksa.
// - Entitas akar baru dengan TenantId 0 diisi dari tenant aktif.
// - Entitas akar milik tenant lain, atau TenantId yang diubah, ditolak.
// - Submission wajib satu tenant dengan Site dan Period-nya; TelemetryDevice dan UserSiteRole dengan Site-nya.
// - Answer, Signoff, dan TelemetryReading wajib milik induk di tenant aktif.
// Jalur sah lintas tenant (SaveChangesAcrossTenantsAsync) wajib mengisi TenantId eksplisit; pemeriksaan konsistensi tetap berlaku.
public sealed class TenantGuardInterceptor : SaveChangesInterceptor
{
    public static readonly TenantGuardInterceptor Instance = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> result)
    {
        if (e.Context is AppDbContext db) Enforce(db);
        return base.SavingChanges(e, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        // SQLite menjalankan perintah secara sinkron, jadi pemeriksaan sinkron tidak memblokir lebih lama dari versi async
        if (e.Context is AppDbContext db) Enforce(db);
        return base.SavingChangesAsync(e, result, ct);
    }

    private static void Enforce(AppDbContext db)
    {
        var active = db.CurrentTenant.TenantId;
        var crossTenant = db.CrossTenantWrite;
        var lookup = new TenantLookup(db);

        foreach (var entry in db.ChangeTracker.Entries<ITenantOwned>().Where(IsChanged).ToList())
        {
            var entity = entry.Entity;
            var name = entry.Metadata.ClrType.Name;

            if (entry.State == EntityState.Added && entity.TenantId == 0)
            {
                if (crossTenant || active is null)
                    throw new InvalidOperationException(
                        $"{name} baru tidak punya tenant: tenant aktif kosong atau TenantId tidak diisi eksplisit.");
                entity.TenantId = active.Value;
            }

            if (entry.State == EntityState.Modified
                && !Equals(entry.Property(nameof(ITenantOwned.TenantId)).OriginalValue, entity.TenantId))
                throw new InvalidOperationException($"TenantId {name} tidak boleh diubah.");

            if (!crossTenant && entity.TenantId != active)
                throw new InvalidOperationException($"{name} milik tenant lain tidak bisa diubah dari tenant aktif.");
        }

        // Navigasi induk dipakai lebih dulu bila terisi: FK entitas yang baru ditambahkan bersama induknya belum berisi Id
        foreach (var entry in db.ChangeTracker.Entries<Submission>().Where(IsAddedOrModified).ToList())
        {
            var s = entry.Entity;
            RequireSame(s.TenantId, s.Site?.TenantId ?? lookup.SiteTenant(s.SiteId), $"Site {s.SiteId} bukan milik tenant submission.");
            RequireSame(s.TenantId, s.Period?.TenantId ?? lookup.PeriodTenant(s.PeriodId), $"Periode {s.PeriodId} bukan milik tenant submission.");
        }

        foreach (var entry in db.ChangeTracker.Entries<TelemetryDevice>().Where(IsAddedOrModified).ToList())
        {
            var d = entry.Entity;
            RequireSame(d.TenantId, d.Site?.TenantId ?? lookup.SiteTenant(d.SiteId), $"Site {d.SiteId} bukan milik tenant device {d.Code}.");
        }

        foreach (var entry in db.ChangeTracker.Entries<UserSiteRole>().Where(IsAddedOrModified).ToList())
        {
            var r = entry.Entity;
            if (r.SiteId is { } siteId)
                RequireSame(r.TenantId, r.Site?.TenantId ?? lookup.SiteTenant(siteId), $"Site {siteId} bukan milik tenant peran ini.");
        }

        if (crossTenant) return;

        foreach (var entry in db.ChangeTracker.Entries<Answer>().Where(IsChanged).ToList())
            RequireActive(active, entry.Entity.Submission?.TenantId ?? lookup.SubmissionTenant(entry.Entity.SubmissionId), nameof(Answer));
        foreach (var entry in db.ChangeTracker.Entries<Signoff>().Where(IsChanged).ToList())
            RequireActive(active, entry.Entity.Submission?.TenantId ?? lookup.SubmissionTenant(entry.Entity.SubmissionId), nameof(Signoff));
        foreach (var entry in db.ChangeTracker.Entries<TelemetryReading>().Where(IsChanged).ToList())
            RequireActive(active, entry.Entity.Device?.TenantId ?? lookup.DeviceTenant(entry.Entity.DeviceId), nameof(TelemetryReading));
    }

    private static bool IsChanged(EntityEntry e) =>
        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;

    private static bool IsAddedOrModified(EntityEntry e) =>
        e.State is EntityState.Added or EntityState.Modified;

    private static void RequireSame(int tenantId, int? ownerTenantId, string message)
    {
        if (ownerTenantId != tenantId) throw new InvalidOperationException(message);
    }

    private static void RequireActive(int? active, int? ownerTenantId, string name)
    {
        if (active is null || ownerTenantId != active)
            throw new InvalidOperationException($"{name} milik tenant lain tidak bisa diubah dari tenant aktif.");
    }

    // Tenant pemilik induk: entitas yang sedang dilacak lebih dulu, lalu database tanpa filter tenant
    // (pemeriksaan ini justru perlu melihat tenant pemilik yang sebenarnya). Hasil di-cache per SaveChanges.
    private sealed class TenantLookup(AppDbContext db)
    {
        private readonly Dictionary<(string, int), int?> _cache = new();

        public int? SiteTenant(int id) => Find(id, db.Sites, s => s.Id == id, s => (int?)s.TenantId);
        public int? PeriodTenant(int id) => Find(id, db.ReportingPeriods, p => p.Id == id, p => (int?)p.TenantId);
        public int? SubmissionTenant(int id) => Find(id, db.Submissions, s => s.Id == id, s => (int?)s.TenantId);
        public int? DeviceTenant(int id) => Find(id, db.TelemetryDevices, d => d.Id == id, d => (int?)d.TenantId);

        private int? Find<T>(int id, DbSet<T> set, Expression<Func<T, bool>> match, Expression<Func<T, int?>> tenantOf)
            where T : class, ITenantOwned
        {
            var key = (typeof(T).Name, id);
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var isMatch = match.Compile();
            var tracked = db.ChangeTracker.Entries<T>().FirstOrDefault(e => e.State != EntityState.Deleted && isMatch(e.Entity));
            var tenantId = tracked is not null
                ? tracked.Entity.TenantId
                : set.IgnoreQueryFilters().Where(match).Select(tenantOf).FirstOrDefault();

            _cache[key] = tenantId;
            return tenantId;
        }
    }
}
