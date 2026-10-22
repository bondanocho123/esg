using System.Text.Json;
using System.Text.Json.Serialization;

namespace GriForms.Application.Forms;

public sealed class JsonFormDefinitionReader : IFormDefinitionReader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public FormDefinition Read(string json) =>
        JsonSerializer.Deserialize<FormDefinition>(json, Options)
            ?? throw new InvalidOperationException("Definisi form kosong.");
}
