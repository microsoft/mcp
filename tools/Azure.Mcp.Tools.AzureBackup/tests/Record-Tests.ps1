#!/usr/bin/env pwsh
# Record-Tests.ps1 — Deploy resources, record live tests, push recordings, verify playback
#
# Prerequisites:
#   1. Connect-AzAccount (Azure PowerShell logged in)
#   2. az login (Azure CLI logged in)
#   3. dotnet build completed for the LiveTests project
#
# Usage:
#   cd c:\github\mcp
#   .\tools\Azure.Mcp.Tools.AzureBackup\tests\Record-Tests.ps1

param(
    [switch]$SkipDeploy,
    [switch]$SkipRecord,
    [switch]$SkipPush,
    [switch]$SkipPlayback
)

$ErrorActionPreference = 'Stop'
$RepoRoot = (git rev-parse --show-toplevel).Trim()
$TestDir = "$RepoRoot/tools/Azure.Mcp.Tools.AzureBackup/tests"
$LiveTestDir = "$TestDir/Azure.Mcp.Tools.AzureBackup.LiveTests"
$AssetsJson = "$LiveTestDir/assets.json"
$TestSettings = "$LiveTestDir/.testsettings.json"

Write-Host "`n=== Azure Backup MCP — Recorded Test Setup ===" -ForegroundColor Cyan

# ── Step 0: Verify prerequisites ──
Write-Host "`n[0/5] Checking prerequisites..." -ForegroundColor Yellow

$azContext = Get-AzContext -ErrorAction SilentlyContinue
if (-not $azContext) {
    Write-Error "Not logged in to Azure PowerShell. Run: Connect-AzAccount"
    exit 1
}
Write-Host "  ✓ Azure PowerShell: $($azContext.Account.Id) ($($azContext.Subscription.Name))" -ForegroundColor Green

if (-not (Test-Path $AssetsJson)) {
    Write-Host "  Creating assets.json..." -ForegroundColor Yellow
    @{
        AssetsRepo = "Azure/azure-sdk-assets"
        AssetsRepoPrefixPath = ""
        TagPrefix = "Azure.Mcp.Tools.AzureBackup.LiveTests"
        Tag = ""
    } | ConvertTo-Json | Set-Content $AssetsJson
}
Write-Host "  ✓ assets.json exists" -ForegroundColor Green

# ── Step 1: Deploy test resources ──
if (-not $SkipDeploy) {
    Write-Host "`n[1/5] Deploying test resources..." -ForegroundColor Yellow
    & "$RepoRoot/eng/scripts/Deploy-TestResources.ps1" -Paths AzureBackup
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Deploy failed. Check Azure permissions and subscription."
        exit 1
    }
    Write-Host "  ✓ Test resources deployed" -ForegroundColor Green
} else {
    Write-Host "`n[1/5] Skipping deploy (--SkipDeploy)" -ForegroundColor DarkGray
}

# ── Step 2: Build ──
Write-Host "`n[2/5] Building LiveTests project..." -ForegroundColor Yellow
dotnet build $LiveTestDir
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit 1
}
Write-Host "  ✓ Build succeeded" -ForegroundColor Green

# ── Step 3: Record tests ──
if (-not $SkipRecord) {
    Write-Host "`n[3/5] Recording tests (TestMode=Record)..." -ForegroundColor Yellow
    @{ TestMode = "Record" } | ConvertTo-Json | Set-Content $TestSettings
    
    dotnet test $LiveTestDir --no-build
    $recordExit = $LASTEXITCODE
    
    if ($recordExit -ne 0) {
        Write-Warning "Some tests failed during recording. Review output above."
        Write-Host "  Continuing to check if recordings were created..." -ForegroundColor Yellow
    } else {
        Write-Host "  ✓ All tests recorded successfully" -ForegroundColor Green
    }
} else {
    Write-Host "`n[3/5] Skipping record (--SkipRecord)" -ForegroundColor DarkGray
}

# ── Step 4: Push recordings ──
if (-not $SkipPush) {
    Write-Host "`n[4/5] Pushing recordings to assets repo..." -ForegroundColor Yellow
    
    # Find or download test proxy
    $proxy = Get-ChildItem -Path "$RepoRoot/.proxy" -Filter "Azure.Sdk.Tools.TestProxy*" -Recurse | Select-Object -First 1
    if (-not $proxy) {
        Write-Host "  Test proxy not found. Running a quick test to trigger download..." -ForegroundColor Yellow
        @{ TestMode = "Playback" } | ConvertTo-Json | Set-Content $TestSettings
        dotnet test $LiveTestDir --no-build --filter "DOESNOTEXIST" 2>$null
        $proxy = Get-ChildItem -Path "$RepoRoot/.proxy" -Filter "Azure.Sdk.Tools.TestProxy*" -Recurse | Select-Object -First 1
    }
    
    if ($proxy) {
        Write-Host "  Using proxy: $($proxy.FullName)" -ForegroundColor DarkGray
        & $proxy.FullName push -a $AssetsJson
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Recordings pushed. assets.json updated with Tag." -ForegroundColor Green
            Get-Content $AssetsJson
        } else {
            Write-Error "Push failed. You may need to configure git credentials for Azure/azure-sdk-assets."
            exit 1
        }
    } else {
        Write-Error "Test proxy binary not found in .proxy/. Run tests once to trigger download."
        exit 1
    }
} else {
    Write-Host "`n[4/5] Skipping push (--SkipPush)" -ForegroundColor DarkGray
}

# ── Step 5: Verify playback ──
if (-not $SkipPlayback) {
    Write-Host "`n[5/5] Verifying playback mode..." -ForegroundColor Yellow
    @{ TestMode = "Playback" } | ConvertTo-Json | Set-Content $TestSettings
    
    dotnet test $LiveTestDir --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Playback verification failed! Recordings may be incomplete."
        exit 1
    }
    Write-Host "  ✓ Playback verification passed" -ForegroundColor Green
} else {
    Write-Host "`n[5/5] Skipping playback (--SkipPlayback)" -ForegroundColor DarkGray
}

# ── Cleanup ──
Remove-Item $TestSettings -ErrorAction SilentlyContinue
Write-Host "`n=== Done! ===" -ForegroundColor Cyan
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Review assets.json — it should now have a populated Tag field"
Write-Host "  2. git add $AssetsJson"
Write-Host "  3. git commit and push"
Write-Host ""
