#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.
param(
	[bool]$ArgumentByName = $False,
	[string]$SourceIdentifier = 'CancellationToken'
)

trap
{
	throw $PSItem
}

$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource
$cancellationToken = $cancellationTokenSource.Token

if ($ArgumentByName)
{
	$cancellationEvent = Register-CancellationTokenEvent -CancellationToken $cancellationToken
}
else
{
	$cancellationEvent = Register-CancellationTokenEvent $cancellationToken
}

Register-ObjectEvent -InputObject $cancellationEvent -EventName 'Cancelled' -SourceIdentifier $SourceIdentifier

try
{
	$cancellationTokenSource.CancelAfter(100)

	$event = Wait-Event -SourceIdentifier $SourceIdentifier

	$event.SourceIdentifier

	Remove-Event -EventIdentifier $event.EventIdentifier
}
finally
{
	Unregister-Event -SourceIdentifier $SourceIdentifier

	$cancellationEvent.Dispose()

	$cancellationTokenSource.Dispose()
}

