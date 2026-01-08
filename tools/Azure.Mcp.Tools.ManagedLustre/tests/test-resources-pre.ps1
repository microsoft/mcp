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