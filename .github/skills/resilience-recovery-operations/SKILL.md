---
name: resilience-recovery-operations
description: 'Operate all Azure Resilience Management MCP tools for usage plans, enrollments, goals, drills, recovery plans, recovery resources, recovery jobs, failover, reprotect, readiness, validation, retry, resume, and finalize. Use when: list/get/create/update/delete resilience resources; run or end drills; include/exclude recovery resources; check readiness; validate or execute recovery operations; monitor, retry, or resume recovery jobs; implement, add, test, or record a Resilience Management tool.'
argument-hint: 'Describe the resilience operation and provide known service group, plan, drill, job, subscription, or resource identifiers'
user-invocable: true
disable-model-invocation: false
---

<!-- cspell:words reprotect reprotection -->

# Azure Resilience Management Operations

For operational Azure requests, use the Azure Resilience Management MCP tools exclusively. Do not use Azure CLI (`az`), `az rest`, direct HTTP/REST calls, PowerShell Azure commands, or SDK code as an operational fallback. This restriction does not apply to development tasks such as implementing, testing, recording, or debugging tools; follow the repository development workflow and use its required tooling. This skill covers all tools registered under the `resilience` namespace.

- See [tool reference](./references/tools.md) for exact tool names, parameters, and enum values.
- See [workflow recipes](./references/workflows.md) for end-to-end operation sequences and state gates.
- See [payload reference](./references/payloads.md) before composing plan actions, groups, or recovery-resource updates.
- See [recorded test guidance](../../../docs/recorded-tests.md) for the repository-wide recording architecture, sanitizers, matchers, and troubleshooting.

## Operating Rules

1. Use get/list tools to resolve unknown IDs and inspect current state before mutations.
2. Never invent service groups, recovery plans, drill names, job IDs, source locations, recovery-resource IDs, identity IDs, or runbook IDs.
3. Use a tenant or subscription only when supplied by the user, available from the active environment, or returned by a preceding tool call.
4. Treat an explicit user request for a named mutation as authorization for that mutation. Ask only for missing choices that cannot be inferred safely.
5. Before irreversible deletion, identify the exact resource. A request such as “delete X” is sufficient confirmation; an ambiguous request is not.
6. Report operation IDs and recovery job IDs returned by asynchronous operations. Use the corresponding get tool to inspect status when requested.
7. Do not claim an accepted asynchronous operation has completed.
8. Do not retry stale jobs to represent a fresh operation. Start a new readiness or validation operation when current state must be assessed.
9. If a prerequisite fails, report per-resource blockers and do not start the dependent destructive operation.
10. Use `recoveryplan` as the recovery plan parameter for every tool.
11. For Recovery Orchestration (RO) recovery plan updates only, do not use or imply HTTP `PATCH`; the current SDK does not support RO recovery plan PATCH. Get the existing plan and preserve unchanged values in the create-or-update request.
12. For operational Azure requests, if no registered Resilience Management MCP tool supports the requested operation, state that the operation is unavailable through the current toolset. Do not work around the gap with `az`, REST, or another execution surface.

## Route the Request

- Usage plan or enrollment → `mcp_azure_mcp_ser_resilience_usageplan_*`
- Goal template, assignment, or member → `mcp_azure_mcp_ser_resilience_goal_*`
- Drill definition, execution, run, or target → `mcp_azure_mcp_ser_resilience_drill_*`
- Recovery plan lifecycle or recovery operation → `mcp_azure_mcp_ser_resilience_recoveryplan_*`
- Recovery plan membership/protection → `mcp_azure_mcp_ser_resilience_recoveryplan_resource_*`
- Recovery job, paused action, retry, or job target → `mcp_azure_mcp_ser_resilience_recoveryjob_*`

## Standard Procedure

### Resolve context

1. Extract all identifiers and explicit choices from the current request.
2. Reuse identifiers from the current conversation only when they unambiguously refer to the same operation and target.
3. If a target name is known, call its get tool directly with `name`; do not list first unnecessarily.
4. If a target is described but not named, list the narrowest parent collection, present matching candidates, and ask only when multiple candidates remain.
5. Never choose among multiple subscriptions, service groups, plans, drills, jobs, locations, or resources without user direction.

### Read-only requests

1. Call the matching get tool without `name` to list resources, or with `name` when the target is already known.
2. If a list response contains only IDs and names and full details are needed, call get again with `name`.
3. Return relevant status, IDs, state, errors, and attention reasons. Do not dump unrelated properties.

### Create or update requests

1. Get the existing resource when update semantics depend on omitted values.
2. Collect conditionally required choices.
3. Call the create/update tool with the intended changes and all values required by its schema.
4. Re-read the resource when the response does not prove the intended state.

### Delete requests

1. Get the named resource and verify it is the intended target.
2. Check for active drill runs, recovery operations, or other state that blocks deletion.
3. Call delete only for an explicit, unambiguous request.
4. Report whether the resource existed and whether deletion was accepted or completed.

### Long-running or destructive requests

1. Resolve and inspect the exact target.
2. Run the mandatory pre-validation in the matching workflow recipe.
3. Stop if no resources qualify, readiness fails, or the state does not support the operation.
4. Execute only after required selectors and choices are provided.
5. Report the status returned by the command. If it returns `Accepted`, include operation/job IDs and inspect subsequent status only through get tools; otherwise, report the completed result.

### Monitor asynchronous operations

1. Preserve every returned operation ID and recovery job ID.
2. Use recovery-job get for job state and drill-run get for drill execution history.
3. If a response is `Accepted`, say “accepted” or “started,” not “completed.”
4. On timeout, query state once if a suitable get tool and identifier exist; otherwise report that completion is unknown.
5. Do not poll indefinitely. Return the latest observed state and IDs when the operation remains in progress.

## Failure Handling

- Surface service error codes, blocking reasons, attention reasons, and recommendations.
- A timeout means completion is unknown; do not report failure or success without a subsequent get.
- `AutomationRunbookExistenceCheckUnavailable` blocks readiness-dependent recovery operations until runbook accessibility is corrected.
- A test-proxy 404 that reports no matching recording is a playback infrastructure failure; do not mutate Azure to work around it. An Azure 404 intentionally preserved in a recording can validly represent a missing plan, job, or other resource.
- When an MCP call fails, retry only when the failure is transient and the operation is safe to repeat. Never switch to Azure CLI or direct REST to bypass the MCP failure.

## Tool Development Standards

For the generic authoring lifecycle, follow [add-azure-mcp-tools](../add-azure-mcp-tools/SKILL.md). Then apply the [Resilience Management development requirements](./references/development.md), including its stricter ToolDescriptionEvaluator gate.

Every new or behaviorally changed Resilience Management tool must have at least two distinct E2E evaluation prompts. For every prompt, the expected tool must rank `#1` with a score of at least `0.6`. This is an intentional Resilience-specific override of the repository-wide top-three and `0.4` baseline.

## Result Format

State what was attempted, identify the target, summarize the outcome, and include operation/job IDs. For validation, list qualified and unqualified resources with reasons. For blocked mutations, state the prerequisite that must be corrected.
