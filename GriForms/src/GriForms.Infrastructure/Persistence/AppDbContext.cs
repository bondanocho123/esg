using GriForms.Application.Abstractions;
using GriForms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppReadDb, IUnitOfWork
{
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

    IQueryable<Site> IAppReadDb.Sites => Sites;
    IQueryable<Submission> IAppReadDb.Submissions => Submissions;
    IQueryable<Answer> IAppReadDb.Answers => Answers;
    IQueryable<FormVersion> IAppReadDb.FormVersions => FormVersions;
    IQueryable<UserSiteRole> IAppReadDb.UserSiteRoles => UserSiteRoles;
    IQueryable<TelemetryDevice> IAppReadDb.TelemetryDevices => TelemetryDevices;
    IQueryable<TelemetryReading> IAppReadDb.TelemetryReadings => TelemetryReadings;

    void IUnitOfWork.Add<T>(T entity) => Add(entity);
    void IUnitOfWork.Remove<T>(T entity) => Remove(entity);

    protected override void OnModelCreating(ModelBuilder b)
    {
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

        b.Entity<ReportingPeriod>().HasIndex(p => p.Year).IsUnique();
        b.Entity<Topic>().HasIndex(t => t.Code).IsUnique();

        // Telemetry: Value double (REAL) agar agregasi bisa di SQL, berbeda dengan decimal di Answer
        b.Entity<TelemetryDevice>().HasIndex(d => d.Code).IsUnique();
        b.Entity<TelemetryDevice>().HasIndex(d => d.ApiKeyHash).IsUnique();
        b.Entity<TelemetryReading>().HasIndex(r => new { r.DeviceId, r.Metric, r.RecordedAt });

        // Data referensi di-seed: 6 site
        b.Entity<Site>().HasData(
            new Site { Id = 1, Name = "Head Office" },
            new Site { Id = 2, Name = "Bakan Mine" },
            new Site { Id = 3, Name = "Penjom Mine" },
            new Site { Id = 4, Name = "Seruyung Mine" },
            new Site { Id = 5, Name = "North Lanut Mine" },
            new Site { Id = 6, Name = "Doup Mine" });
    }
}
