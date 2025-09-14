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

Write-Host "Running Speech post-deployment setup..."

try {
    # Extract outputs from deployment
    $aiServicesName = $AdditionalParameters['aiServicesName']
    $aiServicesEndpoint = $AdditionalParameters['aiServicesEndpoint']
    $aiServicesLocation = $AdditionalParameters['aiServicesLocation']

    Write-Host "Azure AI Services '$aiServicesName' deployed successfully."
    Write-Host "Endpoint: $aiServicesEndpoint"
    Write-Host "Location: $aiServicesLocation"

    Write-Host "Azure AI Services post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete Azure AI Services post-deployment setup: $_"
    throw
}
