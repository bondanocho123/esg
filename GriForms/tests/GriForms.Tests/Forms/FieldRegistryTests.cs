using GriForms.Domain.Enums;
using GriForms.Web.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace GriForms.Tests.Forms;

public class FieldRegistryTests
{
    [Fact]
    public void Every_input_type_has_a_field_component()
    {
        var registry = new ServiceCollection().AddFormFields().BuildServiceProvider()
            .GetRequiredService<IFieldRegistry>();

        foreach (var type in Enum.GetValues<InputType>())
            Assert.True(typeof(FieldBase).IsAssignableFrom(registry.Resolve(type)), $"{type} tidak memetakan ke FieldBase");
    }

    [Fact]
    public void Unregistered_type_throws_not_supported_with_clear_message()
    {
        var registry = new FieldRegistry(Array.Empty<IFieldDescriptor>());

        var ex = Assert.Throws<NotSupportedException>(() => registry.Resolve(InputType.RowsTable));
        Assert.Contains(nameof(InputType.RowsTable), ex.Message);
    }

    // Open/Closed: tipe baru cukup satu komponen dan satu baris AddField
    [Fact]
    public void New_field_is_resolved_through_registration_only()
    {
        var registry = new ServiceCollection()
            .AddSingleton<IFieldRegistry, FieldRegistry>()
            .AddField<NumberField>(InputType.Text)
            .BuildServiceProvider()
            .GetRequiredService<IFieldRegistry>();

        Assert.Equal(typeof(NumberField), registry.Resolve(InputType.Text));
    }
}
