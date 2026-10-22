using GriForms.Application.Forms;

namespace GriForms.Application.Calculations;

public record CalcContext(QuestionDef Question, IReadOnlyDictionary<string, AnswerValue> Cells);

public record ComputedCell(string QuestionKey, string? PeriodKey, decimal Value, string? Unit);

public interface ICalculation
{
    string Ref { get; }                        // mis. "ghg_total_v2"
    IEnumerable<ComputedCell> Compute(CalcContext ctx);
}
