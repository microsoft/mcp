# Vally evaluations for the Azure MCP Server

This directory holds [vally](https://microsoft.github.io/vally) evaluations that measure how
*effectively* an agent uses individual Azure MCP tools. Unlike the end-to-end
tests in `CopilotCliTester`, vally evals are declarative eval specs graded by
vally's built-in, reference-free graders (no gold-standard answer required).

The runner (`Invoke-VallyEval.ps1`) **discovers** every evaluation under this
directory, so adding a new tool evaluation is just a matter of dropping files in
the right place with the right names - no script changes needed.

## Layout and naming convention

Evaluations are organized by **area** (a subfolder, usually an Azure MCP
namespace) and, within each area, by **tool**:

```
tests/Vally/
|-- Invoke-VallyEval.ps1              # Discovers, builds azmcp, provisions, runs, tears down
|-- eventhubs/                        # An AREA (one subfolder per namespace)
|   |-- eventhub-get.experiment.yaml      # <tool>.experiment.yaml - defines baseline + server-mode variants (required)
|   |-- eventhub-get.eval.yaml            # <tool>.eval.yaml        - the shared eval spec every variant runs (required)
|   |-- namespace-get.experiment.yaml     # another tool in the same area
|   |-- namespace-get.eval.yaml           #   (each tool is an independent experiment)
|   |-- New-EventHubsResources.ps1        # New-*Resources.ps1    - per-area provisioning (optional)
|   |-- Remove-EventHubsResources.ps1     # Remove-*Resources.ps1 - per-area teardown     (optional)
|-- README.md                         # This file
```

The **tool name** is the experiment file name with the `.experiment.yaml` suffix
removed (e.g. `eventhub-get`). Each area can hold many tools (e.g. `eventhub-get`,
`namespace-get`, etc.), each an independent experiment. Provisioning scripts are
discovered **per area** and run once for all of that area's tools.

The first area, `eventhubs/`, evaluates every Event Hubs tool
(`eventhubs_namespace_get`/`_update`/`_delete`, `eventhubs_eventhub_get`/`_update`/`_delete`,
and `eventhubs_eventhub_consumergroup_get`/`_update`/`_delete`) - one
`<tool>.experiment.yaml` per tool. Each experiment runs its shared
`<tool>.eval.yaml` as three variants that differ only in whether (and how)
the Azure MCP server is present:

- **namespace**: the agent has the Azure MCP Event Hubs tools, with the server
  in its default `namespace` proxy mode (one tool per namespace).
- **consolidated**: the same tools, but with the server in `consolidated` proxy
  mode (related operations grouped into consolidated tools).
- **baseline** (control): the *same* prompts and graders, but the experiment
  deletes `environment.mcpServers.azure`, so **no** Azure MCP server is connected.

`baseline` is the control; `namespace` and `consolidated` are the server
**candidates**. Each candidate is identical to the baseline except for the
presence (and mode) of the Azure MCP server, so the delta between a candidate and
the baseline isolates the server's contribution in that mode - see
[Server variants vs. baseline](#server-variants-vs-baseline-control) below.

## What the Event Hubs experiment checks

All three variants send the same natural-language prompts (e.g. *"List all of the
Event Hubs in my namespace..."*) and are graded identically by the same
**outcome** grader - an [LLM `prompt` judge](https://microsoft.github.io/vally/reference/graders/)
that checks the agent **actually returned** the requested Event Hubs (real data
from Azure, not a refusal, a deferral, or fabricated values).

Grading is intentionally **outcome-only**: tool-selection assertions (e.g. a
`tool-calls` grader asserting the agent invoked `eventhubs_eventhub_get` or
`eventhubs_namespace_get`) are *not* used, because the baseline variant has no MCP
tool to call, so such a grader could never be satisfied consistently across
variants. Judging every variant on the same outcome question is exactly what makes
the comparison meaningful.

Sign in with `az login` (and provision the resources below) so the agent can
return real Event Hubs data - the shared outcome grader needs it.

## Server variants vs. baseline (control)

Each experiment (e.g. `eventhub-get.experiment.yaml` or `namespace-get.experiment.yaml`)
runs its shared eval spec (e.g. `eventhub-get.eval.yaml` or `namespace-get.eval.yaml`)
as three variants that differ only in whether (and how) the Azure MCP server is present:

- **namespace**: inherits the eval spec as-is, WITH the Azure MCP server in its
  default `namespace` proxy mode.
- **consolidated**: WITH the Azure MCP server, but the variant overrides `--mode`
  to `consolidated` so related operations are grouped into consolidated tools.
- **baseline** (control): the experiment deletes `environment.mcpServers.azure`,
  so the agent runs with only its built-in tools.

Because every variant comes from the *same* eval spec, they share the exact same
prompts and the same outcome grader by construction, so their verdicts are
directly comparable. `namespace` and `consolidated` are the server **candidates**;
each is compared against the shared `baseline`.

| Variant | Azure MCP server | Grading | Typical verdict |
|:--------|:-----------------|:--------|:----------------|
| namespace | yes (`--mode namespace`) | `prompt` (shared outcome) | **PASS** |
| consolidated | yes (`--mode consolidated`) | `prompt` (shared outcome) | **PASS** |
| baseline (control) | no (deleted) | `prompt` (shared outcome) | **FAIL** (often) |

The baseline is the *control*: without the Azure MCP server the agent has no
Event Hubs tool, so the expectation is that it cannot retrieve the data and the
shared outcome grader scores 0. **But the baseline is not guaranteed to fail** -
the agent still has its general-purpose tools, so it may shell out to the Azure
CLI (`az eventhubs ...`) via its built-in `powershell` tool and satisfy the
outcome grader anyway (see [Interpreting effectiveness](#interpreting-effectiveness)
below).

Because every variant grades *outcome*, the agent needs real data to have any
chance of passing; run `az login` (and provision the resources below) first so the
comparison is honest - the candidates can return real Event Hubs while the baseline
still cannot.

## Prerequisites

1. **Vally CLI** - install per the
   [vally install guide](https://microsoft.github.io/vally/get-started/install/)
   and make sure `vally` is on your `PATH`.
2. **.NET SDK** - required to build the `azmcp` server (see the repo root
   `global.json` for the pinned version).
3. **Azure sign-in** - `az login`. Required to provision real Event Hubs (see
   below) and for the server candidates to return real data; the outcome-graded
   baseline needs real data to be a fair test.
4. **Azure CLI** - `az` on your `PATH`, only needed if you use the provisioning
   scripts.

## Provisioning test resources

The eval prompts reference concrete resources (resource group `contoso-rg`,
namespace `contoso-ehns`, event hub `orders`). Rather than hardcoding those
names, the eval specs reference them as
[Vally parameters](https://microsoft.github.io/vally/reference/cli/eval/#parameter-resolution) -
e.g. `${RESOURCE_GROUP=contoso-rg}` - so a prompt reads `resource group
"${RESOURCE_GROUP=contoso-rg}"`. Without any override, the `=contoso-rg`
inline default applies and the eval behaves exactly as if the name were
hardcoded. Two scripts create and remove the underlying resources:

- **`eventhubs/New-EventHubsResources.ps1`** - deploys
  `eventhubs/eventhubs-resources.bicep` (a subscription-scoped Bicep template,
  via `az deployment sub create`) to create the resource group, the Event Hubs
  namespace, and the event hubs (including `orders`). It stamps a
  **`DeleteAfter`** tag (an ISO 8601 UTC timestamp, matching the repo's
  `TestResources` convention) on the resource group so the standard Azure
  clean-up job reclaims it **even if teardown never runs**. As its last step it
  emits a `PSCustomObject` reporting the identifiers it actually provisioned
  (`RESOURCE_GROUP`, `EVENTHUBS_NAMESPACE`, etc.).
- **`eventhubs/Remove-EventHubsResources.ps1`** - deletes the resource group and
  everything in it.

The runner runs these **automatically**: for each area it evaluates, it
auto-discovers a `New-*Resources.ps1` (pre-eval) and a `Remove-*Resources.ps1`
(post-eval) in that area folder and runs them once around all of the area's tools.
The teardown runs in a `finally` block, so it executes even if provisioning or an
eval fails. If provisioning fails, that area's evaluations are **skipped** and the
run **fails** (non-zero exit), but teardown still runs to clean up any partially
created resources. Pass `-SkipProvisioning` to leave existing resources untouched,
or `-PreEvalScript` / `-PostEvalScript` to point at specific scripts. You can also
run the scripts directly:

```powershell
./eventhubs/New-EventHubsResources.ps1 -Subscription <subscription-id>
# ... run evals ...
./eventhubs/Remove-EventHubsResources.ps1 -Subscription <subscription-id>
```

### How provisioned resource names reach the eval prompts

`New-*Resources.ps1` is free to pick its own resource names at run time -
including randomizing a suffix to avoid colliding with a concurrent run -
rather than always using its own fixed defaults. It reports back whatever it
actually created by emitting a `Hashtable`/`PSCustomObject` of `KEY = value`
pairs (e.g. `[pscustomobject]@{ RESOURCE_GROUP = $rg; EVENTHUBS_NAMESPACE =
$ns }`) as the **last** object on its output stream (`Write-Host`/`Write-Info`
logging doesn't interfere - it never touches that stream). `Invoke-VallyEval.ps1`
captures that object and forwards each entry to `vally experiment run` as
`--param KEY=value`, which resolves the matching `${KEY}`/`${KEY=default}`
placeholder in the eval prompts - so the eval always targets what was truly
provisioned, never a name this runner has to guess. The reported
`RESOURCE_GROUP` also flows to the post-eval teardown script (taking
precedence over `-ResourceGroup`'s own default), so teardown targets the same
resource group that was actually provisioned.

If you skip provisioning (`-SkipProvisioning`) or run an eval spec standalone
via `vally experiment run`, no params are forwarded and each placeholder's own
inline `=default` applies - override it yourself with `vally`'s own `--param`
flag if your pre-existing resources use different names.

## Running

From this directory:

```powershell
./Invoke-VallyEval.ps1 -Subscription <subscription-id>
```

The script builds `Azure.Mcp.Server`, prepends the freshly built `azmcp` to
`PATH` (vally does not expand environment variables inside eval specs),
**discovers every `<tool>.experiment.yaml` under every area subfolder**, and for
each one provisions the area, runs the experiment (the baseline plus every server
candidate variant), and tears the area down. It prints a per-tool comparison. Its
exit code is non-zero if any *candidate* variant fails (baseline failures are
expected). Useful switches:

```powershell
# Skip the build if azmcp is already built and on PATH
./Invoke-VallyEval.ps1 -SkipBuild

# Only a single area, or a single tool (wildcards allowed)
./Invoke-VallyEval.ps1 -Area eventhubs
./Invoke-VallyEval.ps1 -Tool eventhub-get

# Resources already exist - don't provision or tear down
./Invoke-VallyEval.ps1 -SkipProvisioning

# Run each experiment multiple times (default 1) and report every iteration's outcome
./Invoke-VallyEval.ps1 -Iterations 3

# Re-print the summary from a previous run's saved artifacts - no build, no Azure, no vally
# (auto-discovers the newest run directory under --output-dir)
./Invoke-VallyEval.ps1 -ReportOnly
# ...for a single tool, showing the newest 3 iterations within that run as iterations
./Invoke-VallyEval.ps1 -ReportOnly -Tool eventhub-get -Iterations 3
# ...from a specific, older run directory instead of the newest one
./Invoke-VallyEval.ps1 -ReportFrom ./.vally-results/2026-07-29T18-19-30-496Z

# Run one explicit experiment (its baseline variant is defined in the experiment spec)
./Invoke-VallyEval.ps1 -ExperimentSpec ./eventhubs/eventhub-get.experiment.yaml
```

## Results

Every invocation of `Invoke-VallyEval.ps1` writes ALL of its output under one
fresh, timestamped **run directory**: `<output-dir>/<run-timestamp>/<area>/<tool>/...`
(default `--output-dir` is `./.vally-results`). This groups everything a single
invocation produces - every area/tool it discovered, and every `-Iterations`
repeat of each - together and unambiguous, rather than scattering results
across independent per-tool timestamps with no shared "this is one run"
grouping. Under `<run-timestamp>/<area>/<tool>/`, `vally experiment run` then
creates one further timestamped subfolder per iteration, with one subfolder
per variant (`baseline`, `namespace`, and `consolidated`), each containing:

- `results.jsonl` - one JSON record per stimulus (a `trial-result`, plus a final
  `run-summary`). Each `trial-result` carries the verdict (`gradeResult.passed`)
  and the efficiency metrics (`trajectory.metrics.tokenUsage.totalTokens`,
  `trajectory.metrics.turnCount`, `trajectory.metrics.wallTimeMs`), and
- `eval-results.md` - a human-readable Markdown summary.

Running an experiment by hand still targets one tool's directory directly (no
run-timestamp level, since you're not going through the wrapper script):

```powershell
# Runs the shared eval spec as every variant (baseline + namespace + consolidated)
# and writes each under a timestamped subfolder of --output-dir.
vally experiment run ./eventhubs/eventhub-get.experiment.yaml --output-dir ./.vally-results/eventhubs/eventhub-get
```

### `session-state/` scratch folder

`vally` has no `--workdir`/`--cwd` option, so `vally experiment run` (and every
per-trial agent sandbox it spawns via the `copilot-sdk` executor) simply
inherits whatever directory it's launched from. That executor spills large
tool outputs (>~20KB) to a `session-state/temp/` folder resolved relative to
that inherited directory - it is **not** scoped per-experiment and is
unrelated to `-OutputDir`/`.vally-results/`. `Invoke-VallyEval.ps1` pins this
by changing into its own script directory before invoking `vally`, so
`session-state/` always lands next to `.vally-results/` here rather than
wherever the caller's shell happened to be (e.g. the repo root) - it is
git-ignored (`session-state/` in the repo root `.gitignore`) and safe to
delete at any time between runs.

### Interpreting effectiveness

The point of the candidate-vs-baseline pairing is to measure the Azure MCP
server's *effectiveness*. After the runs complete, the script reads the newest
`results.jsonl` from the baseline and from each candidate (`namespace`,
`consolidated`) and compares each candidate against the baseline **per stimulus**:

| Baseline (no server) | Candidate (with server) | Category | Meaning |
|:---------------------|:------------------------|:---------|:--------|
| FAIL | PASS | **VALUABLE** | The server enabled an outcome the agent could not achieve without it. |
| PASS | FAIL | **REGRESSION** | The agent succeeded *without* the server but failed *with* it - the server hurt the outcome. |
| PASS | PASS | **BOTH PASS** | The server was not required for the outcome. Efficiency decides: lower **tokens**, **turns**, and **wall time** are better; the script prints each metric and the candidate-vs-baseline delta. |
| FAIL | FAIL | **INCONCLUSIVE** | Neither achieved the outcome. |

A **BOTH PASS** result is common and expected: the baseline agent can shell out to
the Azure CLI via its built-in `powershell` tool, so it often retrieves the data
without the MCP server. That is not a problem in itself - it just means this
stimulus does not *require* the server, and the comparison falls through to
efficiency (fewer tokens/turns/less wall-clock time is better).

The process exit code is `0` when every *candidate* eval verdict passes, and `1`
if any candidate eval fails, an effectiveness **REGRESSION** is detected, **or**
any area's provisioning fails. Baseline outcome failures on their own are expected
and do not affect it, so the script is CI-friendly.

> **Note on non-determinism:** because the baseline depends on the agent choosing
> to shell out to `az`, the same baseline stimulus can PASS on one run and FAIL on
> the next. Increase `defaults.runs` in the spec to average over more trials when a
> stable signal is needed. To surface flakiness across whole experiments, pass
> `-Iterations <n>` to run each experiment multiple times; the results summary
> reports every iteration's outcome and an aggregate pass count.

### Consolidated summary (by tool)

After the per-iteration results above, the script prints a **Consolidated
Summary** section with one entry per tool. Each entry folds every iteration and
stimulus recorded for that tool into a single set of statistics per variant
(pass rate, and average tokens/turns/wall time/AI credits), names the
best-performing *candidate* variant (highest pass rate first, then the most
efficient on ties - the shared `baseline` control is reported for reference but
never chosen as "best," since it's not a server-mode choice), and lists the
other variants beneath it for context. This is the quickest way to answer "which
server mode should we recommend for this tool, across everything we ran?"
without re-reading every iteration's detail. It is produced identically whether
the run just executed the experiments or `-ReportOnly`/`-ReportFrom` reconstructed
them from saved artifacts.

### Re-reporting without re-running

Every run's verdicts and metrics are saved to `results.jsonl`, so the summary can
be regenerated from those artifacts without building, provisioning, or invoking
vally again. Pass `-ReportOnly` (offline, free, and instant) to re-read the
**newest run directory** under `--output-dir`:

```powershell
# Re-print the summary from the last run's artifacts (auto-discovers the newest
# run directory under --output-dir, and prints which one it read)
./Invoke-VallyEval.ps1 -ReportOnly

# Combine with -Area/-Tool to focus, and -Iterations to report the newest N
# iteration(s) recorded WITHIN that run directory for each tool
./Invoke-VallyEval.ps1 -ReportOnly -Tool eventhub-get -Iterations 3

# Report from a SPECIFIC run directory instead of the newest one (e.g. an older
# run, or one copied/archived elsewhere) - implies -ReportOnly
./Invoke-VallyEval.ps1 -ReportFrom ./.vally-results/2026-07-29T18-19-30-496Z
```

`-ReportOnly` honors the same `-Area`/`-Tool` filters, and `-Iterations <n>`
selects how many of the newest timestamped iterations, within the selected run
directory, to report per tool (oldest-first, so iteration numbers read
chronologically). The vally exit code isn't persisted in
the artifacts, so report-only relies on the per-stimulus verdicts in each
`results.jsonl`; the process still exits non-zero if a candidate stimulus failed
or an effectiveness **REGRESSION** is detected. This is handy for re-examining or
debugging a prior run - it's exactly how the summary logic itself is validated.

## Adding more tool evaluations

No script changes are needed - the runner discovers new evals by convention.

### Recommended: use the `create-vally-tool-experiments` skill

The fastest, most consistent way to add a new tool experiment is the
[`create-vally-tool-experiments`](https://github.com/microsoft/mcp/blob/main/.github/skills/create-vally-tool-experiments/SKILL.md)
skill (GitHub Copilot). Ask Copilot to create an experiment for the target
tool (e.g. *"create vally tool experiments for the remaining eventhubs
tools"*) and it will, per tool:

- resolve the tool's full MCP name and area/tool naming (e.g.
  `eventhubs_eventhub_delete` -> area `eventhubs`, tool `eventhub-delete`),
- pull **every** required stimulus prompt for that tool from
  `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` (not invented text),
- write the `<tool>.eval.yaml` / `<tool>.experiment.yaml` pair following the
  baseline/namespace/consolidated pattern and outcome-only grading described
  above,
- extend the area's existing `New-*Resources.ps1`/Bicep (or add a new pair
  for a brand-new area) if the tool needs resources that don't already exist -
  including disposable resources for destructive `_delete` tools so they
  don't clobber other tools' evals, and
- validate with `Invoke-VallyEval.ps1` and report the
  `VALUABLE`/`REGRESSION`/`BOTH PASS`/`INCONCLUSIVE` comparison per stimulus.

See the skill file for the full step-by-step process, including how it
decides area boundaries and how it flags destructive-tool safety
considerations in the generated YAML comments.

### Manual steps

1. Pick an **area** subdirectory (reuse an existing one such as `eventhubs/`, or
   create a new one named after the namespace, e.g. `storage/`).
2. Add the shared eval spec named **`<tool>.eval.yaml`** (e.g.
   `eventhub-update.eval.yaml`) - copy `eventhubs/eventhub-get.eval.yaml` and
   adjust the prompts, the `--namespace` argument, and the `prompt`/`rubric`
   outcome graders. Every prompt must come from
   `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`. Grading is outcome-only
   (see above), so there are no tool-selection graders to maintain. Reference
   any provisioned resource name as a
   [Vally parameter](https://microsoft.github.io/vally/reference/cli/eval/#parameter-resolution)
   with an inline default matching the provisioning script, e.g.
   `"${RESOURCE_GROUP=contoso-rg}"` / `"${EVENTHUBS_NAMESPACE=contoso-ehns}"`,
   rather than hardcoding the literal name - see "How provisioned resource
   names reach the eval prompts" above.
3. Add the experiment named **`<tool>.experiment.yaml`** (e.g.
   `eventhub-update.experiment.yaml`) - copy `eventhubs/eventhub-get.experiment.yaml`,
   point its `evals:` at your new `.eval.yaml`, and keep the `baseline`,
   `namespace`, and `consolidated` variants (the baseline deletes
   `environment.mcpServers.azure`; the `consolidated` variant overrides `--mode`,
   so update its `--namespace` argument to match). No separate baseline eval file
   is needed - the variants are defined here.
4. For a new area needing Azure resources, add `New-*Resources.ps1` and
   `Remove-*Resources.ps1` to that area folder (see the Event Hubs pair as a
   template). They are discovered and run per area. Have `New-*Resources.ps1`
   emit a `PSCustomObject` of the `KEY = value` pairs matching the params your
   new eval spec references (as its last output object - see above) so the
   runner forwards them to `vally` automatically. For a destructive
   (`_delete`) tool in an existing area, prefer extending that area's existing
   provisioning with a disposable, single-purpose resource (see
   `eventhubs/eventhubs-resources.bicep`'s `deletableEventHubName`/
   `deletableConsumerGroupName`/`deletableNamespaceName` parameters) rather
   than deleting a resource other evals depend on.
5. Run everything with `./Invoke-VallyEval.ps1`, or just the new one with
   `-Area <area>` / `-Tool <tool>`.
