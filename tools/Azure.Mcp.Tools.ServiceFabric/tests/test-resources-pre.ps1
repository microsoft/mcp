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

Write-Host "Running Azure.Mcp.Tools.ServiceFabric pre-deployment script"

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

foreach ($deploymentLocation in $regionsToCheck) {
    Write-Host "Checking VM SKUs in '$deploymentLocation'..."
    $vmSku = Get-AzComputeResourceSku -Location $deploymentLocation -ErrorAction Stop |
        Where-Object {
            $sku = $_
            $vCpuCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'vCPUs' }
            $memoryCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }
            $architectureCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'CpuArchitectureType' }
            $hasLocationRestriction = $sku.Restrictions | Where-Object { $_.Type -eq 'Location' }
            $sku.ResourceType -eq 'virtualMachines' -and
                $sku.Name -like 'Standard_D*' -and
                [int]$vCpuCapability.Value -eq 2 -and
                $null -ne $memoryCapability -and
                [double]$memoryCapability.Value -le 8 -and
                $architectureCapability.Value -eq 'x64' -and
                -not $hasLocationRestriction
        } |
        Sort-Object -Descending -Property {
            [double]($_.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }).Value
        } |
        Select-Object -ExpandProperty Name -First 1
    if ($vmSku) {
        break
    }
}

if (!$vmSku) {
    throw "No unrestricted 2-vCPU Standard_D VM SKU with at most 8 GB RAM is available in any of: $($regionsToCheck -join ', ')."
}

Write-Host "Selected VM SKU: $vmSku in '$deploymentLocation'"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['vmSku'] = $vmSku
