using GriForms.Domain.Enums;

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

    // Jejak siklus versi. Katalog global tidak punya tenant, jadi tidak dicatat di AuditEntry (milik tenant).
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    public DateTime? RetiredAt { get; set; }
    public string? RetiredBy { get; set; }

    public Topic Topic { get; set; } = null!;

    // Tanpa setter, jadi tidak dipetakan EF; query memakai IsPublished dan RetiredAt
    public FormVersionStatus Status => StatusOf(IsPublished, RetiredAt);

    public static FormVersionStatus StatusOf(bool isPublished, DateTime? retiredAt) =>
        !isPublished ? FormVersionStatus.Draft
        : retiredAt is null ? FormVersionStatus.Published
        : FormVersionStatus.Retired;
}
