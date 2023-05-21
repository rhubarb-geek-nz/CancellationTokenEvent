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

$xmlDoc = [System.Xml.XmlDocument](Get-Content "$ModuleName.csproj")

$ModuleId = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/PackageId").FirstChild.Value
$Version = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Version").FirstChild.Value
$ProjectUri = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/PackageProjectUrl").FirstChild.Value
$Description = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Description").FirstChild.Value
$Author = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Authors").FirstChild.Value
$Copyright = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Copyright").FirstChild.Value
$AssemblyName = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/AssemblyName").FirstChild.Value

$ModulePath = ( $OutDir + $ModuleId )

if ( Test-Path $ModulePath )
{
	Remove-Item $ModulePath -Recurse -Force
}

$null = New-Item -ItemType Directory -Path $ModulePath

Get-ChildItem -LiteralPath $OutDir -Filter '*.dll' | ForEach-Object {
	$Name = $_.Name

	$_ | Copy-Item -Destination $ModulePath
}

Copy-Item -LiteralPath 'README.md' -Destination $ModulePath

$CmdletsToExport = @('Register-CancellationTokenEvent','Invoke-CommandWithCancellationToken')

New-ModuleManifest -Path "$ModulePath/$ModuleId.psd1" `
				-RootModule "$AssemblyName.dll" `
				-ModuleVersion $Version `
				-Guid 'cb6bb4f1-56ee-4dce-be88-6eb5f7957c7c' `
				-Author $Author `
				-CompanyName $CompanyName `
				-Copyright $Copyright `
				-Description $Description `
				-FunctionsToExport @() `
				-CmdletsToExport $CmdletsToExport `
				-VariablesToExport '*' `
				-AliasesToExport @() `
				-ProjectUri $ProjectUri

Get-Content -LiteralPath "$ModulePath/$ModuleId.psd1" | ForEach-Object {
	$T = $_.Trim()
	if ($T)
	{
		if ( -not $T.StartsWith('#') )
		{
			if ($T.StartsWith('} # End of '))
			{
				$_.Substring(0,$_.IndexOf('}')+1)
			}
			else
			{
				$_
			}
		}
	}
} | Set-Content -LiteralPath "$ModulePath/$ModuleId.psd1.clean"

Remove-Item -LiteralPath "$ModulePath/$ModuleId.psd1"

Move-Item -LiteralPath "$ModulePath/$ModuleId.psd1.clean" -Destination "$ModulePath/$ModuleId.psd1"

Import-PowerShellDataFile -LiteralPath "$ModulePath/$ModuleId.psd1"
