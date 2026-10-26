using GriForms.Application.Abstractions;
using GriForms.Application.Submissions;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Submissions;

// Halaman Periode pelaporan untuk Admin tenant aktif; periode tenant lain tidak terlihat (filter tenant)
public sealed class ReportingPeriods(
    AppDbContext db, ICurrentUser user, ITenantContext tenant, IClock clock, ILogger<ReportingPeriods> logger)
    : IReportingPeriods
{
    private const int MinYear = 2000;
    private const int MaxYear = 2100;

    public async Task<IReadOnlyList<ReportingPeriodView>> GetPeriodsAsync(CancellationToken ct)
    {
        await EnsureTenantAdminAsync(ct);
        return await ReportingWorkspace.PeriodViewsAsync(db, ct);
    }

    public async Task<int> CreatePeriodAsync(int year, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        if (year is < MinYear or > MaxYear)
            throw new InvalidOperationException($"Tahun harus antara {MinYear} dan {MaxYear}.");
        if (await db.ReportingPeriods.AnyAsync(p => p.Year == year, ct))
            throw new InvalidOperationException($"Periode {year} sudah ada.");

        var period = new ReportingPeriod { TenantId = tenantId, Year = year, Status = PeriodStatus.Open };
        try
        {
            // Satu transaksi: periode tidak boleh tersimpan tanpa jejak audit (Id periode dibutuhkan audit)
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            db.ReportingPeriods.Add(period);
            await db.SaveChangesAsync(ct);
            db.AuditEntries.Add(TenancyRules.Audit(tenantId, nameof(ReportingPeriod), period.Id.ToString(), "Created",
                null, $"{year}: {PeriodStatus.Open}", actor, clock.UtcNow));
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Periode {year} sudah ada.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
        db.Entry(period).State = EntityState.Detached;

        using (Scope(tenantId, actor))
            logger.LogInformation("Periode pelaporan {Year} dibuat", year);
        return period.Id;
    }

    public async Task SetStatusAsync(int periodId, PeriodStatus status, CancellationToken ct)
    {
        var (actor, tenantId) = await EnsureTenantAdminAsync(ct);
        if (!Enum.IsDefined(status)) throw new InvalidOperationException("Status periode tidak dikenal.");
        var period = await db.ReportingPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct)
            ?? throw new InvalidOperationException($"Periode {periodId} tidak ditemukan.");
        if (period.Status == status) return;

        var old = period.Status;
        period.Status = status;
        db.AuditEntries.Add(TenancyRules.Audit(tenantId, nameof(ReportingPeriod), period.Id.ToString(),
            status == PeriodStatus.Closed ? "Closed" : "Reopened", old.ToString(), status.ToString(), actor, clock.UtcNow));
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }

        using (Scope(tenantId, actor))
            logger.LogInformation("Periode pelaporan {Year} {Status}", period.Year, status == PeriodStatus.Closed ? "ditutup" : "dibuka kembali");
    }

    private async Task<(string UserId, int TenantId)> EnsureTenantAdminAsync(CancellationToken ct)
    {
        var userId = user.UserId;
        // db.UserSiteRoles sudah dibatasi ke tenant aktif: Admin tenant lain tidak lolos
        if (userId is null || tenant.TenantId is not { } tenantId
            || !await db.UserSiteRoles.AnyAsync(r => r.UserId == userId && r.Role == RoleName.Admin && r.SiteId == null, ct))
            throw new UnauthorizedAccessException("Hanya Admin organisasi aktif yang boleh mengelola periode pelaporan.");
        return (userId, tenantId);
    }

    private IDisposable? Scope(int tenantId, string userId) =>
        logger.BeginScope(new Dictionary<string, object?> { ["TenantId"] = tenantId, ["UserId"] = userId });
}
