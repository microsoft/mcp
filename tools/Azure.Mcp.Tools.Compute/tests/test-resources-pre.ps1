#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupName,

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Compute pre-deployment script"

Write-Host "Additional parameters passed to pre-deployment script:"
foreach ($arg in $AdditionalParameters.GetEnumerator()) {
    Write-Host "Additional parameter: $($arg.Key) = $($arg.Value)"
}

$computeTemplateFile = Join-Path -Path $PSScriptRoot -ChildPath "compute-test-resources.bicep"

if (!(Test-Path $computeTemplateFile)) {
    Write-Error "Compute template file not found at path: $computeTemplateFile"
    exit 1
}

$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName
if (-not $resourceGroup) {
    Write-Error "Resource group '$ResourceGroupName' not found."
    exit 1
}

$availableVms = Get-AzComputeResourceSku -Location $resourceGroup.Location `
        | Where-Object { $_.ResourceType -eq "virtualMachines"  -and $_.Name -like 'Standard_D2*_v6' } `
        | Where-Object { ([int]($_.Capabilities | Where-Object { $_.Name -eq "vCPUs" }).Value) -le 2 }

if (!$availableVms) {
    Write-Error "No suitable VM sizes found in location '$($resourceGroup.Location)'."
    exit 1
}

Write-Host "Selected VM size for deployment: $($availableVms[0].Name)"
$templateFileParameters = @{
    vmSize = $availableVms[0].Name
}

New-AzResourceGroupDeployment `
    -Name $BaseName `
    -ResourceGroupName $resourceGroup.ResourceGroupName `
    -TemplateFile $computeTemplateFile `
    -TemplateParameterObject $templateFileParameters