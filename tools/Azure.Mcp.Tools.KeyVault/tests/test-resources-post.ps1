param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

$keyVaultName = $BaseName

Write-Host "Setting up Key Vault certificates for testing: $keyVaultName" -ForegroundColor Yellow

try {
    # Check if Key Vault exists
    $keyVault = Get-AzKeyVault -VaultName $keyVaultName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue

    if (-not $keyVault) {
        Write-Warning "Key Vault '$keyVaultName' not found in resource group '$ResourceGroupName'"
        return
    }

    Write-Host "Key Vault found: $($keyVault.VaultUri)" -ForegroundColor Green

    # Create a self-signed certificate for testing (matching the name from the original Bicep attempt)
    $certificateName = "foo-bar-certificate"

    Write-Host "Creating self-signed certificate: $certificateName" -ForegroundColor Yellow

    # Check if certificate already exists
    $existingCert = Get-AzKeyVaultCertificate -VaultName $keyVaultName -Name $certificateName -ErrorAction SilentlyContinue

    if ($existingCert) {
        Write-Host "Certificate '$certificateName' already exists" -ForegroundColor Gray
        Write-Host "  - Certificate ID: $($existingCert.Id)" -ForegroundColor Gray
        Write-Host "  - Thumbprint: $($existingCert.Thumbprint)" -ForegroundColor Gray
        if ($existingCert.Certificate) {
            Write-Host "  - Subject: $($existingCert.Certificate.Subject)" -ForegroundColor Gray
            Write-Host "  - Valid From: $($existingCert.Certificate.NotBefore)" -ForegroundColor Gray
            Write-Host "  - Valid To: $($existingCert.Certificate.NotAfter)" -ForegroundColor Gray
        }
    }
    else {
        # Create certificate policy (matching the configuration from the original Bicep)
        $policy = New-AzKeyVaultCertificatePolicy `
            -SubjectName "CN=$certificateName" `
            -IssuerName "Self" `
            -ValidityInMonths 12 `
            -ReuseKeyOnRenewal `
            -KeySize 2048 `
            -KeyType "RSA" `
            -KeyUsage @("DigitalSignature", "KeyEncipherment") `
            -SecretContentType "application/x-pkcs12"

        # Create the certificate
        $certOperation = Add-AzKeyVaultCertificate -VaultName $keyVaultName -Name $certificateName -CertificatePolicy $policy

        if ($certOperation) {
            Write-Host "Certificate creation initiated. Status: $($certOperation.Status)" -ForegroundColor Green

            # Wait for certificate creation to complete (self-signed certificates are usually quick)
            $maxWaitTime = 60 # seconds
            $waitTime = 0
            $pollInterval = 5

            do {
                Start-Sleep -Seconds $pollInterval
                $waitTime += $pollInterval
                $cert = Get-AzKeyVaultCertificate -VaultName $keyVaultName -Name $certificateName -ErrorAction SilentlyContinue

                if ($cert -and $cert.Certificate) {
                    Write-Host "Certificate '$certificateName' created successfully!" -ForegroundColor Green
                    Write-Host "  - Certificate ID: $($cert.Id)" -ForegroundColor Gray
                    Write-Host "  - Thumbprint: $($cert.Thumbprint)" -ForegroundColor Gray
                    Write-Host "  - Subject: $($cert.Certificate.Subject)" -ForegroundColor Gray
                    Write-Host "  - Valid From: $($cert.Certificate.NotBefore)" -ForegroundColor Gray
                    Write-Host "  - Valid To: $($cert.Certificate.NotAfter)" -ForegroundColor Gray
                    break
                }

                Write-Host "Waiting for certificate creation to complete... ($waitTime/$maxWaitTime seconds)" -ForegroundColor Yellow

            } while ($waitTime -lt $maxWaitTime)

            if ($waitTime -ge $maxWaitTime) {
                Write-Warning "Certificate creation is taking longer than expected. It may still be in progress."
                Write-Host "You can check the status later with: Get-AzKeyVaultCertificate -VaultName $keyVaultName -Name $certificateName" -ForegroundColor Gray
            }
        }
        else {
            Write-Warning "Failed to initiate certificate creation"
        }
    }

    Write-Host "Key Vault certificate setup completed!" -ForegroundColor Green
    Write-Host "The certificate can be used for testing the Key Vault certificate MCP commands." -ForegroundColor Gray

}
catch {
    Write-Error "Failed to setup Key Vault certificates: $($_.Exception.Message)"
    Write-Host "Error details: $($_.Exception)" -ForegroundColor Red

    # Provide helpful information for common issues
    Write-Host ""
    Write-Host "Common issues and solutions:" -ForegroundColor Yellow
    Write-Host "1. Insufficient permissions: Ensure you have 'Key Vault Certificates Officer' role" -ForegroundColor Gray
    Write-Host "2. Key Vault not found: Verify the Key Vault was deployed successfully" -ForegroundColor Gray
    Write-Host "3. RBAC issues: Check that RBAC authorization is properly configured" -ForegroundColor Gray

    throw
}
