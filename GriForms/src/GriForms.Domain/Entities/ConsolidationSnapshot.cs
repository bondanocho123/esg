namespace GriForms.Domain.Entities;

public class ConsolidationSnapshot
{
    public int Id { get; set; }
    public int PeriodId { get; set; }
    public int TopicId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public string PayloadJson { get; set; } = "";
}
