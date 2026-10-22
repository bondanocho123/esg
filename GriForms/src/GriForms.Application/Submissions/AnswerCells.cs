using GriForms.Application.Forms;
using GriForms.Domain.Entities;

namespace GriForms.Application.Submissions;

public static class AnswerCells
{
    public static IReadOnlyDictionary<string, AnswerValue> ToCells(IEnumerable<Answer> answers) =>
        answers.ToDictionary(
            a => FormState.CellKey(a.QuestionKey, a.PeriodKey),
            a => new AnswerValue(a.StringValue, a.NumberValue, a.ValueState));
}
