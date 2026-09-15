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
    [string] $DefaultSqlVmSize = "Standard_B2als_v2",

    [Parameter()]
    [hashtable] $AdditionalParameters = @{},

    # Captures any arguments from the deployment script
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

Write-Host "Running Azure.Mcp.Tools.AzureBackup pre-deployment script"

$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName

$sqlVmSize = Get-AzComputeResourceSku -Location $resourceGroup.Location |
    Where-Object {
        $sku = $_
        $vCpuCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'vCPUs' }
        $memoryCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }
        $architectureCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'CpuArchitectureType' }
        $hyperVCapability = $sku.Capabilities | Where-Object { $_.Name -eq 'HyperVGenerations' }
        $sku.ResourceType -eq 'virtualMachines' -and
            $sku.Name -match '^Standard_[BD]' -and
            [int]$vCpuCapability.Value -eq 2 -and
            $null -ne $memoryCapability -and
            [double]$memoryCapability.Value -le 8 -and
            $architectureCapability.Value -eq 'x64' -and
            $hyperVCapability.Value -match 'V2' -and
            ($null -eq $sku.Restrictions -or @($sku.Restrictions).Count -eq 0)
    } |
    Sort-Object -Property @{
        Expression = { if ($_.Name -like 'Standard_B*') { 0 } else { 1 } }
    }, @{
        Expression = { [double]($_.Capabilities | Where-Object { $_.Name -eq 'MemoryGB' }).Value }
        Descending = $true
    }, Name |
    Select-Object -ExpandProperty Name -First 1

if (!$sqlVmSize) {
    Write-Host "No suitable SQL VM size found, using default VM size: $DefaultSqlVmSize"
    $sqlVmSize = $DefaultSqlVmSize
}

Write-Host "Selected SQL VM size: $sqlVmSize"
$templateFileParameters['sqlVmSize'] = $sqlVmSize
