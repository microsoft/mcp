<#
.SYNOPSIS
    Updates azmcp-commands.md with tool metadata information

.DESCRIPTION
    This script executes 'azmcp tools list', extracts metadata for each tool,
    and updates the azmcp-commands.md file by adding metadata information above
    each tool command. Maintains the existing format, schema, and order.
    
.PARAMETER AzmcpPath
    Path to the azmcp.exe executable. Default: ..\..\servers\Azure.Mcp.Server\src\bin\Debug\net9.0\azmcp.exe
    
.PARAMETER DocsPath
    Path to the azmcp-commands.md file. Default: ..\..\servers\Azure.Mcp.Server\docs\azmcp-commands.md
    
.EXAMPLE
    .\Update-AzCommandsMetadata.ps1
    
.EXAMPLE
    .\Update-AzCommandsMetadata.ps1 -AzmcpPath "C:\path\to\azmcp.exe" -DocsPath "C:\path\to\azmcp-commands.md"
#>

[CmdletBinding()]
param(
    [string]$AzmcpPath = "..\..\servers\Azure.Mcp.Server\src\bin\Debug\net9.0\azmcp.exe",
    [string]$DocsPath = "..\..\servers\Azure.Mcp.Server\docs\azmcp-commands.md"
)

$ErrorActionPreference = "Stop"

function Get-MetadataLine {
    param(
        [string]$PropertyName,
        [bool]$Value
    )
    
    $icon = if ($Value) { "✅" } else { "❌" }
    
    # Convert to PascalCase
    $pascalCase = switch ($PropertyName) {
        "destructive" { "Destructive" }
        "idempotent" { "Idempotent" }
        "openWorld" { "OpenWorld" }
        "readOnly" { "ReadOnly" }
        "secret" { "Secret" }
        "localRequired" { "LocalRequired" }
        default { (Get-Culture).TextInfo.ToTitleCase($PropertyName) }
    }
    
    return "$icon $pascalCase"
}

function Get-ToolMetadataString {
    param(
        [PSCustomObject]$Metadata
    )
    
    $lines = @()
    
    # Order: destructive, idempotent, openWorld, readOnly, secret, localRequired
    if ($null -ne $Metadata.destructive) {
        $lines += Get-MetadataLine "destructive" $Metadata.destructive
    }
    
    if ($null -ne $Metadata.idempotent) {
        $lines += Get-MetadataLine "idempotent" $Metadata.idempotent
    }
    
    if ($null -ne $Metadata.openWorld) {
        $lines += Get-MetadataLine "openWorld" $Metadata.openWorld
    }
    
    if ($null -ne $Metadata.readOnly) {
        $lines += Get-MetadataLine "readOnly" $Metadata.readOnly
    }
    
    if ($null -ne $Metadata.secret) {
        $lines += Get-MetadataLine "secret" $Metadata.secret
    }
    
    if ($null -ne $Metadata.localRequired) {
        $lines += Get-MetadataLine "localRequired" $Metadata.localRequired
    }
    
    if ($lines.Count -gt 0) {
        # Add # prefix for markdown comment
        return "# " + ($lines -join " | ")
    }
    
    return $null
}

Write-Host "Starting update of azmcp-commands.md with tool metadata..." -ForegroundColor Cyan

# Validate paths
if (-not (Test-Path $AzmcpPath)) {
    Write-Error "azmcp.exe not found at: $AzmcpPath"
    exit 1
}

if (-not (Test-Path $DocsPath)) {
    Write-Error "azmcp-commands.md not found at: $DocsPath"
    exit 1
}

# Execute azmcp tools list and capture output
Write-Host "Executing 'azmcp tools list'..." -ForegroundColor Yellow
$toolsListOutput = & $AzmcpPath tools list 2>&1 | Out-String

# Parse JSON output
Write-Host "Parsing tools list output..." -ForegroundColor Yellow
try {
    $toolsData = $toolsListOutput | ConvertFrom-Json
    if ($toolsData.status -ne 200) {
        Write-Error "Failed to get tools list. Status: $($toolsData.status), Message: $($toolsData.message)"
        exit 1
    }
} catch {
    Write-Error "Failed to parse tools list output as JSON: $_"
    exit 1
}

# Build a dictionary of command -> metadata
Write-Host "Building command metadata dictionary..." -ForegroundColor Yellow
$commandMetadata = @{}
foreach ($tool in $toolsData.results) {
    if ($tool.command -and $tool.metadata) {
        $commandMetadata[$tool.command] = $tool.metadata
    }
}

Write-Host "Found $($commandMetadata.Count) tools with metadata" -ForegroundColor Green

# Read the docs file
Write-Host "Reading $DocsPath..." -ForegroundColor Yellow

# Split into lines for processing
$docLines = Get-Content $DocsPath

# Process the document
Write-Host "Processing document and adding metadata..." -ForegroundColor Yellow
$updatedLines = @()
$i = 0
$updatedCount = 0
$removedCount = 0

while ($i -lt $docLines.Count) {
    $line = $docLines[$i]
    
    # Check if this is an old metadata line (without # prefix) and skip it
    if ($line -match '^(✅|❌)\s+(Destructive|Idempotent|Openworld|Readonly|Secret|Localrequired)') {
        Write-Verbose "Removing old metadata line: $line"
        $removedCount++
        $i++
        continue
    }
    
    # Check if this line starts with "azmcp " (a command line)
    if ($line -match '^azmcp\s+(.+?)(\s+\\)?$') {
        # Extract the command (without the trailing backslash and parameters)
        $commandLine = $line.Trim()
        
        # Try to find the base command in our metadata dictionary
        $matchedCommand = $null
        $matchedMetadata = $null
        
        foreach ($cmd in $commandMetadata.Keys) {
            # Check if the line starts with this command
            if ($commandLine -match "^$([regex]::Escape($cmd))(\s|\\|$)") {
                $matchedCommand = $cmd
                $matchedMetadata = $commandMetadata[$cmd]
                break
            }
        }
        
        if ($matchedMetadata) {
            # Check if the previous line already has metadata (to avoid duplicates)
            $hasPreviousMetadata = $false
            
            if ($updatedLines.Count -gt 0) {
                $prevLine = $updatedLines[$updatedLines.Count - 1]
                # Check for metadata line with # prefix and PascalCase
                if ($prevLine -match '^#\s+(✅|❌)\s+(Destructive|Idempotent|OpenWorld|ReadOnly|Secret|LocalRequired)') {
                    $hasPreviousMetadata = $true
                }
            }
            
            if (-not $hasPreviousMetadata) {
                # Generate metadata string
                $metadataString = Get-ToolMetadataString $matchedMetadata
                
                if ($metadataString) {
                    # Add metadata line above the command
                    $updatedLines += $metadataString
                    $updatedCount++
                    Write-Verbose "Added metadata for: $matchedCommand"
                }
            }
        }
    }
    
    # Add the current line
    $updatedLines += $line
    $i++
}

# Write the updated content back to the file
Write-Host "Writing updated content to $DocsPath..." -ForegroundColor Yellow
$updatedLines | Out-File -FilePath $DocsPath -Encoding utf8 -Force

# Clean up temporary output file
$tempOutputFile = "..\..\tools-list-output.json"
if (Test-Path $tempOutputFile) {
    Write-Host "Cleaning up temporary output file..." -ForegroundColor Yellow
    Remove-Item $tempOutputFile -Force
    Write-Verbose "Deleted temporary file: $tempOutputFile"
}

Write-Host "`nUpdate complete!" -ForegroundColor Green
Write-Host "  - Total commands found in metadata: $($commandMetadata.Count)" -ForegroundColor Cyan
Write-Host "  - Old metadata lines removed: $removedCount" -ForegroundColor Cyan
Write-Host "  - New metadata lines added: $updatedCount" -ForegroundColor Cyan
Write-Host "`nPlease review the changes in $DocsPath" -ForegroundColor Yellow
