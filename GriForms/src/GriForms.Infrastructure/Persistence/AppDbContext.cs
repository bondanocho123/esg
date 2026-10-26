using GriForms.Application.Abstractions;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenant)
    : DbContext(options), IAppReadDb, IUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<ReportingPeriod> ReportingPeriods => Set<ReportingPeriod>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<FormVersion> FormVersions => Set<FormVersion>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Signoff> Signoffs => Set<Signoff>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<QuestionMapping> QuestionMappings => Set<QuestionMapping>();
    public DbSet<ConsolidationSnapshot> ConsolidationSnapshots => Set<ConsolidationSnapshot>();
    public DbSet<UserSiteRole> UserSiteRoles => Set<UserSiteRole>();
    public DbSet<TelemetryDevice> TelemetryDevices => Set<TelemetryDevice>();
    public DbSet<TelemetryReading> TelemetryReadings => Set<TelemetryReading>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();

    IQueryable<Site> IAppReadDb.Sites => Sites;
    IQueryable<Submission> IAppReadDb.Submissions => Submissions;
    IQueryable<Answer> IAppReadDb.Answers => Answers;
    IQueryable<FormVersion> IAppReadDb.FormVersions => FormVersions;
    IQueryable<UserSiteRole> IAppReadDb.UserSiteRoles => UserSiteRoles;
    IQueryable<TelemetryDevice> IAppReadDb.TelemetryDevices => TelemetryDevices;
    IQueryable<TelemetryReading> IAppReadDb.TelemetryReadings => TelemetryReadings;

    void IUnitOfWork.Add<T>(T entity) => Add(entity);
    void IUnitOfWork.Remove<T>(T entity) => Remove(entity);

    // Dibaca filter global setiap query dijalankan. 0 bukan Id tenant yang sah, jadi tenant aktif kosong
    // menghasilkan nol baris (fail closed), bukan semua data.
    private int ActiveTenantId => tenant.TenantId ?? 0;

    internal ITenantContext CurrentTenant => tenant;

    // Diaktifkan sementara oleh SaveChangesAcrossTenantsAsync, lihat TenantGuardInterceptor
    internal bool CrossTenantWrite { get; private set; }

    // Jalur sah yang menulis ke tenant selain tenant aktif (ingest telemetry, bootstrap, halaman Organisasi).
    // TenantId wajib diisi eksplisit; konsistensi Submission/TelemetryDevice/UserSiteRole dengan site-nya tetap diperiksa.
    internal async Task<int> SaveChangesAcrossTenantsAsync(CancellationToken ct)
    {
        CrossTenantWrite = true;
        try
        {
            return await SaveChangesAsync(ct);
        }
        finally
        {
            CrossTenantWrite = false;
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // Dipasang di sini, bukan di DI, agar selalu aktif (termasuk di tes) dan berjalan setelah AuditInterceptor
        => optionsBuilder.AddInterceptors(TenantGuardInterceptor.Instance);

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Tenant>().HasIndex(t => t.Code).IsUnique();
        b.Entity<Tenant>().HasData(
            new Tenant { Id = Tenant.LegacyId, Code = "ZRES", Name = "Z Resource", IsActive = true });

        b.Entity<UserProfile>().HasKey(p => p.UserId);
        b.Entity<UserProfile>().HasOne<Tenant>().WithMany().HasForeignKey(p => p.ActiveTenantId);
        b.Entity<PlatformAdmin>().HasKey(p => p.UserId);

        // Entitas akar: TenantId wajib dengan FK ke Tenant, dan filter global per tenant aktif
        HasTenant<Site>(b);
        HasTenant<ReportingPeriod>(b);
        HasTenant<Submission>(b);
        HasTenant<UserSiteRole>(b);
        HasTenant<AuditEntry>(b);
        HasTenant<ConsolidationSnapshot>(b);
        HasTenant<TelemetryDevice>(b);
        b.Entity<Site>().HasQueryFilter(e => e.TenantId == ActiveTenantId);
        b.Entity<ReportingPeriod>().HasQueryFilter(e => e.TenantId == ActiveTenantId);
        b.Entity<Submission>().HasQueryFilter(e => e.TenantId == ActiveTenantId);
        b.Entity<UserSiteRole>().HasQueryFilter(e => e.TenantId == ActiveTenantId);
        b.Entity<AuditEntry>().HasQueryFilter(e => e.TenantId == ActiveTenantId);
        b.Entity<ConsolidationSnapshot>().HasQueryFilter(e => e.TenantId == ActiveTenantId);
        b.Entity<TelemetryDevice>().HasQueryFilter(e => e.TenantId == ActiveTenantId);

        // Entitas turunan difilter lewat induknya
        b.Entity<Answer>().HasQueryFilter(a => a.Submission.TenantId == ActiveTenantId);
        b.Entity<Signoff>().HasQueryFilter(s => s.Submission.TenantId == ActiveTenantId);
        b.Entity<TelemetryReading>().HasQueryFilter(r => r.Device.TenantId == ActiveTenantId);

        b.Entity<Submission>()
            .HasIndex(s => new { s.SiteId, s.PeriodId, s.TopicId })
            .IsUnique();
        b.Entity<Submission>().Property(s => s.RowVersion).IsConcurrencyToken();

        b.Entity<Answer>()
            .HasIndex(a => new { a.SubmissionId, a.QuestionKey, a.PeriodKey })
            .IsUnique();
        b.Entity<Answer>().Property(a => a.RowVersion).IsConcurrencyToken();

        // SQLite menyimpan decimal sebagai TEXT; total dihitung di memori (lihat catatan SQLite di bab 2)
        b.Entity<FormVersion>().Property(f => f.DefinitionJson).IsRequired();
        b.Entity<FormVersion>().HasIndex(f => new { f.TopicId, f.VersionNo }).IsUnique();

        b.Entity<Site>().HasIndex(s => new { s.TenantId, s.Code }).IsUnique();
        b.Entity<ReportingPeriod>().HasIndex(p => new { p.TenantId, p.Year }).IsUnique();
        b.Entity<Topic>().HasIndex(t => t.Code).IsUnique();

        // Telemetry: Value double (REAL) agar agregasi bisa di SQL, berbeda dengan decimal di Answer.
        // Code unik per tenant; ApiKeyHash unik global karena ingest mengenali device (dan tenant-nya) dari key saja.
        b.Entity<TelemetryDevice>().HasIndex(d => new { d.TenantId, d.Code }).IsUnique();
        b.Entity<TelemetryDevice>().HasIndex(d => d.ApiKeyHash).IsUnique();
        b.Entity<TelemetryReading>().HasIndex(r => new { r.DeviceId, r.Metric, r.RecordedAt });

        // Data referensi di-seed: 6 site Z Resource. Code = Site_ID lama agar file Excel lama tetap bisa diimport.
        b.Entity<Site>().HasData(
            new Site { Id = 1, TenantId = Tenant.LegacyId, Code = "1", Name = "Head Office" },
            new Site { Id = 2, TenantId = Tenant.LegacyId, Code = "2", Name = "Bakan Mine" },
            new Site { Id = 3, TenantId = Tenant.LegacyId, Code = "3", Name = "Penjom Mine" },
            new Site { Id = 4, TenantId = Tenant.LegacyId, Code = "4", Name = "Seruyung Mine" },
            new Site { Id = 5, TenantId = Tenant.LegacyId, Code = "5", Name = "North Lanut Mine" },
            new Site { Id = 6, TenantId = Tenant.LegacyId, Code = "6", Name = "Doup Mine" });
    }

    // Restrict: tenant tidak pernah dihapus beserta datanya, cukup dinonaktifkan
    private static void HasTenant<T>(ModelBuilder b) where T : class, ITenantOwned
        => b.Entity<T>().HasOne<Tenant>().WithMany()
            .HasForeignKey(nameof(ITenantOwned.TenantId))
            .OnDelete(DeleteBehavior.Restrict);
}
