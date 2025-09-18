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

        # Configure Entra ID administrator (moved from template to avoid early provisioning race)
        Write-Host "Configuring Entra ID administrator (if needed)..." -ForegroundColor Yellow
        $aadConfigured = $false
        try {
            # Try PowerShell cmdlet first (if available in current Az module version)
            $getAdminCmd = Get-Command -Name Get-AzPostgreSqlFlexibleServerActiveDirectoryAdministrator -ErrorAction SilentlyContinue
            $setAdminCmd = Get-Command -Name Set-AzPostgreSqlFlexibleServerActiveDirectoryAdministrator -ErrorAction SilentlyContinue
            if ($getAdminCmd -and $setAdminCmd) {
                $existingAdmin = Get-AzPostgreSqlFlexibleServerActiveDirectoryAdministrator -Name $postgresServerName -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue
                if ($existingAdmin) {
                    Write-Host "  Entra ID administrator already configured: $($existingAdmin.PrincipalName)" -ForegroundColor Gray
                    $aadConfigured = $true
                } else {
                    $retries = 0; $maxRetries = 5
                    while (-not $aadConfigured -and $retries -lt $maxRetries) {
                        try {
                            Write-Host "  Attempting to set AAD admin via Az module (try $($retries+1)/$maxRetries)..." -ForegroundColor Gray
                            Set-AzPostgreSqlFlexibleServerActiveDirectoryAdministrator -Name $postgresServerName -ResourceGroupName $ResourceGroupName -PrincipalType ServicePrincipal -PrincipalName $TestApplicationId -TenantId $TenantId -ErrorAction Stop | Out-Null
                            Write-Host "  Entra ID administrator configured (Az module)." -ForegroundColor Green
                            $aadConfigured = $true
                        } catch {
                            $retries++
                            if ($retries -ge $maxRetries) {
                                Write-Warning "  Failed to configure AAD admin via Az module after $maxRetries attempts: $($_.Exception.Message)"
                            } else {
                                Start-Sleep -Seconds 15
                            }
                        }
                    }
                }
            }
        } catch { }

        if (-not $aadConfigured) {
            # Fallback to az CLI
            $azExists = Get-Command az -ErrorAction SilentlyContinue
            if (-not $azExists) {
                Write-Warning "  az CLI not available; cannot configure Entra ID administrator. AAD auth may fail."
            } else {
                $retries = 0; $maxRetries = 5
                # Determine object id: if $TestApplicationId looks like a GUID assume object id; otherwise attempt to resolve
                $objectId = $null
                if ($TestApplicationId -match '^[0-9a-fA-F-]{36}$') { $objectId = $TestApplicationId }
                if (-not $objectId) {
                    try {
                        $spLookup = az ad sp list --display-name $TestApplicationId --query "[0].id" -o tsv 2>$null
                        if ($spLookup) { $objectId = $spLookup }
                    } catch { }
                }
                if (-not $objectId) { $objectId = $TestApplicationId }
                while (-not $aadConfigured -and $retries -lt $maxRetries) {
                    try {
                        Write-Host "  Attempting to set AAD admin via az CLI (try $($retries+1)/$maxRetries)..." -ForegroundColor Gray
                        az postgres flexible-server ad-admin create --resource-group $ResourceGroupName --server-name $postgresServerName --object-id $objectId --principal-name $TestApplicationId --principal-type ServicePrincipal --tenant $TenantId 1>$null 2>$null
                        if ($LASTEXITCODE -eq 0) {
                            Write-Host "  Entra ID administrator configured (az CLI)." -ForegroundColor Green
                            $aadConfigured = $true
                        } else {
                            throw "az CLI returned exit code $LASTEXITCODE"
                        }
                    } catch {
                        $retries++
                        if ($retries -ge $maxRetries) {
                            Write-Warning "  Failed to configure AAD admin via az CLI after $maxRetries attempts: $($_.Exception.Message)"
                        } else {
                            Start-Sleep -Seconds 15
                        }
                    }
                }
            }
        }

        if (-not $aadConfigured) {
            Write-Warning "Proceeding without confirmed Entra ID administrator. Data seeding using AAD may fail."
        }

        # Prepare test data (create sample table & rows if possible)
        Write-Host "Preparing PostgreSQL test data..." -ForegroundColor Yellow

        $psqlExists = Get-Command psql -ErrorAction SilentlyContinue
        if (-not $psqlExists) {
            Write-Warning "psql is not available on this agent. Skipping data seed step. (Install PostgreSQL client to enable)"
        }
        else {
            try {
                # Acquire an access token for AAD auth to PostgreSQL Flexible Server (oss-rdbms resource)
                $token = (az account get-access-token --resource-type oss-rdbms --query accessToken -o tsv 2>$null)
                if (-not $token) {
                    Write-Warning "Failed to obtain access token for PostgreSQL. Skipping data seed."
                }
                else {
                    $dbName = "testdb"
                    # Derive AAD user for token-based auth. For service principal connections to PostgreSQL Flexible Server,
                    # the principal name is the object id / app id we configured as admin. Use TestApplicationId.
                    $aadUser = $TestApplicationId
                    $fqdn = $postgresServer.FullyQualifiedDomainName

                    # Use access token as password (AAD auth). Token can be large; set env var for psql.
                    $env:PGPASSWORD = $token

                    $psqlBaseArgs = @('-h', $fqdn, '-d', $dbName, '-U', $aadUser, '-v', 'ON_ERROR_STOP=1', '-w')

                    # Detect table existence
                    $tableExists = $false
                    try {
                        $checkSql = "SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name='todos';"
                        $checkResult = & psql @psqlBaseArgs -t -c $checkSql 2>$null
                        if ($LASTEXITCODE -eq 0 -and ($checkResult.Trim() -eq '1')) { $tableExists = $true }
                    } catch { }

                    if (-not $tableExists) {
                        Write-Host "Creating sample table 'todos'..." -ForegroundColor Gray
                        $createSql = @'
CREATE TABLE IF NOT EXISTS public.todos (
    id SERIAL PRIMARY KEY,
    title TEXT NOT NULL,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS ix_todos_completed ON public.todos(is_completed);
'@
                        & psql @psqlBaseArgs -c $createSql
                        if ($LASTEXITCODE -ne 0) { throw "Failed to create table" }

                        Write-Host "Inserting seed rows into 'todos'..." -ForegroundColor Gray
                        $insertSql = @'
INSERT INTO public.todos (title, is_completed) VALUES
 ('Learn MCP Postgres commands', false),
 ('Create sample data', true),
 ('Verify list & query tests', false)
ON CONFLICT DO NOTHING;
'@
                        & psql @psqlBaseArgs -c $insertSql
                        if ($LASTEXITCODE -ne 0) { throw "Failed to insert seed data" }
                    }
                    else {
                        Write-Host "Table 'todos' already exists; skipping creation." -ForegroundColor Gray
                    }

                    # Provide row count for logging
                    try {
                        $countSql = "SELECT COUNT(*) FROM public.todos;"
                        $rowCount = & psql @psqlBaseArgs -t -c $countSql 2>$null
                        if ($LASTEXITCODE -eq 0) { Write-Host "Seed verification: todos row count = $($rowCount.Trim())" -ForegroundColor Gray }
                    } catch { }
                }
            }
            catch {
                Write-Warning "PostgreSQL data seed step failed: $($_.Exception.Message)"
            }
            finally {
                Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue | Out-Null
            }
        }

        Write-Host "PostgreSQL test resources setup completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "PostgreSQL Server '$postgresServerName' not found"
    }
}
catch {
    Write-Error "Error verifying PostgreSQL Server deployment: $($_.Exception.Message)"
    throw
}
