using GriForms.Application.Forms;
using GriForms.Domain.Enums;

namespace GriForms.Application.Calculations;

// Contoh kalkulasi: total tahunan per baris tabel bulanan, dari sel yang terisi saja.
// Hasil memakai kunci baris yang sama dengan PeriodKey kosong, mis. "403-5-B/k3-dasar".
public sealed class RowTotalCalculation : ICalculation
{
    public string Ref => "row_total_v1";

    public IEnumerable<ComputedCell> Compute(CalcContext ctx)
    {
        var prefix = ctx.Question.Key + "/";

        return ctx.Cells
            .Select(c => (Key: FormState.SplitCellKey(c.Key), c.Value))
            .Where(c => c.Key.QuestionKey.StartsWith(prefix, StringComparison.Ordinal)
                     && c.Key.PeriodKey is not null
                     && c.Value.State == ValueState.Filled
                     && c.Value.Number is not null)
            .GroupBy(c => c.Key.QuestionKey)
            .Select(g => new ComputedCell(g.Key, null, g.Sum(c => c.Value.Number!.Value), ctx.Question.Unit));
    }
}
