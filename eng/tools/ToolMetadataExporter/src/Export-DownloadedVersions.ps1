<#
.SYNOPSIS
Exports tool metadata from downloaded Azure MCP versions in semantic-version order.

.DESCRIPTION
Finds package directories whose names end in a semantic version and invokes ToolMetadataExporter
for each tools\any\win-x64\azmcp.exe. Successfully processed executable paths are recorded in a
ledger and skipped on subsequent runs.

The script stops when an expected executable is missing or an export fails. Use -WhatIf to preview
exports and ledger deletion without performing them.

.PARAMETER PackagesDirectory
Directory containing the versioned Azure MCP package directories.

.PARAMETER SuccessfulPathsFile
Path to the success ledger. Defaults to
.work\ToolMetadataExporter\Export-DownloadedVersions.successful.txt under the repository root.

.PARAMETER ExportUntil
Inclusive semantic-version ceiling. For example, 2.0.0 includes prerelease versions such as
2.0.0-beta.1 and the stable 2.0.0 release.

.PARAMETER IsDryRun
Passes --IsDryRun true to each ToolMetadataExporter invocation. Without this switch, detected
changes may be ingested into Kusto.

.PARAMETER Force
Deletes the success ledger before processing, causing eligible versions to be exported again.

.EXAMPLE
.\Export-DownloadedVersions.ps1 -PackagesDirectory C:\downloads\azure.mcp -IsDryRun

Exports every discovered version without ingesting changes.

.EXAMPLE
.\Export-DownloadedVersions.ps1 -ExportUntil 2.0.0 -IsDryRun

Exports versions up to and including 2.0.0.

.EXAMPLE
.\Export-DownloadedVersions.ps1 -Force -WhatIf

Previews deleting the success ledger and reprocessing all eligible versions.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [string] $PackagesDirectory = 'C:\Users\conniey\Downloads\azure.mcp',
    [string] $SuccessfulPathsFile,
    [string] $ExportUntil,
    [switch] $IsDryRun,
    [switch] $Force
)

$exportUntilVersion = if ($ExportUntil) {
    try {
        [System.Management.Automation.SemanticVersion]$ExportUntil
    }
    catch {
        throw "ExportUntil must be a valid semantic version. Received '$ExportUntil'."
    }
}

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
$SuccessfulPathsFile = if ($SuccessfulPathsFile) {
    [System.IO.Path]::GetFullPath($SuccessfulPathsFile)
}
else {
    Join-Path $repositoryRoot '.work\ToolMetadataExporter\Export-DownloadedVersions.successful.txt'
}

if ($Force -and
    (Test-Path -LiteralPath $SuccessfulPathsFile -PathType Leaf) -and
    $PSCmdlet.ShouldProcess($SuccessfulPathsFile, 'Delete successful paths ledger')) {
    Remove-Item -LiteralPath $SuccessfulPathsFile -Force
}

$successfulPathsDirectory = Split-Path -Parent $SuccessfulPathsFile
[System.IO.Directory]::CreateDirectory($successfulPathsDirectory) | Out-Null
[System.IO.File]::Open($SuccessfulPathsFile, [System.IO.FileMode]::OpenOrCreate).Dispose()

$successfulPaths = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase)

foreach ($path in [System.IO.File]::ReadAllLines($SuccessfulPathsFile)) {
    if (-not [string]::IsNullOrWhiteSpace($path)) {
        $successfulPaths.Add([System.IO.Path]::GetFullPath($path.Trim())) | Out-Null
    }
}

$packages = Get-ChildItem -LiteralPath $PackagesDirectory -Directory |
    ForEach-Object {
        if ($_.Name -notmatch '(?<Version>\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?)$') {
            Write-Verbose "Skipping '$($_.Name)' because its name does not end in a semantic version."
            return
        }

        [pscustomobject]@{
            Directory = $_
            Version = [System.Management.Automation.SemanticVersion]$Matches.Version
        }
    } |
    Where-Object { $null -eq $exportUntilVersion -or $_.Version -le $exportUntilVersion } |
    Sort-Object Version

Push-Location $PSScriptRoot
try {
    foreach ($package in $packages) {
        $azmcpExe = [System.IO.Path]::GetFullPath(
            (Join-Path $package.Directory.FullName 'tools\any\win-x64\azmcp.exe'))

        if ($successfulPaths.Contains($azmcpExe)) {
            Write-Host "Skipping $($package.Version); '$azmcpExe' was exported successfully in a previous run."
            continue
        }

        if (-not (Test-Path -LiteralPath $azmcpExe -PathType Leaf)) {
            throw "Could not find azmcp.exe for version $($package.Version) at '$azmcpExe'."
        }

        if ($PSCmdlet.ShouldProcess($azmcpExe, "Export tool metadata for version $($package.Version)")) {
            $arguments = @('run', '--', '--AzmcpExe', $azmcpExe)
            if ($IsDryRun) {
                $arguments += @('--IsDryRun', 'true')
            }

            Write-Host "Exporting tool metadata for $($package.Version)..."
            & dotnet @arguments
            if ($LASTEXITCODE -ne 0) {
                throw "Tool metadata export failed for version $($package.Version) with exit code $LASTEXITCODE."
            }

            [System.IO.File]::AppendAllText(
                $SuccessfulPathsFile,
                "$azmcpExe$([System.Environment]::NewLine)")
            $successfulPaths.Add($azmcpExe) | Out-Null
        }
    }
}
finally {
    Pop-Location
}
