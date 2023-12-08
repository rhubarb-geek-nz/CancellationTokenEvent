#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.

param($Configuration,$TargetFramework,$Platform,$IntDir,$OutDir,$PublishDir)

$ModuleName = 'CancellationTokenEvent'
$CompanyName = 'rhubarb-geek-nz'

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$DSC = [System.IO.Path]::DirectorySeparatorChar

trap
{
	throw $PSItem
}

$xmlDoc = [System.Xml.XmlDocument](Get-Content "$ModuleName.csproj")

$ModuleId = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/PackageId").FirstChild.Value
$Version = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Version").FirstChild.Value
$ProjectUri = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/PackageProjectUrl").FirstChild.Value
$Description = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Description").FirstChild.Value
$Author = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Authors").FirstChild.Value
$Copyright = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Copyright").FirstChild.Value
$AssemblyName = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/AssemblyName").FirstChild.Value

$CmdletsToExport = @('Register-CancellationTokenEvent','Invoke-CommandWithCancellationToken')

New-ModuleManifest -Path "$OutDir/$ModuleId.psd1" `
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

Import-PowerShellDataFile -LiteralPath "$OutDir/$ModuleId.psd1" | Export-PowerShellDataFile | Out-File -LiteralPath "$PublishDir$ModuleId.psd1"

Remove-Item "$OutDir/$ModuleId.psd1"
