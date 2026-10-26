using Bunit;

namespace GriForms.Tests.Support;

// Layanan MudBlazor (KeyInterceptorService, PointerEventsNoneService) hanya IAsyncDisposable, sehingga
// Dispose() sinkron dari xUnit melempar. IAsyncLifetime membuat xUnit membuang konteks secara async lebih dulu.
public abstract class MudBunitContext : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}
