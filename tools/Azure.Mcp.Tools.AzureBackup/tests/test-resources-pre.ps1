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
    [string] $Location = '',

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Azure.Mcp.Tools.AzureBackup pre-deployment script"

# westus has no VM SKUs enabled for some test subscriptions. Honor an explicit
# -Location (the standard New-TestResources.ps1 argument used to create the
# resource group) or a 'location' template parameter override; otherwise
# deploy the co-located vault and VM in westus2.
$deploymentLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} elseif ($Location) {
    $Location
} else {
    'westus2'
}

$sqlVmSize = Get-AzComputeResourceSku -Location $deploymentLocation |
    Where-Object {
        $sku = $_
        $vCpuCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'vCPUs' }
        $memoryCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }
        $architectureCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'CpuArchitectureType' }
        $hyperVCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'HyperVGenerations' }
        $hasLocationRestriction = $sku.Restrictions | Where-Object { $_.Type -eq 'Location' }
        $sku.ResourceType -eq 'virtualMachines' -and
            $sku.Name -like 'Standard_*' -and
            [int]$vCpuCapability.Value -ge 1 -and
            [int]$vCpuCapability.Value -le 4 -and
            $null -ne $memoryCapability -and
            [double]$memoryCapability.Value -ge 2 -and
            [double]$memoryCapability.Value -le 8 -and
            $architectureCapability.Value -eq 'x64' -and
            $hyperVCapability.Value -match 'V2' -and
            -not $hasLocationRestriction
    } |
    Sort-Object -Property @{
        Expression = {
            if ($_.Name -like 'Standard_B*') { 0 }
            elseif ($_.Name -like 'Standard_D*') { 1 }
            else { 2 }
        }
    }, @{
        Expression = { [int]($_.Capabilities | Where-Object { $_.Name -eq 'vCPUs' }).Value }
    }, @{
        Expression = { [double]($_.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }).Value }
    }, Name |
    Select-Object -ExpandProperty Name -First 1

if (!$sqlVmSize) {
    throw "No unrestricted x64 Gen2 VM SKU with 1-4 vCPUs and 2-8 GB RAM is available in '$deploymentLocation'."
}

Write-Host "Selected SQL VM size: $sqlVmSize"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['sqlVmSize'] = $sqlVmSize
