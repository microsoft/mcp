---
name: create-vally-tool-experiments
description: 'Create a Vally experiment (baseline vs. namespace vs. consolidated) that evaluates how effectively an agent uses a specific Azure MCP tool. USE WHEN: create vally eval, add tool experiment, evaluate azmcp tool, measure MCP tool effectiveness, add eval spec, create experiment.yaml.'
argument-hint: 'Name the tool to evaluate (e.g., "eventhubs_eventhub_get" or "storage account list") or the area (e.g., "storage")'
---

# Create a Vally Tool Experiment

## Purpose

Add a new [vally](https://microsoft.github.io/vally) **experiment** (a shared
eval spec run as `baseline` / `namespace` / `consolidated` variants) that
measures how effectively an agent uses one specific Azure MCP tool, following
the pattern established in
`servers/Azure.Mcp.Server/tests/Vally/eventhubs/eventhub-get.*.yaml` and
`namespace-get.*.yaml`.

**Compatible with:** GitHub Copilot (primary; this skill lives under
`.github/skills/` per this repo's convention). The `SKILL.md` frontmatter
format is identical to Claude Code's, so this file also works unmodified if
copied or symlinked to `.claude/skills/create-vally-tool-experiments/SKILL.md`
for Claude Code — treat that as optional, not required.

**Hard requirement — do not skip:** the stimuli `prompt` fields in the new
`<tool>.eval.yaml` **must be sourced from**
`servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`. Every distinct test prompt
listed there for the target tool becomes one stimulus. Do not invent prompts
that aren't in that file, and do not omit a prompt that is there (see
[Step 2](#step-2-collect-the-required-stimuli-from-e2etestpromptsmd)).

## Prerequisites

Read `servers/Azure.Mcp.Server/tests/Vally/README.md` in full before starting —
it defines the layout/naming convention, the baseline-vs-candidate comparison
model, and how `Invoke-VallyEval.ps1` discovers evals. This skill assumes that
document as ground truth; if the two ever disagree, the README wins.

## Step 1: Identify the target tool and its area

1. Resolve the tool to its full MCP tool name, `<area>_<resource>_<operation>`
   (e.g. `eventhubs_eventhub_get`), and its **tool name** in the vally sense —
   the same string minus the area prefix pattern used for file naming, e.g.
   `eventhub-get` (kebab-case, matching an existing `<tool>.experiment.yaml` if
   one exists for a sibling operation).
2. Find the command implementation to confirm the namespace/mode and the
   options it needs (subscription, resource group, resource-specific names):
   `tools/Azure.Mcp.Tools.{Area}/src/Commands/**/*Command.cs`.
3. Decide the **area** subfolder under `servers/Azure.Mcp.Server/tests/Vally/`
   (reuse an existing one, e.g. `eventhubs/`, if the tool belongs to a
   namespace already evaluated there; otherwise create a new folder named
   after the namespace, e.g. `storage/`).

## Step 2: Collect the REQUIRED stimuli from e2eTestPrompts.md

This is the step that must not be shortcut.

1. Open `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md` and find the section
   for the tool's area (the tables are grouped by area, alphabetical, e.g.
   `## Azure Event Hubs`).
2. Extract **every row** whose `Tool Name` column exactly matches the target
   tool (e.g. every row for `eventhubs_eventhub_get`). Use grep, not manual
   scanning, to avoid missing rows:
   ```powershell
   Select-String -Path servers/Azure.Mcp.Server/docs/e2eTestPrompts.md -Pattern '^\| eventhubs_eventhub_get \|'
   ```
3. Each matched `Test Prompt` becomes one `stimuli[].prompt` in the new
   `<tool>.eval.yaml`, in the same order they appear in the file. If two rows
   are near-duplicates that would produce an identical stimulus (rare), keep
   both only if they exercise materially different agent behavior; otherwise
   keep the first and note the dedup in a comment.
4. Replace any placeholder tokens in the prompt (e.g. `<namespace_name>`,
   `<resource_group_name>`, `<event_hub_name>`) with concrete values that match
   real (or to-be-provisioned) test resources — reuse the existing
   `contoso-*` naming convention from `eventhubs/` when the area already has
   one, or invent a consistent `contoso-*` name for a new area. Keep the rest
   of the prompt wording **verbatim** from `e2eTestPrompts.md` — do not
   paraphrase the instructional text itself.
5. If the tool is destructive (`_delete`) or mutating (`_update`, create) and
   there is no safe, idempotent way to run it repeatedly against shared test
   resources, still create the stimulus from the required prompt text, but
   flag this in a comment at the top of the `.eval.yaml` and consider scoping
   provisioning so each run gets a fresh disposable resource (see
   `New-EventHubsResources.ps1` for the tagging/cleanup pattern) — do not drop
   the prompt just because it mutates state.

## Step 3: Write `<tool>.eval.yaml`

Copy `eventhubs/eventhub-get.eval.yaml` as the template and adapt:

- `name`: `<area>-<tool>-eval`.
- `description`: one sentence — what capability/outcome is evaluated.
- `defaults`: keep `runs: 1`, `timeout: 5m`, `model: claude-opus-4.6`,
  `judge_model: gpt-5.5`, `executor: copilot-sdk` unless the tool has a
  specific reason to differ (document it in a comment if so).
- `environment.mcpServers.azure`: `stdio` / `command: azmcp` / `args: [server,
  start, --namespace, <area>, --mode, namespace,
  --dangerously-disable-elicitation]` / `env.AZURE_TOKEN_CREDENTIALS:
  AzureCliCredential`. This is the block the `namespace` variant inherits
  unchanged and the `consolidated` variant overrides `--mode` on.
- `stimuli`: one entry per prompt collected in Step 2, each with:
  - `name`: short kebab-case slug describing the stimulus.
  - `prompt`: the e2e prompt text (placeholders resolved per Step 2.4).
  - `rubric`: bullet points an outcome judge can check — require **real data
    from a live Azure lookup**, forbid fabricated/placeholder values, and
    forbid the assistant merely explaining how the user could do it
    themselves.
  - `graders`: a single `type: prompt` grader whose `config.prompt` restates
    the rubric as scoring instructions with `scoring: binary`.
- `scoring`: `weights.prompt: 1.0`, `threshold: 1.0` — outcome-only grading, no
  `tool-calls` grader (the baseline variant has no MCP tool to call, so a
  tool-selection assertion could never be satisfied consistently across
  variants — see the README's "What ... checks" section).

## Step 4: Write `<tool>.experiment.yaml`

Copy `eventhubs/eventhub-get.experiment.yaml` and adapt:

- `name`: `<area>-<tool>-experiment`.
- `evals: [./<tool>.eval.yaml]`.
- `vary: [/environment/mcpServers]` (only the MCP server map may differ between
  variants).
- `baseline: baseline`.
- `variants`:
  - `baseline`: `environment.mcpServers.azure: null` (removes the server —
    control).
  - `namespace: {}` (inherits the eval spec's server block as-is).
  - `consolidated`: repeats the full `mcpServers.azure` block with `--mode
    consolidated` instead of `namespace`.

## Step 5: Provisioning (only if the area needs new Azure resources)

If the target area already has `New-*Resources.ps1` / `Remove-*Resources.ps1`
(e.g. `eventhubs/`), reuse the resources they provision — do not add a second
provisioning pair for the same area unless the new tool needs resources the
existing scripts don't create (then extend the existing scripts/Bicep rather
than duplicating them).

For a brand-new area, add a `New-<Area>Resources.ps1` / `Remove-<Area>Resources.ps1`
pair plus a Bicep template, modeled directly on
`eventhubs/New-EventHubsResources.ps1` / `eventhubs/Remove-EventHubsResources.ps1`
/ `eventhubs/eventhubs-resources.bicep`:

- Subscription-scoped Bicep deployed via `az deployment sub create`.
- Stamp a `DeleteAfter` tag (ISO 8601 UTC) on the resource group so the repo's
  standard clean-up job reclaims it even if teardown is skipped.
- `Assert-Az` after every `az` call (native command failures don't throw on
  their own with `$ErrorActionPreference = 'Stop'`).
- Resource names must match the concrete values used in Step 2.4.

`Invoke-VallyEval.ps1` auto-discovers these scripts per area and runs them
around every tool's experiment in that area — no runner changes needed.

## Step 6: Validate

```powershell
cd servers/Azure.Mcp.Server/tests/Vally
./Invoke-VallyEval.ps1 -Area <area> -Tool <tool>
```

Gate — do not consider the experiment done until:

- [ ] Every prompt for the target tool in `e2eTestPrompts.md` appears as a
      stimulus (re-run the grep from Step 2 and diff against the `.eval.yaml`).
- [ ] `vally experiment run` completes for all three variants without a spec
      validation error (the `vary` list matches what actually differs).
- [ ] The `namespace` and `consolidated` candidate variants **pass** the
      outcome grader against real, live Azure data (provision resources and
      `az login` first — a passing grade on fabricated data is not a valid
      result).
- [ ] The `baseline` variant is allowed to fail or pass (informational only) —
      it must not be graded on tool-selection.
- [ ] If new provisioning scripts were added, `Remove-*Resources.ps1` cleans
      up everything `New-*Resources.ps1` created.
- [ ] If this is the first tool in a new area, add a short section to
      `servers/Azure.Mcp.Server/tests/Vally/README.md` describing what the
      experiment checks (mirroring the "What the Event Hubs experiment
      checks" section) — existing areas need no README changes since the
      runner discovers evals by convention.

Report the pairwise comparison table (`VALUABLE` / `REGRESSION` /
`CANDIDATE AND BASELINE PASS` / `INCONCLUSIVE` per stimulus) and the best
variant that `Invoke-VallyEval.ps1` prints as the result of this skill.
