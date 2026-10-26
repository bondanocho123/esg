using GriForms.Application.Forms;

namespace GriForms.Application.Abstractions;

// Katalog definisi form GRI (Topic, FormVersion, QuestionMapping): global, dikelola PlatformAdmin saja.
// Selain PlatformAdmin: UnauthorizedAccessException. Input tidak valid atau status versi tidak cocok: InvalidOperationException.
// Siklus versi (bab 7): Draft -> Published (terkunci) -> Retired. Perubahan isi selalu lewat Draft baru.
public interface IFormCatalog
{
    Task<IReadOnlyList<TopicView>> GetTopicsAsync(CancellationToken ct);

    Task<int> CreateTopicAsync(TopicInput input, CancellationToken ct);

    // Code hanya bisa diubah selama topik belum punya versi, karena formCode di definisi harus sama dengan Code
    Task UpdateTopicAsync(int topicId, TopicInput input, CancellationToken ct);

    Task<IReadOnlyList<FormVersionSummary>> GetVersionsAsync(CancellationToken ct);

    Task<FormVersionDetail> GetVersionAsync(int versionId, CancellationToken ct);

    // copyFromVersionId null: Draft dari template kosong. VersionNo selalu nomor tertinggi topik + 1.
    Task<int> CreateDraftAsync(int topicId, int? copyFromVersionId, CancellationToken ct);

    // Hanya Draft. JSON yang tidak bisa dibaca ditolak; masalah validasi lain dikembalikan dan baru memblokir publish.
    Task<DefinitionCheck> SaveDraftAsync(int versionId, string definitionJson, CancellationToken ct);

    Task PublishAsync(int versionId, CancellationToken ct);

    // Versi Retired tidak dipilih untuk submission baru; submission lama tetap membacanya
    Task RetireAsync(int versionId, CancellationToken ct);

    // Hanya Draft yang belum dipakai submission atau pemetaan
    Task DeleteDraftAsync(int versionId, CancellationToken ct);
}
