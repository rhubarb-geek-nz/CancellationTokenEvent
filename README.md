# CancellationTokenEvent
Register an event for a `CancellationToken` to be used by `PowerShell`'s `Register-ObjectEvent`

## Register-CancellationEventToken

Example code

```
$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource

$cancellationEvent = Register-CancellationTokenEvent -CancellationToken $cancellationTokenSource.Token

Register-ObjectEvent -InputObject $cancellationEvent -EventName 'Cancelled' -SourceIdentifier 'CancellationToken'
```

This allows the usage of the standard `PowerShell` event sourced by the cancellation token.

The `dotnet` object used has a `Cancelled` event that can be consumed by `Register-ObjectEvent`.

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

When the cancellation is triggered it will invoke the `Cancelled` action event. When the registration is no longer needed then call `Dispose()` to unregister.
See [UnitTests/CancellationTokenTests.ps1](UnitTests/CancellationTokenTests.ps1) for full example.

To build use

```
dotnet publish CancellationTokenEvent.csproj --configuration Release
```

This will populate the module directory

```
$ ls bin/Release/netstandard2.0/rhubarb-geek-nz.CancellationTokenEvent
README.md
RhubarbGeekNz.CancellationTokenEvent.Registration.dll
RhubarbGeekNz.CancellationTokenEvent.PSCmdlet.dll
rhubarb-geek-nz.CancellationTokenEvent.psd1
```
