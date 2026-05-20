<#
.SYNOPSIS
    Analyzes project.assets.json and writes a dependency report to .work/.

.DESCRIPTION
    Parses NuGet project.assets.json files and produces a JSON report containing:
    - Packages: flat list with requested (from Directory.Packages.props), resolved,
      CPM pin status, and "because" (which parent caused this version)
    - Dependencies: forward dependency tree rooted at servers
    - ReverseDependencies: reverse graph (child -> parents up to server roots)

    When -Server is omitted, all servers are combined into one report.
    Output is written to .work/dependencies.json.

.PARAMETER Server
    Server name (e.g. "Azure.Mcp.Server"). When omitted, all servers are combined.

.EXAMPLE
    ./Analyze-Dependencies.ps1
    # Combined report for all servers -> .work/dependencies.json

.EXAMPLE
    ./Analyze-Dependencies.ps1 -Server "Azure.Mcp.Server"
    # Single server report -> .work/dependencies.json
#>

[CmdletBinding()]
param(
    [string]$Server
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = & git -C $PSScriptRoot rev-parse --show-toplevel 2>$null
if (-not $repoRoot) {
    Write-Error "Could not determine repository root"
    return
}

# ── Parse Directory.Packages.props ────────────────────────────────────────────

$cpmVersions = @{}
$propsPath = Join-Path $repoRoot 'Directory.Packages.props'
if (Test-Path $propsPath) {
    $propsXml = [xml](Get-Content $propsPath -Raw)
    foreach ($pv in $propsXml.SelectNodes('//PackageVersion')) {
        $include = $pv.GetAttribute('Include')
        $ver = $pv.GetAttribute('Version')
        if ($include -and $ver) { $cpmVersions[$include] = $ver }
    }
}

# ── Helpers ───────────────────────────────────────────────────────────────────

function ParseVersionSpec([string]$spec) {
    if (-not $spec) { return '' }
    $spec = $spec.Trim()
    if ($spec -match '^\[([^,\]]+)\]$') { return $Matches[1] }
    if ($spec -match '^\[([^,]+),\s*\)$') { return ">=$($Matches[1])" }
    if ($spec -match '^\(([^,]+),\s*\)$') { return ">$($Matches[1])" }
    return $spec
}

# ── Analyze a single assets file ──────────────────────────────────────────────

function AnalyzeAssets([string]$assetsPath, [string]$serverName) {
    $assets = Get-Content $assetsPath -Raw | ConvertFrom-Json

    $targets = $assets.targets
    $tfm = @($targets.PSObject.Properties.Name)[0]
    $target = $targets.$tfm
    if (-not $target) { return $null }

    # Build resolved package + project reference maps
    $resolvedPackages = @{}
    $projectRefs = @{}
    foreach ($prop in $target.PSObject.Properties) {
        $info = $prop.Value
        $slashIdx = $prop.Name.LastIndexOf('/')
        $name = $prop.Name.Substring(0, $slashIdx)
        $version = $prop.Name.Substring($slashIdx + 1)
        $deps = @{}
        $depsObj = $info.PSObject.Properties['dependencies']
        if ($depsObj -and $depsObj.Value) {
            foreach ($d in $depsObj.Value.PSObject.Properties) {
                $deps[$d.Name] = $d.Value
            }
        }
        if ($info.type -eq 'package') {
            $resolvedPackages[$name] = @{ Version = $version; Dependencies = $deps }
        }
        elseif ($info.type -eq 'project') {
            $projectRefs[$name] = @{ Version = $version; Dependencies = $deps }
        }
    }

    # Framework-level info
    $fwInfo = $assets.project.frameworks.$tfm
    $directDeps = @{}
    if ($fwInfo.dependencies) {
        foreach ($d in $fwInfo.dependencies.PSObject.Properties) {
            $directDeps[$d.Name] = $d.Value
        }
    }

    $pfdgEntries = @($assets.projectFileDependencyGroups.$tfm)
    $pfdgNames = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($entry in $pfdgEntries) {
        [void]$pfdgNames.Add(($entry -split ' ')[0].Trim())
    }

    # "Who pulled me in" map
    $requestedBy = @{}
    foreach ($pkgName in $resolvedPackages.Keys) {
        foreach ($depName in $resolvedPackages[$pkgName].Dependencies.Keys) {
            $depVer = $resolvedPackages[$pkgName].Dependencies[$depName]
            if (-not $requestedBy.ContainsKey($depName)) { $requestedBy[$depName] = [System.Collections.ArrayList]::new() }
            [void]$requestedBy[$depName].Add(@{ From = $pkgName; Wanted = $depVer })
        }
    }

    # Build packages list
    $packages = [ordered]@{}
    foreach ($pkgName in ($resolvedPackages.Keys | Sort-Object)) {
        $pkg = $resolvedPackages[$pkgName]
        $isDirect = $false
        $requestedSpec = $null
        $pinned = $false

        if ($directDeps.ContainsKey($pkgName)) {
            $isDirect = $true
            $depInfo = $directDeps[$pkgName]
            if ($depInfo -is [PSCustomObject]) {
                $versionProp = $depInfo.PSObject.Properties['version']
                $requestedSpec = if ($versionProp) { ParseVersionSpec ($versionProp.Value) } else { '' }
                $cpmProp = $depInfo.PSObject.Properties['versionCentrallyManaged']
                $pinned = if ($cpmProp) { [bool]$cpmProp.Value } else { $false }
            }
            else {
                $requestedSpec = [string]$depInfo
            }
        }

        if ($cpmVersions.ContainsKey($pkgName)) {
            $pinned = $true
            if (-not $requestedSpec) { $requestedSpec = $cpmVersions[$pkgName] }
        }

        $requesters = @()
        if ($requestedBy.ContainsKey($pkgName)) { $requesters = @($requestedBy[$pkgName]) }
        $because = $null

        if (-not $isDirect -and $requesters.Count -gt 0) {
            $because = ($requesters | ForEach-Object { "$($_.From) (wanted $($_.Wanted))" }) -join ', '
        }
        elseif ($isDirect -and $requesters.Count -gt 0) {
            $nonSelf = @($requesters | Where-Object { $_.From -ne $pkgName })
            if ($nonSelf.Count -gt 0) {
                $because = ($nonSelf | ForEach-Object { "$($_.From) (wanted $($_.Wanted))" }) -join ', '
            }
        }

        $entry = [ordered]@{ resolved = $pkg.Version; direct = $isDirect }
        if ($requestedSpec) { $entry.requested = $requestedSpec }
        if ($pinned) { $entry.pinnedByCpm = $true }
        if ($because) { $entry.because = $because }

        $packages[$pkgName] = $entry
    }

    # Forward edges (packages + project refs)
    $forwardEdges = @{}
    foreach ($pkgName in $resolvedPackages.Keys) {
        $forwardEdges[$pkgName] = @($resolvedPackages[$pkgName].Dependencies.Keys)
    }
    foreach ($projName in $projectRefs.Keys) {
        $forwardEdges[$projName] = @($projectRefs[$projName].Dependencies.Keys)
    }

    # Direct names for tree roots
    $directNames = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($n in $pfdgNames) { [void]$directNames.Add($n) }
    foreach ($n in $directDeps.Keys) { [void]$directNames.Add($n) }

    # Forward dependency tree
    function BuildTree([string]$name, [System.Collections.Generic.HashSet[string]]$visited) {
        if ($visited.Contains($name) -or -not $resolvedPackages.ContainsKey($name)) { return [ordered]@{} }
        [void]$visited.Add($name)
        $children = [ordered]@{}
        foreach ($depName in ($resolvedPackages[$name].Dependencies.Keys | Sort-Object)) {
            $newVisited = [System.Collections.Generic.HashSet[string]]::new($visited)
            $children[$depName] = BuildTree $depName $newVisited
        }
        return $children
    }

    $depTree = [ordered]@{}
    foreach ($name in ($directNames | Sort-Object)) {
        if ($resolvedPackages.ContainsKey($name)) {
            $depTree[$name] = BuildTree $name ([System.Collections.Generic.HashSet[string]]::new())
        }
    }

    return @{
        Server       = $serverName
        Framework    = $tfm
        Packages     = $packages
        Dependencies = $depTree
        ForwardEdges = $forwardEdges
        DirectNames  = $directNames
    }
}

# ── Merge multiple server results ─────────────────────────────────────────────

function MergeResults([array]$results) {
    $merged = [ordered]@{}
    $servers = @()
    $serverTrees = [ordered]@{}
    $mergedForward = @{}

    foreach ($r in $results) {
        $servers += $r.Server
        $serverTrees[$r.Server] = $r.Dependencies

        # Server root -> direct deps edge
        $mergedForward[$r.Server] = @($r.DirectNames)

        foreach ($pkg in $r.ForwardEdges.Keys) {
            if (-not $mergedForward.ContainsKey($pkg)) {
                $mergedForward[$pkg] = @($r.ForwardEdges[$pkg])
            }
            else {
                $existing = [System.Collections.Generic.HashSet[string]]::new([string[]]$mergedForward[$pkg])
                foreach ($dep in $r.ForwardEdges[$pkg]) { [void]$existing.Add($dep) }
                $mergedForward[$pkg] = @($existing)
            }
        }

        foreach ($pkgName in $r.Packages.Keys) {
            $pkg = $r.Packages[$pkgName]
            if (-not $merged.Contains($pkgName)) {
                $entry = [ordered]@{}
                foreach ($key in $pkg.Keys) { $entry[$key] = $pkg[$key] }
                $entry.servers = @($r.Server)
                $merged[$pkgName] = $entry
            }
            else {
                $existing = $merged[$pkgName]
                $existing.servers = @($existing.servers) + $r.Server
                if ($existing.resolved -ne $pkg.resolved) {
                    $existing.conflict = "$($r.Server) resolves $($pkg.resolved)"
                }
                if ($pkg.direct) { $existing.direct = $true }
                if ($pkg.Contains('pinnedByCpm')) { $existing.pinnedByCpm = $true }
                if ($pkg.Contains('because')) {
                    if ($existing.Contains('because')) {
                        $newReasons = $pkg.because -split ', ' | Where-Object { -not $existing.because.Contains($_) }
                        if ($newReasons) {
                            $existing.because = $existing.because + ', ' + ($newReasons -join ', ')
                        }
                    }
                    else {
                        $existing.because = $pkg.because
                    }
                }
            }
        }
    }

    $sorted = [ordered]@{}
    foreach ($k in ($merged.Keys | Sort-Object)) { $sorted[$k] = $merged[$k] }

    return @{
        Servers      = $servers
        Framework    = ($results | ForEach-Object { $_.Framework } | Select-Object -Unique) -join ', '
        Packages     = $sorted
        Dependencies = $serverTrees
        ForwardEdges = $mergedForward
    }
}

# ── Build reverse dependency graph ────────────────────────────────────────────

function BuildReverseGraph([hashtable]$forwardEdges, [string[]]$serverRoots) {
    # Add server root edges for single-server mode
    $reverseEdges = @{}
    foreach ($parent in $forwardEdges.Keys) {
        foreach ($child in $forwardEdges[$parent]) {
            if (-not $reverseEdges.ContainsKey($child)) { $reverseEdges[$child] = [System.Collections.ArrayList]::new() }
            [void]$reverseEdges[$child].Add($parent)
        }
    }

    # Build reverse tree for every package
    function BuildReverseTree([string]$name, [System.Collections.Generic.HashSet[string]]$visited) {
        if ($visited.Contains($name)) { return [ordered]@{} }
        [void]$visited.Add($name)
        $parents = [ordered]@{}
        if ($reverseEdges.ContainsKey($name)) {
            foreach ($parentName in ($reverseEdges[$name] | Sort-Object)) {
                $parents[$parentName] = BuildReverseTree $parentName ([System.Collections.Generic.HashSet[string]]::new($visited))
            }
        }
        return $parents
    }

    $reverseGraph = [ordered]@{}
    foreach ($pkgName in ($forwardEdges.Keys | Where-Object { $_ -notin $serverRoots } | Sort-Object)) {
        $tree = BuildReverseTree $pkgName ([System.Collections.Generic.HashSet[string]]::new())
        if ($tree.Count -gt 0) {
            $reverseGraph[$pkgName] = $tree
        }
    }
    return $reverseGraph
}

# ── Main ──────────────────────────────────────────────────────────────────────

$serverExplicit = $PSBoundParameters.ContainsKey('Server')

if ($serverExplicit) {
    $assetsPath = Join-Path $repoRoot "servers/$Server/src/obj/project.assets.json"
    if (-not (Test-Path $assetsPath)) {
        Write-Error "Assets file not found: $assetsPath`nRun 'dotnet restore' on the $Server project first."
        return
    }
    $r = AnalyzeAssets $assetsPath $Server
    if (-not $r) { return }

    # Add server root edge for reverse graph
    $forwardEdges = @{}
    $forwardEdges[$r.Server] = @($r.DirectNames)
    foreach ($pkg in $r.ForwardEdges.Keys) {
        $forwardEdges[$pkg] = @($r.ForwardEdges[$pkg])
    }

    $serverRoots = @($Server)
    $reverseGraph = BuildReverseGraph $forwardEdges $serverRoots

    $output = [ordered]@{
        server              = $Server
        framework           = $r.Framework
        totalPackageCount   = $r.Packages.Count
        packages            = $r.Packages
        dependencies        = [ordered]@{ $Server = $r.Dependencies }
        reverseDependencies = $reverseGraph
    }
}
else {
    $serverDirs = Get-ChildItem (Join-Path $repoRoot 'servers') -Directory
    $results = @()
    foreach ($dir in $serverDirs) {
        $assetsPath = Join-Path $dir.FullName 'src/obj/project.assets.json'
        if (Test-Path $assetsPath) {
            $r = AnalyzeAssets $assetsPath $dir.Name
            if ($r) { $results += $r }
        }
    }
    if ($results.Count -eq 0) {
        Write-Error "No assets files found. Run 'dotnet restore' first."
        return
    }
    $merged = MergeResults $results
    $serverRoots = @($merged.Servers)
    $reverseGraph = BuildReverseGraph $merged.ForwardEdges $serverRoots

    $output = [ordered]@{
        servers             = $merged.Servers
        framework           = $merged.Framework
        totalPackageCount   = $merged.Packages.Count
        packages            = $merged.Packages
        dependencies        = $merged.Dependencies
        reverseDependencies = $reverseGraph
    }
}

# Write output
$workDir = Join-Path $repoRoot '.work'
if (-not (Test-Path $workDir)) { New-Item -ItemType Directory -Path $workDir -Force | Out-Null }

$outPath = Join-Path $workDir 'dependencies.json'
$output | ConvertTo-Json -Depth 20 | Set-Content $outPath -Encoding utf8

$pkgCount = $output.totalPackageCount
$revCount = $output.reverseDependencies.Count
Write-Host "Wrote $outPath ($pkgCount packages, $revCount reverse entries)"
