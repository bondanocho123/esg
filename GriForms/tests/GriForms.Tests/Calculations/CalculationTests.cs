using GriForms.Application;
using GriForms.Application.Calculations;
using GriForms.Application.Forms;
using GriForms.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Tests.Calculations;

public class CalculationTests
{
    [Fact]
    public void Row_total_sums_filled_months_only()
    {
        var q = new QuestionDef("403-5-B", InputType.MonthlyTable, "Peserta", Unit: "orang");
        var cells = new Dictionary<string, AnswerValue>
        {
            [FormState.CellKey("403-5-B/k3", "2026-01")] = new(null, 10),
            [FormState.CellKey("403-5-B/k3", "2026-02")] = new(null, 2.5m),
            [FormState.CellKey("403-5-B/k3", "2026-03")] = new(null, null, ValueState.NotApplicable),
            [FormState.CellKey("403-9-A")] = new(null, 99),
        };

        var result = Assert.Single(new RowTotalCalculation().Compute(new CalcContext(q, cells)));

        Assert.Equal(new ComputedCell("403-5-B/k3", null, 12.5m, "orang"), result);
    }

    // Open/Closed: kalkulasi baru cukup kelas baru dan satu registrasi DI
    [Fact]
    public void New_calculation_is_resolved_through_registration_only()
    {
        var registry = new ServiceCollection()
            .AddApplication()
            .AddCalculation<ConstantCalculation>()
            .BuildServiceProvider()
            .GetRequiredService<CalcRegistry>();

        Assert.True(registry.Contains("row_total_v1"));
        Assert.IsType<ConstantCalculation>(registry.Get("constant_v1"));
    }

    [Fact]
    public void Unknown_calc_ref_throws_clear_error()
    {
        var registry = new CalcRegistry(Array.Empty<ICalculation>());

        var ex = Assert.Throws<NotSupportedException>(() => registry.Get("tidak_ada_v1"));
        Assert.Contains("tidak_ada_v1", ex.Message);
    }

    private sealed class ConstantCalculation : ICalculation
    {
        public string Ref => "constant_v1";
        public IEnumerable<ComputedCell> Compute(CalcContext ctx) => new[] { new ComputedCell(ctx.Question.Key, null, 1, null) };
    }
}
