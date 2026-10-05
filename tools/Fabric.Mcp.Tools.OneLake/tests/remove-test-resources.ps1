#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

#Requires -Version 7.0

param(
    [string] $ResourceType,
    [string] $TestResourcesDirectory,
    [switch] $CI,
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

$workspaceId = $env:ONELAKE_TEST_WORKSPACE_ID
if (!$workspaceId) {
    Write-Warning 'ONELAKE_TEST_WORKSPACE_ID is not set; no Fabric workspace can be removed.'
    return
}

$fabricApiEndpoint = if ($env:FABRIC_API_ENDPOINT) {
    $env:FABRIC_API_ENDPOINT.TrimEnd('/')
} else {
    'https://api.fabric.microsoft.com'
}

$accessToken = az account get-access-token `
    --resource $fabricApiEndpoint `
    --query accessToken `
    --output tsv
if ($LASTEXITCODE -ne 0 -or !$accessToken) {
    throw "Unable to acquire a Microsoft Fabric access token. Run 'az login' and verify that the account can delete Fabric workspaces."
}

try {
    Invoke-RestMethod `
        -Method Delete `
        -Uri "$fabricApiEndpoint/v1/workspaces/$workspaceId" `
        -Headers @{ Authorization = "Bearer $accessToken" }
} catch {
    if ($_.Exception.Response.StatusCode -ne 404) {
        throw
    }
}