#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.

param($Configuration,$TargetFramework,$Platform,$IntDir,$OutDir,$TargetDir)

$ModuleName = 'CancellationTokenEvent'
$CompanyName = 'rhubarb-geek-nz'

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$DSC = [System.IO.Path]::DirectorySeparatorChar

trap
{
	throw $PSItem
}

if ( -not ( Test-Path $IntDir ))
{
	throw "$IntDir not found"
}

if ( -not ( Test-Path $OutDir ))
{
	throw "$OutDir not found"
}

$xmlDoc = [System.Xml.XmlDocument](Get-Content "$ModuleName.nuspec")

$ModuleId = $xmlDoc.SelectSingleNode("/package/metadata/id").FirstChild.Value
$Version = $xmlDoc.SelectSingleNode("/package/metadata/version").FirstChild.Value
$ProjectUri = $xmlDoc.SelectSingleNode("/package/metadata/projectUrl").FirstChild.Value
$Description = $xmlDoc.SelectSingleNode("/package/metadata/description").FirstChild.Value
$Author = $xmlDoc.SelectSingleNode("/package/metadata/authors").FirstChild.Value
$Copyright = $xmlDoc.SelectSingleNode("/package/metadata/copyright").FirstChild.Value

$ModulePath = ( $OutDir + $ModuleId )

if ( Test-Path $ModulePath )
{
	Remove-Item $ModulePath -Recurse -Force
}

$null = New-Item -ItemType Directory -Path $ModulePath

Get-ChildItem -LiteralPath $OutDir -Filter '*.dll' | ForEach-Object {
	$Name = $_.Name

	$_ | Copy-Item -Destination $ModulePath

	if ( Test-Path -LiteralPath 'signtool.ps1' )
	{
		pwsh ./signtool.ps1 -Path ( $ModulePath + $DSC + $Name )

		If ( $LastExitCode -ne 0 )
		{
			throw "signtool.ps1 $ModulePath$DSC$Name"
		}
	}
}

Copy-Item -LiteralPath ( '..'+$DSC+'README.md' ) -Destination $ModulePath

$CmdletsToExport = "'Register-CancellationTokenEvent'"

@"
@{
	RootModule = 'RhubarbGeekNz.$ModuleName.PSCmdlet.dll'
	ModuleVersion = '$Version'
	GUID = 'cb6bb4f1-56ee-4dce-be88-6eb5f7957c7c'
	Author = '$Author'
	CompanyName = '$CompanyName'
	Copyright = '$Copyright'
	Description = '$Description'
	FunctionsToExport = @()
	CmdletsToExport = @($CmdletsToExport)
	VariablesToExport = '*'
	AliasesToExport = @()
	PrivateData = @{
		PSData = @{
			ProjectUri = '$ProjectUri'
		}
	}
}
"@ | Set-Content -Path "$ModulePath/$ModuleId.psd1"
