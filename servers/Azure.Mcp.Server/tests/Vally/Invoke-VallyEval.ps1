#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Discovers and runs Azure MCP Vally experiments.

.DESCRIPTION
Builds Azure.Mcp.Server, provisions area resources, runs each selected
<tool>.experiment.yaml through `vally experiment run`, and tears resources down.
Every Vally run loads the typed reporter under ./reporter. The reporter owns
candidate-vs-baseline comparisons and writes versioned JSON and Markdown
artifacts. This wrapper only orchestrates runs and invokes the reporter's
offline aggregator.

.PARAMETER ExperimentSpec
Optional path to one experiment spec. Cannot be combined with -Area or -Tool.

.PARAMETER Iterations
Number of complete experiment runs per selected tool. Defaults to 1.

.PARAMETER OutputDir
Parent directory for timestamped invocation output. Defaults to ./.vally-results.

.PARAMETER PreEvalScript
Optional provisioning script override. If it emits a Hashtable/PSCustomObject of
`KEY = value` pairs as the LAST object on its output stream, those are forwarded
to vally as `--param KEY=value`, resolving any `${KEY}`/`${KEY=default}`
placeholder the eval prompts reference - so the eval targets whatever resource
identifiers the script actually provisioned (fixed or randomized).

.PARAMETER PostEvalScript
Optional teardown script override. -Subscription is always forwarded, and
-ResourceGroup is forwarded too UNLESS the pre-eval script reported a
`RESOURCE_GROUP` value (see -PreEvalScript) - that reported value wins.

.PARAMETER Area
Optional repeatable area filter.

.PARAMETER Tool
Optional repeatable tool-name filter. Supports wildcards.

.PARAMETER SkipProvisioning
Do not provision or tear down resources.

.PARAMETER ResourceGroup
Optional resource group name forwarded to the pre/post provisioning scripts (when
they declare a -ResourceGroup parameter). If omitted, each script's own default
applies, and - if the pre-eval script reports back the resource group it
actually used - that reported value flows to both vally (as the
`RESOURCE_GROUP` eval param) and the post-eval teardown script.

.PARAMETER Subscription
Optional Azure subscription id or name forwarded to provisioning scripts.

.PARAMETER SkipBuild
Skip the Azure.Mcp.Server build.

.PARAMETER ReportOnly
Do not build the server, provision resources, or invoke Vally. Rebuild the
TypeScript reporter and aggregate its artifacts from the newest invocation.

.PARAMETER ReportFrom
Specific invocation directory to aggregate. Implies -ReportOnly.

.EXAMPLE
./Invoke-VallyEval.ps1 -Subscription <subscription-id>

.EXAMPLE
./Invoke-VallyEval.ps1 -Area eventhubs -Tool eventhub-get -Iterations 3

.EXAMPLE
./Invoke-VallyEval.ps1 -ReportFrom ./.vally-results/2026-07-29T18-19-30-496Z
#>

[CmdletBinding()]
param(
    [string[]] $Area,
    [string[]] $Tool,
    [string] $ExperimentSpec,
    [ValidateRange(1, [int]::MaxValue)]
    [int] $Iterations = 1,
    [string] $OutputDir = (Join-Path $PSScriptRoot '.vally-results'),
    [string] $PreEvalScript,
    [string] $PostEvalScript,
    [string] $ResourceGroup,
    [string] $Subscription,
    [switch] $SkipBuild,
    [switch] $SkipProvisioning,
    [switch] $ReportOnly,
    [string] $ReportFrom
)

$ErrorActionPreference = 'Stop'
$OutputEncoding = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

if ($ReportFrom) { $ReportOnly = $true }
if ($ExperimentSpec -and ($Area -or $Tool)) {
    throw '-ExperimentSpec cannot be combined with -Area or -Tool.'
}
if ($ReportOnly -and ($PreEvalScript -or $PostEvalScript -or $SkipProvisioning -or $SkipBuild)) {
    Write-Warning '-ReportOnly ignores -PreEvalScript/-PostEvalScript/-SkipProvisioning/-SkipBuild.'
}
elseif ($SkipProvisioning -and ($PreEvalScript -or $PostEvalScript)) {
    Write-Warning '-SkipProvisioning was set; ignoring -PreEvalScript/-PostEvalScript.'
    $PreEvalScript = $null
    $PostEvalScript = $null
}

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..' '..')
$ServerProject = Join-Path $RepoRoot 'servers' 'Azure.Mcp.Server' 'src' 'Azure.Mcp.Server.csproj'
$ReporterPackageDir = Join-Path $PSScriptRoot 'reporter'
$ReporterPlugin = Join-Path $ReporterPackageDir 'dist' 'plugin.js'
$ReporterCli = Join-Path $ReporterPackageDir 'dist' 'cli.js'
$ReporterFileName = 'azure-mcp-report.json'
$ExperimentSuffix = '.experiment.yaml'
$TimestampPattern = '^\d{4}-\d{2}-\d{2}T'

function Write-Info($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Cyan }
function Write-Warn($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Yellow }
function Write-Failure($Message) { Write-Error "[vally-eval] $Message" -ErrorAction Continue }

function Initialize-VallyReporter {
    if (-not (Get-Command 'node' -ErrorAction SilentlyContinue)) {
        throw 'Node.js was not found on PATH. It is required for the TypeScript Vally reporter.'
    }
    if (-not (Get-Command 'npm' -ErrorAction SilentlyContinue)) {
        throw 'npm was not found on PATH. It is required for the TypeScript Vally reporter.'
    }

    $packageLock = Join-Path $ReporterPackageDir 'package-lock.json'
    $dependencyMarker = Join-Path $ReporterPackageDir 'node_modules' '.azure-mcp-package-lock-hash'
    $packageLockHash = (Get-FileHash -LiteralPath $packageLock -Algorithm SHA256).Hash
    $installedHash = (Test-Path -LiteralPath $dependencyMarker) `
        ? [System.IO.File]::ReadAllText($dependencyMarker).Trim() `
        : $null
    if ($installedHash -ne $packageLockHash) {
        Write-Info 'Restoring Vally reporter dependencies ...'
        npm ci --prefix $ReporterPackageDir --prefer-offline --no-audit --no-fund | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "npm ci failed for the Vally reporter with exit code $LASTEXITCODE."
        }
        [System.IO.File]::WriteAllText($dependencyMarker, $packageLockHash)
    }

    Write-Info 'Building the TypeScript Vally reporter ...'
    npm run build --prefix $ReporterPackageDir | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "The Vally reporter TypeScript build failed with exit code $LASTEXITCODE."
    }
}

function Resolve-InvocationDirectory {
    if ($ReportOnly) {
        if ($ReportFrom) {
            $resolved = Resolve-Path -Path $ReportFrom -ErrorAction SilentlyContinue
            if (-not $resolved) { throw "-ReportFrom run directory not found: $ReportFrom" }
            return $resolved.Path
        }

        if (-not (Test-Path -LiteralPath $OutputDir)) {
            throw "No run artifacts found under '$OutputDir'."
        }
        $newest = Get-ChildItem -Path $OutputDir -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match $TimestampPattern } |
        Sort-Object Name -Descending |
        Select-Object -First 1
        if (-not $newest) {
            throw "No timestamped run directories found under '$OutputDir'."
        }
        return $newest.FullName
    }

    return Join-Path $OutputDir ([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH-mm-ss-fffZ'))
}

function Initialize-AzureMcpServer {
    if (-not (Get-Command 'vally' -ErrorAction SilentlyContinue)) {
        throw "The 'vally' CLI was not found on PATH. See https://microsoft.github.io/vally/get-started/install/."
    }

    $exeName = $IsWindows ? 'azmcp.exe' : 'azmcp'
    if (-not $SkipBuild) {
        Write-Info "Building $ServerProject ..."
        dotnet build $ServerProject -c Debug | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE."
        }
    }

    $serverBinDir = Join-Path (Split-Path $ServerProject -Parent) 'bin'
    $azmcp = Get-ChildItem -Path $serverBinDir -Recurse -Filter $exeName -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
    if (-not $azmcp) {
        throw "Could not find '$exeName' under '$serverBinDir'."
    }

    Write-Info "Using azmcp: $($azmcp.FullName)"
    $env:PATH = "$(Split-Path $azmcp.FullName -Parent)$([IO.Path]::PathSeparator)$env:PATH"
}

function Get-SelectedExperiments {
    $files = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    if ($ExperimentSpec) {
        $resolved = Resolve-Path -Path $ExperimentSpec -ErrorAction SilentlyContinue
        if (-not $resolved) { throw "Experiment spec not found: $ExperimentSpec" }
        $files.Add((Get-Item $resolved.Path))
    }
    else {
        $candidates = Get-ChildItem -Path $PSScriptRoot -Recurse -File -Filter "*$ExperimentSuffix" |
        Sort-Object FullName
        foreach ($candidate in $candidates) {
            $areaName = Split-Path (Split-Path $candidate.FullName -Parent) -Leaf
            $toolName = $candidate.Name.Substring(0, $candidate.Name.Length - $ExperimentSuffix.Length)
            if ($Area -and -not ($Area | Where-Object { $areaName -like $_ })) { continue }
            if ($Tool -and -not ($Tool | Where-Object { $toolName -like $_ })) { continue }
            $files.Add($candidate)
        }
    }

    if ($files.Count -eq 0) {
        throw "No experiments matched (Area='$($Area -join ',')', Tool='$($Tool -join ',')')."
    }

    return @($files | ForEach-Object {
            $areaDir = Split-Path $_.FullName -Parent
            [pscustomobject]@{
                Area       = Split-Path $areaDir -Leaf
                AreaDir    = $areaDir
                Tool       = $_.Name.Substring(0, $_.Name.Length - $ExperimentSuffix.Length)
                Experiment = $_.FullName
            }
        })
}

function Resolve-AreaProvisioning {
    param([Parameter(Mandatory)] [string] $AreaDir)

    if ($SkipProvisioning) {
        return [pscustomobject]@{ Pre = $null; Post = $null }
    }

    $pre = $PreEvalScript
    if (-not $pre) {
        $pre = Get-ChildItem -Path $AreaDir -Filter 'New-*Resources.ps1' -File -ErrorAction SilentlyContinue |
        Sort-Object Name | Select-Object -First 1 -ExpandProperty FullName
    }
    $post = $PostEvalScript
    if (-not $post) {
        $post = Get-ChildItem -Path $AreaDir -Filter 'Remove-*Resources.ps1' -File -ErrorAction SilentlyContinue |
        Sort-Object Name | Select-Object -First 1 -ExpandProperty FullName
    }

    return [pscustomobject]@{ Pre = $pre; Post = $post }
}

function Invoke-ProvisioningScript {
    param(
        [Parameter(Mandatory)] [string] $Label,
        [Parameter(Mandatory)] [string] $Path,
        [hashtable] $ParamOverrides = @{}
    )

    $resolved = Resolve-Path -Path $Path -ErrorAction SilentlyContinue
    if (-not $resolved) { throw "$Label script not found: $Path" }

    $arguments = @{}
    $parameters = (Get-Command $resolved.Path).Parameters
    foreach ($key in $ParamOverrides.Keys) {
        $value = $ParamOverrides[$key]
        if ($value -and $parameters.ContainsKey($key)) { $arguments[$key] = $value }
    }

    Write-Info "[$Label] Running: $($resolved.Path)"
    $global:LASTEXITCODE = 0
    $output = & $resolved.Path @arguments
    if ($LASTEXITCODE) { throw "$Label script failed with exit code $LASTEXITCODE." }

    # Find the LAST dictionary-shaped output object, if any, and treat it as the
    # script's reported resource params (e.g. a randomized resource group/
    # namespace name it actually created), so the eval can target what was
    # truly provisioned instead of a hardcoded guess.
    $reportedParams = [ordered]@{}
    foreach ($item in @($output)) {
        if ($item -is [System.Collections.IDictionary]) {
            $reportedParams = [ordered]@{}
            foreach ($key in $item.Keys) { $reportedParams[[string]$key] = [string]$item[$key] }
        }
        elseif ($item -is [pscustomobject]) {
            $reportedParams = [ordered]@{}
            foreach ($prop in $item.psobject.Properties) { $reportedParams[$prop.Name] = [string]$prop.Value }
        }
    }
    return $reportedParams
}

function Invoke-VallyExperiment {
    param(
        [Parameter(Mandatory)] [string] $Label,
        [Parameter(Mandatory)] [string] $ExperimentFile,
        [Parameter(Mandatory)] [string] $RunOutputDir,
        [System.Collections.IDictionary] $Params
    )

    $before = @{}
    if (Test-Path -LiteralPath $RunOutputDir) {
        Get-ChildItem -Path $RunOutputDir -Directory | ForEach-Object { $before[$_.FullName] = $true }
    }

    $arguments = @(
        'experiment', 'run', $ExperimentFile,
        '--output-dir', $RunOutputDir,
        '--reporter-plugin', $ReporterPlugin
    )
    # Forward every reported/overridden resource identifier as its own --param
    # flag, so vally resolves any ${KEY}/${KEY=default} placeholder the eval
    # spec references (e.g. the resource group/namespace the provisioning
    # script actually created) per its parameter-resolution rules:
    # https://microsoft.github.io/vally/reference/cli/eval/#parameter-resolution
    foreach ($key in @($Params.Keys)) { $arguments += @('--param', "$key=$($Params[$key])") }
    Write-Info "[$Label] Running: vally $($arguments -join ' ')"
    Push-Location $PSScriptRoot
    try {
        & vally @arguments | Out-Host
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    $runDirectory = Get-ChildItem -Path $RunOutputDir -Directory -ErrorAction SilentlyContinue |
    Where-Object { -not $before.ContainsKey($_.FullName) } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
    if (-not $runDirectory) {
        $runDirectory = Get-ChildItem -Path $RunOutputDir -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    }

    return [pscustomobject]@{
        Exit       = $exitCode
        ReportFile = $runDirectory ? (Join-Path $runDirectory.FullName $ReporterFileName) : $null
    }
}

function Get-ExperimentResultsFromRuns {
    param([Parameter(Mandatory)] $Experiments)

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($areaGroup in ($Experiments | Group-Object AreaDir)) {
        $areaDir = $areaGroup.Name
        $areaName = Split-Path $areaDir -Leaf
        $provisioning = Resolve-AreaProvisioning -AreaDir $areaDir
        $provisioned = $true

        # Resource identifiers reported back by the pre-eval provisioning script
        # (e.g. a randomized resource group/namespace name it actually created),
        # forwarded to vally as eval params. Falls back to an empty map when
        # there's no pre-eval script, or it reports nothing.
        $reportedParams = [ordered]@{}
        # The resource group to forward to the POST-eval (teardown) script: the
        # user's explicit -ResourceGroup wins; otherwise, if the pre-eval script
        # reported the one it actually provisioned (e.g. a randomized name),
        # teardown must target THAT one, not this runner's own default guess.
        $teardownResourceGroup = $ResourceGroup

        try {
            if ($provisioning.Pre) {
                try {
                    $reportedParams = Invoke-ProvisioningScript -Label "$areaName pre-eval provisioning" -Path $provisioning.Pre `
                        -ParamOverrides @{ ResourceGroup = $ResourceGroup; Subscription = $Subscription }
                    if (-not $ResourceGroup -and $reportedParams.Contains('RESOURCE_GROUP')) {
                        $teardownResourceGroup = $reportedParams['RESOURCE_GROUP']
                    }
                }
                catch {
                    $provisioned = $false
                    Write-Warn "[$areaName] Provisioning failed: $($_.Exception.Message)"
                    foreach ($experiment in $areaGroup.Group) {
                        $results.Add([pscustomobject]@{
                                Name               = "$($experiment.Area)/$($experiment.Tool)"
                                ProvisioningFailed = $true
                                Iterations         = @()
                            })
                    }
                }
            }

            if ($provisioned) {
                foreach ($experiment in $areaGroup.Group) {
                    $label = "$($experiment.Area)/$($experiment.Tool)"
                    $runRoot = Join-Path (Join-Path $script:RunDir $experiment.Area) $experiment.Tool
                    $iterationResults = [System.Collections.Generic.List[object]]::new()
                    for ($index = 1; $index -le $Iterations; $index++) {
                        $iterationLabel = ($Iterations -gt 1) ? "$label ($index/$Iterations)" : $label
                        $run = Invoke-VallyExperiment -Label $iterationLabel -ExperimentFile $experiment.Experiment -RunOutputDir $runRoot -Params $reportedParams
                        $iterationResults.Add([pscustomobject]@{
                                Iteration      = $index
                                ExperimentExit = $run.Exit
                                ReportFile     = $run.ReportFile
                            })
                    }
                    $results.Add([pscustomobject]@{
                            Name               = $label
                            ProvisioningFailed = $false
                            Iterations         = $iterationResults
                        })
                }
            }
        }
        finally {
            if ($provisioning.Post) {
                try {
                    $null = Invoke-ProvisioningScript -Label "$areaName post-eval teardown" -Path $provisioning.Post `
                        -ParamOverrides @{ ResourceGroup = $teardownResourceGroup; Subscription = $Subscription }
                }
                catch {
                    Write-Warn "[$areaName] Teardown failed: $($_.Exception.Message)"
                    Write-Warn 'Resources may still exist; the DeleteAfter tag provides eventual cleanup.'
                }
            }
        }
    }

    return $results
}

function Get-ExperimentResultsFromArtifacts {
    param([Parameter(Mandatory)] $Experiments)

    $results = [System.Collections.Generic.List[object]]::new()
    foreach ($experiment in $Experiments) {
        $label = "$($experiment.Area)/$($experiment.Tool)"
        $runRoot = Join-Path (Join-Path $script:RunDir $experiment.Area) $experiment.Tool
        if (-not (Test-Path -LiteralPath $runRoot)) {
            Write-Warn "[$label] No artifacts under '$runRoot'."
            continue
        }

        $directories = @(Get-ChildItem -Path $runRoot -Directory |
            Where-Object { $_.Name -match $TimestampPattern } |
            Sort-Object Name -Descending |
            Select-Object -First $Iterations |
            Sort-Object Name)
        if ($directories.Count -eq 0) {
            Write-Warn "[$label] No timestamped iteration directories under '$runRoot'."
            continue
        }

        $index = 0
        $iterationResults = @($directories | ForEach-Object {
                $index++
                [pscustomobject]@{
                    Iteration      = $index
                    ExperimentExit = 0
                    ReportFile     = Join-Path $_.FullName $ReporterFileName
                }
            })
        $results.Add([pscustomobject]@{
                Name               = $label
                ProvisioningFailed = $false
                Iterations         = $iterationResults
            })
    }

    return $results
}

function Invoke-CustomReporterSummary {
    param([Parameter(Mandatory)] $Results)

    $reportFiles = [System.Collections.Generic.List[string]]::new()
    $missingReport = $false
    foreach ($result in $Results) {
        if ($result.ProvisioningFailed) {
            $missingReport = $true
            continue
        }
        foreach ($iteration in @($result.Iterations)) {
            if ($iteration.ReportFile -and (Test-Path -LiteralPath $iteration.ReportFile)) {
                $reportFiles.Add($iteration.ReportFile)
            }
            else {
                Write-Warn "[$($result.Name)] Missing reporter artifact for iteration $($iteration.Iteration): $($iteration.ReportFile)"
                $missingReport = $true
            }
        }
    }

    if ($reportFiles.Count -eq 0) {
        Write-Warn 'No custom reporter artifacts were found.'
        return $true
    }

    $arguments = @($ReporterCli, '--output-dir', $script:RunDir)
    foreach ($reportFile in $reportFiles) { $arguments += @('--report', $reportFile) }
    & node @arguments | Out-Host
    $reporterExit = $LASTEXITCODE
    $reporterFailed = $reporterExit -ne 0
    if ($reporterExit -eq 1) {
        Write-Failure 'The consolidated report detected one or more candidate failures.'
    }
    elseif ($reporterFailed) {
        Write-Failure "The reporter aggregator failed (exit $reporterExit)."
    }

    Write-Info "Run directory: $script:RunDir"
    return $missingReport -or $reporterFailed
}

$script:RunDir = [System.IO.Path]::GetFullPath((Resolve-InvocationDirectory), (Get-Location).ProviderPath)
Write-Info "Run directory: $script:RunDir"
Initialize-VallyReporter
if (-not $ReportOnly) { Initialize-AzureMcpServer }

$experiments = @(Get-SelectedExperiments)
Write-Info ("Discovered {0} experiment(s): {1}" -f $experiments.Count, (($experiments | ForEach-Object { "$($_.Area)/$($_.Tool)" }) -join ', '))

$results = $ReportOnly `
    ? (Get-ExperimentResultsFromArtifacts -Experiments $experiments) `
    : (Get-ExperimentResultsFromRuns -Experiments $experiments)
if (@($results).Count -eq 0) {
    throw "No results found under '$script:RunDir' for the selected experiments."
}

$failed = Invoke-CustomReporterSummary -Results $results
exit ($failed ? 1 : 0)
