# RhubarbGeekNz.CancellationTokenEvent.Extensions

[CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) tools for `PowerShell`

## AddCancellationTokenEventCmdlets

Adds the Cmdlets to an [InitialSessionState](https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.runspaces.initialsessionstate)

```
InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
initialSessionState.AddCancellationTokenEventCmdlets();
```
