# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# cspell:ignore pycache snupkg venv
#Requires -Version 7.0
<#
.SYNOPSIS
Reports logical build-output bytes without building, deleting, or following links.
.DESCRIPTION
Run against a quiescent checkout after building the desired profile. Profile is a
caller-supplied label, not a build instruction. JSON is emitted on the success
stream and a human-readable summary on the information stream. No files are
written unless OutputPath is supplied. Reports omit timestamps for reproducibility.

Each file belongs to the first matching rule, in categoryRules order. Source
files are excluded. All .work output, including relocated shared test hosts, is
included except known build-cache roots and this report. Dependency-directory
names such as node_modules and packages are excluded outside output trees, but
included inside output so packaged dependencies are measured. Git directories
and .work/{cache,caches,.cache,.nuget,.npm,.yarn,.pnpm-store,.dotnet} are always
excluded. AdditionalOutputPath
identifies other repository-contained output directories. External caches,
Docker's image store, physical disk allocation, and hard-link deduplication are
not measured. Exported image archives are measured.

SHA256 is calculated only for same-size groups at or above MinimumDuplicateBytes
(default 1 MiB). PotentialDuplicateBytes is (copies - 1) * logical size, not a
claim that the copies can safely be removed. Read, enumeration, and hash failures
terminate the report rather than silently presenting incomplete totals.
.PARAMETER RepoRoot
Repository root. Defaults to two directories above this script.
.PARAMETER Profile
Descriptive label for an already-built profile, for example Debug or Release.
.PARAMETER OutputPath
Optional JSON file, absolute or relative to RepoRoot. Its contents are excluded
from measurement, including on subsequent runs. This is the only requested write.
.PARAMETER AdditionalOutputPath
Additional output directories inside RepoRoot, absolute or relative to RepoRoot.
Known cache directories and reparse points are excluded even under these roots.
.PARAMETER MinimumDuplicateBytes
Inclusive minimum logical file size for duplicate candidate hashing. Default 1 MiB.
.PARAMETER Top
Maximum number of largest directories and files to report. Directory totals are
recursive and overlap; only category totals are disjoint.
.EXAMPLE
.\eng\scripts\Measure-BuildOutput.ps1 -Profile Debug
.EXAMPLE
.\eng\scripts\Measure-BuildOutput.ps1 -Profile Release -OutputPath .work\size-release.json
#>
[CmdletBinding()]
param(
    [string] $RepoRoot = (Join-Path $PSScriptRoot '..\..'),
    [ValidateNotNullOrEmpty()]
    [string] $Profile = 'unspecified',
    [string] $OutputPath,
    [string[]] $AdditionalOutputPath = @(),
    [ValidateRange(1, [long]::MaxValue)]
    [long] $MinimumDuplicateBytes = 1MB,
    [ValidateRange(1, 1000)]
    [int] $Top = 20
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Assert-NoBuildOutputLink {
    param([string] $Path)

    $current = $Path
    while ($current) {
        if (Test-Path -LiteralPath $current -ErrorAction Stop) {
            $item = Get-Item -LiteralPath $current -Force -ErrorAction Stop
            if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "Link/reparse path is not allowed: '$current'."
            }
        }
        # An explicitly selected root can live below an OS path alias (for
        # example macOS /var). Only enforce link boundaries within that root.
        if ($current -eq $root) { break }
        $current = [IO.Path]::GetDirectoryName($current)
    }
}

$root = [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($RepoRoot))
Assert-NoBuildOutputLink $root
if (-not (Get-Item -LiteralPath $root -Force).PSIsContainer) {
    throw "Repository root is not a directory: '$root'."
}
$comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
$pathComparer = if ($IsWindows) { [StringComparer]::OrdinalIgnoreCase } else { [StringComparer]::Ordinal }
$outputFile = if ($OutputPath) { [IO.Path]::GetFullPath($OutputPath, $root) } else { $null }
if ($outputFile) { Assert-NoBuildOutputLink $outputFile }
[string[]] $extraRoots = @(
    foreach ($path in $AdditionalOutputPath) {
        $fullPath = [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($path, $root))
        if (-not $fullPath.StartsWith("$root$([IO.Path]::DirectorySeparatorChar)", $comparison)) {
            throw "Additional output must be a directory inside the repository: '$path'."
        }
        Assert-NoBuildOutputLink $fullPath
        if (-not (Get-Item -LiteralPath $fullPath -Force).PSIsContainer) {
            throw "Additional output is not a directory: '$path'."
        }
        [IO.Path]::GetRelativePath($root, $fullPath).Replace('\', '/')
    }
)
[Array]::Sort($extraRoots, [StringComparer]::Ordinal)

$rules = @(
    [ordered]@{ category = 'testRuntime'; match = '.work/test-runtime/**, including shared hosts and nested output' }
    [ordered]@{ category = 'publish'; match = '.work/build/**' }
    [ordered]@{ category = 'packageStaging'; match = '.work/temp*/** or docker_staged/docker_staging directory segment' }
    [ordered]@{ category = 'finalPackagesAndImages'; match = 'Archives in .work/packages_*/**' }
    [ordered]@{ category = 'packageStaging'; match = 'Remaining .work/packages_*/** content' }
    [ordered]@{ category = 'obj'; match = 'obj directory segment' }
    [ordered]@{ category = 'publish'; match = 'publish directory segment' }
    [ordered]@{ category = 'finalPackagesAndImages'; match = 'Archive in recognized output' }
    [ordered]@{ category = 'bin'; match = 'bin directory segment' }
    [ordered]@{ category = 'otherBuildOutput'; match = 'Remaining .work, artifacts, .artifacts, TestResults, BenchmarkDotNet.Artifacts, .perf-results, and additional output' }
)
$categories = [ordered]@{}
foreach ($name in @('testRuntime', 'publish', 'packageStaging', 'finalPackagesAndImages', 'obj', 'bin', 'otherBuildOutput')) {
    $categories[$name] = [pscustomobject][ordered]@{ name = $name; fileCount = [long]0; logicalBytes = [long]0 }
}
$files = [Collections.Generic.List[object]]::new()
$skipped = [Collections.Generic.List[object]]::new()
$directories = [Collections.Generic.Dictionary[string, object]]::new($pathComparer)
$pending = [Collections.Generic.Stack[string]]::new()
$pending.Push($root)
$cacheNames = @('node_modules', '.nuget', '.npm', '.yarn', '.pnpm-store', '.cache', 'packages', '.dotnet', '.venv', '__pycache__')

while ($pending.Count -gt 0) {
    $directory = $pending.Pop()
    try { $entries = @(Get-ChildItem -LiteralPath $directory -Force -ErrorAction Stop) }
    catch { throw "Cannot enumerate '$directory': $($_.Exception.Message)" }
    foreach ($entry in $entries) {
        $relative = [IO.Path]::GetRelativePath($root, $entry.FullName).Replace('\', '/')
        if ($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            $skipped.Add([pscustomobject][ordered]@{ path = $relative; reason = 'linkOrReparsePoint' })
            continue
        }
        $scopePath = if ($entry.PSIsContainer) { "$relative/" } else { $relative }
        $inOutput = $scopePath -match '(^|/)(bin|obj|publish|artifacts|\.artifacts|TestResults|BenchmarkDotNet\.Artifacts|\.perf-results|docker_staged|docker_staging)/' -or
            $scopePath.StartsWith('.work/', $comparison)
        foreach ($extra in $extraRoots) {
            if ($scopePath.StartsWith("$extra/", $comparison)) { $inOutput = $true }
        }
        if ($entry.PSIsContainer) {
            $namedBuildCache = $relative -match '^\.work/(cache|caches|\.cache|\.nuget|\.npm|\.yarn|\.pnpm-store|\.dotnet)$'
            if ($entry.Name -eq '.git' -or $namedBuildCache -or (-not $inOutput -and $entry.Name -in $cacheNames)) {
                $skipped.Add([pscustomobject][ordered]@{ path = $relative; reason = 'dependencyCacheOrGit' })
            }
            else { $pending.Push($entry.FullName) }
            continue
        }
        if ($outputFile -and $entry.FullName.Equals($outputFile, $comparison)) { continue }
        if (-not $inOutput) { continue }
        $archive = $entry.Extension -in @('.zip', '.tgz', '.gz', '.nupkg', '.snupkg', '.whl', '.vsix', '.mcpb', '.tar')
        if ($relative -match '^\.work/test-runtime/') { $category = 'testRuntime' }
        elseif ($relative -match '^\.work/build/') { $category = 'publish' }
        elseif ($relative -match '^\.work/temp[^/]*/' -or $relative -match '(^|/)(docker_staged|docker_staging)/') { $category = 'packageStaging' }
        elseif ($relative -match '^\.work/packages_[^/]+/') {
            $category = if ($archive) { 'finalPackagesAndImages' } else { 'packageStaging' }
        }
        elseif ($relative -match '(^|/)obj/') { $category = 'obj' }
        elseif ($relative -match '(^|/)publish/') { $category = 'publish' }
        elseif ($archive) { $category = 'finalPackagesAndImages' }
        elseif ($relative -match '(^|/)bin/') { $category = 'bin' }
        else { $category = 'otherBuildOutput' }
        try {
            $length = $entry.Length
            if ($null -eq $length) { throw 'File length is unavailable.' }
            $size = [long]$length
        }
        catch { throw "Cannot read size of '$($entry.FullName)': $($_.Exception.Message)" }
        $files.Add([pscustomobject][ordered]@{ path = $relative; category = $category; logicalBytes = $size })
        $categories[$category].fileCount++
        $categories[$category].logicalBytes += $size
        $parent = [IO.Path]::GetDirectoryName($relative).Replace('\', '/')
        while ($parent) {
            if (-not $directories.ContainsKey($parent)) {
                $directories[$parent] = [pscustomobject][ordered]@{ path = $parent; fileCount = [long]0; logicalBytes = [long]0 }
            }
            $directories[$parent].fileCount++
            $directories[$parent].logicalBytes += $size
            $parent = [IO.Path]::GetDirectoryName($parent).Replace('\', '/')
        }
    }
}

$duplicates = [Collections.Generic.List[object]]::new()
[long] $hashedFileCount = 0
[long] $potentialDuplicateBytes = 0
foreach ($sizeGroup in @($files | Where-Object logicalBytes -GE $MinimumDuplicateBytes | Group-Object logicalBytes)) {
    if ($sizeGroup.Count -lt 2) { continue }
    $hashes = @{}
    foreach ($file in $sizeGroup.Group) {
        $fullPath = Join-Path $root $file.path
        try {
            Assert-NoBuildOutputLink $fullPath
            $before = Get-Item -LiteralPath $fullPath -Force
            if ($before.Length -ne $file.logicalBytes) { throw 'File size changed during measurement.' }
            $hash = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256 -ErrorAction Stop).Hash
            $after = Get-Item -LiteralPath $fullPath -Force
            if ($after.Length -ne $before.Length -or $after.LastWriteTimeUtc -ne $before.LastWriteTimeUtc) {
                throw 'File changed during hashing.'
            }
        }
        catch { throw "Cannot hash '$fullPath': $($_.Exception.Message)" }
        $hashedFileCount++
        if (-not $hashes.ContainsKey($hash)) { $hashes[$hash] = [Collections.Generic.List[string]]::new() }
        $hashes[$hash].Add($file.path)
    }
    foreach ($hash in $hashes.Keys) {
        if ($hashes[$hash].Count -lt 2) { continue }
        $paths = $hashes[$hash].ToArray()
        [Array]::Sort($paths, [StringComparer]::Ordinal)
        [long] $size = $sizeGroup.Group[0].logicalBytes
        [long] $potential = ($paths.Count - 1) * $size
        $potentialDuplicateBytes += $potential
        $duplicates.Add([pscustomobject][ordered]@{
            sha256 = $hash; fileBytes = $size; fileCount = [long]$paths.Count
            potentialDuplicateBytes = $potential; paths = @($paths)
        })
    }
}

# Stable ordinal path ranking breaks equal-size ties independently of enumeration order.
$rankedFiles = $files.ToArray()
[Array]::Sort($rankedFiles, [Comparison[object]]{
    param($a, $b)
    $order = $b.logicalBytes.CompareTo($a.logicalBytes)
    if ($order -eq 0) { $order = [StringComparer]::Ordinal.Compare($a.path, $b.path) }
    $order
})
$rankedDirectories = @($directories.Values)
[Array]::Sort($rankedDirectories, [Comparison[object]]{
    param($a, $b)
    $order = $b.logicalBytes.CompareTo($a.logicalBytes)
    if ($order -eq 0) { $order = [StringComparer]::Ordinal.Compare($a.path, $b.path) }
    $order
})
$rankedDuplicates = $duplicates.ToArray()
[Array]::Sort($rankedDuplicates, [Comparison[object]]{
    param($a, $b)
    $order = $b.potentialDuplicateBytes.CompareTo($a.potentialDuplicateBytes)
    if ($order -eq 0) { $order = [StringComparer]::Ordinal.Compare($a.sha256, $b.sha256) }
    $order
})
$rankedSkipped = $skipped.ToArray()
[Array]::Sort($rankedSkipped, [Comparison[object]]{ param($a, $b) [StringComparer]::Ordinal.Compare($a.path, $b.path) })
[long] $totalBytes = 0
foreach ($category in $categories.Values) { $totalBytes += $category.logicalBytes }
$report = [ordered]@{
    schemaVersion = 1
    profile = $Profile
    repoRoot = $root
    measurement = 'logicalBytes'
    categoryRules = @($rules)
    additionalOutputPaths = @($extraRoots)
    total = [ordered]@{ fileCount = [long]$files.Count; logicalBytes = $totalBytes }
    categories = @($categories.Values)
    duplicateAnalysis = [ordered]@{
        algorithm = 'SHA256'
        minimumFileBytes = $MinimumDuplicateBytes
        hashedFileCount = $hashedFileCount
        potentialDuplicateBytes = $potentialDuplicateBytes
        caveat = 'Identical content is not necessarily removable. Logical sizes do not account for hard links or disk allocation.'
        groups = @($rankedDuplicates)
    }
    topLimit = $Top
    topDirectories = @($rankedDirectories | Select-Object -First $Top)
    topFiles = @($rankedFiles | Select-Object -First $Top)
    directoryTotalsOverlap = $true
    skippedPaths = @($rankedSkipped)
}
$json = $report | ConvertTo-Json -Depth 12
if ($outputFile) {
    Assert-NoBuildOutputLink $outputFile
    $outputDirectory = [IO.Path]::GetDirectoryName($outputFile)
    [IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
    [IO.File]::WriteAllText($outputFile, "$json`n", [Text.UTF8Encoding]::new($false))
}
Write-Host "Build output profile '$Profile': $($files.Count) files, $totalBytes logical bytes"
foreach ($category in $categories.Values) {
    Write-Host ("  {0}: {1} files, {2} bytes" -f $category.name, $category.fileCount, $category.logicalBytes)
}
Write-Host "Potential duplicate bytes: $potentialDuplicateBytes (threshold: $MinimumDuplicateBytes; not necessarily removable)"
Write-Host "Largest directories (recursive totals overlap):"
foreach ($directory in $report.topDirectories) { Write-Host "  $($directory.logicalBytes) bytes  $($directory.path)" }
Write-Host "Largest files:"
foreach ($file in $report.topFiles) { Write-Host "  $($file.logicalBytes) bytes  $($file.path)" }
Write-Host "Skipped paths: $($skipped.Count). See JSON for details."
$json
