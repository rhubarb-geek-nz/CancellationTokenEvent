# RhubarbGeekNz.CancellationTokenEvent.Registration

[CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) tools

## CancellationTokenEventRegistration

Use [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) with `.NET` events

The `.NET` class has a `Cancelled` event that can have [Action](https://learn.microsoft.com/en-us/dotnet/api/system.action) delegates added.

```
public class CancellationTokenEventRegistration : IDisposable
{
    public event Action Cancelled;
    public void Dispose() => CancellationTokenRegistration.Dispose();
    private readonly IDisposable CancellationTokenRegistration;
    public CancellationTokenEventRegistration(System.Threading.CancellationToken ct)
    {
        CancellationTokenRegistration = ct.Register(() => Cancelled());
    }
}
```

When the cancellation is triggered it will invoke the `Cancelled` [Action](https://learn.microsoft.com/en-us/dotnet/api/system.action) event. When the registration is no longer needed then call [Dispose](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable.dispose?view=net-7.0#system-idisposable-dispose) to unregister.
