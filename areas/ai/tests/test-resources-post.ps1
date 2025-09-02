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

Write-Host "Running AI post-deployment setup..."

try {
    # Extract outputs from deployment
    $openaiServiceName = $AdditionalParameters['openaiServiceName']
    $testDeploymentName = $AdditionalParameters['testDeploymentName']
    $endpoint = $AdditionalParameters['endpoint']

    Write-Host "Azure OpenAI service created: $openaiServiceName"
    Write-Host "Test deployment created: $testDeploymentName"
    Write-Host "Endpoint: $endpoint"

    Write-Host "AI post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete AI post-deployment setup: $_"
    throw
}
