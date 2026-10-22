using GriForms.Domain.Enums;

namespace GriForms.Domain.Entities;

public class QuestionMapping
{
    public int Id { get; set; }
    public int FromFormVersionId { get; set; }
    public string FromKey { get; set; } = "";
    public int ToFormVersionId { get; set; }
    public string ToKey { get; set; } = "";
    public MappingKind Kind { get; set; }
    public string? Note { get; set; }
}
