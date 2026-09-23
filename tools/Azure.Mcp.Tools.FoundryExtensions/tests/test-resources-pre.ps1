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

Write-Host "Running Azure.Mcp.Tools.FoundryExtensions pre-deployment script"

$desiredCapacity = 2
$minCapacity = 1

$candidateRegions = @(
    'eastus2',
    'westus3',
    'southcentralus',
    'northcentralus',
    'eastus'
)

$explicitLocation = if ($templateFileParameters.ContainsKey('location')) {
    [string]$templateFileParameters['location']
} elseif ($Location) {
    $Location
} else {
    $null
}
$regionsToCheck = if ($explicitLocation) { @($explicitLocation) } else { $candidateRegions }

function Get-ModelQuotaAvailable([array] $usage, [string] $skuName, [string] $modelName) {
    # The exact quota dimension name format hasn't been confirmed against a live
    # subscription (it may be a machine name like 'OpenAI.Standard.gpt-4o' or a
    # human-readable string). Require both the SKU tier and the model name to appear,
    # anchoring the model name at the end so 'gpt-4o' doesn't also match 'gpt-4o-mini'.
    # 'GlobalStandard' also contains the substring 'Standard', so a plain 'Standard'
    # SKU match must specifically exclude it via a negative look-behind.
    $skuPattern = if ($skuName -eq 'Standard') { '(?<!Global)Standard' } else { [regex]::Escape($skuName) }
    $modelPattern = [regex]::Escape($modelName) + '$'
    $entry = $usage | Where-Object { $_.Name -match $skuPattern -and $_.Name -match $modelPattern } | Select-Object -First 1
    if (!$entry) {
        return $null
    }
    return [double]$entry.Limit - [double]$entry.CurrentValue
}

$bestRegion = $null
$bestCapacity = 0

foreach ($region in $regionsToCheck) {
    Write-Host "Checking Azure OpenAI quota in '$region'..."
    try {
        $usage = Get-AzCognitiveServicesUsage -Location $region
    } catch {
        Write-Warning "Could not read Cognitive Services usage for '$region': $_"
        continue
    }

    $gpt4oAvailable = Get-ModelQuotaAvailable -usage $usage -skuName 'Standard' -modelName 'gpt-4o'
    $gpt4oMiniAvailable = Get-ModelQuotaAvailable -usage $usage -skuName 'GlobalStandard' -modelName 'gpt-4o-mini'
    $embeddingAvailable = Get-ModelQuotaAvailable -usage $usage -skuName 'GlobalStandard' -modelName 'text-embedding-3-small'

    if ($null -eq $gpt4oAvailable -or $null -eq $gpt4oMiniAvailable -or $null -eq $embeddingAvailable) {
        Write-Host "  Skipping '$region': one or more required models/SKUs were not found in the quota usage report."
        continue
    }

    $regionCapacity = [Math]::Min([Math]::Min($gpt4oAvailable, $gpt4oMiniAvailable), $embeddingAvailable)
    Write-Host "  '$region' has $regionCapacity(K TPM) of headroom common to all three models."

    if ($regionCapacity -ge $desiredCapacity) {
        $bestRegion = $region
        $bestCapacity = $desiredCapacity
        break
    }

    if ($regionCapacity -ge $minCapacity -and $regionCapacity -gt $bestCapacity) {
        $bestRegion = $region
        $bestCapacity = [Math]::Floor($regionCapacity)
    }
}

# Don't hard-fail the deployment if quota couldn't be confirmed (e.g. unexpected
# quota-name format, transient API issue, or genuinely no headroom anywhere we
# checked) - fall back to the explicit/first candidate region with the documented
# minimum capacity and let the actual deployment surface a concrete quota error if
# one occurs, which is far more actionable than a script pre-check.
if (!$bestRegion) {
    $bestRegion = if ($explicitLocation) { $explicitLocation } else { $candidateRegions[0] }
    $bestCapacity = $minCapacity
    Write-Warning ("Could not confirm quota headroom in any of: $($regionsToCheck -join ', '). " +
        "Falling back to '$bestRegion' with the minimum capacity of $bestCapacity(K TPM).")
} elseif ($bestCapacity -lt $desiredCapacity) {
    Write-Warning ("No candidate region had the desired $desiredCapacity(K TPM); deploying to " +
        "'$bestRegion' with a reduced capacity of $bestCapacity(K TPM) instead.")
} else {
    Write-Host "Selected region '$bestRegion' with capacity $bestCapacity(K TPM)."
}

$templateFileParameters['location'] = $bestRegion
$templateFileParameters['modelCapacity'] = [int]$bestCapacity

