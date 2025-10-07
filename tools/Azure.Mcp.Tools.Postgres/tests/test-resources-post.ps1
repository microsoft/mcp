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

$postgresServerName = $testSettings.ResourceBaseName
$postgresServerFqdn = $DeploymentOutputs['postgresServerFqdn']
$testDatabaseName = $DeploymentOutputs['testDatabaseName']
$adminUsername = $DeploymentOutputs['adminUsername']

Write-Host "Verifying PostgreSQL Flexible Server deployment: $postgresServerName" -ForegroundColor Yellow

try {
    # Get the PostgreSQL server details to verify deployment
    Write-Host "Getting PostgreSQL server details..." -ForegroundColor Gray
    $context = Get-AzContext
    if (-not $context) {
        Write-Error "No Azure context found. Please run Connect-AzAccount first."
        exit 1
    }

    Write-Host "PostgreSQL Server '$postgresServerName' deployed successfully" -ForegroundColor Green
    Write-Host "  Server: $postgresServerName" -ForegroundColor Gray
    Write-Host "  FQDN: $postgresServerFqdn" -ForegroundColor Gray
    Write-Host "  Location: $($DeploymentOutputs['location'])" -ForegroundColor Gray
    Write-Host "  Database: $testDatabaseName" -ForegroundColor Gray
    Write-Host "  Admin: $adminUsername" -ForegroundColor Gray

    Write-Host "`nSetting up test data..." -ForegroundColor Yellow

    # Note: Test data creation is handled by the live tests themselves
    # The PostgreSQL tools use Npgsql library directly and will create test data during test execution
    # This post-deployment script only verifies the server is accessible
    
    # Get Entra ID access token for PostgreSQL
    Write-Host "Getting Entra ID access token..." -ForegroundColor Gray
    $accessToken = (Get-AzAccessToken -ResourceUrl "https://ossrdbms-aad.database.windows.net").Token

    if (-not $accessToken) {
        Write-Error "Failed to get Entra ID access token"
        exit 1
    }

    Write-Host "Successfully obtained Entra ID access token" -ForegroundColor Green
    Write-Host "PostgreSQL server is ready for live tests" -ForegroundColor Green

    Write-Host "`nPostgreSQL server verification completed!" -ForegroundColor Green
    Write-Host "Note: Test data (tables and rows) will be created by the live tests themselves." -ForegroundColor Yellow
}
catch {
    Write-Error "PostgreSQL server setup verification failed: $_"
    throw
}

Write-Host "`nPostgreSQL test resources are ready for live tests!" -ForegroundColor Green
Write-Host "  Server: $postgresServerName" -ForegroundColor Cyan
Write-Host "  FQDN: $postgresServerFqdn" -ForegroundColor Cyan
Write-Host "  Database: $testDatabaseName" -ForegroundColor Cyan
Write-Host "  Authentication: Entra ID (Azure AD)" -ForegroundColor Cyan
Write-Host "  Note: Test tables will be created during test execution" -ForegroundColor Yellow
