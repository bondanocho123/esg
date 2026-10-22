using GriForms.Application.Abstractions;

namespace GriForms.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
