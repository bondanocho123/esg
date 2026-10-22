using GriForms.Application.Forms;

namespace GriForms.Application.Validation;

public interface IValidationEngine
{
    ValidationResult Validate(FormDefinition definition, IReadOnlyDictionary<string, AnswerValue> cells, bool forSubmit);
}
