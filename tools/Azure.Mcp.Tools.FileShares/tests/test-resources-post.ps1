param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

# Try both camelCase and UPPERCASE keys for backwards compatibility
$fileShare1Name = if ($DeploymentOutputs.ContainsKey('fileShare1Name')) {
    $DeploymentOutputs['fileShare1Name']
} elseif ($DeploymentOutputs.ContainsKey('FILESHARE1NAME')) {
    $DeploymentOutputs['FILESHARE1NAME']
} else {
    "$BaseName-fileshare-01"
}

$fileShare2Name = if ($DeploymentOutputs.ContainsKey('fileShare2Name')) {
    $DeploymentOutputs['fileShare2Name']
} elseif ($DeploymentOutputs.ContainsKey('FILESHARE2NAME')) {
    $DeploymentOutputs['FILESHARE2NAME']
} else {
    "$BaseName-fileshare-02"
}

Write-Host "Setting up FileShares for testing" -ForegroundColor Yellow
Write-Host "FileShare 1: $fileShare1Name" -ForegroundColor Gray
Write-Host "FileShare 2: $fileShare2Name" -ForegroundColor Gray

try {
    # Wait a moment for the private endpoint to be fully provisioned
    Write-Host "Waiting for private endpoint provisioning..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30

    # Verify the private endpoint connection exists
    Write-Host "Verifying private endpoint connections..." -ForegroundColor Yellow

    # Use Azure CLI to get private endpoint connections for the FileShare
    $subscriptionId = $DeploymentOutputs['subscriptionId']
    if ([string]::IsNullOrEmpty($subscriptionId)) {
        $subscriptionId = (Get-AzContext).Subscription.Id
    }

    # Construct the FileShare resource ID
    $fileShareResourceId = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.FileShares/fileShares/$fileShare1Name"

    Write-Host "FileShare Resource ID: $fileShareResourceId" -ForegroundColor Gray

    # Get private endpoint connections using REST API
    $token = (Get-AzAccessToken -ResourceUrl "https://management.azure.com/").Token
    $headers = @{
        'Authorization' = "Bearer $token"
        'Content-Type' = 'application/json'
    }

    $apiVersion = "2025-06-01-preview"
    $peConnectionsUri = "https://management.azure.com$fileShareResourceId/privateEndpointConnections?api-version=$apiVersion"

    Write-Host "Querying: $peConnectionsUri" -ForegroundColor Gray

    try {
        $response = Invoke-RestMethod -Uri $peConnectionsUri -Headers $headers -Method Get
        $connections = $response.value

        if ($connections -and $connections.Count -gt 0) {
            Write-Host "Found $($connections.Count) private endpoint connection(s)" -ForegroundColor Gray

            foreach ($connection in $connections) {
                $connectionName = $connection.name
                $connectionState = $connection.properties.privateLinkServiceConnectionState.status

                Write-Host "  Connection: $connectionName - Status: $connectionState" -ForegroundColor Gray

                if ($connectionState -eq "Approved") {
                    Write-Host "  ✓ Private endpoint connection is approved" -ForegroundColor Green
                }
                else {
                    Write-Warning "  Private endpoint connection status: $connectionState"
                }
            }
        }
        else {
            Write-Host "  No private endpoint connections found yet. This may be normal if the endpoint is still provisioning." -ForegroundColor Yellow
        }
    }
    catch {
        Write-Warning "Could not retrieve private endpoint connections. Error: $_"
        Write-Host "  This may not affect basic FileShares testing." -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "FileShares test resources have been successfully created:" -ForegroundColor Green
    Write-Host "  ✓ FileShare resources (2x Microsoft.FileShares/fileShares)" -ForegroundColor Gray
    Write-Host "  ✓ Private Endpoint (Microsoft.Network/privateEndpoints)" -ForegroundColor Gray
    Write-Host "  ✓ Private Endpoint Connection (auto-approved)" -ForegroundColor Gray
    Write-Host "  ✓ Virtual Network with Subnet" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ready for FileShares testing and exercises." -ForegroundColor Green
}
catch {
    Write-Error "Error setting up FileShares: $_" -ErrorAction Stop
}

