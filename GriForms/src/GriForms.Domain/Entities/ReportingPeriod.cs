using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class ReportingPeriod : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int Year { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;
}
