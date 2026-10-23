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

Write-Host "Running ManagedLustre pre-deployment script"

# Preferred US regions, in order, when no location is requested. The first one with AMLFS zone support is used.
$defaultLocations = @('westus2', 'eastus2', 'eastus', 'centralus', 'southcentralus', 'westus3')

function ConvertTo-LocationKey([string] $Value) {
    $Value.Replace(' ', '').ToLowerInvariant()
}

# The live tests create zonal filesystems, and AMLFS availability zones are only offered in some
# regions. Mirror the service's zonal check: more than one zone for an AMLFS SKU in the region.
# No Az/az cmdlet exposes AMLFS SKUs, so call the REST API directly. If this call changes, see:
# https://learn.microsoft.com/rest/api/storagecache/skus/list
$subscriptionId = (Get-AzContext).Subscription.Id
$skuResponse = Invoke-AzRestMethod -Method GET -Path "/subscriptions/$subscriptionId/providers/Microsoft.StorageCache/skus?api-version=2024-07-01"
if ($skuResponse.StatusCode -ne 200) {
    throw "Failed to list Microsoft.StorageCache SKUs: $($skuResponse.Content)"
}

$zonesByLocation = @{}
(ConvertFrom-Json $skuResponse.Content).value |
    Where-Object { $_.resourceType -eq 'amlFilesystems' } |
    ForEach-Object { $_.locationInfo } |
    Where-Object { @($_.zones).Count -gt 1 } |
    ForEach-Object {
        $key = ConvertTo-LocationKey $_.location
        $zones = @($_.zones)
        # Keep only zones offered by every SKU in the region.
        $zonesByLocation[$key] = if ($zonesByLocation.ContainsKey($key)) {
            @($zonesByLocation[$key] | Where-Object { $_ -in $zones })
        } else {
            $zones
        }
    }

$zonalLocations = @($zonesByLocation.Keys | Where-Object { $zonesByLocation[$_].Count -gt 0 } | Sort-Object)

# Honor an explicit -Location (the standard New-TestResources.ps1 argument used to create the
# resource group) or a 'location' template parameter override, but it must be zonal.
$requestedLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} else {
    $Location
}

if ($requestedLocation) {
    if ((ConvertTo-LocationKey $requestedLocation) -notin $zonalLocations) {
        throw "AMLFS availability zones are not supported in '$requestedLocation'. Zonal regions: $($zonalLocations -join ', ')"
    }
    $deploymentLocation = $requestedLocation
} else {
    $deploymentLocation = $defaultLocations |
        Where-Object { $_ -in $zonalLocations } |
        Select-Object -First 1

    if (!$deploymentLocation) {
        throw "No AMLFS zonal region found. Zonal regions: $($zonalLocations -join ', ')"
    }
}

Write-Host "Selected AMLFS zonal location: $deploymentLocation"
$zone = [string]($zonesByLocation[(ConvertTo-LocationKey $deploymentLocation)] | Sort-Object { [int]$_ } | Select-Object -First 1)
Write-Host "Selected AMLFS zone: $zone"
$templateFileParameters['location'] = $deploymentLocation
$templateFileParameters['zone'] = $zone

# Auto-resolve hpcCacheRpObjectId for AMLFS test resources if template expects it and it's not already supplied
$templateFile = Join-Path $PSScriptRoot "test-resources.bicep"
if (Test-Path $templateFile) {
    # Read the template to check if hpcCacheRpObjectId parameter is expected
    $templateContent = Get-Content -Path $templateFile -Raw
    if ($templateContent -match 'param\s+hpcCacheRpObjectId\s+string') {
        Write-Host "Resolving HPC Cache Resource Provider service principal for hpcCacheRpObjectId parameter"

        try {
            $sp = Get-AzADServicePrincipal -DisplayName 'HPC Cache Resource Provider' -ErrorAction Stop
            if ($sp -and $sp.Id) {
                # Set the parameter for the template deployment
                $templateFileParameters['hpcCacheRpObjectId'] = $sp.Id
                Write-Host "Success ✓ Set hpcCacheRpObjectId."
            } else {
                Write-Warning "HPC Cache Resource Provider service principal not found; 'hpcCacheRpObjectId' will be missing and deployment may fail."
            }
        } catch {
            Write-Warning "Failed to resolve HPC Cache Resource Provider service principal: $_"
            Write-Warning "Deployment may fail if the service principal is required."
        }
    }
}

Write-Host "ManagedLustre pre-deployment script completed"