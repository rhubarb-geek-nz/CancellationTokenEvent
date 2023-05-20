#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.
param(
	[parameter(Mandatory=$true)][string]$name,
	[parameter(Mandatory=$true)][System.Threading.CancellationToken]$cancellationToken,
	[parameter(Mandatory=$true)][System.Threading.CancellationTokenSource]$cancellationTokenSource
)

trap
{
	throw $PSItem
}

switch ($name)
{
	'TestInvokeCommandWithCancellation' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			$cancellationTokenSource.CancelAfter(100)
			Wait-Event
		} -CancellationToken $cancellationToken -NoNewScope
	}
	'TestInvokeCommandWithoutCancellation' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			Start-Sleep -Milliseconds 100
			'sleep ok'
		} -CancellationToken $cancellationToken -NoNewScope
	}
	'TestInvokeCommandWithCancellationInPowerShell' {
		$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource
		$cancellationToken = $cancellationTokenSource.Token
		try
		{
			try
			{
				Invoke-CommandWithCancellationToken -ScriptBlock {
					$cancellationTokenSource.CancelAfter(100)
					Wait-Event
				} -CancellationToken $cancellationToken -NoNewScope
			}
			catch
			{
				($PSItem.Exception.CancellationToken -eq $cancellationToken)
			}
		}
		finally
		{
			$cancellationTokenSource.Dispose()
		}
	}
	'TestInvokeCommandWithArgumentList' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			param($name,$cancellationToken)
			$name
		} -CancellationToken $cancellationToken -NoNewScope -ArgumentList $name,$cancellationToken
	}
	'TestInvokeCommandWithInputObject' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			$Input
		} -CancellationToken $cancellationToken -NoNewScope -InputObject $name
	}
	'TestOutputPipeline' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			'one'
			'two'
			'three'
		} -CancellationToken $cancellationToken -NoNewScope
	}
	'TestCancelledOutputPipeline' {
		try
		{
			Invoke-CommandWithCancellationToken -ScriptBlock {
				'eins'
				'zwei'
				Wait-Event
				'vier'
			} -CancellationToken $cancellationToken -NoNewScope
			'fuenf'
		}
		catch
		{
			'drei'
		}
	}
	'TestStop' {
		$cancellationTokenSourceLocal = New-Object -Type System.Threading.CancellationTokenSource
		try
		{
			Invoke-CommandWithCancellationToken -ScriptBlock {
				'alpha'
				$cancellationTokenSource.CancelAfter(100)
				Wait-Event
				'bravo'
			} -CancellationToken $cancellationTokenSourceLocal.Token -NoNewScope
			'charlie'
		}
		finally
		{
			$cancellationTokenSourceLocal.Dispose()
		}
	}
	'TestAlreadyCancelled' {
		try
		{
			Invoke-CommandWithCancellationToken -ScriptBlock {
				'alpha'
				Wait-Event
				'bravo'
			} -CancellationToken $cancellationToken -NoNewScope
			'charlie'
		}
		catch
		{
			$PSItem.FullyQualifiedErrorId
		}
	}
	'TestStopWaitEvent' {
		'alpha'
		$cancellationTokenSource.CancelAfter(100)
		Wait-Event
		'bravo'
	}
	'TestStopInvokeCommand' {
		Invoke-Command -ScriptBlock {
			'alpha'
			$cancellationTokenSource.CancelAfter(100)
			Wait-Event
			'bravo'
		} -NoNewScope
		'charlie'
	}
	default {
		throw "unknown test - $name"
	}
}
