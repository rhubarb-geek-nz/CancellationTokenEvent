# CancellationTokenEvent
`CancellationToken` tools for `PowerShell`

## Register-CancellationTokenEvent

Use `CancellationToken` with `PowerShell` events

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

## Invoke-CommandWithCancellationToken

Use a `CancellationToken` to stop a `ScriptBlock`

Example code

```
$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource
$cancellationTokenSource.CancelAfter(5000)
$cancellationToken = $cancellationTokenSource.Token

Invoke-CommandWithCancellationToken -ScriptBlock {
    Wait-Event
} -CancellationToken $cancellationToken -NoNewScope
```

This will stop the `Wait-Event` after 5 seconds. Internally this works by running the `ScriptBlock` with `Invoke-Command` and using the `CancellationTokento` call `Stop`.

## Build

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
