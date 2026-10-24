namespace GriForms.Application.Forms;

public sealed record FormVersionSummary(
    int Id, string TopicCode, string TopicName, int VersionNo, string StandardRef,
    int EffectiveFromYear, int? EffectiveToYear, bool IsPublished, int MappingCount);
