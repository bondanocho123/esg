using GriForms.Application.Forms;

namespace GriForms.Application.Abstractions;

public interface IAnswerWriter
{
    Task SaveCellsAsync(int submissionId, IEnumerable<CellChange> changes, CancellationToken ct);
}
