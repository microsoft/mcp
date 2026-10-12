#Requires -Version 7
# cspell:ignore longpaths

<#
.SYNOPSIS
Refreshes bundled Fabric REST API specifications and item-definition articles on demand.

.PARAMETER RestApiRevision
A full microsoft/fabric-rest-api-specs commit SHA, or main (resolved once).

.PARAMETER ItemDefinitionRevision
A full MicrosoftDocs/fabric-rest-apis-docs commit SHA, or main (resolved once).

.EXAMPLE
.\eng\scripts\Update-FabricDocsResources.ps1 -WhatIf

.EXAMPLE
.\eng\scripts\Update-FabricDocsResources.ps1 -RestApiRevision ab5bb1b60ea82ca36c7836f9d2fcdb5c596c464a -ItemDefinitionRevision 4800cf118b2be4f119957dbe015cad9b1e080098
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidatePattern('^(main|[0-9a-fA-F]{40})$')]
    [string] $RestApiRevision = 'main',

    [ValidatePattern('^(main|[0-9a-fA-F]{40})$')]
    [string] $ItemDefinitionRevision = 'main'
)

. (Join-Path $PSScriptRoot 'helpers' 'FabricDocsResourceHelpers.ps1')

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..' '..'))
$resourcePath = Join-Path 'tools' 'Fabric.Mcp.Tools.Docs' 'src' 'Resources'
$destination = Join-Path $repoRoot $resourcePath
$managedDirectories = @('fabric-rest-api-specs', 'item-definitions')
$gitRoot = & git -c core.longpaths=true -C $repoRoot rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0 -or [System.IO.Path]::GetFullPath($gitRoot) -ne $repoRoot) {
    throw 'Run this script from its location in a microsoft/mcp Git checkout.'
}
for ($directory = Get-Item -LiteralPath $destination; $directory.FullName -ne $repoRoot; $directory = $directory.Parent) {
    if ($directory.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Refusing to update resources through a link: $($directory.FullName)"
    }
}
$original = Get-ManagedSnapshot $destination $managedDirectories
$temporaryDirectory = [System.IO.Directory]::CreateDirectory(
    (Join-Path ([System.IO.Path]::GetTempPath()) "fabric-docs-refresh-$([guid]::NewGuid().ToString('N'))")).FullName
try {
    $rest = Save-UpstreamSnapshot 'microsoft/fabric-rest-api-specs' $RestApiRevision (Join-Path $temporaryDirectory 'rest') {
        param($tree)
        $categories = @($tree | Where-Object { $_.path -cmatch '^[^/]+/swagger\.json$' } |
            ForEach-Object { $_.path.Split('/')[0] }) + @('common')
        $files = @($tree | Where-Object {
            $_.type -eq 'blob' -and
            (($_.path.Split('/')[0] -cin $categories -and $_.path.EndsWith('.json', [System.StringComparison]::Ordinal)) -or
                $_.path -ceq 'LICENSE.txt')
        })
        $paths = @($files | ForEach-Object { $_.path })
        foreach ($required in @('admin/swagger.json', 'platform/swagger.json', 'common/definitions.json', 'LICENSE.txt')) {
            if (-not ($paths -ccontains $required)) {
                throw "Required upstream file is missing: $required. Review the source layout before refreshing."
            }
        }
        return $files
    }
    $articles = Save-UpstreamSnapshot 'MicrosoftDocs/fabric-rest-apis-docs' $ItemDefinitionRevision (Join-Path $temporaryDirectory 'articles') {
        param($tree)
        $files = @($tree | Where-Object {
            $_.type -eq 'blob' -and
            ($_.path -cmatch '^docs-ref-conceptual/item-management/definitions/[^/]+-definition\.md$' -or $_.path -ceq 'LICENSE')
        })
        $paths = @($files | ForEach-Object { $_.path })
        foreach ($required in @('docs-ref-conceptual/item-management/definitions/notebook-definition.md', 'LICENSE')) {
            if (-not ($paths -ccontains $required)) {
                throw "Required upstream file is missing: $required. Review the source layout before refreshing."
            }
        }
        return $files
    }
    if ([System.IO.File]::ReadAllText((Join-Path $rest.Directory 'LICENSE.txt')) -notmatch '(?m)^MIT License\r?$' -or
        [System.IO.File]::ReadAllText((Join-Path $articles.Directory 'LICENSE')) -notmatch '\AAttribution 4\.0 International') {
        throw 'An upstream license changed. Review attribution and reuse terms before refreshing.'
    }

    $staging = Join-Path $temporaryDirectory 'staged'
    foreach ($name in $managedDirectories) {
        [System.IO.Directory]::CreateDirectory((Join-Path $staging $name)) | Out-Null
    }
    foreach ($file in $rest.Files | Where-Object path -CNE 'LICENSE.txt') {
        $target = Join-Path $staging 'fabric-rest-api-specs' 'contents' $file.path
        [System.IO.Directory]::CreateDirectory((Split-Path $target -Parent)) | Out-Null
        [System.IO.File]::Copy((Join-Path $rest.Directory $file.path), $target)
    }
    foreach ($file in $articles.Files | Where-Object path -CNE 'LICENSE' | Sort-Object path -CaseSensitive) {
        $name = [System.IO.Path]::GetFileName($file.path)
        $converted = Convert-FabricItemDefinition ([System.IO.File]::ReadAllText((Join-Path $articles.Directory $file.path))) $name
        [System.IO.File]::WriteAllText((Join-Path $staging 'item-definitions' $name), $converted, [System.Text.UTF8Encoding]::new($false))
    }

    $expected = Get-ManagedSnapshot $staging $managedDirectories
    $changes = @(Get-SnapshotChange $original $expected)
    $restChanges = @($changes | Where-Object { $_.Path.StartsWith('fabric-rest-api-specs/contents/', [System.StringComparison]::Ordinal) })
    $articleChanges = @($changes | Where-Object { $_.Path.StartsWith('item-definitions/', [System.StringComparison]::Ordinal) })
    foreach ($change in $changes) {
        Write-Verbose "$($change.Change): $($change.Path)"
    }
    $result = [pscustomobject]@{
        RestApi = [pscustomobject]@{
            Commit = $rest.Commit; Files = $rest.Files.Count - 1
            Added = @($restChanges | Where-Object Change -EQ 'Added').Count
            Modified = @($restChanges | Where-Object Change -EQ 'Modified').Count
            Removed = @($restChanges | Where-Object Change -EQ 'Removed').Count
        }
        ItemDefinitions = [pscustomobject]@{
            Commit = $articles.Commit; Files = $articles.Files.Count - 1
            Added = @($articleChanges | Where-Object Change -EQ 'Added').Count
            Modified = @($articleChanges | Where-Object Change -EQ 'Modified').Count
            Removed = @($articleChanges | Where-Object Change -EQ 'Removed').Count
        }
        Applied = $false
        Changes = $changes
    }
    if ($changes.Count -eq 0) {
        Write-Host 'Both managed snapshots already match these commits.'
        return $result
    }
    if (-not $PSCmdlet.ShouldProcess($destination, "Apply $($changes.Count) validated file changes from both upstream snapshots")) {
        return $result
    }
    $gitPaths = @($managedDirectories | ForEach-Object { Join-Path $resourcePath $_ })
    $status = & git -c core.longpaths=true -C $repoRoot status --porcelain=v1 --untracked-files=all --ignored=matching -- $gitPaths
    if ($LASTEXITCODE -ne 0) {
        throw 'Unable to check the managed resources for local changes.'
    }
    if ($status) {
        throw "The managed resources have local changes. Commit or move them before refreshing; no files were overwritten.`n$($status -join "`n")"
    }
    if (@(Get-SnapshotChange $original (Get-ManagedSnapshot $destination $managedDirectories)).Count -ne 0) {
        throw 'The managed resources changed during the download; no files were overwritten.'
    }

    # Remove old paths first, including case-only renames, without deleting outside the managed trees.
    foreach ($change in $changes | Where-Object Change -EQ 'Removed') {
        [System.IO.File]::Delete((Join-Path $destination $change.Path))
    }
    foreach ($name in $managedDirectories) {
        $path = Join-Path $destination $name
        if (Test-Path -LiteralPath $path) {
            foreach ($directory in Get-ChildItem -LiteralPath $path -Directory -Recurse -Force |
                Sort-Object { $_.FullName.Length } -Descending) {
                if (@($directory.EnumerateFileSystemInfos()).Count -eq 0) {
                    $directory.Delete()
                }
            }
        }
    }
    foreach ($change in $changes | Where-Object Change -NE 'Removed') {
        $target = Join-Path $destination $change.Path
        [System.IO.Directory]::CreateDirectory((Split-Path $target -Parent)) | Out-Null
        [System.IO.File]::Copy((Join-Path $staging $change.Path), $target, $true)
    }
    if (@(Get-SnapshotChange (Get-ManagedSnapshot $destination $managedDirectories) $expected).Count -ne 0) {
        throw 'The resulting resources do not match the staged snapshots. Inspect the worktree before retrying.'
    }
    $result.Applied = $true
    return $result
}
finally {
    Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force -WhatIf:$false
}
