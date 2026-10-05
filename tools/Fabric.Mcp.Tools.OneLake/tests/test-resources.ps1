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
$lakehouseName = if ($AdditionalParameters -and $AdditionalParameters.ContainsKey('LakehouseName')) {
    $AdditionalParameters['LakehouseName']
} else {
    "azmcp-lakehouse-$([Guid]::NewGuid().ToString('N').Substring(0, 12))"
}
$capacityId = if ($AdditionalParameters -and $AdditionalParameters.ContainsKey('CapacityId')) {
    $AdditionalParameters['CapacityId']
} else {
    $env:FABRIC_TEST_CAPACITY_ID
}
if (!$capacityId) {
    throw "A Fabric capacity is required for OneLake testing. Pass -AdditionalParameters @{ CapacityId = '<capacity-id>' } or set FABRIC_TEST_CAPACITY_ID."
}

$accessToken = az account get-access-token `
    --resource $fabricApiEndpoint `
    --query accessToken `
    --output tsv
if ($LASTEXITCODE -ne 0 -or !$accessToken) {
    throw "Unable to acquire a Microsoft Fabric access token. Run 'az login' and verify that the account can create Fabric workspaces."
}

$headers = @{ Authorization = "Bearer $accessToken" }
$body = @{
    displayName = $workspaceName
    capacityId = $capacityId
} | ConvertTo-Json
$workspace = Invoke-RestMethod `
    -Method Post `
    -Uri "$fabricApiEndpoint/v1/workspaces" `
    -Headers $headers `
    -ContentType 'application/json' `
    -Body $body

if (!$workspace.id) {
    throw 'Microsoft Fabric did not return an ID for the created workspace.'
}

try {
    $lakehouseResponse = Invoke-WebRequest `
        -Method Post `
        -Uri "$fabricApiEndpoint/v1/workspaces/$($workspace.id)/lakehouses" `
        -Headers $headers `
        -ContentType 'application/json' `
        -Body (@{ displayName = $lakehouseName } | ConvertTo-Json) `
        -SkipHttpErrorCheck

    if ($lakehouseResponse.StatusCode -eq 201) {
        $lakehouse = $lakehouseResponse.Content | ConvertFrom-Json
    } elseif ($lakehouseResponse.StatusCode -eq 202) {
        $operationUrl = $lakehouseResponse.Headers.Location | Select-Object -First 1
        if (!$operationUrl) {
            throw 'Microsoft Fabric did not return an operation URL for the Lakehouse creation request.'
        }

        $deadline = [DateTime]::UtcNow.AddMinutes(5)
        do {
            Start-Sleep -Seconds 10
            $operation = Invoke-RestMethod -Method Get -Uri $operationUrl -Headers $headers
            if ($operation.status -eq 'Failed') {
                throw "Microsoft Fabric failed to create Lakehouse '$lakehouseName'."
            }
        } while ($operation.status -ne 'Succeeded' -and [DateTime]::UtcNow -lt $deadline)

        if ($operation.status -ne 'Succeeded') {
            throw "Microsoft Fabric did not create Lakehouse '$lakehouseName' within 5 minutes."
        }

        $lakehouse = Invoke-RestMethod -Method Get -Uri "$operationUrl/result" -Headers $headers
    } else {
        throw "Microsoft Fabric returned HTTP $($lakehouseResponse.StatusCode) while creating Lakehouse '$lakehouseName': $($lakehouseResponse.Content)"
    }

    if (!$lakehouse.id) {
        throw 'Microsoft Fabric did not return an ID for the created Lakehouse.'
    }
} catch {
    $provisioningError = $_
    try {
        Invoke-RestMethod `
            -Method Delete `
            -Uri "$fabricApiEndpoint/v1/workspaces/$($workspace.id)" `
            -Headers $headers
    } catch {
        Write-Warning "Failed to remove workspace '$($workspace.id)' after Lakehouse provisioning failed: $_"
    }
    throw $provisioningError
}

@{
    EnvironmentVariables = @{
        FABRIC_API_ENDPOINT = $fabricApiEndpoint
        ONELAKE_TEST_WORKSPACE_ID = $workspace.id
        ONELAKE_TEST_WORKSPACE_NAME = $workspace.displayName
        ONELAKE_TEST_ITEM_NAME = $lakehouse.displayName
    }
}