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
    Write-Host "FileShares test resources have been successfully created:" -ForegroundColor Green
    Write-Host "  ✓ FileShare resources (2x Microsoft.FileShares/fileShares)" -ForegroundColor Gray
    Write-Host "  ✓ Private Endpoint (Microsoft.Network/privateEndpoints)" -ForegroundColor Gray
    Write-Host "  ✓ Virtual Network with Subnet" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ready for FileShares testing and exercises." -ForegroundColor Green
}
catch {
    Write-Error "Error setting up FileShares: $_" -ErrorAction Stop
}
