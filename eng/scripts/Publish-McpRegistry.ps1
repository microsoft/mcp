#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Functions for authenticating and publishing to the MCP registry using Azure Key Vault signing.
.DESCRIPTION
    This script provides functions to:
    - Authenticate with the MCP registry using DNS-based authentication with Azure Key Vault
    - Sign timestamps using ECDSA P-384 (ES384) via Azure Key Vault
    - Publish server.json files to the MCP registry
    
    This implementation replaces the Go-based publisher tool with native PowerShell code.
#>

function ConvertFrom-Base64Url {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Base64Url
    )
    
    # Convert base64url to base64 by replacing URL-safe characters and adding padding
    $base64 = $Base64Url.Replace('-', '+').Replace('_', '/')
    # Round up to the nearest multiple of 4 for base64 padding requirements
    $paddedBase64 = $base64.PadRight(($base64.Length + 3) -band -bnot 3, '=')
    
    return [Convert]::FromBase64String($paddedBase64)
}

function Invoke-AzureKeyVaultSign {
    param(
        [Parameter(Mandatory=$true)]
        [string]$KeyVaultName,
        [Parameter(Mandatory=$true)]
        [string]$KeyName,
        [Parameter(Mandatory=$true)]
        [byte[]]$Digest
    )
    
    $vaultUrl = "https://$KeyVaultName.vault.azure.net"
    
    # Get access token for Key Vault
    $token = Get-AzAccessToken -AsSecureString -ResourceUrl "https://vault.azure.net"
    
    # Prepare sign request
    $headers = @{
        "Authorization" = "Bearer $($token.Token | ConvertFrom-SecureString -AsPlainText)"
        "Content-Type" = "application/json"
    }
    
    # Convert digest to base64url format (no padding)
    $digestBase64 = [Convert]::ToBase64String($Digest).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    
    $body = @{
        alg = "ES384"
        value = $digestBase64
    } | ConvertTo-Json
    
    $signUrl = "$vaultUrl/keys/$KeyName/sign?api-version=7.4"
    
    Write-Host "Executing sign request to Azure Key Vault..."
    $response = Invoke-RestMethod -Uri $signUrl -Method Post -Headers $headers -Body $body
    
    # Return the signature (already in base64url format from Key Vault)
    return $response.value
}

function Get-McpRegistryToken {
    param(
        [Parameter(Mandatory=$true)]
        [string]$RegistryUrl,
        [Parameter(Mandatory=$true)]
        [string]$Domain,
        [Parameter(Mandatory=$true)]
        [string]$KeyVaultName,
        [Parameter(Mandatory=$true)]
        [string]$KeyName
    )
    
    Write-Host "Authenticating with MCP registry using DNS authentication..."
    
    # Generate timestamp in RFC3339 format
    $timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    Write-Host "Timestamp: $timestamp"
    
    # Compute SHA-384 hash of timestamp
    $sha384 = [System.Security.Cryptography.SHA384]::Create()
    try {
        $timestampBytes = [System.Text.Encoding]::UTF8.GetBytes($timestamp)
        $digest = $sha384.ComputeHash($timestampBytes)
    }
    finally {
        $sha384.Dispose()
    }
    
    # Sign the digest with Azure Key Vault
    $signatureBase64Url = Invoke-AzureKeyVaultSign -KeyVaultName $KeyVaultName -KeyName $KeyName -Digest $digest
    
    # Convert base64url signature to hex
    $signatureBytes = ConvertFrom-Base64Url -Base64Url $signatureBase64Url
    $signatureHex = [BitConverter]::ToString($signatureBytes).Replace('-', '').ToLower()
    
    # Exchange signature for registry token
    $authUrl = "$RegistryUrl/v0/auth/dns"
    $authBody = @{
        domain = $Domain
        timestamp = $timestamp
        signed_timestamp = $signatureHex
    } | ConvertTo-Json
    
    Write-Host "Exchanging signature for registry token..."
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $authResponse = Invoke-RestMethod -Uri $authUrl -Method Post -Body $authBody -Headers $headers
    
    Write-Host "✓ Successfully authenticated with MCP registry"
    return $authResponse.registry_token
}

function Publish-McpServerJson {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ServerJsonPath,
        [Parameter(Mandatory=$true)]
        [string]$RegistryUrl,
        [Parameter(Mandatory=$true)]
        [string]$Token
    )
    
    Write-Host "Publishing to $RegistryUrl..."
    
    # Read and validate server.json
    if (!(Test-Path $ServerJsonPath)) {
        throw "Server JSON file not found: $ServerJsonPath"
    }
    
    $body = Get-Content $ServerJsonPath -Raw
    
    # Publish to registry
    $publishUrl = "$RegistryUrl/v0/publish"
    $headers = @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $Token"
    }
    
    $response = Invoke-RestMethod -Uri $publishUrl -Method Post -Body $body -Headers $headers
    
    Write-Host "✓ Successfully published"
    Write-Host "✓ Server $($response.server.name) version $($response.server.version)"
    
    return $response
}

function Publish-ToMcpRegistry {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ServerJsonPath,
        [Parameter(Mandatory=$true)]
        [string]$RegistryUrl,
        [Parameter(Mandatory=$true)]
        [string]$Domain,
        [Parameter(Mandatory=$true)]
        [string]$KeyVaultName,
        [Parameter(Mandatory=$true)]
        [string]$KeyName
    )
    
    # Get registry token
    $token = Get-McpRegistryToken `
        -RegistryUrl $RegistryUrl `
        -Domain $Domain `
        -KeyVaultName $KeyVaultName `
        -KeyName $KeyName
    
    # Publish server.json
    Publish-McpServerJson `
        -ServerJsonPath $ServerJsonPath `
        -RegistryUrl $RegistryUrl `
        -Token $token
}
