using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class ReportingPeriod
{
    public int Id { get; set; }
    public int Year { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;
}
