using System.Text.Json;
using System.Text.RegularExpressions;
using GriForms.Application.Calculations;
using GriForms.Domain.Enums;

namespace GriForms.Application.Forms;

// Path memakai nama properti JSON, mis. "disclosures[0].questions[2].key"
public sealed record DefinitionIssue(string Path, string Message);

// Definition null bila JSON tidak bisa dibaca sama sekali
public sealed record DefinitionCheck(FormDefinition? Definition, IReadOnlyList<DefinitionIssue> Issues)
{
    public bool IsValid => Definition is not null && Issues.Count == 0;
}

// Validator definisi sebelum publish (bab 6): key unik, tipe dikenal, opsi tidak kosong, VisibleWhen dan CalcRef dikenal.
// Draft boleh disimpan meski masih ada masalah; publish ditolak sampai Issues kosong.
public sealed partial class FormDefinitionValidator(IFormDefinitionReader reader, CalcRegistry calcs)
{
    private const int MinYear = 2000;
    private const int MaxYear = 2100;
    private static readonly string[] ValueTypes = ["number", "text"];

    // Cara bacaan sensor diringkas per periode form
    public static readonly string[] Aggregations = ["sum", "avg", "min", "max", "last"];

    // Tanpa '/' dan '|': keduanya pemisah kunci sel (FormState.CellKey, kunci baris/kolom tabel)
    [GeneratedRegex(@"^[\w\-.]+$")]
    private static partial Regex KeyPattern();

    [GeneratedRegex(@"^[a-z][a-z0-9_]*$")]
    private static partial Regex MetricPattern();

    public DefinitionCheck Check(string json, string topicCode, int versionNo)
    {
        FormDefinition definition;
        try
        {
            definition = reader.Read(json);
        }
        catch (JsonException ex)
        {
            var where = ex.LineNumber is { } line ? $"baris {line + 1}" : "JSON";
            var path = ex.Path is { Length: > 2 } p && p.StartsWith("$.") ? p[2..] : "$";
            return new DefinitionCheck(null, [new DefinitionIssue(path, $"JSON tidak valid ({where}): {Describe(ex)}")]);
        }
        catch (InvalidOperationException ex)
        {
            return new DefinitionCheck(null, [new DefinitionIssue("$", ex.Message)]);
        }

        var issues = new List<DefinitionIssue>();
        CheckHeader(definition, topicCode, versionNo, issues);
        CheckDisclosures(definition, issues);
        return new DefinitionCheck(definition, issues);
    }

    private static void CheckHeader(FormDefinition d, string topicCode, int versionNo, List<DefinitionIssue> issues)
    {
        if (!string.Equals(d.FormCode?.Trim(), topicCode, StringComparison.Ordinal))
            issues.Add(new("formCode", $"formCode harus sama dengan kode topik ({topicCode})."));
        if (string.IsNullOrWhiteSpace(d.Title))
            issues.Add(new("title", "title wajib diisi."));
        if (d.VersionNo != versionNo)
            issues.Add(new("versionNo", $"versionNo harus {versionNo} (nomor versi ini)."));
        if (d.EffectiveFromYear is < MinYear or > MaxYear)
            issues.Add(new("effectiveFromYear", $"effectiveFromYear harus antara {MinYear} dan {MaxYear}."));
        if (d.EffectiveToYear is { } to && to < d.EffectiveFromYear)
            issues.Add(new("effectiveToYear", "effectiveToYear tidak boleh lebih kecil dari effectiveFromYear."));
    }

    private void CheckDisclosures(FormDefinition d, List<DefinitionIssue> issues)
    {
        var disclosures = d.Disclosures ?? [];
        if (disclosures.Count == 0)
        {
            issues.Add(new("disclosures", "Minimal satu disclosure."));
            return;
        }

        // Kunci yang dirujuk VisibleWhen diperiksa setelah semua key dikumpulkan
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var visibleRefs = new List<(string Path, string Key)>();
        var numbers = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < disclosures.Count; i++)
        {
            var disclosure = disclosures[i];
            var path = $"disclosures[{i}]";
            if (disclosure is null)
            {
                issues.Add(new(path, "Disclosure kosong."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(disclosure.No))
                issues.Add(new($"{path}.no", "no wajib diisi."));
            else if (!numbers.Add(disclosure.No.Trim()))
                issues.Add(new($"{path}.no", $"Nomor disclosure {disclosure.No} dipakai lebih dari sekali."));
            if (string.IsNullOrWhiteSpace(disclosure.Title))
                issues.Add(new($"{path}.title", "title wajib diisi."));

            var questions = disclosure.Questions ?? [];
            if (questions.Count == 0)
                issues.Add(new($"{path}.questions", "Minimal satu pertanyaan."));

            for (var j = 0; j < questions.Count; j++)
            {
                var q = questions[j];
                var qPath = $"{path}.questions[{j}]";
                if (q is null)
                {
                    issues.Add(new(qPath, "Pertanyaan kosong."));
                    continue;
                }
                CheckQuestion(q, qPath, keys, issues);
                if (!string.IsNullOrWhiteSpace(q.VisibleWhen))
                {
                    if (VisibleRules.TryParse(q.VisibleWhen, out var refKey))
                        visibleRefs.Add(($"{qPath}.visibleWhen", refKey));
                    else
                        issues.Add(new($"{qPath}.visibleWhen", "Format visibleWhen harus kunci == 'nilai' atau kunci != 'nilai'."));
                }
            }
        }

        // Kunci sel tabel (kunci/baris/kolom) juga boleh dirujuk; yang diperiksa kunci pertanyaannya
        foreach (var (path, key) in visibleRefs)
            if (!keys.Contains(key.Split('/')[0]))
                issues.Add(new(path, $"visibleWhen merujuk key {key} yang tidak ada di form ini."));
    }

    private void CheckQuestion(QuestionDef q, string path, HashSet<string> keys, List<DefinitionIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(q.Key))
            issues.Add(new($"{path}.key", "key wajib diisi."));
        else if (!KeyPattern().IsMatch(q.Key))
            issues.Add(new($"{path}.key", $"key {q.Key} hanya boleh huruf, angka, '_', '-' atau '.'."));
        else if (!keys.Add(q.Key))
            issues.Add(new($"{path}.key", $"key {q.Key} dipakai lebih dari sekali."));

        if (!Enum.IsDefined(q.Type))
            issues.Add(new($"{path}.type", "type tidak dikenal."));
        if (string.IsNullOrWhiteSpace(q.Label))
            issues.Add(new($"{path}.label", "label wajib diisi."));
        CheckValueType(q.ValueType, $"{path}.valueType", issues);

        switch (q.Type)
        {
            case InputType.Select:
                var options = q.Options ?? [];
                if (options.Count == 0)
                    issues.Add(new($"{path}.options", "Pertanyaan select wajib punya minimal satu opsi."));
                CheckUnique(options.Select(o => o?.Value), $"{path}.options", "value", "Nilai opsi", requirePattern: false, issues);
                break;

            case InputType.MonthlyTable:
                if ((q.Rows ?? []).Count == 0 && !q.AllowCustomRows)
                    issues.Add(new($"{path}.rows", "monthlyTable wajib punya baris, atau aktifkan allowCustomRows."));
                CheckUnique((q.Rows ?? []).Select(r => r?.Key), $"{path}.rows", "key", "Kunci baris", requirePattern: true, issues);
                break;

            case InputType.RowsTable:
                var columns = q.Columns ?? [];
                if (columns.Count == 0)
                    issues.Add(new($"{path}.columns", "rowsTable wajib punya minimal satu kolom."));
                CheckUnique(columns.Select(c => c?.Key), $"{path}.columns", "key", "Kunci kolom", requirePattern: true, issues);
                for (var c = 0; c < columns.Count; c++)
                    CheckValueType(columns[c]?.ValueType, $"{path}.columns[{c}].valueType", issues);
                CheckUnique((q.Rows ?? []).Select(r => r?.Key), $"{path}.rows", "key", "Kunci baris", requirePattern: true, issues);
                break;
        }

        if (q.Validation is { } v)
        {
            if (v.Min is { } min && v.Max is { } max && min > max)
                issues.Add(new($"{path}.validation", "min tidak boleh lebih besar dari max."));
            if (v.MaxLength is <= 0)
                issues.Add(new($"{path}.validation.maxLength", "maxLength harus lebih dari 0."));
        }

        if (!string.IsNullOrWhiteSpace(q.CalcRef) && !calcs.Contains(q.CalcRef))
            issues.Add(new($"{path}.calcRef", $"Kalkulasi {q.CalcRef} tidak terdaftar."));

        CheckTelemetry(q, path, issues);
    }

    // Pertanyaan numerik wajib diklasifikasikan kelayakan telemetry-nya agar rencana otomasi tidak terlupa.
    // rowsTable dianggap numerik bila punya minimal satu kolom angka; tabel teks murni tidak bisa diotomasi.
    public static bool NeedsTelemetry(QuestionDef q) => q.Type switch
    {
        InputType.Number or InputType.MonthlyTable => true,
        InputType.RowsTable => (q.Columns ?? []).Any(c => c?.ValueType == "number"),
        _ => false,
    };

    private static void CheckTelemetry(QuestionDef q, string path, List<DefinitionIssue> issues)
    {
        var tPath = $"{path}.telemetry";
        var rows = q.Rows ?? [];
        if (q.Telemetry is null)
        {
            if (NeedsTelemetry(q))
                issues.Add(new(tPath, "Pertanyaan numerik wajib punya telemetry.feasibility: manual, measured, atau derived."));
        }
        else
        {
            var t = q.Telemetry;
            if (t.Feasibility is not { } feasibility || !Enum.IsDefined(feasibility))
                issues.Add(new($"{tPath}.feasibility", "feasibility wajib diisi: manual, measured, atau derived."));
            else if (feasibility == TelemetryFeasibility.Measured)
            {
                // Metrik boleh ditetapkan per baris; kalau tidak ada di pertanyaan, setiap baris tetap wajib punya
                if (string.IsNullOrWhiteSpace(t.Metric))
                {
                    if (q.AllowCustomRows)
                        issues.Add(new($"{tPath}.metric", "Baris bebas (allowCustomRows) tidak bisa dipetakan ke sensor; tetapkan metric di pertanyaan."));
                    else if (rows.Count == 0)
                        issues.Add(new($"{tPath}.metric", "metric wajib diisi untuk feasibility measured."));
                    else
                        for (var r = 0; r < rows.Count; r++)
                            if (IsMeasuredRow(rows[r]) && string.IsNullOrWhiteSpace(rows[r]?.Telemetry?.Metric))
                                issues.Add(new($"{path}.rows[{r}].telemetry.metric", "metric wajib diisi di pertanyaan atau di setiap baris untuk feasibility measured."));
                }
                CheckMeasuredFields(t, tPath, requireMetric: false, issues);
                // Agregasi pertanyaan boleh kosong hanya bila setiap baris menetapkan sendiri (atau barisnya bukan measured)
                if (string.IsNullOrWhiteSpace(t.Aggregation) && !(rows.Count > 0 && rows.All(RowHasOwnAggregation)))
                    issues.Add(new($"{tPath}.aggregation", $"aggregation wajib diisi untuk feasibility measured: {string.Join(", ", Aggregations)}."));
            }
            else if (feasibility == TelemetryFeasibility.Derived && string.IsNullOrWhiteSpace(q.CalcRef) && string.IsNullOrWhiteSpace(t.Note))
                issues.Add(new($"{tPath}.note", "feasibility derived wajib punya calcRef di pertanyaan atau note yang menjelaskan sumbernya."));
        }

        for (var r = 0; r < rows.Count; r++)
        {
            if (rows[r]?.Telemetry is not { } rt) continue;
            var rPath = $"{path}.rows[{r}].telemetry";
            if (rt.Feasibility is not { } rf || !Enum.IsDefined(rf))
                issues.Add(new($"{rPath}.feasibility", "feasibility wajib diisi: manual, measured, atau derived."));
            else if (rf == TelemetryFeasibility.Measured)
            {
                CheckMeasuredFields(rt, rPath, requireMetric: true, issues);
                if (string.IsNullOrWhiteSpace(rt.Aggregation) && string.IsNullOrWhiteSpace(q.Telemetry?.Aggregation))
                    issues.Add(new($"{rPath}.aggregation", $"aggregation wajib diisi di baris atau di pertanyaan: {string.Join(", ", Aggregations)}."));
            }
            else if (rf == TelemetryFeasibility.Derived && string.IsNullOrWhiteSpace(q.CalcRef) && string.IsNullOrWhiteSpace(rt.Note))
                issues.Add(new($"{rPath}.note", "feasibility derived wajib punya calcRef di pertanyaan atau note yang menjelaskan sumbernya."));
        }
    }

    // Baris tanpa telemetry sendiri mewarisi klasifikasi pertanyaan
    private static bool IsMeasuredRow(RowDef? row) =>
        row?.Telemetry?.Feasibility is null or TelemetryFeasibility.Measured;

    private static bool RowHasOwnAggregation(RowDef? row) =>
        !IsMeasuredRow(row) || !string.IsNullOrWhiteSpace(row?.Telemetry?.Aggregation);

    // Format metric/unit/aggregation; metric belum dicocokkan ke TelemetryMetrics.Known (didaftarkan saat sensor dipasang)
    private static void CheckMeasuredFields(TelemetryDef t, string path, bool requireMetric, List<DefinitionIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(t.Metric))
        {
            if (requireMetric) issues.Add(new($"{path}.metric", "metric wajib diisi untuk feasibility measured."));
        }
        else
        {
            if (!MetricPattern().IsMatch(t.Metric))
                issues.Add(new($"{path}.metric", $"metric {t.Metric} harus huruf kecil, angka, dan '_' (mis. energy_kwh)."));
            if (string.IsNullOrWhiteSpace(t.Unit))
                issues.Add(new($"{path}.unit", "unit wajib diisi bila metric diisi."));
        }
        if (!string.IsNullOrWhiteSpace(t.Aggregation) && !Aggregations.Contains(t.Aggregation))
            issues.Add(new($"{path}.aggregation", $"aggregation harus salah satu dari: {string.Join(", ", Aggregations)}."));
    }

    private static void CheckValueType(string? valueType, string path, List<DefinitionIssue> issues)
    {
        if (valueType is not null && !ValueTypes.Contains(valueType))
            issues.Add(new(path, $"valueType harus salah satu dari: {string.Join(", ", ValueTypes)}."));
    }

    private static void CheckUnique(IEnumerable<string?> values, string path, string property, string what,
        bool requirePattern, List<DefinitionIssue> issues)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var i = 0;
        foreach (var value in values)
        {
            var itemPath = $"{path}[{i++}].{property}";
            if (string.IsNullOrWhiteSpace(value))
                issues.Add(new(itemPath, $"{what} wajib diisi."));
            else if (requirePattern && !KeyPattern().IsMatch(value))
                issues.Add(new(itemPath, $"{what} {value} hanya boleh huruf, angka, '_', '-' atau '.'."));
            else if (!seen.Add(value))
                issues.Add(new(itemPath, $"{what} {value} dipakai lebih dari sekali."));
        }
    }

    // Pesan System.Text.Json diakhiri "Path: ... | LineNumber: ..."; path dan baris sudah ditampilkan terpisah
    private static string Describe(JsonException ex)
    {
        var message = ex.Message.Split('\n')[0];
        foreach (var marker in new[] { " Path: ", " LineNumber: " })
            if (message.IndexOf(marker, StringComparison.Ordinal) is var i and > 0)
                message = message[..i];
        return message.Trim();
    }
}
