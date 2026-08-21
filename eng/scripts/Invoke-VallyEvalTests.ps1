#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Runs Vally evaluations against Azure MCP tool command specs.

.DESCRIPTION
    Collects generated eval.yaml specification files from the Vally evals directory
    (`<repo-root>/.work/vally/evals` by default), then invokes the `vally eval`
    command against all discovered specifications. Only these generated specs are
    used by default: their graders are crafted by VallyEvaluator to always be
    satisfiable by a correct agent. The hand-authored `eval.yaml` files checked
    into each toolset's `tests/` directory (referenced via `build_info.json`
    `pathsToTest`) are excluded by default because their graders are not
    guaranteed to be satisfiable (e.g. they may require live Azure resources or
    subscriptions that do not exist in a local/sandboxed run). Pass
    -IncludeTestResourceEvals to include them explicitly.

    The script publishes a plain (non-packaged) Azure MCP Server build to
    `.work/build` before calling Vally, skipping the npm packaging step used by
    Build-Local.ps1 since Vally launches the published executable directly. Use
    -SkipBuild when reusing an existing build.

.PARAMETER WorkDirectory
    The working directory passed to `vally`. Defaults to the repository root.

.PARAMETER EvalsDirectory
    Directory containing additional eval.yaml files to include. Defaults to
    `<repo-root>/.work/vally/evals`.

.PARAMETER IncludeTestResourceEvals
    Also include the hand-authored eval.yaml files referenced by `build_info.json`
    `pathsToTest` (e.g. `tools/Azure.Mcp.Tools.AppConfig/tests/eval.yaml`). These
    are excluded by default because, unlike the generated evals under
    `.work/vally/evals`, their graders are not crafted to always be satisfiable
    (e.g. they may require live Azure resources or subscriptions), so they can
    fail for reasons unrelated to server behavior.

.PARAMETER BuildInfoPath
    Path to the build_info.json file produced by New-BuildInfo.ps1. Only used
    when -IncludeTestResourceEvals is specified. Defaults to
    `<repo-root>/.work/build_info.json`.

.PARAMETER SkipBuild
    Skip rebuilding Azure MCP Server and use the existing binaries and build info.

.PARAMETER OutputPath
    Optional path for Vally output. By default, results are written to a
    mode-specific directory under `<repo-root>/.work/vally`:
    `vally-results-default`, `vally-results-two-step`, or
    `vally-results-three-step`.

.PARAMETER NumberOfRuns
    The number of times to run each eval spec. Defaults to 1.

.PARAMETER EvalPathFilter
    One or more wildcard patterns applied to each discovered eval.yaml path. When
    specified, only matching eval specifications are run.

.PARAMETER ToolDiscoveryMode
    Selects the Azure MCP Server discovery mode used for both the `.vally.yaml`
    environment and the results output directory, so each mode's results are kept
    separate and never overwrite one another. `Default` starts the server with no
    discovery-mode flags (automatic size-based three-step fallback enabled, the
    out-of-the-box behavior). `TwoStep` explicitly passes
    `--disable-automatic-three-step-tool-discovery` to force legacy two-step
    behavior. `ThreeStep` starts the server with `--three-step-tool-discovery`.
    Defaults to `Default`.

.PARAMETER IsDebug
    When specified, adds `--verbose` to the `vally eval` invocation for
    additional diagnostic output.

.PARAMETER SkipAgentsInstructions
    When specified, skips overwriting `AGENTS.md` in the work directory with the
    Vally evaluation instructions from `eng/tools/VallyEvaluator/src/Resources/eval.instructions.md`.
    By default this script temporarily replaces `AGENTS.md` (restoring it afterward)
    so local runs match the behavior of the `vally-eval.yml` CI workflow, which
    performs the same substitution. Without these instructions, the evaluated agent
    does not know to treat placeholders as synthetic test data or skip
    `subscription_list`/`az login`, which commonly causes spurious local failures
    that do not occur in CI.

.PARAMETER Warmup
    Before the measured run, execute the same eval specification(s) once against
    a throwaway output directory (discarded afterward) to prime the model
    provider's prompt cache (e.g. Anthropic prompt caching) for the system
    prompt/tool schema prefix used by this mode's environment. Without this, the
    first mode measured in a comparison run starts with a cold cache while later
    modes may partially benefit from residual cache state, making AI-credit
    comparisons across modes noisy rather than reflecting genuine discovery-mode
    behavior differences. The warmup run's results are not included in the
    measured output and its failures (if any) are reported as warnings, not
    fatal errors.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1

    Runs Vally using default paths derived from the repository root.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -BuildInfoPath '.work/custom_build_info.json' -IsDebug

    Runs Vally with a custom build info file and verbose output enabled.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -SkipBuild

    Runs Vally using the existing Azure MCP Server build.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -ToolDiscoveryMode ThreeStep

    Runs Vally against the three-step Azure MCP Server discovery environment,
    writing results to a directory separate from the Default and TwoStep runs.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -ToolDiscoveryMode TwoStep

    Runs Vally against the legacy two-step Azure MCP Server discovery
    environment, writing results to a directory separate from the Default and
    ThreeStep runs.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -EvalPathFilter '*AppConfig*', '*Storage*'

    Runs only eval specifications whose paths match either wildcard pattern.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -IncludeTestResourceEvals

    Runs the generated evals plus the hand-authored eval.yaml files checked into
    each toolset's tests/ directory.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -ToolDiscoveryMode TwoStep -Warmup

    Runs a throwaway warmup pass against the two-step environment to prime the
    model provider's prompt cache, then runs the measured pass.
#>

param(
    [string]$WorkDirectory,
    [string]$BuildInfoPath,
    [string]$EvalsDirectory,
    [string]$OutputPath,
    [int]$NumberOfRuns = 1,
    [string[]]$EvalPathFilter,
    [ValidateSet("Default", "TwoStep", "ThreeStep")]
    [string]$ToolDiscoveryMode = "Default",
    [switch]$SkipBuild,
    [switch]$IsDebug,
    [switch]$SkipAgentsInstructions,
    [switch]$IncludeTestResourceEvals,
    [switch]$Warmup
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if (!$WorkDirectory) {
    $WorkDirectory = $RepoRoot
}

$workArtifactsDirectory = Join-Path $WorkDirectory ".work"
$vallyArtifactsDirectory = Join-Path $workArtifactsDirectory "vally"

if (!$EvalsDirectory) {
    $EvalsDirectory = Join-Path $vallyArtifactsDirectory "evals"
}

if (!$OutputPath) {
    $outputDirectoryName = switch ($ToolDiscoveryMode) {
        "ThreeStep" { "vally-results-three-step" }
        "TwoStep" { "vally-results-two-step" }
        default { "vally-results-default" }
    }
    $OutputPath = Join-Path $vallyArtifactsDirectory $outputDirectoryName
}

if ($SkipBuild) {
    Write-Host "Skipping Azure MCP Server build."
} else {
    Write-Host "Building Azure MCP Server..."
    # The Vally environments launch the plain published executable directly
    # (`.work/build/Azure.Mcp.Server/<os>-x64/azmcp`), not an npm-packaged
    # artifact. Call New-BuildInfo.ps1 + Build-Code.ps1 directly instead of
    # Build-Local.ps1 so we skip the npm packaging/tarball step (Pack-Npm.ps1),
    # which is unnecessary overhead for Vally and was the source of most of the
    # verbose build output.
    & "$PSScriptRoot/New-BuildInfo.ps1" -ServerName Azure.Mcp.Server -PublishTarget none -BuildId 12345
    if ($LASTEXITCODE -ne 0) {
        Write-Error "New-BuildInfo.ps1 failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }

    & "$PSScriptRoot/Build-Code.ps1" -ServerName Azure.Mcp.Server -SelfContained -Trimmed -Architectures x64
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Azure MCP Server build failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# build_info.json is only needed to locate the hand-authored test resource evals,
# which are opt-in via -IncludeTestResourceEvals.
if ($IncludeTestResourceEvals) {
    if (!$BuildInfoPath) {
        $BuildInfoPath = Join-Path $workArtifactsDirectory "build_info.json"
    }

    if (!(Test-Path $BuildInfoPath)) {
        Write-Error "Build info file not found at $BuildInfoPath. Please run New-BuildInfo.ps1 first."
        exit 1
    }
}

$environment = "";
if ($IsWindows) {
    $environment = "windows"
} elseif ($IsLinux) {
    $environment = "linux"
} else {
    Write-Error "Unsupported platform. This script only supports Windows, Linux, and macOS."
    exit 1
}

if ($ToolDiscoveryMode -eq "ThreeStep") {
    $environment += "-three-step"
} elseif ($ToolDiscoveryMode -eq "TwoStep") {
    $environment += "-two-step"
} else {
    $environment += "-default"
}

$evalPaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$commandArg = ""

if ($IncludeTestResourceEvals) {
    $buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

    foreach ($path in $buildInfo.pathsToTest) {
        if ([string]::IsNullOrEmpty($path.testResourcesPath)) {
            continue
        }

        $evalPath = Join-Path $RepoRoot $path.testResourcesPath "eval.yaml"
        if (Test-Path $evalPath) {
            [void]$evalPaths.Add((Resolve-Path -LiteralPath $evalPath).Path)
        }
    }
}

if (Test-Path $EvalsDirectory) {
    Write-Host "Getting eval paths from VallyEvaluator"
    Get-ChildItem -Path $EvalsDirectory -Filter "eval.yaml" -Recurse | ForEach-Object {
        [void]$evalPaths.Add($_.FullName)
    }
}

$results = @($evalPaths | Sort-Object)
if ($EvalPathFilter.Count -gt 0) {
    $results = @(
        $results | Where-Object {
            $evalPath = $_
            $EvalPathFilter | Where-Object { $evalPath -like $_ }
        })
}

if ($results.Count -eq 0) {
    $filterDescription = if ($EvalPathFilter.Count -gt 0) {
        " matching filters: $($EvalPathFilter -join ', ')"
    } else {
        ""
    }
    $generateCommand = "dotnet run --project `"$RepoRoot/eng/tools/VallyEvaluator/src/VallyEvaluator.csproj`" -- --serverName Azure.Mcp.Server"
    Write-Error "No eval.yaml files were found$filterDescription. Generate them by running:`n  $generateCommand"
    exit 1
}

Write-Host "Running $($results.Count) eval specification(s)."
$results | ForEach-Object { $commandArg += "--eval-spec '$($_)' " }

if ([string]::IsNullOrEmpty($commandArg)) {
    Write-Host "No eval.yaml files found to execute vally from."
    exit 0
}

$expression = "vally eval --work-dir '$WorkDirectory' --output-dir '$OutputPath' --runs $NumberOfRuns --param ENVIRONMENT=$environment"

if ($IsDebug) {
    $expression += " --verbose"
}

$expression += " $commandArg"

# The evaluated agent needs the Vally-specific behavioral instructions (synthetic
# subscription id, treat placeholders as supplied test values, never call
# subscription_list/az login, etc.) or it will legitimately ask for clarification
# on placeholder prompts and fail tool-call graders. The vally-eval.yml CI
# workflow achieves this by force-copying eval.instructions.md over AGENTS.md
# before running; replicate that here so local runs match CI behavior, and
# restore the original AGENTS.md (or remove it if none existed) afterward.
$agentsPath = Join-Path $WorkDirectory "AGENTS.md"
$instructionsPath = Join-Path $RepoRoot "eng/tools/VallyEvaluator/src/Resources/eval.instructions.md"
$agentsBackupPath = $null

if (!$SkipAgentsInstructions) {
    if (!(Test-Path $instructionsPath)) {
        Write-Error "Vally eval instructions file not found at $instructionsPath."
        exit 1
    }

    if (Test-Path $agentsPath) {
        $agentsBackupPath = Join-Path ([System.IO.Path]::GetTempPath()) "AGENTS.md.vally-backup-$([Guid]::NewGuid())"
        Copy-Item -LiteralPath $agentsPath -Destination $agentsBackupPath -Force
    }

    Write-Host "Temporarily replacing $agentsPath with Vally evaluation instructions."
    Copy-Item -LiteralPath $instructionsPath -Destination $agentsPath -Force
}

try {
    if ($Warmup) {
        # Prime the model provider's prompt cache (e.g. Anthropic prompt caching) for
        # this mode's system prompt/tool schema prefix before the measured run, so
        # AI-credit comparisons across modes aren't skewed by which mode happened to
        # run first (and start with a fully cold cache) versus later modes that may
        # benefit from residual cache state. Results are written to a throwaway
        # directory and discarded; a warmup failure is reported as a warning, not
        # fatal, since it doesn't affect the measured run's correctness.
        $warmupOutputPath = Join-Path ([System.IO.Path]::GetTempPath()) "vally-warmup-$([Guid]::NewGuid())"
        New-Item -ItemType Directory -Path $warmupOutputPath | Out-Null
        try {
            $warmupExpression = "vally eval --work-dir '$WorkDirectory' --output-dir '$warmupOutputPath' --runs 1 --param ENVIRONMENT=$environment"
            if ($IsDebug) {
                $warmupExpression += " --verbose"
            }
            $warmupExpression += " $commandArg"

            Write-Host "Running warmup command: $warmupExpression"
            Invoke-Expression $warmupExpression
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Warmup run exited with code $LASTEXITCODE. Continuing with the measured run anyway."
            }
        }
        catch {
            Write-Warning "Warmup run failed: $_. Continuing with the measured run anyway."
        }
        finally {
            if (Test-Path $warmupOutputPath) {
                Remove-Item -Recurse -Force $warmupOutputPath
            }
        }
    }

    Write-Host "Running command: $expression"
    Invoke-Expression $expression
}
finally {
    if (!$SkipAgentsInstructions) {
        if ($agentsBackupPath) {
            Move-Item -LiteralPath $agentsBackupPath -Destination $agentsPath -Force
        }
        elseif (Test-Path $agentsPath) {
            Remove-Item -LiteralPath $agentsPath -Force
        }
    }
}
