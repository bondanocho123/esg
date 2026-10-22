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
    List<ColumnDef>? Columns = null);   // kolom tetap untuk rowsTable

public record OptionDef(string Value, string Label);
public record RowDef(string Key, string Label);
public record ColumnDef(string Key, string Label, string? ValueType = null);
public record ValidationDef(decimal? Min = null, decimal? Max = null, bool IntegerOnly = false, int? MaxLength = null);
