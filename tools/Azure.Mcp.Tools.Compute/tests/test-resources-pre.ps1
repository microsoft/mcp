#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

#Requires -Version 7.0
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

Write-Host "Running Azure.Mcp.Tools.Compute pre-deployment script"

# Honor explicit locations; otherwise try US regions in preference order.
$candidateRegions = @(
    'westus2', 'westus3', 'eastus2', 'eastus', 'centralus',
    'northcentralus', 'southcentralus', 'westcentralus', 'westus'
)
$explicitLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} elseif ($Location) {
    $Location
} else {
    $null
}
$regionsToCheck = if ($explicitLocation) { @($explicitLocation) } else { $candidateRegions }

# Pick the cheapest eligible SKU: prefer B-series over D-series (then any other family),
# then fewest vCPUs, then least memory.
foreach ($deploymentLocation in $regionsToCheck) {
    Write-Host "Checking VM SKUs in '$deploymentLocation'..."
    $vmSize = Get-AzComputeResourceSku -Location $deploymentLocation -ErrorAction Stop |
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
    if ($vmSize) {
        break
    }
}

if (!$vmSize) {
    throw "No unrestricted x64 Gen2 VM SKU with 1-4 vCPUs and 2-8 GB RAM is available in any of: $($regionsToCheck -join ', ')."
}

Write-Host "Selected VM size: $vmSize in '$deploymentLocation'"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['vmSize'] = $vmSize
