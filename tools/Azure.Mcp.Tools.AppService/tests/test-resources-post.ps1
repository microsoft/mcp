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

# $testSettings contains:
# - TenantId
# - TenantName
# - SubscriptionId
# - SubscriptionName
# - ResourceGroupName
# - ResourceBaseName

# $DeploymentOutputs keys are all UPPERCASE

Write-Host "Running App Service post-deployment setup..." -ForegroundColor Yellow

try {
    # Extract common outputs from deployment (keys are UPPERCASE)
    $webAppName = $DeploymentOutputs['WEBAPPNAME']
    $webAppResourceGroup = $DeploymentOutputs['WEBAPPRESOURCEGROUP']

    $sqlServerName = $DeploymentOutputs['SQLSERVERNAME']
    $sqlDatabaseName = $DeploymentOutputs['SQLDATABASENAME']
    $sqlConnectionString = $DeploymentOutputs['SQLCONNECTIONSTRING']

    # $mysqlServerName = $DeploymentOutputs['MYSQLSERVERNAME']
    # $mysqlDatabaseName = $DeploymentOutputs['MYSQLDATABASENAME']
    # $mysqlConnectionString = $DeploymentOutputs['MYSQLCONNECTIONSTRING']

    # $postgresServerName = $DeploymentOutputs['POSTGRESSERVERNAME']
    # $postgresDatabaseName = $DeploymentOutputs['POSTGRESDATABASENAME']
    # $postgresConnectionString = $DeploymentOutputs['POSTGRESCONNECTIONSTRING']

    $cosmosAccountName = $DeploymentOutputs['COSMOSACCOUNTNAME']
    $cosmosDatabaseName = $DeploymentOutputs['COSMOSDATABASENAME']
    $cosmosConnectionString = $DeploymentOutputs['COSMOSCONNECTIONSTRING']

    Write-Host "Web App: $webAppName (resource group: $webAppResourceGroup)" -ForegroundColor Green

    if ($sqlServerName) {
        Write-Host "SQL Server: $sqlServerName, Database: $sqlDatabaseName" -ForegroundColor Green
        Write-Host "SQL Connection String: $sqlConnectionString" -ForegroundColor Green
    }

    # if ($mysqlServerName) {
    #     Write-Host "MySQL Server: $mysqlServerName, Database: $mysqlDatabaseName" -ForegroundColor Green
    #     Write-Host "MySQL Connection String: $mysqlConnectionString" -ForegroundColor Green
    # }

    # if ($postgresServerName) {
    #     Write-Host "Postgres Server: $postgresServerName, Database: $postgresDatabaseName" -ForegroundColor Green
    #     Write-Host "Postgres Connection String: $postgresConnectionString" -ForegroundColor Green
    # }

    if ($cosmosAccountName) {
        Write-Host "Cosmos Account: $cosmosAccountName, Database: $cosmosDatabaseName" -ForegroundColor Green
        Write-Host "Cosmos Connection String: $cosmosConnectionString" -ForegroundColor Green
    }

    Write-Host "App Service post-deployment setup completed successfully." -ForegroundColor Green
}
catch {
    Write-Error "Failed to complete App Service post-deployment setup: $_"
    throw
}

