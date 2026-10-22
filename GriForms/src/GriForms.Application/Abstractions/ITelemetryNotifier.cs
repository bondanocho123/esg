namespace GriForms.Application.Abstractions;

public interface ITelemetryNotifier
{
    // Argumen: Id device yang baru mengirim
    event Action<int>? ReadingsReceived;

    void Publish(int deviceId);
}
