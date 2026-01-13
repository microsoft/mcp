#!/bin/env pwsh
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

function Get-AzureKeyVaultPublicKey {
    param(
        [Parameter(Mandatory=$true)]
        [string]$KeyVaultName,
        [Parameter(Mandatory=$true)]
        [string]$KeyName
    )
    
    $vaultUrl = "https://$KeyVaultName.vault.azure.net"
    $keyUrl = "$vaultUrl/keys/$KeyName"
    
    Write-Host "Getting public key from Azure Key Vault: $KeyVaultName, key: $KeyName"
    
    # Get access token for Key Vault
    $token = Get-AzAccessToken -AsSecureString -ResourceUrl "https://vault.azure.net"
    
    # Get key details including public key
    $headers = @{
        "Authorization" = "Bearer $($token.Token | ConvertFrom-SecureString -AsPlainText)"
        "Content-Type" = "application/json"
    }
    
    $response = Invoke-RestMethod -Uri "$keyUrl/?api-version=7.4" -Method Get -Headers $headers
    
    if ($response.key.kty -ne "EC" -and $response.key.kty -ne "EC-HSM") {
        throw "Unsupported key type: $($response.key.kty). Only EC or EC-HSM keys are supported."
    }
    
    if ($response.key.crv -ne "P-384") {
        throw "Unsupported curve: $($response.key.crv). Only P-384 is supported."
    }
    
    return @{
        X = $response.key.x
        Y = $response.key.y
        Curve = $response.key.crv
        KeyId = $response.key.kid
    }
}

function Get-CompressedPublicKey {
    param(
        [Parameter(Mandatory=$true)]
        [string]$X,
        [Parameter(Mandatory=$true)]
        [string]$Y
    )
    
    # Decode base64url to bytes
    $xBytes = [Convert]::FromBase64String(($X.Replace('-', '+').Replace('_', '/').PadRight(($X.Length + 3) -band -bnot 3, '=')))
    $yBytes = [Convert]::FromBase64String(($Y.Replace('-', '+').Replace('_', '/').PadRight(($Y.Length + 3) -band -bnot 3, '=')))
    
    # For P-384, compressed format is: 0x02/0x03 (depending on Y's parity) + X coordinate
    # If Y is even, use 0x02; if Y is odd, use 0x03
    $yLast = $yBytes[$yBytes.Length - 1]
    $prefix = if (($yLast -band 1) -eq 0) { 0x02 } else { 0x03 }
    
    $compressed = [byte[]]::new($xBytes.Length + 1)
    $compressed[0] = $prefix
    [Array]::Copy($xBytes, 0, $compressed, 1, $xBytes.Length)
    
    return [Convert]::ToBase64String($compressed)
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
    
    # Get public key info
    $keyInfo = Get-AzureKeyVaultPublicKey -KeyVaultName $KeyVaultName -KeyName $KeyName
    
    # Get compressed public key for proof record
    $compressedPublicKey = Get-CompressedPublicKey -X $keyInfo.X -Y $keyInfo.Y
    Write-Host "Expected proof record:"
    Write-Host "v=MCPv1; k=ecdsap384; p=$compressedPublicKey"
    
    # Generate timestamp in RFC3339 format
    $timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    Write-Host "Timestamp: $timestamp"
    
    # Compute SHA-384 hash of timestamp
    $sha384 = [System.Security.Cryptography.SHA384]::Create()
    $timestampBytes = [System.Text.Encoding]::UTF8.GetBytes($timestamp)
    $digest = $sha384.ComputeHash($timestampBytes)
    
    # Sign the digest with Azure Key Vault
    $signatureBase64Url = Invoke-AzureKeyVaultSign -KeyVaultName $KeyVaultName -KeyName $KeyName -Digest $digest
    
    # Convert base64url signature to hex
    $signatureBytes = [Convert]::FromBase64String(($signatureBase64Url.Replace('-', '+').Replace('_', '/').PadRight(($signatureBase64Url.Length + 3) -band -bnot 3, '=')))
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
    
    $serverJson = Get-Content $ServerJsonPath -Raw | ConvertFrom-Json
    
    # Publish to registry
    $publishUrl = "$RegistryUrl/v0/publish"
    $headers = @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer $Token"
    }
    
    $body = Get-Content $ServerJsonPath -Raw
    
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
