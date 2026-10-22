using GriForms.Application.Imports;

namespace GriForms.Application.Abstractions;

// Excel: importer dan exporter tidak dipaksa saling mengenal
public interface IExcelImporter
{
    IEnumerable<LegacyRow> Read(string path);
}
