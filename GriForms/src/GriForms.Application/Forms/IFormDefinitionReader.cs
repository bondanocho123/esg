namespace GriForms.Application.Forms;

public interface IFormDefinitionReader
{
    FormDefinition Read(string json);
}
