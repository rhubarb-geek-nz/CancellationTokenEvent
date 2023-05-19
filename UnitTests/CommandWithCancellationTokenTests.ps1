#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.
param($name,$cancellationToken)

trap
{
	throw $PSItem
}

switch ($name)
{
	'TestInvokeCommandWithCancellation' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			Wait-Event
		} -CancellationToken $cancellationToken -NoNewScope
	}
	'TestInvokeCommandWithoutCancellation' {
		Invoke-CommandWithCancellationToken -ScriptBlock {
			Start-Sleep -Seconds 5
			'sleep ok'
		} -CancellationToken $cancellationToken -NoNewScope
	}
	'TestInvokeCommandWithCancellationInPowerShell' {
		$cancellationTokenSource = New-Object -Type System.Threading.CancellationTokenSource
		$cancellationTokenSource.CancelAfter(5000)
		$cancellationToken = $cancellationTokenSource.Token
		try
		{
			try
			{
				Invoke-CommandWithCancellationToken -ScriptBlock {
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
		Invoke-CommandWithCancellationToken -ScriptBlock {
			'alpha'
			Wait-Event
			'bravo'
		} -CancellationToken $cancellationToken -NoNewScope
		'charlie'
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
		Wait-Event
		'bravo'
	}
	'TestStopInvokeCommand' {
		Invoke-Command -ScriptBlock {
			'alpha'
			Wait-Event
			'bravo'
		} -NoNewScope
		'charlie'
	}
	default {
		throw "unknown test - $name"
	}
}
