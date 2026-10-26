using GriForms.Domain.Enums;

namespace GriForms.Application.Forms;

// SubmissionCount dihitung lintas tenant: versi yang sudah dipakai tidak boleh dihapus
public sealed record FormVersionSummary(
    int Id, int TopicId, string TopicCode, string TopicName, int VersionNo, string StandardRef,
    int EffectiveFromYear, int? EffectiveToYear, FormVersionStatus Status, int MappingCount, int SubmissionCount)
{
    public bool IsPublished => Status != FormVersionStatus.Draft;
}

// CreatedBy, PublishedBy, RetiredBy berisi email pelaku (UserId bila akunnya tidak ditemukan)
public sealed record FormVersionDetail(
    FormVersionSummary Summary, string DefinitionJson,
    DateTime CreatedAt, string CreatedBy,
    DateTime? PublishedAt, string? PublishedBy,
    DateTime? RetiredAt, string? RetiredBy);

public sealed record TopicView(int Id, string Code, string Category, string Name, int VersionCount);

public sealed record TopicInput(string Code, string Category, string Name);
