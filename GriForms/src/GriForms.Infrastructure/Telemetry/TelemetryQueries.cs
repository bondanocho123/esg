using GriForms.Application.Abstractions;
using GriForms.Application.Telemetry;
using GriForms.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GriForms.Infrastructure.Telemetry;

public sealed class TelemetryQueries(IAppReadDb db) : ITelemetryQueries
{
    public async Task<TelemetryAccess> GetAccessAsync(string userId, CancellationToken ct)
    {
        var roles = await db.UserSiteRoles.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => new { r.Role, r.SiteId })
            .ToListAsync(ct);

        var isAdmin = roles.Any(r => r.Role == RoleName.Admin);

        // Peran tanpa site (mis. Admin) berlaku untuk semua site
        if (roles.Any(r => r.SiteId is null)) return new TelemetryAccess(isAdmin, null);

        return new TelemetryAccess(isAdmin, roles.Select(r => r.SiteId!.Value).Distinct().Order().ToList());
    }

    public async Task<IReadOnlyList<SiteOption>> GetSitesAsync(TelemetryAccess access, CancellationToken ct)
    {
        var sites = db.Sites.AsNoTracking().Where(s => s.IsActive);
        if (access.SiteIds is { } siteIds)
            sites = sites.Where(s => siteIds.Contains(s.Id));

        return await sites.OrderBy(s => s.Id).Select(s => new SiteOption(s.Id, s.Name)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DeviceSnapshot>> GetDevicesAsync(TelemetryAccess access, CancellationToken ct)
    {
        var devices = db.TelemetryDevices.AsNoTracking();
        if (access.SiteIds is { } siteIds)
            devices = devices.Where(d => siteIds.Contains(d.SiteId));

        var list = await devices
            .OrderBy(d => d.SiteId).ThenBy(d => d.Code)
            .Select(d => new { d.Id, d.Code, d.Name, d.SiteId, SiteName = d.Site.Name, d.IsActive, d.LastSeenAt })
            .ToListAsync(ct);

        var ids = list.Select(d => d.Id).ToList();

        // Nilai terakhir per (device, metrik), memakai index (DeviceId, Metric, RecordedAt)
        var latest = await db.TelemetryReadings.AsNoTracking()
            .Where(r => ids.Contains(r.DeviceId)
                     && r.Id == db.TelemetryReadings
                            .Where(x => x.DeviceId == r.DeviceId && x.Metric == r.Metric)
                            .OrderByDescending(x => x.RecordedAt).ThenByDescending(x => x.Id)
                            .Select(x => x.Id)
                            .FirstOrDefault())
            .Select(r => new { r.DeviceId, Reading = new LatestReading(r.Metric, r.Value, r.Unit, r.RecordedAt) })
            .ToListAsync(ct);

        var byDevice = latest.ToLookup(x => x.DeviceId, x => x.Reading);

        return list.Select(d => new DeviceSnapshot(
                d.Id, d.Code, d.Name, d.SiteId, d.SiteName, d.IsActive, d.LastSeenAt,
                byDevice[d.Id].OrderBy(r => r.Metric).ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<TelemetryPoint>> GetSeriesAsync(TelemetryAccess access, int deviceId, DateTime fromUtc, CancellationToken ct)
    {
        var readings = db.TelemetryReadings.AsNoTracking().Where(r => r.DeviceId == deviceId && r.RecordedAt >= fromUtc);
        if (access.SiteIds is { } siteIds)
            readings = readings.Where(r => siteIds.Contains(r.Device.SiteId));

        return await readings
            .OrderBy(r => r.RecordedAt)
            .Select(r => new TelemetryPoint(r.Metric, r.RecordedAt, r.Value))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MonthlyAggregate>> GetMonthlyAggregateAsync(int deviceId, string metric, int year, CancellationToken ct)
    {
        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddYears(1);

        var rows = await db.TelemetryReadings.AsNoTracking()
            .Where(r => r.DeviceId == deviceId && r.Metric == metric && r.RecordedAt >= from && r.RecordedAt < to)
            .GroupBy(r => r.RecordedAt.Month)
            .Select(g => new
            {
                Month = g.Key,
                Avg = g.Average(r => r.Value),
                Min = g.Min(r => r.Value),
                Max = g.Max(r => r.Value),
                Count = g.Count(),
            })
            .ToListAsync(ct);

        return rows
            .OrderBy(r => r.Month)
            .Select(r => new MonthlyAggregate($"{year}-{r.Month:00}", r.Avg, r.Min, r.Max, r.Count))
            .ToList();
    }
}
