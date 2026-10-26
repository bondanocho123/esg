using GriForms.Domain.Enums;

namespace GriForms.Application.Forms;

public record FormDefinition(
    string FormCode, string Title, string StandardRef, int VersionNo,
    int EffectiveFromYear, int? EffectiveToYear,
    List<DisclosureDef> Disclosures);

public record DisclosureDef(string No, string Title, string? Description, List<QuestionDef> Questions);

public record QuestionDef(
    string Key, InputType Type, string Label,
    string? HelpText = null, bool Required = false,
    string? ValueType = null, string? Unit = null,
    List<OptionDef>? Options = null,
    List<RowDef>? Rows = null, bool AllowCustomRows = false,
    ValidationDef? Validation = null,
    string? VisibleWhen = null,   // mis. "403-1-A == 'true'"
    string? CalcRef = null,       // mis. "ghg_total_v2", rujukan ke kalkulasi di kode
    List<ColumnDef>? Columns = null,    // kolom tetap untuk rowsTable
    TelemetryDef? Telemetry = null);    // wajib untuk pertanyaan numerik, lihat FormDefinitionValidator

public record OptionDef(string Value, string Label);

// Telemetry per baris bila sumber tiap baris berbeda (mis. Diesel dari flow meter, PLN dari kWh meter)
public record RowDef(string Key, string Label, TelemetryDef? Telemetry = null);

// Feasibility nullable agar "telemetry": {} terdeteksi belum diklasifikasikan, bukan dianggap Manual.
// Metric/Unit/Aggregation wajib untuk Measured; Derived wajib CalcRef pertanyaan atau Note.
// Contoh: { "feasibility": "measured", "metric": "diesel_l", "unit": "L", "aggregation": "sum" }
public record TelemetryDef(
    TelemetryFeasibility? Feasibility, string? Metric = null, string? Unit = null,
    string? Aggregation = null, string? Note = null);

public record ColumnDef(string Key, string Label, string? ValueType = null);
public record ValidationDef(decimal? Min = null, decimal? Max = null, bool IntegerOnly = false, int? MaxLength = null);
