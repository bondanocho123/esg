using GriForms.Application.Abstractions;

namespace GriForms.Application.Telemetry;

// Singleton: ingest (request HTTP) memberi tahu halaman dashboard yang terbuka lewat circuit Blazor masing-masing
public sealed class TelemetryNotifier : ITelemetryNotifier
{
    public event Action<int>? ReadingsReceived;

    // Handler yang melempar tidak boleh menggagalkan ingest: data sudah tersimpan, device akan mengirim ulang dan menggandakan baris
    public void Publish(int deviceId)
    {
        var handlers = ReadingsReceived;
        if (handlers is null) return;

        foreach (var handler in handlers.GetInvocationList().Cast<Action<int>>())
        {
            try
            {
                handler(deviceId);
            }
            catch
            {
                // Satu halaman yang rusak tidak menghalangi halaman lain
            }
        }
    }
}
