#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

#Requires -Version 7.0

param(
    [string] $ResourceType,
    [string] $TestResourcesDirectory,
    [hashtable] $AdditionalParameters,
    [hashtable] $EnvironmentVariables,
    [int] $DeleteAfterHours,
    [switch] $CI,
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

$fabricApiEndpoint = 'https://api.fabric.microsoft.com'
$workspaceName = if ($AdditionalParameters -and $AdditionalParameters.ContainsKey('WorkspaceName')) {
    $AdditionalParameters['WorkspaceName']
} else {
    "azmcp-onelake-$([Guid]::NewGuid().ToString('N').Substring(0, 12))"
}

$accessToken = az account get-access-token `
    --resource $fabricApiEndpoint `
    --query accessToken `
    --output tsv
if ($LASTEXITCODE -ne 0 -or !$accessToken) {
    throw "Unable to acquire a Microsoft Fabric access token. Run 'az login' and verify that the account can create Fabric workspaces."
}

$headers = @{ Authorization = "Bearer $accessToken" }
$body = @{ displayName = $workspaceName } | ConvertTo-Json
$workspace = Invoke-RestMethod `
    -Method Post `
    -Uri "$fabricApiEndpoint/v1/workspaces" `
    -Headers $headers `
    -ContentType 'application/json' `
    -Body $body

if (!$workspace.id) {
    throw 'Microsoft Fabric did not return an ID for the created workspace.'
}

@{
    EnvironmentVariables = @{
        FABRIC_API_ENDPOINT = $fabricApiEndpoint
        ONELAKE_TEST_WORKSPACE_ID = $workspace.id
        ONELAKE_TEST_WORKSPACE_NAME = $workspace.displayName
    }
}