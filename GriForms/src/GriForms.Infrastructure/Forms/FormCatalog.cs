using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using GriForms.Application.Abstractions;
using GriForms.Application.Forms;
using GriForms.Domain.Entities;
using GriForms.Domain.Enums;
using GriForms.Infrastructure.Persistence;
using GriForms.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GriForms.Infrastructure.Forms;

// Topic, FormVersion, dan QuestionMapping adalah katalog GRI global (tanpa tenant), dikelola PlatformAdmin saja.
// Hak diperiksa ulang ke tabel PlatformAdmin di setiap operasi, tidak hanya di halaman.
// Status versi dibaca tanpa tracking dan perubahan status ditulis dengan ExecuteUpdate bersyarat, sehingga versi
// yang sudah terbit di circuit lain tidak bisa ditimpa dari entity yang usang di DbContext circuit ini.
public sealed class FormCatalog(
    AppDbContext db, ICurrentUser user, IUserAccounts accounts, IClock clock, FormDefinitionValidator validator,
    ILogger<FormCatalog> logger)
    : IFormCatalog
{
    private const int MaxIssuesInMessage = 3;

    // Satu topik GRI jauh di bawah ini; batas pesan SignalR diatur sesuai di Program.cs
    public const int MaxDefinitionLength = 256 * 1024;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        // Definisi disimpan dan ditampilkan di editor, bukan disisipkan ke HTML; teks seperti "&" tetap terbaca
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async Task<IReadOnlyList<TopicView>> GetTopicsAsync(CancellationToken ct)
    {
        await EnsurePlatformAdminAsync(ct);

        return await db.Topics.AsNoTracking()
            .OrderBy(t => t.Code)
            .Select(t => new TopicView(t.Id, t.Code, t.Category, t.Name, db.FormVersions.Count(v => v.TopicId == t.Id)))
            .ToListAsync(ct);
    }

    public async Task<int> CreateTopicAsync(TopicInput input, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var (code, category, name) = ValidTopic(input);
        if (await db.Topics.AnyAsync(t => t.Code == code, ct))
            throw new InvalidOperationException($"Kode topik {code} sudah dipakai.");

        var topic = new Topic { Code = code, Category = category, Name = name };
        db.Topics.Add(topic);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Kode topik {code} sudah dipakai.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }

        using (Scope(actor))
            logger.LogInformation("Topik {TopicCode} dibuat", code);
        return topic.Id;
    }

    public async Task UpdateTopicAsync(int topicId, TopicInput input, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var (code, category, name) = ValidTopic(input);
        var topic = await db.Topics.FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new InvalidOperationException($"Topik {topicId} tidak ditemukan.");

        if (code != topic.Code)
        {
            if (await db.FormVersions.AnyAsync(v => v.TopicId == topicId, ct))
                throw new InvalidOperationException("Kode topik tidak bisa diubah karena topik sudah punya versi form.");
            if (await db.Topics.AnyAsync(t => t.Code == code && t.Id != topicId, ct))
                throw new InvalidOperationException($"Kode topik {code} sudah dipakai.");
        }

        (topic.Code, topic.Category, topic.Name) = (code, category, name);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Kode topik {code} sudah dipakai.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }

        using (Scope(actor))
            logger.LogInformation("Topik {TopicCode} diubah", code);
    }

    public async Task<IReadOnlyList<FormVersionSummary>> GetVersionsAsync(CancellationToken ct)
    {
        await EnsurePlatformAdminAsync(ct);

        var versions = await db.FormVersions.AsNoTracking()
            .OrderBy(v => v.Topic.Code).ThenBy(v => v.VersionNo)
            .Select(v => new VersionRow(v.Id, v.TopicId, v.Topic.Code, v.Topic.Name, v.VersionNo, v.StandardRef,
                v.EffectiveFromYear, v.EffectiveToYear, v.IsPublished, v.RetiredAt))
            .ToListAsync(ct);

        // IgnoreQueryFilters: jumlah submission semua tenant yang memakai versi; hanya dihitung, data tenant tidak dibaca
        var submissions = await db.Submissions.IgnoreQueryFilters().AsNoTracking()
            .GroupBy(s => s.FormVersionId)
            .Select(g => new { VersionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VersionId, x => x.Count, ct);
        var mappings = await db.QuestionMappings.AsNoTracking()
            .Select(m => new { m.FromFormVersionId, m.ToFormVersionId })
            .ToListAsync(ct);

        return versions.Select(v => v.ToSummary(
                mappings.Count(m => m.FromFormVersionId == v.Id || m.ToFormVersionId == v.Id),
                submissions.GetValueOrDefault(v.Id)))
            .ToList();
    }

    public async Task<FormVersionDetail> GetVersionAsync(int versionId, CancellationToken ct)
    {
        await EnsurePlatformAdminAsync(ct);
        var v = await FindVersionAsync(versionId, ct);

        var summary = new VersionRow(v.Id, v.TopicId, v.Topic.Code, v.Topic.Name, v.VersionNo, v.StandardRef,
                v.EffectiveFromYear, v.EffectiveToYear, v.IsPublished, v.RetiredAt)
            .ToSummary(await MappingCountAsync(versionId, ct), await SubmissionCountAsync(versionId, ct));

        // Jejak siklus ditampilkan sebagai email; UserId bila akunnya sudah tidak ada
        var actors = new[] { v.CreatedBy, v.PublishedBy, v.RetiredBy }.OfType<string>().Where(id => id.Length > 0).Distinct().ToList();
        var emails = await accounts.GetEmailsAsync(actors, ct);
        string? Who(string? userId) => userId is null ? null : emails.GetValueOrDefault(userId) ?? userId;

        return new FormVersionDetail(summary, v.DefinitionJson, v.CreatedAt, Who(v.CreatedBy) ?? "",
            v.PublishedAt, Who(v.PublishedBy), v.RetiredAt, Who(v.RetiredBy));
    }

    public async Task<int> CreateDraftAsync(int topicId, int? copyFromVersionId, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var topic = await db.Topics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new InvalidOperationException($"Topik {topicId} tidak ditemukan.");
        var versionNo = (await db.FormVersions.Where(v => v.TopicId == topicId).MaxAsync(v => (int?)v.VersionNo, ct) ?? 0) + 1;

        string json;
        if (copyFromVersionId is { } sourceId)
        {
            var source = await db.FormVersions.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == sourceId && v.TopicId == topicId, ct)
                ?? throw new InvalidOperationException($"Versi sumber {sourceId} tidak ditemukan di topik {topic.Code}.");
            json = WithVersionNo(source.DefinitionJson, versionNo);
        }
        else
        {
            json = Template(topic, versionNo, clock.UtcNow.Year);
        }

        var definition = validator.Check(json, topic.Code, versionNo).Definition
            ?? throw new InvalidOperationException("Definisi sumber tidak bisa dibaca.");
        var version = new FormVersion
        {
            TopicId = topicId, VersionNo = versionNo, DefinitionJson = json,
            StandardRef = StandardRefOf(definition),
            EffectiveFromYear = definition.EffectiveFromYear, EffectiveToYear = definition.EffectiveToYear,
            CreatedAt = clock.UtcNow, CreatedBy = actor,
        };

        db.FormVersions.Add(version);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            throw new InvalidOperationException($"Versi {versionNo} topik {topic.Code} baru saja dibuat di sesi lain. Muat ulang lalu coba lagi.");
        }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
        db.Entry(version).State = EntityState.Detached;   // status selanjutnya selalu dibaca ulang dari database

        using (Scope(actor))
            logger.LogInformation("Draft {TopicCode} v{VersionNo} dibuat (salinan dari {SourceVersionId})",
                topic.Code, versionNo, copyFromVersionId);
        return version.Id;
    }

    public async Task<DefinitionCheck> SaveDraftAsync(int versionId, string definitionJson, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var version = await FindVersionAsync(versionId, ct);
        RequireDraft(version, "Versi terbit tidak bisa diedit. Salin menjadi Draft baru.");
        if (definitionJson is { Length: > MaxDefinitionLength })
            throw new InvalidOperationException($"Definisi terlalu besar (maksimal {MaxDefinitionLength / 1024} KB).");

        var check = validator.Check(definitionJson ?? "", version.Topic.Code, version.VersionNo);
        if (check.Definition is not { } definition)
            throw new InvalidOperationException($"Draft tidak disimpan. {check.Issues[0].Message}");

        var standardRef = StandardRefOf(definition);
        var updated = await db.FormVersions
            .Where(v => v.Id == versionId && !v.IsPublished)
            .ExecuteUpdateAsync(s => s
                .SetProperty(v => v.DefinitionJson, definitionJson)
                .SetProperty(v => v.StandardRef, standardRef)
                .SetProperty(v => v.EffectiveFromYear, definition.EffectiveFromYear)
                .SetProperty(v => v.EffectiveToYear, definition.EffectiveToYear), ct);
        if (updated == 0)
            throw new InvalidOperationException("Versi ini baru saja diterbitkan atau dihapus di sesi lain. Draft tidak disimpan.");

        using (Scope(actor))
            logger.LogInformation("Draft {TopicCode} v{VersionNo} disimpan dengan {IssueCount} masalah validasi",
                version.Topic.Code, version.VersionNo, check.Issues.Count);
        return check;
    }

    public async Task PublishAsync(int versionId, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var version = await FindVersionAsync(versionId, ct);
        RequireDraft(version, "Hanya Draft yang bisa diterbitkan.");

        var check = validator.Check(version.DefinitionJson, version.Topic.Code, version.VersionNo);
        if (!check.IsValid)
            throw new InvalidOperationException($"Definisi belum valid, perbaiki dulu: {Summarize(check.Issues)}");

        // Syarat DefinitionJson sama: yang diterbitkan persis definisi yang baru saja divalidasi
        var json = version.DefinitionJson;
        var now = clock.UtcNow;
        var updated = await db.FormVersions
            .Where(v => v.Id == versionId && !v.IsPublished && v.DefinitionJson == json)
            .ExecuteUpdateAsync(s => s
                .SetProperty(v => v.IsPublished, true)
                .SetProperty(v => v.PublishedAt, now)
                .SetProperty(v => v.PublishedBy, actor), ct);
        if (updated == 0)
            throw new InvalidOperationException("Draft baru saja diubah atau diterbitkan di sesi lain. Muat ulang lalu coba lagi.");

        using (Scope(actor))
            logger.LogInformation("Versi {TopicCode} v{VersionNo} diterbitkan, berlaku {FromYear}-{ToYear}",
                version.Topic.Code, version.VersionNo, version.EffectiveFromYear, version.EffectiveToYear);
    }

    public async Task RetireAsync(int versionId, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var version = await FindVersionAsync(versionId, ct);
        if (version.Status != FormVersionStatus.Published)
            throw new InvalidOperationException("Hanya versi terbit yang bisa dipensiunkan.");

        var now = clock.UtcNow;
        var updated = await db.FormVersions
            .Where(v => v.Id == versionId && v.IsPublished && v.RetiredAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(v => v.RetiredAt, now)
                .SetProperty(v => v.RetiredBy, actor), ct);
        if (updated == 0)
            throw new InvalidOperationException("Versi ini baru saja diubah di sesi lain. Muat ulang lalu coba lagi.");

        using (Scope(actor))
            logger.LogInformation("Versi {TopicCode} v{VersionNo} dipensiunkan", version.Topic.Code, version.VersionNo);
    }

    public async Task DeleteDraftAsync(int versionId, CancellationToken ct)
    {
        var actor = await EnsurePlatformAdminAsync(ct);
        var version = await FindVersionAsync(versionId, ct);
        RequireDraft(version, "Versi terbit tidak bisa dihapus. Pensiunkan bila tidak dipakai lagi.");
        if (await SubmissionCountAsync(versionId, ct) > 0)
            throw new InvalidOperationException("Draft ini dipakai submission dan tidak bisa dihapus.");
        if (await MappingCountAsync(versionId, ct) > 0)
            throw new InvalidOperationException("Draft ini dipakai pemetaan pertanyaan dan tidak bisa dihapus.");

        // FK Submission -> FormVersion cascade, jadi syarat tanpa submission ikut di perintah DELETE-nya.
        // IgnoreQueryFilters: submission tenant mana pun menghalangi penghapusan.
        var deleted = await db.FormVersions
            .IgnoreQueryFilters()
            .Where(v => v.Id == versionId && !v.IsPublished && !db.Submissions.Any(s => s.FormVersionId == v.Id))
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            throw new InvalidOperationException("Draft baru saja diubah di sesi lain. Muat ulang lalu coba lagi.");

        using (Scope(actor))
            logger.LogInformation("Draft {TopicCode} v{VersionNo} dihapus", version.Topic.Code, version.VersionNo);
    }

    private async Task<FormVersion> FindVersionAsync(int versionId, CancellationToken ct) =>
        await db.FormVersions.AsNoTracking().Include(v => v.Topic).FirstOrDefaultAsync(v => v.Id == versionId, ct)
            ?? throw new InvalidOperationException($"Versi form {versionId} tidak ditemukan.");

    // IgnoreQueryFilters: submission semua tenant; hanya dihitung
    private Task<int> SubmissionCountAsync(int versionId, CancellationToken ct) =>
        db.Submissions.IgnoreQueryFilters().CountAsync(s => s.FormVersionId == versionId, ct);

    private Task<int> MappingCountAsync(int versionId, CancellationToken ct) =>
        db.QuestionMappings.CountAsync(m => m.FromFormVersionId == versionId || m.ToFormVersionId == versionId, ct);

    private static void RequireDraft(FormVersion version, string message)
    {
        if (version.Status != FormVersionStatus.Draft) throw new InvalidOperationException(message);
    }

    // Kolom StandardRef dan tahun berlaku adalah indeks query; isinya selalu mengikuti JSON
    private static string StandardRefOf(FormDefinition definition) => definition.StandardRef?.Trim() ?? "";

    private static (string Code, string Category, string Name) ValidTopic(TopicInput? input)
    {
        if (input is null) throw new InvalidOperationException("Data topik wajib diisi.");
        return (TenancyRules.Code(input.Code, "Kode topik"),
            TenancyRules.Name(input.Category, "Kategori topik"),
            TenancyRules.Name(input.Name, "Nama topik"));
    }

    // Hanya versionNo yang diganti; urutan field dan field lain di definisi sumber dipertahankan
    private static string WithVersionNo(string json, int versionNo)
    {
        JsonObject root;
        try
        {
            root = JsonNode.Parse(json) as JsonObject
                ?? throw new InvalidOperationException("Definisi sumber bukan objek JSON.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Definisi sumber tidak bisa dibaca.");
        }

        root["versionNo"] = versionNo;
        return root.ToJsonString(WriteOptions);
    }

    // Template kosong yang langsung lolos validasi, agar pratinjau bisa tampil sejak awal
    private static string Template(Topic topic, int versionNo, int year) => new JsonObject
    {
        ["formCode"] = topic.Code,
        ["title"] = $"{topic.Code} {topic.Name}",
        ["standardRef"] = $"GRI {topic.Code}",
        ["versionNo"] = versionNo,
        ["effectiveFromYear"] = year,
        ["effectiveToYear"] = null,
        ["disclosures"] = new JsonArray(new JsonObject
        {
            ["no"] = $"{topic.Code}-1",
            ["title"] = "Judul disclosure",
            ["questions"] = new JsonArray(new JsonObject
            {
                ["key"] = $"{topic.Code}-1-A",
                ["type"] = "text",
                ["label"] = "Teks pertanyaan",
                ["required"] = false,
            }),
        }),
    }.ToJsonString(WriteOptions);

    private static string Summarize(IReadOnlyList<DefinitionIssue> issues)
    {
        var shown = string.Join("; ", issues.Take(MaxIssuesInMessage).Select(i => $"{i.Path}: {i.Message}"));
        return issues.Count > MaxIssuesInMessage ? $"{shown} (+{issues.Count - MaxIssuesInMessage} lainnya)" : shown;
    }

    private async Task<string> EnsurePlatformAdminAsync(CancellationToken ct)
    {
        var userId = user.UserId;
        if (userId is null || !await db.PlatformAdmins.AnyAsync(p => p.UserId == userId, ct))
            throw new UnauthorizedAccessException("Hanya PlatformAdmin yang boleh mengelola definisi form.");
        return userId;
    }

    private IDisposable? Scope(string userId) =>
        logger.BeginScope(new Dictionary<string, object?> { ["UserId"] = userId });

    private sealed record VersionRow(
        int Id, int TopicId, string TopicCode, string TopicName, int VersionNo, string StandardRef,
        int EffectiveFromYear, int? EffectiveToYear, bool IsPublished, DateTime? RetiredAt)
    {
        public FormVersionSummary ToSummary(int mappingCount, int submissionCount) => new(
            Id, TopicId, TopicCode, TopicName, VersionNo, StandardRef, EffectiveFromYear, EffectiveToYear,
            FormVersion.StatusOf(IsPublished, RetiredAt),
            mappingCount, submissionCount);
    }
}
