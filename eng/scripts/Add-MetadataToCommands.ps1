#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
.SYNOPSIS
    Adds metadata information above each command in azmcp-commands.md without reordering content.

.DESCRIPTION
    This script reads the existing azmcp-commands.md file, discovers all MCP commands metadata,
    and adds metadata badges above each command line while preserving all existing content and order.

.PARAMETER InputPath
    Path to the input markdown file. Defaults to docs/azmcp-commands.md

.PARAMETER OutputPath
    Path to the output markdown file. Defaults to the same as InputPath

.PARAMETER BuildConfiguration
    Build configuration to use (Debug or Release). Defaults to Debug.

.EXAMPLE
    ./eng/scripts/Add-MetadataToCommands.ps1

.EXAMPLE
    ./eng/scripts/Add-MetadataToCommands.ps1 -BuildConfiguration Release
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$InputPath = "docs/azmcp-commands.md",

    [Parameter()]
    [string]$OutputPath = "",

    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$BuildConfiguration = 'Debug'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Get repository root
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$InputPath = Join-Path $repoRoot $InputPath
if (-not $OutputPath) {
    $OutputPath = $InputPath
}
else {
    $OutputPath = Join-Path $repoRoot $OutputPath
}

Write-Host "Repository root: $repoRoot" -ForegroundColor Cyan
Write-Host "Input path: $InputPath" -ForegroundColor Cyan
Write-Host "Output path: $OutputPath" -ForegroundColor Cyan

# Find the Azure MCP Server binary
$serverProjectPath = Join-Path $repoRoot "servers/Azure.Mcp.Server"
$binaryPath = Join-Path $serverProjectPath "src/bin/$BuildConfiguration/net9.0/azmcp.dll"

if (-not (Test-Path $binaryPath)) {
    Write-Host "Building Azure MCP Server..." -ForegroundColor Yellow
    $projectFile = Join-Path $serverProjectPath "src/Azure.Mcp.Server.csproj"
    dotnet build $projectFile -c $BuildConfiguration
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Build encountered errors, but continuing if binary exists..."
    }
    
    if (-not (Test-Path $binaryPath)) {
        throw "Could not find Azure MCP Server binary at: $binaryPath after build"
    }
}

Write-Host "Loading assemblies from: $binaryPath" -ForegroundColor Cyan

# Load required assemblies
$binDirectory = Split-Path $binaryPath -Parent
$assemblies = @(
    $binaryPath,
    (Join-Path $binDirectory "Azure.Mcp.Core.dll"),
    (Join-Path $binDirectory "Microsoft.Mcp.Core.dll"),
    (Join-Path $binDirectory "Microsoft.Extensions.DependencyInjection.dll"),
    (Join-Path $binDirectory "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
    (Join-Path $binDirectory "Microsoft.Extensions.Logging.dll"),
    (Join-Path $binDirectory "Microsoft.Extensions.Logging.Abstractions.dll"),
    (Join-Path $binDirectory "System.CommandLine.dll")
)

foreach ($assembly in $assemblies) {
    if (Test-Path $assembly) {
        Write-Host "Loading: $assembly" -ForegroundColor Gray
        try {
            [System.Reflection.Assembly]::LoadFrom($assembly) | Out-Null
        }
        catch {
            Write-Warning "Failed to load $assembly : $($_.Exception.Message)"
        }
    }
}

# Function to recursively extract commands from command groups
function Get-CommandsRecursive {
    param(
        [object]$CommandGroup,
        [string]$ParentPath = ""
    )

    $commands = @{}
    
    if ($null -eq $CommandGroup) {
        return $commands
    }

    $currentPath = if ($ParentPath) { "$ParentPath $($CommandGroup.Name)" } else { $CommandGroup.Name }

    if ($CommandGroup.Subcommands -and $CommandGroup.Subcommands.Count -gt 0) {
        foreach ($subCmd in $CommandGroup.Subcommands) {
            if ($subCmd.Subcommands -and $subCmd.Subcommands.Count -gt 0) {
                $subCommands = Get-CommandsRecursive -CommandGroup $subCmd -ParentPath $currentPath
                foreach ($key in $subCommands.Keys) {
                    $commands[$key] = $subCommands[$key]
                }
            }
            else {
                $fullPath = "$currentPath $($subCmd.Name)"
                
                # Extract metadata
                $metadata = @{
                    Destructive = $true
                    Idempotent = $false
                    OpenWorld = $true
                    ReadOnly = $false
                    Secret = $false
                    LocalRequired = $false
                }
                
                try {
                    $metadataProperty = $subCmd.GetType().GetProperty('Metadata')
                    if ($metadataProperty) {
                        $cmdMetadata = $metadataProperty.GetValue($subCmd)
                        if ($cmdMetadata) {
                            $metadata.Destructive = $cmdMetadata.Destructive
                            $metadata.Idempotent = $cmdMetadata.Idempotent
                            $metadata.OpenWorld = $cmdMetadata.OpenWorld
                            $metadata.ReadOnly = $cmdMetadata.ReadOnly
                            $metadata.Secret = $cmdMetadata.Secret
                            $metadata.LocalRequired = $cmdMetadata.LocalRequired
                        }
                    }
                }
                catch {
                    # Use defaults
                }

                $commands[$fullPath] = $metadata
            }
        }
    }

    return $commands
}

# Discover commands and metadata
Write-Host "Discovering commands..." -ForegroundColor Cyan

$serviceCollectionType = [Microsoft.Extensions.DependencyInjection.ServiceCollection]
$serviceCollection = New-Object $serviceCollectionType

$loggingServiceCollectionExtensions = [Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions]
$loggingServiceCollectionExtensions::AddLogging($serviceCollection) | Out-Null

$programType = [System.Reflection.Assembly]::LoadFrom($binaryPath).GetType('Program')
if (-not $programType) {
    throw "Could not find Program type in assembly"
}

$configureServicesMethod = $programType.GetMethod('ConfigureServices', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static)
if (-not $configureServicesMethod) {
    throw "Could not find ConfigureServices method in Program type"
}

try {
    $unwrappedCollection = $serviceCollection.PSObject.BaseObject
    $parameters = , $unwrappedCollection
    $configureServicesMethod.Invoke($null, $parameters)
}
catch {
    Write-Host "Error details: $($_.Exception.InnerException.Message)" -ForegroundColor Red
    throw
}

$serviceProvider = [Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions]::BuildServiceProvider($serviceCollection)

$commandFactoryType = [Azure.Mcp.Core.Commands.CommandFactory]
$commandFactory = $serviceProvider.GetService($commandFactoryType)

if (-not $commandFactory) {
    throw "Could not get CommandFactory from service provider"
}

$rootCommand = $commandFactory.RootCommand

# Build command metadata lookup dictionary
Write-Host "Extracting command metadata..." -ForegroundColor Cyan
$commandMetadata = @{}

foreach ($subCommand in $rootCommand.Subcommands) {
    if ($subCommand.Subcommands.Count -gt 0) {
        $subCommandMetadata = Get-CommandsRecursive -CommandGroup $subCommand
        foreach ($key in $subCommandMetadata.Keys) {
            $commandMetadata[$key] = $subCommandMetadata[$key]
        }
    }
}

Write-Host "Found metadata for $($commandMetadata.Count) commands" -ForegroundColor Green

# Debug: Output all command paths found in metadata (for first 20 commands)
Write-Host "`nSample of command paths in metadata dictionary:" -ForegroundColor Cyan
$commandMetadata.Keys | Select-Object -First 20 | Sort-Object | ForEach-Object {
    Write-Host "  $_" -ForegroundColor Gray
}
Write-Host "  ..." -ForegroundColor Gray

# Read existing file
Write-Host "`nReading existing documentation..." -ForegroundColor Cyan
$content = Get-Content -Path $InputPath -Raw

# Split into lines for processing
$lines = $content -split "`r?`n"
$newLines = @()
$i = 0
$commandsProcessed = 0
$commandsNotFound = @()

Write-Host "Processing file..." -ForegroundColor Cyan

while ($i -lt $lines.Count) {
    $line = $lines[$i]
    
    # Check if this line contains an azmcp command
    if ($line -match '^\s*azmcp\s+(.+?)(\s+--|\s+\[--|\s*\\|\s*$)') {
        $commandPath = $matches[1].Trim()
        
        # Try alternative paths for commands that have different names in docs vs code
        $alternativePaths = @($commandPath)
        
        # Handle known mismatches between documentation and code structure
        # MySQL/Postgres: "table schema get" in docs = "table schema schema" in code
        if ($commandPath -match '^(mysql|postgres) table schema get$') {
            $alternativePaths += $commandPath -replace ' get$', ' schema'
        }
        # MySQL/Postgres: "server config get" in docs = "server config config" in code  
        if ($commandPath -match '^(mysql|postgres) server config get$') {
            $alternativePaths += $commandPath -replace ' get$', ' config'
        }
        # MySQL/Postgres: "server param get" in docs = "server param param" in code
        if ($commandPath -match '^(mysql|postgres) server param get$') {
            $alternativePaths += $commandPath -replace ' get$', ' param'
        }
        # KeyVault: "admin settings get" in docs = "admin get" in code (no "settings" level)
        if ($commandPath -eq 'keyvault admin settings get') {
            $alternativePaths += 'keyvault admin get'
        }
        # Search: "search list" might be "search service list"
        if ($commandPath -eq 'search list') {
            $alternativePaths += 'search service list'
        }
        # Redis: "cache list accesspolicy" might be "cache accesspolicy list" 
        if ($commandPath -eq 'redis cache list accesspolicy') {
            $alternativePaths += 'redis cache accesspolicy list'
        }
        # Extension: "extension az" is commented out in code, try "extension az" directly
        if ($commandPath -eq 'extension az') {
            $alternativePaths += 'az'  # Command name is just "az"
        }
        # Best practices: "bestpractices get" in docs = "get_bestpractices get" in code
        if ($commandPath -eq 'bestpractices get') {
            $alternativePaths += 'get_bestpractices get'
        }
        
        # Look up metadata for this command (try all alternative paths)
        $metadata = $null
        foreach ($path in $alternativePaths) {
            if ($commandMetadata.ContainsKey($path)) {
                $metadata = $commandMetadata[$path]
                break
            }
        }
        
        # Special handling for commands that are documented but not implemented
        $notImplementedCommands = @('extension az')  # Commands that are commented out in code
        
        if ($metadata) {
            
            # Build metadata badge line - show all properties with checkmarks/X marks
            $metadataBadges = @()
            $metadataBadges += if ($metadata.Destructive) { "✅ Destructive" } else { "❌ Destructive" }
            $metadataBadges += if ($metadata.ReadOnly) { "✅ ReadOnly" } else { "❌ ReadOnly" }
            $metadataBadges += if ($metadata.Idempotent) { "✅ Idempotent" } else { "❌ Idempotent" }
            $metadataBadges += if ($metadata.Secret) { "✅ Secret" } else { "❌ Secret" }
            $metadataBadges += if ($metadata.OpenWorld) { "✅ OpenWorld" } else { "❌ OpenWorld" }
            $metadataBadges += if ($metadata.LocalRequired) { "✅ LocalRequired" } else { "❌ LocalRequired" }
            
            $metadataLine = "# $($metadataBadges -join ' | ')"
            
            # Check if metadata already exists on the previous line
            if ($i -gt 0) {
                $prevLine = $lines[$i - 1]
                # Check if previous line already has metadata badges
                if ($prevLine -match '[✅❌🔴📖♻️🔐🌍💻]') {
                    # Update existing metadata by replacing the previous line
                    if ($prevLine -ne $metadataLine) {
                        $newLines[$newLines.Count - 1] = $metadataLine
                        Write-Host "  Updated metadata for: $commandPath" -ForegroundColor Yellow
                        $commandsProcessed++
                    } else {
                        Write-Host "  Metadata already up-to-date for: $commandPath" -ForegroundColor Gray
                    }
                } else {
                    # Add new metadata comment above the command
                    $newLines += $metadataLine
                    $commandsProcessed++
                    Write-Host "  Added metadata for: $commandPath" -ForegroundColor Green
                }
            } else {
                # First line is a command - add metadata above it
                $newLines += $metadataLine
                $commandsProcessed++
                Write-Host "  Added metadata for: $commandPath" -ForegroundColor Green
            }
        } elseif ($notImplementedCommands -contains $commandPath) {
            # Command is documented but not implemented in code
            $metadataLine = "# ⚠️ NOT IMPLEMENTED - This command is documented but not currently enabled in the codebase"
            
            # Check if metadata already exists on the previous line
            if ($i -gt 0) {
                $prevLine = $lines[$i - 1]
                # Check if previous line already has a NOT IMPLEMENTED badge
                if ($prevLine -match 'NOT IMPLEMENTED') {
                    Write-Host "  NOT IMPLEMENTED badge already present for: $commandPath" -ForegroundColor Gray
                } elseif ($prevLine -match '[✅❌🔴📖♻️🔐🌍💻⚠️]') {
                    # Update existing line
                    $newLines[$newLines.Count - 1] = $metadataLine
                    Write-Host "  Added NOT IMPLEMENTED badge for: $commandPath" -ForegroundColor Cyan
                    $commandsProcessed++
                } else {
                    # Add new NOT IMPLEMENTED comment
                    $newLines += $metadataLine
                    $commandsProcessed++
                    Write-Host "  Added NOT IMPLEMENTED badge for: $commandPath" -ForegroundColor Cyan
                }
            } else {
                # First line is a command - add badge above it
                $newLines += $metadataLine
                $commandsProcessed++
                Write-Host "  Added NOT IMPLEMENTED badge for: $commandPath" -ForegroundColor Cyan
            }
        } else {
            # Command not found in metadata
            $commandsNotFound += $commandPath
        }
    }
    
    # Add the original line
    $newLines += $line
    $i++
}

# Report commands not found in metadata
if ($commandsNotFound.Count -gt 0) {
    Write-Host "`n⚠️  Commands found in documentation but not in metadata ($($commandsNotFound.Count)):" -ForegroundColor Yellow
    $commandsNotFound | Select-Object -Unique | Sort-Object | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Yellow
    }
}

# Write output file
Write-Host "Writing updated documentation..." -ForegroundColor Cyan
$newContent = $newLines -join "`n"
$newContent | Out-File -FilePath $OutputPath -Encoding utf8 -NoNewline

Write-Host "✅ Documentation updated successfully!" -ForegroundColor Green
Write-Host "   Commands processed: $commandsProcessed" -ForegroundColor Green
Write-Host "   Total commands in metadata: $($commandMetadata.Count)" -ForegroundColor Green
