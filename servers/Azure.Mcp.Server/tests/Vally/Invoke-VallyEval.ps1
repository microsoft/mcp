#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Discovers and runs the vally evaluations for the Azure MCP tools.

.DESCRIPTION
This is a wrapper around the vally CLI (https://microsoft.github.io/vally) that:

  1. Builds the Azure.Mcp.Server project (unless -SkipBuild is passed) so the
     freshly compiled `azmcp` executable is available.
  2. Prepends the build output directory to PATH so the `command: azmcp` entry in
     each eval spec resolves to your local build (vally does not interpolate
     environment variables inside eval specs).
  3. Discovers evaluations laid out by area and tool (see below). For each area it
     optionally runs a pre-evaluation provisioning script to create the Azure
     resources the evals expect, and always runs a post-evaluation teardown script
     afterwards (even when an eval or provisioning fails), so resources are not
     leaked.
  4. For each discovered tool, runs two evaluations for comparison:
       - treatment (<tool>.eval.yaml)          - WITH the Azure MCP server, and
       - baseline  (<tool>.eval.baseline.yaml) - WITHOUT the Azure MCP server.
     The delta between them isolates the Azure MCP server's contribution.

Layout / naming convention (discovered automatically):

    tests/Vally/
      <area>/                          # e.g. eventhubs
        <tool>.eval.yaml               # treatment spec (required)
        <tool>.eval.baseline.yaml      # control spec   (optional)
        New-*Resources.ps1             # per-area provisioning (optional)
        Remove-*Resources.ps1          # per-area teardown     (optional)

The tool name is the eval file name without the '.eval.yaml' suffix (e.g.
'eventhub-get'). Use -Area / -Tool to filter what runs, or -EvalSpec to run a
single spec.

Vally must be installed and on your PATH. See
https://microsoft.github.io/vally/get-started/install/.

The treatment grades tool *selection* and task *outcome*; the baseline reuses the
shared *outcome* graders WITHOUT the Azure MCP server, so it is EXPECTED TO FAIL.
Provide a real subscription (via `az login`) so the treatment can return real data
and the comparison is meaningful. The script's exit code is non-zero if any
treatment eval fails; baseline failures are expected and do not affect it.

.PARAMETER EvalSpec
Path to the treatment eval spec (WITH the Azure MCP server). Defaults to
./eventhubs/eval.yaml next to this script.

.PARAMETER BaselineSpec
Path to the baseline/control eval spec (WITHOUT the Azure MCP server). Defaults to
./eventhubs/eval.baseline.yaml next to this script.

.PARAMETER OutputDir
Parent directory where vally writes its run artifacts. Each run goes to a
'treatment' or 'baseline' subdirectory. Defaults to ./.vally-results next to this
script.

.PARAMETER Model
Overrides the model configured in the eval specs (defaults.model).

.PARAMETER PreEvalScript
Optional path to a provisioning script run BEFORE the evaluations (e.g.
./eventhubs/New-EventHubsResources.ps1). Use it to create the Azure resources the
eval prompts reference. -ResourceGroup and -Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `New-*Resources.ps1` script next to the eval spec and uses that.

.PARAMETER PostEvalScript
Optional path to a teardown script run AFTER the evaluations (e.g.
./eventhubs/Remove-EventHubsResources.ps1). It runs in a finally block, so it
executes even if the eval or the pre-eval provisioning fails. -ResourceGroup and
-Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `Remove-*Resources.ps1` script next to the eval spec and uses
that.

.PARAMETER Area
Optional filter: only evaluate specs under the named area subdirectory (e.g.
'eventhubs'). Matches the immediate subfolder name under this script's directory.
Repeatable. When omitted, all areas are discovered.

.PARAMETER Tool
Optional filter: only evaluate specs whose tool name matches (e.g. 'eventhub-get').
The tool name is the eval file name without the '.eval.yaml' suffix. Supports
wildcards (e.g. 'eventhub-*'). Repeatable. When omitted, all tools are evaluated.

.PARAMETER EvalSpec
Optional path to a single treatment eval spec to run instead of discovering them.
Its baseline (if any) is derived by replacing '.eval.yaml' with
'.eval.baseline.yaml'. Cannot be combined with -Area or -Tool.

.PARAMETER OutputDir
Parent directory where vally writes its run artifacts. Each spec's runs go to
'<area>/<tool>/treatment' and '<area>/<tool>/baseline' subdirectories. Defaults to
./.vally-results next to this script.

.PARAMETER Model
Overrides the model configured in the eval specs (defaults.model).

.PARAMETER PreEvalScript
Optional path to a provisioning script run BEFORE the evaluations of an area (e.g.
./eventhubs/New-EventHubsResources.ps1). Use it to create the Azure resources the
eval prompts reference. -ResourceGroup and -Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `New-*Resources.ps1` script in each area directory and uses that.

.PARAMETER PostEvalScript
Optional path to a teardown script run AFTER an area's evaluations (e.g.
./eventhubs/Remove-EventHubsResources.ps1). It runs in a finally block, so it
executes even if the eval or the pre-eval provisioning fails. -ResourceGroup and
-Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `Remove-*Resources.ps1` script in each area directory and uses
that.

.PARAMETER SkipProvisioning
Do not run (or auto-discover) any pre/post provisioning scripts. Use this when the
Azure resources already exist and should be left untouched.

.PARAMETER ResourceGroup
Optional resource group name forwarded to the pre/post provisioning scripts.

.PARAMETER Subscription
Optional Azure subscription (id or name) forwarded to the pre/post provisioning
scripts.

.PARAMETER SkipBuild
Skip building the server. Use this when azmcp is already built and on PATH.

.PARAMETER SkipBaseline
Only run the treatment evals; skip the baseline/control runs.

.PARAMETER Verbose
Passes --verbose to vally for full agent output.

.EXAMPLE
# Discover and run every <tool>.eval.yaml under every area subfolder,
# provisioning + tearing down each area as needed.
./Invoke-VallyEval.ps1 -Subscription <subscription-id>

.EXAMPLE
# Only the Event Hubs area
./Invoke-VallyEval.ps1 -Area eventhubs -Subscription <subscription-id>

.EXAMPLE
# Only a specific tool (wildcards allowed)
./Invoke-VallyEval.ps1 -Tool eventhub-get -Verbose

.EXAMPLE
# Resources already exist - don't provision or tear down
./Invoke-VallyEval.ps1 -SkipProvisioning

.EXAMPLE
# Run one explicit spec
./Invoke-VallyEval.ps1 -EvalSpec ./eventhubs/eventhub-get.eval.yaml -SkipProvisioning
#>

[CmdletBinding()]
param(
    [string[]] $Area,
    [string[]] $Tool,
    [string] $EvalSpec,
    [string] $OutputDir = (Join-Path $PSScriptRoot '.vally-results'),
    [string] $Model,
    [string] $PreEvalScript,
    [string] $PostEvalScript,
    [string] $ResourceGroup,
    [string] $Subscription,
    [switch] $SkipBuild,
    [switch] $SkipBaseline,
    [switch] $SkipProvisioning
)

$ErrorActionPreference = 'Stop'

# vally emits UTF-8 (box-drawing rules, check marks, etc.). If the console is on a
# legacy OEM code page (e.g. 437), those bytes are decoded wrong and show up as
# mojibake like '?öÇ'/'?£ö'. Force UTF-8 (BOM-less, so redirected/CI output stays
# clean) so vally's output is captured and displayed correctly.
$OutputEncoding = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

# Repo root is five levels up: servers/Azure.Mcp.Server/tests/Vally -> repo root
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..' '..')
$ServerProject = Join-Path $RepoRoot 'servers' 'Azure.Mcp.Server' 'src' 'Azure.Mcp.Server.csproj'

function Write-Info($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Cyan }
function Write-Warn($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Yellow }

# 1. Verify the vally CLI is available.
$vally = Get-Command 'vally' -ErrorAction SilentlyContinue
if (-not $vally) {
    throw "The 'vally' CLI was not found on PATH. Install it first: https://microsoft.github.io/vally/get-started/install/"
}
Write-Info "Using vally: $($vally.Source)"

# 2. Build the server so a current azmcp binary exists.
$exeName = $IsWindows ? 'azmcp.exe' : 'azmcp'
if (-not $SkipBuild) {
    Write-Info "Building $ServerProject ..."
    dotnet build $ServerProject -c Debug | Write-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE."
    }
}

# 3. Locate the freshly built azmcp executable and prepend it to PATH.
$serverBinDir = Join-Path (Split-Path $ServerProject -Parent) 'bin'
$azmcp = Get-ChildItem -Path $serverBinDir -Recurse -Filter $exeName -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $azmcp) {
    throw "Could not find '$exeName' under '$serverBinDir'. Build the server first (omit -SkipBuild)."
}

$azmcpDir = Split-Path $azmcp.FullName -Parent
Write-Info "Using azmcp: $($azmcp.FullName)"
$env:PATH = "$azmcpDir$([IO.Path]::PathSeparator)$env:PATH"

# 4. Run the evaluation(s).
function Invoke-VallyEvalSpec {
    param(
        [Parameter(Mandatory)] [string] $Label,
        [Parameter(Mandatory)] [string] $Spec,
        [Parameter(Mandatory)] [string] $RunOutputDir
    )

    $vallyArgs = @(
        'eval'
        '--eval-spec', $Spec
        '--output-dir', $RunOutputDir
    )
    if ($Model) { $vallyArgs += @('--model', $Model) }
    if ($VerbosePreference -ne 'SilentlyContinue') { $vallyArgs += '--verbose' }

    Write-Info "[$Label] Running: vally $($vallyArgs -join ' ')"
    # Pipe vally's output to the host so it is displayed but NOT emitted on this
    # function's output stream. Without Out-Host, vally's stdout is bundled with
    # the exit code into the caller's variable, turning it into an array; the later
    # ($exit -eq 0) checks then misread a passing run (exit 0) as FAIL.
    & vally @vallyArgs | Out-Host
    return $LASTEXITCODE
}

# Milliseconds -> "27.4s" for compact display.
function Format-Ms {
    param([int] $Ms)
    return '{0:0.0}s' -f ($Ms / 1000.0)
}

# Formats a treatment-vs-baseline delta (lower is better) as e.g. "+12343 (+28%)"
# or "-2.8s (-9%)". A negative value means the treatment used less - i.e. better.
function Format-Delta {
    param(
        [Parameter(Mandatory)] [int] $Treatment,
        [Parameter(Mandatory)] [int] $Baseline,
        [switch] $AsSeconds
    )
    $delta = $Treatment - $Baseline
    $sign = ($delta -gt 0) ? '+' : ''
    $body = $AsSeconds ? ('{0}{1:0.0}s' -f $sign, ($delta / 1000.0)) : ('{0}{1}' -f $sign, $delta)
    if ($Baseline -ne 0) {
        $pct = [math]::Round(($delta / [double] $Baseline) * 100)
        $body += ' ({0}{1}%)' -f (($pct -gt 0) ? '+' : ''), $pct
    }
    return $body
}

# Reads vally's machine-readable results.jsonl for a completed run and returns an
# ordered hashtable keyed by stimulus name, each value carrying the per-stimulus
# verdict and the efficiency metrics the comparison needs (tokens, turns, wall
# time). Returns $null when no results file is present.
function Get-VallyStimulusResults {
    param([Parameter(Mandatory)] [string] $RunOutputDir)

    if (-not (Test-Path -LiteralPath $RunOutputDir)) { return $null }

    # vally writes each run to a timestamped subdirectory; pick the newest one that
    # actually contains a results.jsonl.
    $resultsFile = Get-ChildItem -Path $RunOutputDir -Recurse -Filter 'results.jsonl' -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if (-not $resultsFile) { return $null }

    $byStimulus = [ordered]@{}
    foreach ($line in Get-Content -LiteralPath $resultsFile.FullName) {
        if (-not $line) { continue }
        try { $record = $line | ConvertFrom-Json } catch { continue }
        if ($record.type -ne 'trial-result') { continue }

        $metrics = $record.trajectory.metrics
        $byStimulus[[string] $record.stimulus] = [pscustomobject]@{
            Stimulus    = [string] $record.stimulus
            Passed      = [bool] $record.gradeResult.passed
            TotalTokens = [int] $metrics.tokenUsage.totalTokens
            TurnCount   = [int] $metrics.turnCount
            WallTimeMs  = [int] $metrics.wallTimeMs
        }
    }
    return $byStimulus
}

# Classifies a single stimulus by comparing the treatment (WITH the Azure MCP
# server) against the baseline (WITHOUT it). This encodes the effectiveness
# criteria: the server is VALUABLE when it enables an outcome the baseline could
# not achieve; it is a REGRESSION when the baseline succeeds without it but the
# treatment fails; when BOTH PASS the tool was not required for the outcome and
# efficiency (tokens/turns/wall time, lower is better) decides; INCONCLUSIVE when
# neither succeeds.
function Get-EffectivenessCategory {
    param($Treatment, $Baseline)

    if (-not $Treatment) { return 'NO DATA' }
    if (-not $Baseline) { return $Treatment.Passed ? 'PASS (no baseline)' : 'FAIL (no baseline)' }
    if ($Treatment.Passed -and -not $Baseline.Passed) { return 'VALUABLE' }
    if (-not $Treatment.Passed -and $Baseline.Passed) { return 'REGRESSION' }
    if (-not $Treatment.Passed -and -not $Baseline.Passed) { return 'INCONCLUSIVE' }
    return 'BOTH PASS'
}

# When both treatment and baseline pass, decides which is more efficient using the
# three lower-is-better metrics, returning a short human-readable judgment.
function Get-EfficiencyJudgment {
    param($Treatment, $Baseline)

    $wins = 0
    $losses = 0
    foreach ($pair in @(
            , @($Treatment.TotalTokens, $Baseline.TotalTokens)
            , @($Treatment.TurnCount, $Baseline.TurnCount)
            , @($Treatment.WallTimeMs, $Baseline.WallTimeMs))) {
        if ($pair[0] -lt $pair[1]) { $wins++ }
        elseif ($pair[0] -gt $pair[1]) { $losses++ }
    }
    if ($wins -gt 0 -and $losses -eq 0) { return "treatment more efficient (better on $wins/3 metrics)" }
    if ($losses -gt 0 -and $wins -eq 0) { return "treatment less efficient (worse on $losses/3 metrics)" }
    if ($wins -eq 0 -and $losses -eq 0) { return 'equivalent efficiency' }
    return "mixed (treatment better on $wins, worse on $losses)"
}

# Picks the console color that reflects an effectiveness category / judgment.
function Get-VerdictColor {
    param([string] $Category, [string] $Judgment)
    switch -Wildcard ($Category) {
        'VALUABLE' { return 'Green' }
        'REGRESSION' { return 'Red' }
        'INCONCLUSIVE' { return 'Red' }
        'BOTH PASS' {
            if ($Judgment -like 'treatment more efficient*' -or $Judgment -like 'equivalent*') { return 'Green' }
            return 'Yellow'
        }
        'PASS*' { return 'Green' }
        'FAIL*' { return 'Red' }
        default { return 'Gray' }
    }
}

# Invokes a pre/post provisioning script, forwarding -ResourceGroup / -Subscription
# only when the target script declares those parameters.
function Invoke-ProvisioningScript {
    param(
        [Parameter(Mandatory)] [string] $Label,
        [Parameter(Mandatory)] [string] $Path
    )

    $resolved = Resolve-Path -Path $Path -ErrorAction SilentlyContinue
    if (-not $resolved) {
        throw "$Label script not found: $Path"
    }

    $scriptArgs = @{}
    $scriptParams = (Get-Command $resolved.Path).Parameters
    if ($ResourceGroup -and $scriptParams.ContainsKey('ResourceGroup')) { $scriptArgs['ResourceGroup'] = $ResourceGroup }
    if ($Subscription -and $scriptParams.ContainsKey('Subscription')) { $scriptArgs['Subscription'] = $Subscription }

    Write-Info "[$Label] Running: $($resolved.Path)"
    $global:LASTEXITCODE = 0
    & $resolved.Path @scriptArgs
    if ($LASTEXITCODE) {
        throw "$Label script failed with exit code $LASTEXITCODE."
    }
}

# The treatment spec file name suffix. The tool name is the file name with this
# suffix removed; the baseline is the same name with the baseline suffix.
$TreatmentSuffix = '.eval.yaml'
$BaselineSuffix = '.eval.baseline.yaml'

# Resolves the pre/post provisioning scripts for an area directory: an explicit
# override (if given) wins, otherwise auto-discover by convention.
function Resolve-AreaProvisioning {
    param([Parameter(Mandatory)] [string] $AreaDir)

    $pre = $null
    $post = $null
    if ($SkipProvisioning) {
        return [pscustomobject]@{ Pre = $null; Post = $null }
    }

    if ($PreEvalScript) {
        $pre = $PreEvalScript
    }
    else {
        $auto = Get-ChildItem -Path $AreaDir -Filter 'New-*Resources.ps1' -File -ErrorAction SilentlyContinue |
            Sort-Object Name | Select-Object -First 1
        if ($auto) { $pre = $auto.FullName }
    }

    if ($PostEvalScript) {
        $post = $PostEvalScript
    }
    else {
        $auto = Get-ChildItem -Path $AreaDir -Filter 'Remove-*Resources.ps1' -File -ErrorAction SilentlyContinue |
            Sort-Object Name | Select-Object -First 1
        if ($auto) { $post = $auto.FullName }
    }

    return [pscustomobject]@{ Pre = $pre; Post = $post }
}

# --- Discover the evaluations to run -----------------------------------------
# An "eval" is a treatment spec (<tool>.eval.yaml) plus an optional baseline
# (<tool>.eval.baseline.yaml). Evals are grouped by their area (the immediate
# subdirectory of this script) so provisioning runs once per area.
if ($EvalSpec -and ($Area -or $Tool)) {
    throw '-EvalSpec cannot be combined with -Area or -Tool.'
}
if ($SkipProvisioning -and ($PreEvalScript -or $PostEvalScript)) {
    Write-Warn '-SkipProvisioning was set; ignoring -PreEvalScript/-PostEvalScript.'
    $PreEvalScript = $null
    $PostEvalScript = $null
}

$treatmentSpecs = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

if ($EvalSpec) {
    $resolvedSpec = Resolve-Path -Path $EvalSpec -ErrorAction SilentlyContinue
    if (-not $resolvedSpec) { throw "Eval spec not found: $EvalSpec" }
    $treatmentSpecs.Add((Get-Item $resolvedSpec.Path))
}
else {
    # Every *.eval.yaml (excluding *.eval.baseline.yaml) under any subfolder.
    $candidates = Get-ChildItem -Path $PSScriptRoot -Recurse -File -Filter '*.eval.yaml' -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notlike "*$BaselineSuffix" } |
        Sort-Object FullName

    foreach ($c in $candidates) {
        $areaName = Split-Path (Split-Path $c.FullName -Parent) -Leaf
        $toolName = $c.Name.Substring(0, $c.Name.Length - $TreatmentSuffix.Length)

        if ($Area -and -not ($Area | Where-Object { $areaName -like $_ })) { continue }
        if ($Tool -and -not ($Tool | Where-Object { $toolName -like $_ })) { continue }

        $treatmentSpecs.Add($c)
    }
}

if ($treatmentSpecs.Count -eq 0) {
    throw "No evaluations matched (Area='$($Area -join ',')', Tool='$($Tool -join ',')'). Expected files named '<tool>$TreatmentSuffix'."
}

# Build the ordered list of eval descriptors and group them by area directory.
$evals = foreach ($spec in $treatmentSpecs) {
    $areaDir = Split-Path $spec.FullName -Parent
    $areaName = Split-Path $areaDir -Leaf
    $toolName = $spec.Name.Substring(0, $spec.Name.Length - $TreatmentSuffix.Length)
    $baselinePath = Join-Path $areaDir ($toolName + $BaselineSuffix)

    [pscustomobject]@{
        Area = $areaName
        AreaDir = $areaDir
        Tool = $toolName
        Treatment = $spec.FullName
        Baseline = (Test-Path $baselinePath) ? $baselinePath : $null
    }
}

Write-Info ("Discovered {0} evaluation(s): {1}" -f $evals.Count, (($evals | ForEach-Object { "$($_.Area)/$($_.Tool)" }) -join ', '))

# --- Run the evaluations, one area at a time ---------------------------------
$results = [System.Collections.Generic.List[object]]::new()

foreach ($areaGroup in ($evals | Group-Object AreaDir)) {
    $areaDir = $areaGroup.Name
    $areaName = Split-Path $areaDir -Leaf
    $provisioning = Resolve-AreaProvisioning -AreaDir $areaDir
    $provisioned = $true

    try {
        # Provision once for the whole area. A provisioning failure fails the run:
        # record it, skip this area's evals (the resources aren't there), but still
        # run teardown below to clean up anything partially created.
        if ($provisioning.Pre) {
            try {
                Invoke-ProvisioningScript -Label "[$areaName] pre-eval provisioning" -Path $provisioning.Pre
            }
            catch {
                $provisioned = $false
                Write-Warn "[$areaName] pre-eval provisioning FAILED: $($_.Exception.Message)"
                Write-Warn "[$areaName] Skipping this area's evaluations."
                foreach ($eval in $areaGroup.Group) {
                    $results.Add([pscustomobject]@{
                        Name = "$($eval.Area)/$($eval.Tool)"
                        ProvisioningFailed = $true
                        TreatmentExit = $null
                        BaselineExit = $null
                        HasBaseline = [bool]$eval.Baseline
                    })
                }
            }
        }
        else {
            Write-Info "[$areaName] No pre-eval provisioning script (resources assumed to already exist)."
        }

        if ($provisioned) {
            foreach ($eval in $areaGroup.Group) {
                $label = "$($eval.Area)/$($eval.Tool)"
                $runRoot = Join-Path (Join-Path $OutputDir $eval.Area) $eval.Tool
                $treatmentDir = Join-Path $runRoot 'treatment'
                $baselineDir = Join-Path $runRoot 'baseline'

                $treatmentExit = Invoke-VallyEvalSpec `
                    -Label "$label treatment (with Azure MCP)" `
                    -Spec $eval.Treatment `
                    -RunOutputDir $treatmentDir

                $baselineExit = $null
                if (-not $SkipBaseline -and $eval.Baseline) {
                    # The baseline is a control that is EXPECTED to fail; keep going.
                    $baselineExit = Invoke-VallyEvalSpec `
                        -Label "$label baseline (no Azure MCP)" `
                        -Spec $eval.Baseline `
                        -RunOutputDir $baselineDir
                }

                $results.Add([pscustomobject]@{
                    Name = $label
                    ProvisioningFailed = $false
                    TreatmentExit = $treatmentExit
                    BaselineExit = $baselineExit
                    HasBaseline = [bool]$eval.Baseline
                    TreatmentDir = $treatmentDir
                    BaselineDir = ($SkipBaseline -or -not $eval.Baseline) ? $null : $baselineDir
                })
            }
        }
    }
    finally {
        # Tear down the area even if provisioning or an eval threw.
        if ($provisioning.Post) {
            try {
                Invoke-ProvisioningScript -Label "[$areaName] post-eval teardown" -Path $provisioning.Post
            }
            catch {
                Write-Warn "[$areaName] post-eval teardown failed: $($_.Exception.Message)"
                Write-Warn 'Resources may still exist. The DeleteAfter tag ensures they are cleaned up later.'
            }
        }
    }
}

# --- Report the comparison ---------------------------------------------------
# The goal is to measure the Azure MCP server's effectiveness, per stimulus, by
# comparing treatment (WITH the server) against baseline (WITHOUT it):
#
#   * baseline FAIL + treatment PASS -> VALUABLE      (the server enabled the outcome)
#   * baseline PASS + treatment FAIL -> REGRESSION    (the server hurt the outcome)
#   * both PASS                      -> the tool was not required; efficiency decides,
#                                        using tokens / turns / wall time (lower is better)
#   * neither PASS                   -> INCONCLUSIVE
#
# A run is only considered failed (non-zero exit) when the treatment itself failed
# to execute, or an effectiveness REGRESSION is detected. Baseline outcome failures
# are expected and do not fail the run.
Write-Host ''
Write-Info '======================= Results ======================='
$anyFailure = $false
foreach ($r in $results) {
    if ($r.ProvisioningFailed) {
        Write-Warn ("{0,-32} PROVISIONING FAILED (evaluations skipped)" -f $r.Name)
        $anyFailure = $true
        continue
    }

    $treatmentVerdict = ($r.TreatmentExit -eq 0) ? 'PASS' : 'FAIL'
    if ($r.TreatmentExit -ne 0) { $anyFailure = $true }

    # Top-line run status (whether the whole spec executed and passed its threshold).
    if (-not $r.HasBaseline) {
        Write-Info ("{0,-32} treatment {1} (no baseline)" -f $r.Name, $treatmentVerdict)
    }
    elseif ($null -eq $r.BaselineExit) {
        Write-Info ("{0,-32} treatment {1} (baseline skipped)" -f $r.Name, $treatmentVerdict)
    }
    else {
        $baselineVerdict = ($r.BaselineExit -eq 0) ? 'PASS' : 'FAIL'
        Write-Info ("{0,-32} treatment {1} | baseline {2}" -f $r.Name, $treatmentVerdict, $baselineVerdict)
    }

    # Per-stimulus effectiveness comparison from vally's machine-readable results.
    $treatmentStimuli = Get-VallyStimulusResults -RunOutputDir $r.TreatmentDir
    $baselineStimuli = $r.BaselineDir ? (Get-VallyStimulusResults -RunOutputDir $r.BaselineDir) : $null
    if (-not $treatmentStimuli) {
        Write-Warn '  (no per-stimulus results.jsonl found - skipping effectiveness comparison)'
        continue
    }

    foreach ($stimulus in $treatmentStimuli.Keys) {
        $t = $treatmentStimuli[$stimulus]
        $b = $baselineStimuli ? $baselineStimuli[$stimulus] : $null
        $category = Get-EffectivenessCategory -Treatment $t -Baseline $b

        $detail = ''
        if ($category -eq 'BOTH PASS') {
            $judgment = Get-EfficiencyJudgment -Treatment $t -Baseline $b
            $detail = " - $judgment; " +
                ("tokens {0} vs {1} ({2}), turns {3} vs {4} ({5}), wall {6} vs {7} ({8})" -f `
                    $t.TotalTokens, $b.TotalTokens, (Format-Delta -Treatment $t.TotalTokens -Baseline $b.TotalTokens),
                    $t.TurnCount, $b.TurnCount, (Format-Delta -Treatment $t.TurnCount -Baseline $b.TurnCount),
                    (Format-Ms $t.WallTimeMs), (Format-Ms $b.WallTimeMs), (Format-Delta -Treatment $t.WallTimeMs -Baseline $b.WallTimeMs -AsSeconds))
            # Both passing means the outcome did not require the server - surface it,
            # but it does not fail the run.
            if ($judgment -like 'treatment less efficient*' -or $judgment -like 'mixed*') {
                $detail += ' [review: server did not improve efficiency]'
            }
        }
        elseif ($category -eq 'VALUABLE') {
            $detail = ' - the Azure MCP server enabled an outcome the baseline could not achieve'
        }
        elseif ($category -eq 'REGRESSION') {
            $detail = ' - baseline succeeded WITHOUT the server but the treatment FAILED'
            $anyFailure = $true
        }
        elseif ($category -eq 'INCONCLUSIVE') {
            $detail = ' - neither treatment nor baseline achieved the outcome'
        }

        $color = Get-VerdictColor -Category $category -Judgment $judgment
        Write-Host ("  - {0,-30} {1}{2}" -f $stimulus, $category, $detail) -ForegroundColor $color
        # Reset $judgment so it does not leak into the next stimulus' coloring.
        $judgment = $null
    }
}
Write-Info "Artifacts under: $OutputDir"

# Non-zero if any treatment eval failed, any area's provisioning failed, or an
# effectiveness regression (baseline PASS while treatment FAIL) was detected;
# baseline outcome failures on their own are expected and do not affect the exit code.
exit ($anyFailure ? 1 : 0)
