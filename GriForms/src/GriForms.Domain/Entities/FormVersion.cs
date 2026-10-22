namespace GriForms.Domain.Entities;

public class FormVersion
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public int VersionNo { get; set; }
    public string StandardRef { get; set; } = "";
    public int EffectiveFromYear { get; set; }
    public int? EffectiveToYear { get; set; }
    public string DefinitionJson { get; set; } = "";
    public bool IsPublished { get; set; }

    public Topic Topic { get; set; } = null!;
}
