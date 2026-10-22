namespace GriForms.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
