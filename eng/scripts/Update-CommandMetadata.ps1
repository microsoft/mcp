#!/usr/bin/env pwsh
#Requires -Version 7.0

<#
.SYNOPSIS
    Updates azmcp-commands.md with tool metadata information.

.DESCRIPTION
    This script scans all MCP tool command classes in the tools/ and core/Azure.Mcp.Core directories,
    extracts their metadata properties (Destructive, Idempotent, OpenWorld, ReadOnly, Secret, LocalRequired),
    and updates the azmcp-commands.md file to include metadata badges above each command.
    
    The script will:
    1. Find all *Command.cs files in tools/ and core/Azure.Mcp.Core/
    2. Extract ToolMetadata from each command class
    3. Parse the azmcp-commands.md file
    4. Add metadata information above each matching command
    5. Use ✅ for true values and ❌ for false values
    6. Skip commands that already have metadata
    
    The metadata format is:
    # ✅/❌ Destructive | ✅/❌ Idempotent | ✅/❌ OpenWorld | ✅/❌ ReadOnly | ✅/❌ Secret | ✅/❌ LocalRequired

.PARAMETER CommandsFilePath
    Path to the azmcp-commands.md file. Defaults to docs/azmcp-commands.md.

.PARAMETER DryRun
    If specified, shows what changes would be made without actually modifying the file.

.EXAMPLE
    .\Update-CommandMetadata.ps1
    Updates the commands file with metadata information.

.EXAMPLE
    .\Update-CommandMetadata.ps1 -DryRun
    Shows what changes would be made without modifying the file.

.EXAMPLE
    .\Update-CommandMetadata.ps1 -Verbose
    Updates the file with verbose output showing each matched command.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [string]$CommandsFilePath = "$PSScriptRoot\..\..\docs\azmcp-commands.md",
    
    [Parameter()]
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Helper function to extract metadata from a command class file
function Get-CommandMetadata {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )
    
    try {
        $content = Get-Content -Path $FilePath -Raw
        
        # Extract the command name pattern: azmcp {namespace} {resource} {operation}
        # First get the namespace from the folder structure
        $toolsMatch = $FilePath -match 'tools[/\\]([^/\\]+)[/\\]'
        if (-not $toolsMatch) {
            $toolsMatch = $FilePath -match 'core[/\\]Azure\.Mcp\.Core'
        }
        
        # Extract metadata properties
        $metadata = @{
            Destructive = $null
            Idempotent = $null
            OpenWorld = $null
            ReadOnly = $null
            Secret = $null
            LocalRequired = $null
            Name = $null
            ClassName = ""
        }
        
        # Look for Metadata property initialization
        # Use [regex]::Match with Singleline option to match across newlines
        $metadataPattern = 'public\s+override\s+ToolMetadata\s+Metadata\s*=>\s*new\(\)\s*\{(.+?)\};'
        $match = [regex]::Match($content, $metadataPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        
        if ($match.Success) {
            $metadataBlock = $match.Groups[1].Value
            
            # Extract each property
            if ($metadataBlock -match 'Destructive\s*=\s*(true|false)') {
                $metadata.Destructive = [bool]::Parse($Matches[1])
            }
            if ($metadataBlock -match 'Idempotent\s*=\s*(true|false)') {
                $metadata.Idempotent = [bool]::Parse($Matches[1])
            }
            if ($metadataBlock -match 'OpenWorld\s*=\s*(true|false)') {
                $metadata.OpenWorld = [bool]::Parse($Matches[1])
            }
            if ($metadataBlock -match 'ReadOnly\s*=\s*(true|false)') {
                $metadata.ReadOnly = [bool]::Parse($Matches[1])
            }
            if ($metadataBlock -match 'Secret\s*=\s*(true|false)') {
                $metadata.Secret = [bool]::Parse($Matches[1])
            }
            if ($metadataBlock -match 'LocalRequired\s*=\s*(true|false)') {
                $metadata.LocalRequired = [bool]::Parse($Matches[1])
            }
        }
        
        # Extract command name
        if ($content -match 'public\s+override\s+string\s+Name\s*=>\s*"([^"]+)"') {
            $metadata.Name = $Matches[1]
        }
        elseif ($content -match 'public\s+override\s+string\s+Name\s*=>\s*(\w+);') {
            # Handle cases where Name references a const (e.g., "=> CommandName;")
            $constName = $Matches[1]
            if ($content -match "const\s+string\s+$constName\s*=\s*`"([^`"]+)`"") {
                $metadata.Name = $Matches[1]
            }
        }
        
        # Extract class name for better identification
        if ($content -match 'public\s+sealed\s+class\s+(\w+)') {
            $metadata.ClassName = $Matches[1]
        }
        else {
            $metadata.ClassName = ""
        }
        
        return $metadata
    }
    catch {
        Write-Warning "Failed to parse file $FilePath : $_"
        return $null
    }
}

# Helper function to build command path from file path
function Get-CommandPath {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,
        [Parameter(Mandatory)]
        [hashtable]$Metadata
    )
    
    # Extract namespace from path
    $namespace = ""
    if ($FilePath -match 'tools[/\\]Azure\.Mcp\.Tools\.([^/\\]+)') {
        $toolName = $Matches[1]
        # Convert tool name to lowercase namespace (e.g., "Storage" -> "storage")
        $namespace = $toolName.ToLower()
    }
    elseif ($FilePath -match 'tools[/\\]Fabric\.Mcp\.Tools\.([^/\\]+)') {
        $toolName = $Matches[1]
        $namespace = "fabric-" + $toolName.ToLower()
    }
    elseif ($FilePath -match 'core[/\\]Azure\.Mcp\.Core[/\\]src[/\\]Areas[/\\]([^/\\]+)') {
        # For core commands, extract namespace from Areas folder
        $namespace = $Matches[1].ToLower()
    }
    
    # Extract resource from folder structure (e.g., Commands/Account/)
    $resource = ""
    if ($FilePath -match 'Commands[/\\]([^/\\]+)[/\\]') {
        $resource = $Matches[1].ToLower()
    }
    
    # Build full command path - note: we can't always determine the exact path
    # because command groups might use aliases (e.g., "kv" for "keyvalue")
    # So we return multiple possible paths
    $commandParts = @("azmcp")
    if ($namespace) { $commandParts += $namespace }
    if ($resource -and $resource -ne "commands") { $commandParts += $resource }
    if ($Metadata.Name) { $commandParts += $Metadata.Name }
    
    $fullPath = $commandParts -join " "
    
    # Store both full path and class name for better matching
    return @{
        FullPath = $fullPath
        ClassName = $Metadata.ClassName
        CommandName = $Metadata.Name
        Namespace = $namespace
        Resource = $resource
    }
}

# Helper function to format metadata as a badge line
function Format-MetadataBadge {
    param(
        [Parameter(Mandatory)]
        [hashtable]$Metadata
    )
    
    $badges = @()
    
    # Only show metadata that is explicitly set
    if ($null -ne $Metadata.Destructive) {
        $icon = if ($Metadata.Destructive) { "✅" } else { "❌" }
        $badges += "$icon Destructive"
    }
    if ($null -ne $Metadata.Idempotent) {
        $icon = if ($Metadata.Idempotent) { "✅" } else { "❌" }
        $badges += "$icon Idempotent"
    }
    if ($null -ne $Metadata.OpenWorld) {
        $icon = if ($Metadata.OpenWorld) { "✅" } else { "❌" }
        $badges += "$icon OpenWorld"
    }
    if ($null -ne $Metadata.ReadOnly) {
        $icon = if ($Metadata.ReadOnly) { "✅" } else { "❌" }
        $badges += "$icon ReadOnly"
    }
    if ($null -ne $Metadata.Secret) {
        $icon = if ($Metadata.Secret) { "✅" } else { "❌" }
        $badges += "$icon Secret"
    }
    if ($null -ne $Metadata.LocalRequired) {
        $icon = if ($Metadata.LocalRequired) { "✅" } else { "❌" }
        $badges += "$icon LocalRequired"
    }
    
    if ($badges.Count -gt 0) {
        return "# " + ($badges -join " | ")
    }
    
    return $null
}

# Main script
Write-Host "Scanning for command files..." -ForegroundColor Cyan

# Find all command files
# Note: Some command files don't end with "Command.cs" (e.g., AgentsQueryAndEvaluate.cs)
# so we need to include all .cs files in Commands folders
$commandFiles = @()
$commandFiles += Get-ChildItem -Path "$PSScriptRoot\..\..\tools" -Filter "*.cs" -Recurse -File | Where-Object { $_.FullName -match '[/\\]Commands[/\\]' }
$commandFiles += Get-ChildItem -Path "$PSScriptRoot\..\..\core\Azure.Mcp.Core" -Filter "*.cs" -Recurse -File | Where-Object { $_.FullName -match '[/\\]Commands[/\\]' }

Write-Host "Found $($commandFiles.Count) command files" -ForegroundColor Green

# Build metadata lookup table
$metadataLookup = @{}
$metadataByCommand = @{}  # Lookup by command name for fuzzy matching
$metadataByClassName = @{}  # Lookup by class name
foreach ($file in $commandFiles) {
    $metadata = Get-CommandMetadata -FilePath $file.FullName
    if ($metadata -and $metadata.Name) {
        $commandInfo = Get-CommandPath -FilePath $file.FullName -Metadata $metadata
        $fullPath = $commandInfo.FullPath
        
        # Store by full path
        if (-not $metadataLookup.ContainsKey($fullPath)) {
            $metadataLookup[$fullPath] = $metadata
        }
        
        # Also store by command name + namespace for fuzzy matching
        $key = "$($commandInfo.Namespace) $($metadata.Name)"
        if (-not $metadataByCommand.ContainsKey($key)) {
            $metadataByCommand[$key] = @{
                Metadata = $metadata
                CommandInfo = $commandInfo
            }
        }
        
        # Store by class name for direct lookup
        if ($metadata.ClassName) {
            $metadataByClassName[$metadata.ClassName] = $metadata
        }
        
        Write-Verbose "Found command: $fullPath (namespace: $($commandInfo.Namespace), command: $($metadata.Name), class: $($metadata.ClassName))"
    }
}

Write-Host "Extracted metadata for $($metadataLookup.Count) commands" -ForegroundColor Green

# Read the commands file
if (-not (Test-Path $CommandsFilePath)) {
    Write-Error "Commands file not found: $CommandsFilePath"
    exit 1
}

$commandsContent = Get-Content -Path $CommandsFilePath -Raw
$lines = $commandsContent -split "`r?`n"

# Process the file and add metadata
$updatedLines = @()
$addedMetadata = 0
$skippedAlreadyHasMetadata = 0

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    
    # Check if this line contains an azmcp command
    if ($line -match '^(#\s+)?azmcp\s+(.+?)(?:\s+\\)?$') {
        $commandLine = $line -replace '\s+\\$', ''  # Remove trailing backslash
        $commandLine = $commandLine.Trim()
        $commandLine = $commandLine -replace '^#\s+', ''  # Remove leading comment
        
        # Extract base command (without arguments starting with -- or [)
        # Handle optional arguments in square brackets like [--option]
        if ($commandLine -match '^(azmcp(?:\s+[\w-]+)+)(?:\s+(?:--|\[).*)?$') {
            $baseCommand = $Matches[1].Trim()
            
            # Skip server start command - it's not a regular tool
            if ($baseCommand -eq "azmcp server start") {
                Write-Verbose "Skipping server start command (not a regular tool)"
                $updatedLines += $line
                continue
            }
            
            # Check if we have metadata for this command
            $metadata = $null
            
            # Strategy 1: Exact match
            if ($metadataLookup.ContainsKey($baseCommand)) {
                $metadata = $metadataLookup[$baseCommand]
                Write-Verbose "Exact match for: $baseCommand"
            }
            
            # Strategy 1.5: Known command mappings (for docs that don't match Setup structure)
            if (-not $metadata) {
                $knownMappings = @{
                    "azmcp role assignment list" = "RoleAssignmentListCommand"
                    "azmcp redis cache list accesspolicy" = "AccessPolicyListCommand"
                    "azmcp redis cache accesspolicy list" = "AccessPolicyListCommand"
                    "azmcp monitor healthmodels entity gethealth" = "EntityGetHealthCommand"
                }
                
                if ($knownMappings.ContainsKey($baseCommand) -and $metadataByClassName.ContainsKey($knownMappings[$baseCommand])) {
                    $metadata = $metadataByClassName[$knownMappings[$baseCommand]]
                    Write-Verbose "Known mapping match for '$baseCommand' using class '$($knownMappings[$baseCommand])'"
                }
            }
            
            # Strategy 2: Fuzzy matching by extracting namespace and last token (command name)
            if (-not $metadata) {
                $parts = $baseCommand -split '\s+'
                if ($parts.Count -ge 3) {
                    # parts[0] = "azmcp", parts[1] = namespace, parts[-1] = command name
                    $namespace = $parts[1]
                    $commandName = $parts[-1]
                    $fuzzyKey = "$namespace $commandName"
                    
                    if ($metadataByCommand.ContainsKey($fuzzyKey)) {
                        $metadata = $metadataByCommand[$fuzzyKey].Metadata
                        Write-Verbose "Fuzzy matched '$baseCommand' using key '$fuzzyKey'"
                    }
                }
            }
            
            # Strategy 3: Try matching by command name pattern in class name
            # e.g., "azmcp kusto database list" -> try DatabaseListCommand
            # This is the most important strategy since command groups in Setup files
            # may not match the folder structure
            if (-not $metadata) {
                # Extract all parts after "azmcp namespace"
                if ($baseCommand -match '^azmcp\s+([\w-]+)\s+(.+)$') {
                    $namespace = $Matches[1]
                    $restOfCommand = $Matches[2].Trim()
                    
                    # Try to build potential class names
                    # Split by spaces and convert to PascalCase
                    $commandParts = $restOfCommand -split '\s+'
                    $potentialClassNames = @()
                    
                    # Option 1: All parts combined (e.g., "database list" -> "DatabaseListCommand")
                    $className1 = ($commandParts | ForEach-Object { 
                        $part = $_ -replace '-', ''  # Remove hyphens
                        if ($part.Length -gt 0) {
                            $part.Substring(0,1).ToUpper() + $part.Substring(1).ToLower()
                        }
                    }) -join ''
                    $className1 += "Command"
                    $potentialClassNames += $className1
                    
                    # Option 2: Last part only (e.g., "database list" -> "ListCommand")
                    if ($commandParts.Count -gt 1) {
                        $lastPart = $commandParts[-1] -replace '-', ''
                        $className2 = $lastPart.Substring(0,1).ToUpper() + $lastPart.Substring(1).ToLower() + "Command"
                        $potentialClassNames += $className2
                    }
                    
                    # Option 3: With hyphens converted (e.g., "query-and-evaluate" -> "QueryAndEvaluateCommand", "agents query-and-evaluate" -> "AgentsQueryAndEvaluateCommand")
                    $className3 = ($commandParts | ForEach-Object {
                        $subParts = $_ -split '-'
                        ($subParts | ForEach-Object {
                            if ($_.Length -gt 0) {
                                $_.Substring(0,1).ToUpper() + $_.Substring(1).ToLower()
                            }
                        }) -join ''
                    }) -join ''
                    $className3 += "Command"
                    $potentialClassNames += $className3
                    
                    # Try each potential class name
                    foreach ($potentialClassName in $potentialClassNames) {
                        if ($metadataByClassName.ContainsKey($potentialClassName)) {
                            $metadata = $metadataByClassName[$potentialClassName]
                            Write-Verbose "Class name match for '$baseCommand' using class '$potentialClassName'"
                            break
                        }
                    }
                }
            }
            
            if ($metadata) {
                # Check if the previous line already contains metadata
                $prevLine = if ($i -gt 0) { $lines[$i - 1] } else { "" }
                
                if ($prevLine -match '^#\s+(✅|❌)') {
                    $skippedAlreadyHasMetadata++
                    Write-Verbose "Skipping $baseCommand - already has metadata"
                }
                else {
                    # Add metadata line before the command
                    $badge = Format-MetadataBadge -Metadata $metadata
                    if ($badge) {
                        $updatedLines += $badge
                        $addedMetadata++
                        Write-Verbose "Adding metadata for: $baseCommand"
                    }
                }
            }
        }
    }
    
    $updatedLines += $line
}

# Write results
if ($DryRun) {
    Write-Host "`nDRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    Write-Host "Would add metadata to $addedMetadata commands" -ForegroundColor Cyan
    Write-Host "Skipped $skippedAlreadyHasMetadata commands that already have metadata" -ForegroundColor Gray
    
    # Show a sample of changes
    Write-Host "`nSample changes:" -ForegroundColor Cyan
    $sampleCount = [Math]::Min(5, $addedMetadata)
    $shown = 0
    for ($i = 0; $i -lt $updatedLines.Count -and $shown -lt $sampleCount; $i++) {
        if ($updatedLines[$i] -match '^#\s+(✅|❌)') {
            Write-Host $updatedLines[$i] -ForegroundColor Green
            Write-Host $updatedLines[$i + 1] -ForegroundColor White
            Write-Host ""
            $shown++
        }
    }
}
else {
    # Write the updated content
    $updatedContent = $updatedLines -join "`n"
    Set-Content -Path $CommandsFilePath -Value $updatedContent -NoNewline
    
    Write-Host "`nSuccessfully updated $CommandsFilePath" -ForegroundColor Green
    Write-Host "Added metadata to $addedMetadata commands" -ForegroundColor Cyan
    Write-Host "Skipped $skippedAlreadyHasMetadata commands that already have metadata" -ForegroundColor Gray
}

Write-Host "`nDone!" -ForegroundColor Green
