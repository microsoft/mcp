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

Write-Host "Running ManagedLustre pre-deployment script"

# Auto-resolve hpcCacheRpObjectId for AMLFS test resources if template expects it and it's not already supplied
$templateFile = Join-Path $PSScriptRoot "test-resources.bicep"
if (Test-Path $templateFile) {
    # Read the template to check if hpcCacheRpObjectId parameter is expected
    $templateContent = Get-Content -Path $templateFile -Raw
    if ($templateContent -match 'param\s+hpcCacheRpObjectId\s+string') {
        Write-Host "Resolving HPC Cache Resource Provider service principal for hpcCacheRpObjectId parameter"

        # The AMLFS (StorageCache) resource provider's first-party service principal is
        # created when the Microsoft.StorageCache provider is registered. Ensure it is
        # registered so the service principal exists in the tenant (sovereign clouds such
        # as Azure US Government may not have it registered by default).
        try {
            $providerState = (Get-AzResourceProvider -ProviderNamespace 'Microsoft.StorageCache' -ErrorAction Stop |
                Select-Object -First 1).RegistrationState
            if ($providerState -ne 'Registered') {
                Write-Host "Registering Microsoft.StorageCache resource provider (current state: $providerState)"
                Register-AzResourceProvider -ProviderNamespace 'Microsoft.StorageCache' -ErrorAction Stop | Out-Null
            }
        } catch {
            Write-Warning "Failed to ensure Microsoft.StorageCache provider registration: $_"
        }

        # The service principal display name differs across clouds/stamps: it is
        # 'HPC Cache Resource Provider' in most environments, but the pre-GA name
        # 'storagecache Resource Provider' is still used in some sovereign clouds
        # (e.g. Azure US Government). Try each known display name in turn.
        $spDisplayNames = @('HPC Cache Resource Provider', 'storagecache Resource Provider')
        $sp = $null
        foreach ($displayName in $spDisplayNames) {
            try {
                $sp = Get-AzADServicePrincipal -DisplayName $displayName -ErrorAction Stop | Select-Object -First 1
            } catch {
                Write-Warning "Lookup for service principal '$displayName' failed: $_"
                $sp = $null
            }

            if ($sp -and $sp.Id) {
                Write-Host "Resolved service principal via display name '$displayName'."
                break
            }
        }

        if ($sp -and $sp.Id) {
            # Set the parameter for the template deployment
            $AdditionalParameters['hpcCacheRpObjectId'] = $sp.Id
            Write-Host "Success ✓ Set hpcCacheRpObjectId."
        } else {
            Write-Warning "HPC Cache Resource Provider service principal not found under any known display name ($($spDisplayNames -join ', ')); 'hpcCacheRpObjectId' will be missing and deployment may fail."
        }
    }
}

Write-Host "ManagedLustre pre-deployment script completed"