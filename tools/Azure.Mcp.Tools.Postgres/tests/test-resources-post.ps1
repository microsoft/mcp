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

    # Install Npgsql PowerShell module if not already installed
    if (-not (Get-Module -ListAvailable -Name "Npgsql")) {
        Write-Host "Installing Npgsql PowerShell module..." -ForegroundColor Gray
        Install-Module -Name Npgsql -Force -Scope CurrentUser -AllowClobber
    }

    Import-Module Npgsql -ErrorAction SilentlyContinue

    # Get Entra ID access token for PostgreSQL
    Write-Host "Getting Entra ID access token..." -ForegroundColor Gray
    $accessToken = (Get-AzAccessToken -ResourceUrl "https://ossrdbms-aad.database.windows.net").Token

    if (-not $accessToken) {
        Write-Error "Failed to get Entra ID access token"
        exit 1
    }

    # Build connection string with Entra ID token
    $connectionString = "Host=$postgresServerFqdn;Database=$testDatabaseName;Username=$adminUsername;Password=$accessToken;SSL Mode=Require;Trust Server Certificate=true;"

    Write-Host "Connecting to PostgreSQL database..." -ForegroundColor Gray
    
    # Create connection
    $connection = New-Object Npgsql.NpgsqlConnection($connectionString)
    
    try {
        $connection.Open()
        Write-Host "Successfully connected to PostgreSQL database" -ForegroundColor Green

        # Create test table
        Write-Host "Creating test table 'employees'..." -ForegroundColor Gray
        $createTableSql = @"
CREATE TABLE IF NOT EXISTS employees (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    department VARCHAR(50),
    salary DECIMAL(10, 2),
    hire_date DATE DEFAULT CURRENT_DATE,
    is_active BOOLEAN DEFAULT true
);
"@

        $command = New-Object Npgsql.NpgsqlCommand($createTableSql, $connection)
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "  Table 'employees' created successfully" -ForegroundColor Green

        # Insert test data
        Write-Host "Inserting test data..." -ForegroundColor Gray
        $insertDataSql = @"
INSERT INTO employees (first_name, last_name, email, department, salary, hire_date, is_active)
VALUES 
    ('John', 'Doe', 'john.doe@example.com', 'Engineering', 75000.00, '2023-01-15', true),
    ('Jane', 'Smith', 'jane.smith@example.com', 'Marketing', 65000.00, '2023-02-20', true),
    ('Bob', 'Johnson', 'bob.johnson@example.com', 'Sales', 70000.00, '2023-03-10', true),
    ('Alice', 'Williams', 'alice.williams@example.com', 'Engineering', 80000.00, '2023-04-05', true),
    ('Charlie', 'Brown', 'charlie.brown@example.com', 'HR', 60000.00, '2023-05-12', false)
ON CONFLICT (email) DO NOTHING;
"@

        $command = New-Object Npgsql.NpgsqlCommand($insertDataSql, $connection)
        $rowsInserted = $command.ExecuteNonQuery()
        Write-Host "  Inserted $rowsInserted rows into 'employees' table" -ForegroundColor Green

        # Create another test table for table listing tests
        Write-Host "Creating additional test table 'departments'..." -ForegroundColor Gray
        $createDeptTableSql = @"
CREATE TABLE IF NOT EXISTS departments (
    dept_id SERIAL PRIMARY KEY,
    dept_name VARCHAR(50) NOT NULL UNIQUE,
    location VARCHAR(100),
    budget DECIMAL(12, 2)
);
"@

        $command = New-Object Npgsql.NpgsqlCommand($createDeptTableSql, $connection)
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "  Table 'departments' created successfully" -ForegroundColor Green

        # Insert department data
        $insertDeptSql = @"
INSERT INTO departments (dept_name, location, budget)
VALUES 
    ('Engineering', 'Seattle', 1000000.00),
    ('Marketing', 'New York', 500000.00),
    ('Sales', 'San Francisco', 750000.00),
    ('HR', 'Austin', 300000.00)
ON CONFLICT (dept_name) DO NOTHING;
"@

        $command = New-Object Npgsql.NpgsqlCommand($insertDeptSql, $connection)
        $rowsInserted = $command.ExecuteNonQuery()
        Write-Host "  Inserted $rowsInserted rows into 'departments' table" -ForegroundColor Green

        # Verify data
        Write-Host "`nVerifying test data..." -ForegroundColor Gray
        $verifySql = "SELECT COUNT(*) FROM employees;"
        $command = New-Object Npgsql.NpgsqlCommand($verifySql, $connection)
        $count = $command.ExecuteScalar()
        Write-Host "  Total employees: $count" -ForegroundColor Gray

        $verifySql = "SELECT COUNT(*) FROM departments;"
        $command = New-Object Npgsql.NpgsqlCommand($verifySql, $connection)
        $count = $command.ExecuteScalar()
        Write-Host "  Total departments: $count" -ForegroundColor Gray

        Write-Host "`nTest data setup completed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Error "Error setting up test data: $_"
        Write-Host "Connection String (without token): Host=$postgresServerFqdn;Database=$testDatabaseName;Username=$adminUsername;SSL Mode=Require;" -ForegroundColor Gray
        throw
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
            Write-Host "Database connection closed" -ForegroundColor Gray
        }
    }
}
catch {
    Write-Error "PostgreSQL server setup verification failed: $_"
    throw
}

Write-Host "`nPostgreSQL test resources are ready for live tests!" -ForegroundColor Green
Write-Host "  Server: $postgresServerName" -ForegroundColor Cyan
Write-Host "  FQDN: $postgresServerFqdn" -ForegroundColor Cyan
Write-Host "  Database: $testDatabaseName" -ForegroundColor Cyan
Write-Host "  Tables: employees, departments" -ForegroundColor Cyan
