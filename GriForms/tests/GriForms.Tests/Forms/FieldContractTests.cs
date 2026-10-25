using Bunit;
using GriForms.Application.Forms;
using GriForms.Tests.Support;
using GriForms.Web.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using InputType = GriForms.Domain.Enums.InputType;

namespace GriForms.Tests.Forms;

// Tes kontrak Liskov yang dijalankan untuk setiap turunan FieldBase:
// hormati ReadOnly, tampilkan Errors, dan jangan melempar untuk State kosong.
public abstract class FieldContractTests<TField> : MudBunitContext where TField : FieldBase
{
    protected FieldContractTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<IFormContext>(new FakeFormContext(2026));
        Render<MudBlazor.MudPopoverProvider>();
    }

    protected abstract QuestionDef Question { get; }

    private IRenderedComponent<TField> RenderField(FormState state, bool readOnly, IReadOnlyList<string>? errors = null) =>
        Render<TField>(p => p
            .Add(x => x.Question, Question)
            .Add(x => x.State, state)
            .Add(x => x.ReadOnly, readOnly)
            .Add(x => x.Errors, errors ?? Array.Empty<string>()));

    [Fact]
    public void Renders_without_throwing_for_empty_state()
    {
        var cut = RenderField(new FormState(), readOnly: false);

        Assert.Contains(Question.Label, cut.Markup);
    }

    [Fact]
    public void Renders_without_throwing_for_null_values()
    {
        var state = new FormState();
        state.Set(Question.Key, null, new AnswerValue(null, null));

        RenderField(state, readOnly: false);
    }

    [Fact]
    public void Shows_errors()
    {
        var cut = RenderField(new FormState(), readOnly: false, errors: new[] { "Pesan error uji" });

        Assert.Contains("Pesan error uji", cut.Markup);
    }

    [Fact]
    public void Is_editable_when_not_read_only()
    {
        var cut = RenderField(new FormState(), readOnly: false);

        Assert.Contains(cut.FindAll("input, textarea"), e => !e.HasAttribute("disabled"));
    }

    [Fact]
    public void Read_only_disables_every_control()
    {
        var cut = RenderField(new FormState(), readOnly: true);

        var controls = cut.FindAll("input, textarea, select");
        Assert.NotEmpty(controls);
        Assert.All(controls, e => Assert.True(
            e.HasAttribute("disabled") || e.HasAttribute("readonly"),
            $"Kontrol <{e.TagName}> masih bisa diedit saat ReadOnly"));
    }
}

public class TextFieldContract : FieldContractTests<TextField>
{
    protected override QuestionDef Question => new("t", InputType.Text, "Uraian pelatihan");
}

public class NumberFieldContract : FieldContractTests<NumberField>
{
    protected override QuestionDef Question => new("n", InputType.Number, "Jam kerja", Unit: "jam");
}

public class SelectFieldContract : FieldContractTests<SelectField>
{
    protected override QuestionDef Question => new("s", InputType.Select, "Sumber data",
        Options: new() { new OptionDef("hse", "HSE"), new OptionDef("hr", "HR") });
}

public class BooleanFieldContract : FieldContractTests<BooleanField>
{
    protected override QuestionDef Question => new("b", InputType.Boolean, "Ada kecelakaan fatal?");
}

public class MonthlyTableFieldContract : FieldContractTests<MonthlyTableField>
{
    protected override QuestionDef Question => new("m", InputType.MonthlyTable, "Peserta pelatihan", Unit: "orang",
        Rows: new() { new RowDef("k3", "K3 Dasar") });
}

public class RowsTableFieldContract : FieldContractTests<RowsTableField>
{
    protected override QuestionDef Question => new("r", InputType.RowsTable, "Rincian kecelakaan",
        Rows: new() { new RowDef("r1", "Kejadian 1") },
        Columns: new() { new ColumnDef("jumlah", "Jumlah", "number"), new ColumnDef("ket", "Keterangan") });
}
