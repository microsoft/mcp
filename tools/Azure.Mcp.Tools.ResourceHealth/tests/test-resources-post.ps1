#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [hashtable] $AdditionalParameters
)

Write-Host "Running ResourceHealth post-deployment setup..."

try {
    # Extract outputs from deployment
    $storageAccountResourceId = $AdditionalParameters['RESOURCEHEALTH_STORAGE_ACCOUNT_RESOURCE_ID']
    $storageAccountName = $AdditionalParameters['RESOURCEHEALTH_STORAGE_ACCOUNT_NAME']
    
    if ($storageAccountResourceId) {
        Write-Host "Storage account created for ResourceHealth testing: $storageAccountName"
        Write-Host "Resource ID: $storageAccountResourceId"
    } else {
        Write-Warning "Storage account resource ID not found in deployment outputs"
    }

    Write-Host "ResourceHealth post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete ResourceHealth post-deployment setup: $_"
    throw
}
