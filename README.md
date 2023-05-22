# CancellationTokenEvent
[CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) tools for `PowerShell`

## Register-CancellationTokenEvent

Use [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) with `PowerShell` events

Example code

```
$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource

$cancellationEvent = Register-CancellationTokenEvent -CancellationToken $cancellationTokenSource.Token

Register-ObjectEvent -InputObject $cancellationEvent -EventName 'Cancelled' -SourceIdentifier 'CancellationToken'
```

This allows the usage of the standard `PowerShell` event sourced by the cancellation token.

The `dotnet` object used has a `Cancelled` event that can be consumed by [Register-ObjectEvent](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/register-objectevent).

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

When the cancellation is triggered it will invoke the `Cancelled` action event. When the registration is no longer needed then call [Dispose](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable.dispose?view=net-7.0#system-idisposable-dispose) to unregister.
See [UnitTests/CancellationTokenTests.ps1](UnitTests/CancellationTokenTests.ps1) for full example.

## Invoke-CommandWithCancellationToken

Use a [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) to stop a [ScriptBlock](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_script_blocks)

Example code

```
$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource
$cancellationTokenSource.CancelAfter(5000)
$cancellationToken = $cancellationTokenSource.Token

Invoke-CommandWithCancellationToken -ScriptBlock {
    Wait-Event
} -CancellationToken $cancellationToken -NoNewScope
```

This will stop the [Wait-Event](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/wait-event) after 5 seconds. Internally this works by running the [ScriptBlock](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_script_blocks) with [Invoke-Command](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/invoke-command) and using the [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) to call [Stop](https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.powershell.stop).

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
