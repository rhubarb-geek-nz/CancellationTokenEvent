#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.
param(
	$ArgumentByName = $False,
	$SourceIdentifier = 'CancellationToken'
)

trap
{
	throw $PSItem
}

$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource
$cancellationTokenSource.CancelAfter(5000)
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

$event = Wait-Event -SourceIdentifier $SourceIdentifier

$event.SourceIdentifier

Remove-Event -EventIdentifier $event.EventIdentifier

Unregister-Event -SourceIdentifier $SourceIdentifier

$cancellationEvent.Dispose()

$cancellationTokenSource.Dispose()
