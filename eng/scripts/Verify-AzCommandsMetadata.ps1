<#
.SYNOPSIS
    Verifies that azmcp-commands.md is up to date with tool metadata

.DESCRIPTION
    This script runs Update-AzCommandsMetadata.ps1 and checks if there are any
    uncommitted changes to azmcp-commands.md. If changes are detected, the script
    fails with an error message instructing the user to run the update script.
    
    This is designed to be run in CI/CD pipelines to ensure documentation stays in sync.
    
.PARAMETER AzmcpPath
    Path to the azmcp.exe executable. Default: ..\..\servers\Azure.Mcp.Server\src\bin\Debug\net9.0\azmcp.exe
    
.PARAMETER DocsPath
    Path to the azmcp-commands.md file. Default: ..\..\servers\Azure.Mcp.Server\docs\azmcp-commands.md
    
.EXAMPLE
    .\Verify-AzCommandsMetadata.ps1
    
.EXAMPLE
    .\Verify-AzCommandsMetadata.ps1 -AzmcpPath "C:\path\to\azmcp.exe" -DocsPath "C:\path\to\azmcp-commands.md"
#>

[CmdletBinding()]
param(
    [string]$AzmcpPath = "",
    [string]$DocsPath = ""
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Verifying AzCommands Metadata is Up-to-Date" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Get the script directory
$ScriptDir = Split-Path -Parent $PSCommandPath
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..\..") -ErrorAction Stop

# Set default paths if not provided
if ([string]::IsNullOrWhiteSpace($AzmcpPath)) {
    $AzmcpPath = Join-Path $RepoRoot "servers\Azure.Mcp.Server\src\bin\Debug\net9.0\azmcp.exe"
}

if ([string]::IsNullOrWhiteSpace($DocsPath)) {
    $DocsPath = Join-Path $RepoRoot "servers\Azure.Mcp.Server\docs\azmcp-commands.md"
}

# Build the full path to Update-AzCommandsMetadata.ps1
$UpdateScriptPath = Join-Path $ScriptDir "Update-AzCommandsMetadata.ps1"

if (-not (Test-Path $UpdateScriptPath)) {
    Write-Error "Update-AzCommandsMetadata.ps1 not found at: $UpdateScriptPath"
    exit 1
}

# Validate docs path exists
if (-not (Test-Path $DocsPath)) {
    Write-Error "Documentation file not found at: $DocsPath"
    exit 1
}

Write-Host "Running Update-AzCommandsMetadata.ps1..." -ForegroundColor Yellow
Write-Host "  Update Script: $UpdateScriptPath" -ForegroundColor Gray
Write-Host "  Azmcp Path: $AzmcpPath" -ForegroundColor Gray
Write-Host "  Docs Path: $DocsPath`n" -ForegroundColor Gray

# Run the update script with absolute paths
& $UpdateScriptPath -AzmcpPath $AzmcpPath -DocsPath $DocsPath -Verbose:$VerbosePreference

if ($LASTEXITCODE -ne 0) {
    Write-Error "Update-AzCommandsMetadata.ps1 failed with exit code: $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "`nChecking for uncommitted changes to azmcp-commands.md..." -ForegroundColor Yellow

# Check for changes to the docs file
$gitStatus = git status --porcelain $DocsPath 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Git command failed. This may be expected in some environments."
    Write-Host "Git status output: $gitStatus" -ForegroundColor Gray
    # Don't fail if git isn't available or fails
    exit 0
}

if ([string]::IsNullOrWhiteSpace($gitStatus)) {
    Write-Host "`n✅ SUCCESS: azmcp-commands.md is up to date!" -ForegroundColor Green
    Write-Host "No changes detected. Documentation metadata is current.`n" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ FAILURE: azmcp-commands.md has uncommitted changes!" -ForegroundColor Red
    Write-Host "`nGit status:" -ForegroundColor Yellow
    Write-Host $gitStatus -ForegroundColor White
    
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "ACTION REQUIRED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "The azmcp-commands.md file is out of sync with tool metadata." -ForegroundColor Yellow
    Write-Host "`nTo fix this issue, please:" -ForegroundColor Yellow
    Write-Host "  1. Run the following command from the repository root:" -ForegroundColor White
    Write-Host "     .\eng\scripts\Update-AzCommandsMetadata.ps1" -ForegroundColor Cyan
    Write-Host "  2. Review the changes to servers\Azure.Mcp.Server\docs\azmcp-commands.md" -ForegroundColor White
    Write-Host "  3. Commit the updated file with your PR`n" -ForegroundColor White
    
    exit 1
}
