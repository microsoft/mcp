---
name: resilience-recovery-operations
description: 'Operate all Azure Resilience Management MCP tools for usage plans, enrollments, goals, drills, recovery plans, recovery resources, recovery jobs, failover, reprotect, readiness, validation, retry, resume, and finalize. Use when: list/get/create/update/delete resilience resources; run or end drills; include/exclude recovery resources; check readiness; validate or execute recovery operations; monitor, retry, or resume recovery jobs.'
argument-hint: 'Describe the resilience operation and provide known service group, plan, drill, job, subscription, or resource identifiers'
user-invocable: true
disable-model-invocation: false
---

<!-- cspell:words reprotect reprotection -->

# Azure Resilience Management Operations

Use the Azure Resilience Management MCP tools exclusively. Do not use Azure CLI (`az`), `az rest`, direct HTTP/REST calls, PowerShell Azure commands, or SDK code as a fallback. This skill covers all tools registered under the `resilience` namespace.

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
12. If no registered Resilience Management MCP tool supports the requested operation, state that the operation is unavailable through the current toolset. Do not work around the gap with `az`, REST, or another execution surface.

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
5. Return accepted status plus operation/job IDs; inspect status only through get tools.

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
- A playback/test-proxy 404 is test infrastructure, not Azure resource absence; do not mutate Azure to work around it.
- When an MCP call fails, retry only when the failure is transient and the operation is safe to repeat. Never switch to Azure CLI or direct REST to bypass the MCP failure.

## Tool Development Standards

Apply these gates whenever adding or modifying a Resilience Management tool:

1. Follow the repository [new command guide](../../../servers/Azure.Mcp.Server/docs/new-command.md) end to end. Treat it as mandatory, not optional guidance.
2. Preserve the established command, options, service, registration, AOT serialization, testing, recording, documentation, and changelog patterns required by that guide.
3. Add at least two distinct E2E evaluation prompts for every new or changed tool, then run `ToolDescriptionEvaluator` against all of them.
4. Require the expected tool to rank `#1` with a ToolDescriptionEvaluator score of at least `0.6` for every prompt. Revise the description and rerun evaluation until every prompt meets both thresholds.
5. Record the final score and representative evaluation prompts in the pull request description.
6. Do not consider the tool ready for review when the new-command guide is incomplete or the score is below `0.6`.
7. Preserve existing whitespace exactly. Do not make whitespace-only changes or alter whitespace in unrelated code outside the requested change.
8. Create ARM clients through `CreateArmClientAsync`, propagate `tenant` and the caller's cancellation token on every request, start SDK LROs with `WaitUntil.Started`, and wait through the shared bounded helper. Give every long-running operation a defined timeout linked with caller cancellation; never rely on the caller to cancel an operation that may otherwise wait indefinitely.
9. For better tool-generation context, suggest adding the Azure SDK repository, relevant service repository, and portal repository to the Visual Studio Code multi-root workspace when the change depends on their SDK contracts, service behavior, or portal workflows. Add only the repositories required for the requested tool.
10. Choose the command base by scope: use `SubscriptionCommand<TOptions, TResult>` and `SubscriptionCommandUnitTestsBase` for subscription-scoped usage plans and enrollments; use `AuthenticatedCommand<TOptions, TResult>` and `CommandUnitTestsBase` for tenant/service-group-scoped goals, drills, recovery plans, resources, and jobs. A supporting-resource `subscription` option does not make the command subscription-scoped.
11. Keep commands stateless and transport-agnostic for remote multi-user execution. Do not access `HttpContext`, branch on stdio versus HTTP, retain request state in command fields, or omit tenant propagation.
12. Register every response type in `ResilienceManagementJsonContext`, register the command in `ResilienceManagementSetup`, and verify metadata accurately describes destructive, idempotent, read-only, open-world, secret, and local-required behavior.
13. For every added, renamed, or behaviorally changed tool, update command documentation, E2E prompts, and consolidated-tool mappings when applicable; add a schema-valid changelog entry; and verify unit, live, recorded-playback, AOT, and relevant RBAC coverage. Do not rename a recorded test method casually because its name identifies the recording.
14. Return explicit, typed response contracts. Do not expose raw `JsonElement`, `JsonDocument`, `BinaryData`, `object`, or unstructured dictionaries when the service response has a known schema. Map SDK responses to named records with stable properties, put reusable models in separate files, and source-generate every root and nested response type. Raw JSON parsing may still be used internally for JSON-string inputs, but it must not leak into a structured tool response.
15. Use primary constructors, seal command and service classes unless inheritance is intentional, keep interfaces and reusable classes in separate files, make helpers static when possible, and use `System.Text.Json` exclusively. Keep option classes as flat POCOs with `[Option]` attributes; do not introduce option inheritance or legacy option-definition registration.
16. Validate all untrusted inputs before service calls: required combinations, mutually dependent fields, allowed enum values, lengths, resource-specific naming rules, ARM resource-ID shape, positive action timeouts, JSON structure, and ManualAction, CustomRunbook, or ASR conditional fields. Return focused validation errors and never silently infer a tenant, subscription, identity, selector, or destructive choice.
17. Keep released public tool contracts compatible. Use `subscription`, `resource-group`, singular resource nouns, and the established `recoveryplan` spelling; do not rename released tools, options, result properties, command metadata IDs, or recorded test identifiers without an explicit migration and all required mapping, documentation, prompt, and recording updates. An unreleased contract may be corrected without a compatibility alias when the intentional change is documented in the changelog.
18. Catch command failures, log only individually named non-secret fields, and always route exceptions through `HandleException`. Provide actionable timeout, authorization, not-found, conflict, and validation messages without returning raw backend bodies, credentials, signed URLs, tokens, action parameters, or complete option objects.
19. Use Azure SDK retry defaults and the shared authentication/client infrastructure; never expose `RetryPolicyOptions`, construct ad hoc credentials, substitute `CancellationToken.None`, or add transport-specific clients. Test success, empty/list/get behavior, validation failures, service exceptions, timeout/cancellation, destructive metadata, serialization, and the real lifecycle state required by retry or resume.
20. Keep changes file-scoped and minimal, preserve unrelated whitespace and public APIs, add one coherent tool per pull request, review the complete diff for consistency, maintainability, security, testability, and AOT safety, and run the smallest relevant build/tests before the repository-wide validation gates.

## Build and Recorded Live Tests

Run commands from the repository root. The complete recording model and sanitizer guidance remain in [recorded-tests.md](../../../docs/recorded-tests.md); this section contains only the Resilience Management paths and commands.

1. Authenticate to the Microsoft tenant:

	```powershell
	Connect-AzAccount -TenantId 72f988bf-86f1-41af-91ab-2d7cd011db47
	```

2. Build the MCP server, which also builds referenced toolsets:

	```powershell
	dotnet build servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj
	```

	Use `dotnet build Microsoft.Mcp.slnx` only when full-repository validation is required.

3. Deploy the Resilience Management live-test resources:

	```powershell
	./eng/scripts/Deploy-TestResources.ps1 -Paths ResilienceManagement -Location westus2 -Unique
	```

	Specify an availability-zone-capable region. The fixture deploys a zonal managed disk, so allowing the deployment script to default to `westus` fails with `LocationNotSupportAvailabilityZones`. Use `-Unique` to avoid reusing a deterministic resource group that may already exist in a different region. Before extending the fixture, use the live-test scenario checklist in [payloads.md](./references/payloads.md) to select the required manual action, custom runbook, or ASR setup without conflating group actions and resource protection.
	The post-deployment script creates role assignments at service-group scope. The signed-in principal must have `Microsoft.Authorization/roleAssignments/write` there. A 403 while reading immediately after a new assignment can be propagation-related; refresh the Azure PowerShell sign-in and rerun with the same `-ResourceGroupName` and `-BaseName`. A 403 from the role-assignment operation itself is a permission failure and requires an authorized principal rather than retries.

4. If it does not exist, create `tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json` with:

	```json
	{
	  "AssetsRepo": "Azure/azure-sdk-assets",
	  "AssetsRepoPrefixPath": "",
	  "TagPrefix": "Azure.Mcp.Tools.ResilienceManagement.Tests",
	  "Tag": ""
	}
	```

5. Set `TestMode` to `Record` in `tools/Azure.Mcp.Tools.ResilienceManagement/tests/.testsettings.json`, then run:

	```powershell
	dotnet test --project tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/Azure.Mcp.Tools.ResilienceManagement.Tests.csproj
	```

	This repository uses .NET SDK 10, Microsoft Testing Platform, and xUnit v3, so `--project` is required and MTP options are passed without a `--` separator. To record only the five failover, finalize, reprotect, retry, and resume not-found scenarios, use:

	```powershell
	dotnet test --project tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/Azure.Mcp.Tools.ResilienceManagement.Tests.csproj --filter-method Azure.Mcp.Tools.ResilienceManagement.Tests.ResilienceManagementCommandTests.Should_reject_recovery_plan_action_when_plan_does_not_exist Azure.Mcp.Tools.ResilienceManagement.Tests.ResilienceManagementCommandTests.Should_reject_recovery_job_action_when_job_does_not_exist
	```

	Those five theory cases verify tool invocation and missing-resource error handling only. They do not replace successful lifecycle coverage, which requires qualified recovery resources, a failed job for retry, and a paused `ManualAction` job for resume.

6. Locate and inspect the generated recordings before publishing:

	```powershell
	./.proxy/Azure.Sdk.Tools.TestProxy.exe config locate -a tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json
	```

7. Set `TestMode` back to `Playback`, rerun the test command, and publish only after playback passes:

	```powershell
	./.proxy/Azure.Sdk.Tools.TestProxy.exe push -a tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json
	```

	Commit the updated `assets.json`; never commit `.proxy/` or `.assets/`.

8. Run the repository spelling check and resolve every issue in changed files rather than adding domain prose typos to the dictionary:

	```powershell
	.\eng\common\spelling\Invoke-Cspell.ps1
	```

### Resilience recording safeguards

- Preserve LRO polling paths by disabling the default `AZSDK2003` sanitizer in `ResilienceManagementCommandTests`; replacing the entire `Location` header breaks playback.
- Sanitize only the signed `t`, `c`, `s`, and `h` query values in LRO locations, while retaining the path and operation identifier used for request matching.
- Sanitize `operation-id` and `x-ms-operation-identifier` headers because they can contain per-run or identity data.
- Register generated names, operation IDs, and job IDs as playback variables when later requests depend on the same value.
- Use a custom matcher only for request-body differences that are intentionally irrelevant; do not hide a contract or payload regression by disabling body comparison broadly.
- Inspect every generated recording for credentials, tenant/object identifiers, signed query values, unstable timestamps, and environment-specific resource paths before pushing assets.

## Result Format

State what was attempted, identify the target, summarize the outcome, and include operation/job IDs. For validation, list qualified and unqualified resources with reasons. For blocked mutations, state the prerequisite that must be corrected.
