using GriForms.Domain.Enums;

namespace GriForms.Web.Components.Forms;

public interface IFieldDescriptor
{
    InputType Type { get; }
    Type ComponentType { get; }
}

public sealed record FieldDescriptor<TField>(InputType Type) : IFieldDescriptor
    where TField : FieldBase
{
    public Type ComponentType => typeof(TField);
}

public interface IFieldRegistry
{
    Type Resolve(InputType type);
}

public sealed class FieldRegistry(IEnumerable<IFieldDescriptor> descriptors) : IFieldRegistry
{
    private readonly IReadOnlyDictionary<InputType, Type> _map =
        descriptors.ToDictionary(d => d.Type, d => d.ComponentType);

    public Type Resolve(InputType type) =>
        _map.TryGetValue(type, out var component)
            ? component
            : throw new NotSupportedException($"Tipe input {type} belum punya komponen.");
}

public static class FieldServiceExtensions
{
    public static IServiceCollection AddField<TField>(this IServiceCollection services, InputType type)
        where TField : FieldBase
        => services.AddSingleton<IFieldDescriptor>(new FieldDescriptor<TField>(type));

    // Tipe input baru: satu komponen turunan FieldBase dan satu baris AddField di sini
    public static IServiceCollection AddFormFields(this IServiceCollection services)
    {
        services.AddSingleton<IFieldRegistry, FieldRegistry>();
        return services
            .AddField<TextField>(InputType.Text)
            .AddField<NumberField>(InputType.Number)
            .AddField<SelectField>(InputType.Select)
            .AddField<BooleanField>(InputType.Boolean)
            .AddField<MonthlyTableField>(InputType.MonthlyTable)
            .AddField<RowsTableField>(InputType.RowsTable);
    }
}
