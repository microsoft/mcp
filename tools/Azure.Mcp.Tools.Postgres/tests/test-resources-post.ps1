
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

$postgresServerName = "$($testSettings.ResourceBaseName)-postgres"

Write-Host "Verifying PostgreSQL Server deployment: $postgresServerName" -ForegroundColor Yellow

# Get the PostgreSQL server details to verify deployment
try {
    $postgresServer = Get-AzPostgreSqlFlexibleServer -ResourceGroupName $ResourceGroupName -Name $postgresServerName

    if ($postgresServer) {
        Write-Host "PostgreSQL Server '$postgresServerName' deployed successfully" -ForegroundColor Green
        Write-Host "  Server: $($postgresServer.Name)" -ForegroundColor Gray
        Write-Host "  FQDN: $($postgresServer.FullyQualifiedDomainName)" -ForegroundColor Gray
        Write-Host "  Location: $($postgresServer.Location)" -ForegroundColor Gray
        Write-Host "  Version: $($postgresServer.Version)" -ForegroundColor Gray
        Write-Host "  State: $($postgresServer.State)" -ForegroundColor Gray

        # List databases
        try {
            $databases = Get-AzPostgreSqlFlexibleServerDatabase -ResourceGroupName $ResourceGroupName -ServerName $postgresServerName
            Write-Host "  Databases:" -ForegroundColor Gray
            foreach ($db in $databases) {
                Write-Host "    - $($db.Name) (Charset: $($db.Charset), Collation: $($db.Collation))" -ForegroundColor Gray
            }
        }
        catch {
            Write-Warning "Could not list databases: $($_.Exception.Message)"
        }

        # List firewall rules
        try {
            $firewallRules = Get-AzPostgreSqlFlexibleServerFirewallRule -ResourceGroupName $ResourceGroupName -ServerName $postgresServerName
            Write-Host "  Firewall Rules:" -ForegroundColor Gray
            foreach ($rule in $firewallRules) {
                Write-Host "    - $($rule.Name): $($rule.StartIpAddress) - $($rule.EndIpAddress)" -ForegroundColor Gray
            }
        }
        catch {
            Write-Warning "Could not list firewall rules: $($_.Exception.Message)"
        }

        # Wait for server to be ready
        Write-Host "Waiting for PostgreSQL server to be ready..." -ForegroundColor Yellow
        $maxWaitTime = 300 # 5 minutes
        $waitInterval = 15 # 15 seconds
        $elapsedTime = 0

        do {
            Start-Sleep -Seconds $waitInterval
            $elapsedTime += $waitInterval
            $currentServer = Get-AzPostgreSqlFlexibleServer -ResourceGroupName $ResourceGroupName -Name $postgresServerName
            Write-Host "  Server state: $($currentServer.State)" -ForegroundColor Gray
            
            if ($currentServer.State -eq "Ready") {
                Write-Host "PostgreSQL server is ready!" -ForegroundColor Green
                break
            }
            
            if ($elapsedTime -ge $maxWaitTime) {
                Write-Warning "Timeout waiting for PostgreSQL server to be ready. Current state: $($currentServer.State)"
                break
            }
        } while ($currentServer.State -ne "Ready")

        # Prepare test data
        Write-Host "Preparing test data..." -ForegroundColor Yellow
        
        # The connection string and data preparation would typically be done here
        # However, since we're using MCP tools for testing, the actual data preparation
        # will be done as part of the live tests themselves
        
        Write-Host "PostgreSQL test resources setup completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "PostgreSQL Server '$postgresServerName' not found"
    }
}
catch {
    Write-Error "Error verifying PostgreSQL Server deployment: $($_.Exception.Message)"
    throw
}
