using GriForms.Application.Forms;

namespace GriForms.Application.Abstractions;

// Katalog definisi form GRI (Topic, FormVersion, QuestionMapping): global, dikelola PlatformAdmin saja.
// Selain PlatformAdmin: UnauthorizedAccessException.
public interface IFormCatalog
{
    Task<IReadOnlyList<FormVersionSummary>> GetVersionsAsync(CancellationToken ct);
}
