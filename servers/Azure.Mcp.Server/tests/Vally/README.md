# Vally evaluations for the Azure MCP Server

This directory holds [vally](https://microsoft.github.io/vally) evaluations that measure how
*effectively* an agent uses individual Azure MCP tools. Unlike the end-to-end
tests in `CopilotCliTester`, vally evals are declarative eval specs graded by
vally's built-in, reference-free graders (no gold-standard answer required).

The runner (`Invoke-VallyEval.ps1`) **discovers** every evaluation under this
directory, so adding a new tool evaluation is just a matter of dropping files in
the right place with the right names — no script changes needed.

## Layout and naming convention

Evaluations are organized by **area** (a subfolder, usually an Azure MCP
namespace) and, within each area, by **tool**:

```
tests/Vally/
??? Invoke-VallyEval.ps1              # Discovers, builds azmcp, provisions, runs, tears down
??? eventhubs/                        # An AREA (one subfolder per namespace)
?   ??? eventhub-get.eval.yaml            # <tool>.eval.yaml          - treatment (required)
?   ??? eventhub-get.eval.baseline.yaml   # <tool>.eval.baseline.yaml - control   (optional)
?   ??? New-EventHubsResources.ps1        # New-*Resources.ps1    - per-area provisioning (optional)
?   ??? Remove-EventHubsResources.ps1     # Remove-*Resources.ps1 - per-area teardown     (optional)
??? README.md                         # This file
```

The **tool name** is the eval file name with the `.eval.yaml` suffix removed
(e.g. `eventhub-get`). Each area can hold many tools (e.g. `eventhub-get`,
`namespace-get`, …), each an independent treatment/baseline pair. Provisioning
scripts are discovered **per area** and run once for all of that area's tools.

The first area, `eventhubs/`, evaluates the **get Event Hub** tool
(`eventhubs_eventhub_get`) with a matched pair of specs:

- **`eventhub-get.eval.yaml`** (treatment) — the agent has the Azure MCP Event
  Hubs tools.
- **`eventhub-get.eval.baseline.yaml`** (control) — the *same* prompts, but with
  **no** Azure MCP server connected.

The two are identical except for the presence of the Azure MCP server, so the
delta between them isolates the server's contribution. They share the same
outcome graders; the treatment layers tool-selection graders on top — see
[Treatment vs. baseline](#treatment-vs-baseline-control) below.

## What the Event Hubs experiment checks

Both specs send the same natural-language prompts (e.g. *"List all of the Event
Hubs in my namespace…"*) and share the same **outcome** grader — an
[LLM `prompt` judge](https://microsoft.github.io/vally/reference/graders/) that
checks the agent **actually returned** the requested Event Hubs (real data from
Azure, not a refusal, a deferral, or fabricated values).

The **treatment** (`eventhub-get.eval.yaml`) additionally uses the
[`tool-calls`](https://microsoft.github.io/vally/reference/graders/) grader to
assert that:

- the agent **selects and invokes** `eventhubs_eventhub_get`, and
- it does **not** invoke a destructive Event Hubs tool (e.g. `*_delete`).

Sign in with `az login` (and provision the resources below) so the agent can
return real Event Hubs data — the shared outcome grader needs it.

## Treatment vs. baseline (control)

`eventhub-get.eval.baseline.yaml` is the **control**: it reuses the exact same
prompts but omits the `environment.mcpServers` block, so the agent runs with only
its built-in tools.

To keep the comparison consistent, **both specs share the same outcome grader**
(the `prompt` LLM judge, "did the agent return the requested Event Hubs?"). The
treatment adds `tool-calls` graders on top — those are treatment-only, because
with no MCP server the control has no tools to call and "did it call
`eventhubs_eventhub_get`?" would be impossible by construction rather than a
meaningful measurement.

| Run | Azure MCP server | Graders | Typical verdict |
|:----|:-----------------|:--------|:----------------|
| `eventhub-get.eval.yaml` (treatment) | ? connected | `prompt` (shared outcome) **+** `tool-calls` (tool selection) | **PASS** |
| `eventhub-get.eval.baseline.yaml` (control) | ? absent | `prompt` (shared outcome) only | **FAIL** (often) |

Both are judged on the same outcome question, so their verdicts are directly
comparable. The treatment can both return the data (shared grader) and be verified
to use the right tool (tool-selection graders).

The baseline is the *control*: without the Azure MCP server the agent has no
Event Hubs tool, so the expectation is that it cannot retrieve the data and the
shared outcome grader scores 0. **But the baseline is not guaranteed to fail** —
the agent still has its general-purpose tools, so it may shell out to the Azure
CLI (`az eventhubs ...`) via its built-in `powershell` tool and satisfy the
outcome grader anyway (see [Interpreting effectiveness](#interpreting-effectiveness)
below). Keep the `stimuli` prompts and the shared `prompt` graders in sync between
the two files so the presence of the server is the only variable.

Because both specs grade *outcome*, the agent needs real data to have any chance
of passing; run `az login` (and provision the resources below) first so the
comparison is honest — the treatment can return real Event Hubs while the baseline
still cannot.

## Prerequisites

1. **Vally CLI** — install per the
   [vally install guide](https://microsoft.github.io/vally/get-started/install/)
   and make sure `vally` is on your `PATH`.
2. **.NET SDK** — required to build the `azmcp` server (see the repo root
   `global.json` for the pinned version).
3. **Azure sign-in** — `az login`. Required to provision real Event Hubs (see
   below) and for the treatment to return real data; the outcome-graded baseline
   needs real data to be a fair test.
4. **Azure CLI** — `az` on your `PATH`, only needed if you use the provisioning
   scripts.

## Provisioning test resources

The eval prompts reference concrete resources (resource group `contoso-rg`,
namespace `contoso-ehns`, event hub `orders`). Two scripts create and remove them:

- **`eventhubs/New-EventHubsResources.ps1`** — creates the resource group, the
  Event Hubs namespace, and the event hubs (including `orders`). It stamps a
  **`DeleteAfter`** tag (an ISO 8601 UTC timestamp, matching the repo's
  `TestResources` convention) on the resource group so the standard Azure
  clean-up job reclaims it **even if teardown never runs**.
- **`eventhubs/Remove-EventHubsResources.ps1`** — deletes the resource group and
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

## Running

From this directory:

```powershell
./Invoke-VallyEval.ps1 -Subscription <subscription-id>
```

The script builds `Azure.Mcp.Server`, prepends the freshly built `azmcp` to
`PATH` (vally does not expand environment variables inside eval specs),
**discovers every `<tool>.eval.yaml` under every area subfolder**, and for each
one provisions the area, runs the treatment and baseline, and tears the area down.
It prints a per-tool comparison. Its exit code is non-zero if any *treatment* eval
fails (baseline failures are expected). Useful switches:

```powershell
# Skip the build if azmcp is already built and on PATH
./Invoke-VallyEval.ps1 -SkipBuild

# Only a single area, or a single tool (wildcards allowed)
./Invoke-VallyEval.ps1 -Area eventhubs
./Invoke-VallyEval.ps1 -Tool eventhub-get

# Resources already exist - don't provision or tear down
./Invoke-VallyEval.ps1 -SkipProvisioning

# Only run the treatments; skip the baseline/control runs
./Invoke-VallyEval.ps1 -SkipBaseline

# Override the model and show full agent output
./Invoke-VallyEval.ps1 -Model claude-opus-4.6 -Verbose

# Run one explicit spec (its baseline is derived automatically)
./Invoke-VallyEval.ps1 -EvalSpec ./eventhubs/eventhub-get.eval.yaml
```

To run either spec by hand (with `azmcp` already on `PATH`):

```powershell
# Treatment (with the Azure MCP server)
vally eval --eval-spec ./eventhubs/eventhub-get.eval.yaml --output-dir ./.vally-results/eventhubs/eventhub-get/treatment --verbose

# Baseline (control, no Azure MCP server) - expected to fail
vally eval --eval-spec ./eventhubs/eventhub-get.eval.baseline.yaml --output-dir ./.vally-results/eventhubs/eventhub-get/baseline --verbose
```

## Results

The runner writes each run under `--output-dir` (default `./.vally-results`) in an
`<area>/<tool>/treatment` or `<area>/<tool>/baseline` subdirectory. Vally then
creates a timestamped folder per run containing:

- `results.jsonl` — one JSON record per stimulus (a `trial-result`, plus a final
  `run-summary`). Each `trial-result` carries the verdict (`gradeResult.passed`)
  and the efficiency metrics (`trajectory.metrics.tokenUsage.totalTokens`,
  `trajectory.metrics.turnCount`, `trajectory.metrics.wallTimeMs`), and
- `eval-results.md` — a human-readable Markdown summary.

### Interpreting effectiveness

The point of the treatment-vs-baseline pairing is to measure the Azure MCP
server's *effectiveness*. After both runs complete, the script reads the newest
`results.jsonl` from each side and compares them **per stimulus**:

| Baseline (no server) | Treatment (with server) | Category | Meaning |
|:---------------------|:------------------------|:---------|:--------|
| FAIL | PASS | **VALUABLE** | The server enabled an outcome the agent could not achieve without it. |
| PASS | FAIL | **REGRESSION** | The agent succeeded *without* the server but failed *with* it — the server hurt the outcome. |
| PASS | PASS | **BOTH PASS** | The server was not required for the outcome. Efficiency decides: lower **tokens**, **turns**, and **wall time** are better; the script prints each metric and the treatment-vs-baseline delta. |
| FAIL | FAIL | **INCONCLUSIVE** | Neither achieved the outcome. |

A **BOTH PASS** result is common and expected: the baseline agent can shell out to
the Azure CLI via its built-in `powershell` tool, so it often retrieves the data
without the MCP server. That is not a problem in itself — it just means this
stimulus does not *require* the server, and the comparison falls through to
efficiency (fewer tokens/turns/less wall-clock time is better).

The process exit code is `0` when every *treatment* eval verdict passes, and `1`
if any treatment eval fails, an effectiveness **REGRESSION** is detected, **or**
any area's provisioning fails. Baseline outcome failures on their own are expected
and do not affect it, so the script is CI-friendly.

> **Note on non-determinism:** because the baseline depends on the agent choosing
> to shell out to `az`, the same baseline stimulus can PASS on one run and FAIL on
> the next. Increase `defaults.runs` in the spec to average over more trials when a
> stable signal is needed.

## Adding more tool evaluations

No script changes are needed — the runner discovers new evals by convention:

1. Pick an **area** subdirectory (reuse an existing one such as `eventhubs/`, or
   create a new one named after the namespace, e.g. `storage/`).
2. Add a treatment spec named **`<tool>.eval.yaml`** (e.g.
   `eventhub-update.eval.yaml`) — copy `eventhubs/eventhub-get.eval.yaml` and
   adjust the prompts, the `--namespace` argument, the shared `prompt`/`rubric`
   outcome graders, and the `tool-calls` grader `name` patterns.
3. Optionally add a matching **`<tool>.eval.baseline.yaml`** control — copy
   `eventhubs/eventhub-get.eval.baseline.yaml`, keep the `stimuli` prompts **and
   the shared `prompt` outcome graders identical** to the treatment, and leave out
   the `environment.mcpServers` block and the treatment-only `tool-calls` graders.
4. For a new area needing Azure resources, add `New-*Resources.ps1` and
   `Remove-*Resources.ps1` to that area folder (see the Event Hubs pair as a
   template). They are discovered and run per area.
5. Run everything with `./Invoke-VallyEval.ps1`, or just the new one with
   `-Area <area>` / `-Tool <tool>`.
