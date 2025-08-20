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

$sqlServerName = $testSettings.ResourceBaseName

Write-Host "Verifying SQL Server deployment: $sqlServerName" -ForegroundColor Yellow

# Get the SQL server details to verify deployment
$sqlServer = Get-AzSqlServer -ResourceGroupName $ResourceGroupName -ServerName $sqlServerName

if ($sqlServer) {
    Write-Host "SQL Server '$sqlServerName' deployed successfully" -ForegroundColor Green
    Write-Host "  Server: $($sqlServer.ServerName)" -ForegroundColor Gray
    Write-Host "  FQDN: $($sqlServer.FullyQualifiedDomainName)" -ForegroundColor Gray
    Write-Host "  Location: $($sqlServer.Location)" -ForegroundColor Gray

    # List databases
    $databases = Get-AzSqlDatabase -ResourceGroupName $ResourceGroupName -ServerName $sqlServerName
    Write-Host "  Databases:" -ForegroundColor Gray
    foreach ($db in $databases) {
        if ($db.DatabaseName -ne "master") {
            Write-Host "    - $($db.DatabaseName) ($($db.Edition))" -ForegroundColor Gray
        }
    }
} else {
    Write-Error "SQL Server '$sqlServerName' not found"
}
