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

# Save updated test settings
$testSettings | ConvertTo-Json | Out-File (Join-Path $PSScriptRoot '.testsettings.json') -Encoding UTF8

Write-Host "Test resources deployed successfully for DocumentDB"
Write-Host "Connection string saved to .testsettings.json"

# Initialize test database and collections using MongoDB driver
Write-Host "Initializing test database and collections..."

try {
    # Check if mongosh is available
    $mongoshPath = Get-Command mongosh -ErrorAction SilentlyContinue

    if ($null -eq $mongoshPath) {
        Write-Warning "mongosh not found. Skipping database initialization."
        Write-Warning "You may need to manually create the 'test' database and 'items' collection."
        Write-Warning "Install mongosh from: https://www.mongodb.com/try/download/shell"
    } else {
        $connectionString = $DeploymentOutputs['DOCUMENTDB_CONNECTION_STRING']

        # Wait for firewall rules to propagate
        Write-Host "Waiting for firewall rules to propagate (30 seconds)..."
        Start-Sleep -Seconds 30

        # Create init script
        $initScript = @"
use test
db.createCollection('items')
db.items.insertMany([
    { name: 'item1', value: 100, category: 'A' },
    { name: 'item2', value: 200, category: 'B' },
    { name: 'item3', value: 300, category: 'A' }
])
print('Test database and collection initialized successfully')
"@

        $scriptPath = Join-Path $env:TEMP "documentdb-init.js"
        $initScript | Out-File -FilePath $scriptPath -Encoding UTF8

        Write-Host "Running initialization script..."
        $retries = 3
        $success = $false

        for ($i = 1; $i -le $retries; $i++) {
            try {
                Write-Host "Attempt $i of $retries..."
                & mongosh "$connectionString" --file $scriptPath --quiet
                $success = $true
                Write-Host "Database initialization completed successfully"
                break
            } catch {
                if ($i -lt $retries) {
                    Write-Warning "Connection failed, retrying in 10 seconds..."
                    Start-Sleep -Seconds 10
                } else {
                    throw
                }
            }
        }

        Remove-Item $scriptPath -ErrorAction SilentlyContinue

        if (-not $success) {
            Write-Warning "Database initialization failed after $retries attempts."
            Write-Warning "You may need to manually initialize the database and collection."
        }
    }
} catch {
    Write-Warning "Failed to initialize database: $_"
    Write-Warning "Tests may fail if database and collections don't exist."
    Write-Warning "You can manually run: mongosh `"$($DeploymentOutputs['DOCUMENTDB_CONNECTION_STRING'])`""
}

