#!/usr/bin/env pwsh

Write-Host "Publishing Azure MCP Server..." -ForegroundColor Cyan

try {
    # Resolve and validate project path
    $projectPath = Join-Path $PSScriptRoot "../src"
    if (!(Test-Path $projectPath)) {
        throw "Project path not found: $projectPath"
    }
    $projectPath = Resolve-Path $projectPath
    Write-Host "Project path: $projectPath" -ForegroundColor Gray

    # Setup destination directory
    $dstBase = Join-Path $PSScriptRoot "server"
    Write-Host "Output path: $dstBase" -ForegroundColor Gray

    if (!(Test-Path $dstBase)) {
        Write-Host "Creating output directory..." -ForegroundColor Gray
        New-Item -ItemType Directory -Path $dstBase -Force | Out-Null
    }

    # Run dotnet publish with explicit error handling
    Write-Host "Running dotnet publish..." -ForegroundColor Yellow
    $output = dotnet publish $projectPath -c Release --self-contained false -o $dstBase 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "dotnet publish failed with exit code $LASTEXITCODE" -ForegroundColor Red
        Write-Host "Output:" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        exit $LASTEXITCODE
    }

    Write-Host "Done publishing Azure MCP Server!" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "Error during publish: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
