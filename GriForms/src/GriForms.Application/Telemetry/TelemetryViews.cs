namespace GriForms.Application.Telemetry;

// Site yang boleh dilihat pengguna, dari UserSiteRole. SiteIds null berarti semua site.
public sealed record TelemetryAccess(bool IsAdmin, IReadOnlyList<int>? SiteIds)
{
    public static readonly TelemetryAccess None = new(false, []);
}

public sealed record SiteOption(int Id, string Name);

public sealed record LatestReading(string Metric, double Value, string Unit, DateTime RecordedAt);

public sealed record DeviceSnapshot(
    int DeviceId, string Code, string Name, int SiteId, string SiteName,
    bool IsActive, DateTime? LastSeenAt, IReadOnlyList<LatestReading> Latest)
{
    // Device dianggap offline bila tidak mengirim lebih dari 3 menit (normalnya tiap 5 detik)
    public static readonly TimeSpan OfflineAfter = TimeSpan.FromMinutes(3);

    public bool IsOnline(DateTime utcNow) => LastSeenAt is { } seen && utcNow - seen <= OfflineAfter;
}

public sealed record TelemetryPoint(string Metric, DateTime RecordedAt, double Value);

// PeriodKey berformat yyyy-MM, sama dengan MonthlyTableField
public sealed record MonthlyAggregate(string PeriodKey, double Avg, double Min, double Max, int Count);
