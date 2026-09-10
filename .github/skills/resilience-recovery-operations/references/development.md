# Resilience Management Tool Development

<!-- cspell:words reprotect reprotection resync zonal -->

Follow [add-azure-mcp-tools](../../add-azure-mcp-tools/SKILL.md) as the canonical command, options, service, security, registration, AOT, documentation, changelog, testing, recording, and pull-request workflow. This reference contains only Resilience Management-specific requirements and overrides.

## Automated Delivery State Machine

Use this workflow when a developer names a Resilience Management API and asks the skill to deliver the MCP tool. Continue autonomously between gates and persist enough state in the branch and PR for a later session to resume safely.

### State 0: Intake gate

Before editing code or creating Azure resources, collect:

- the API/operation name and authoritative contract or SDK surface;
- the test subscription and tenant;
- existing service group, recovery plan, drill, resource group, identities, runbooks, jobs, or other reusable fixtures;
- permission to create missing test resources in that subscription;
- the PR target branch and linked issue, when known.

Resolve names and inspect existing resources after the user supplies this context. Do not choose a subscription or repurpose an ambiguous resource. Once resource creation is approved at intake, create missing live-test resources automatically; do not ask separately for every resource declared by the reviewed test fixture.

### State 1: Research and implement

1. Inspect the generated SDK method, request model, result model, LRO behavior, and API version.
2. Select the closest existing Resilience Management command and test as the implementation reference.
3. Implement options, command, service contract, service logic, typed results, JSON context, registration, consolidated mapping, documentation, prompts, changelog, and tests.
4. Build and run focused tests after each implementation slice. Fix failures before widening validation.
5. Review the complete diff for security, AOT safety, compatibility, maintainability, and one-tool-per-PR scope.

### State 2: Live test and record

1. Deploy or update the approved test fixture automatically.
2. Run the successful live lifecycle, not only not-found invocation tests.
3. Diagnose service and lifecycle failures from operation status and resource state; update implementation or fixture as required.
4. Record the test, inspect sanitization, publish the complete recording asset set, return to Playback, and run the full Resilience Management test project.
5. Run required build, formatting, spelling, consolidated-mode, documentation, and ToolDescriptionEvaluator gates. Every Resilience prompt must rank `#1` with score at least `0.6`.

### State 3: Publication gate

Prepare the branch, commit set, PR title, and PR body, including test evidence, recording evidence, evaluator scores, security notes, and the repository-required live-test invocation section. Show the final diff and validation summary and obtain explicit approval before the first push and PR creation.

After approval, create or update the branch and open the PR. Include this machine-readable manifest marker exactly once in the PR body so the scheduled conflict monitor can identify it:

```html
<!-- resilience-tool-delivery:v1 -->
```

Do not remove the marker while the PR is open. Do not auto-merge.

### State 4: PR maintenance loop

Whenever the workflow remains active or is resumed:

1. Locate the existing PR and target branch; fetch both before making new changes.
2. Inspect PR mergeability, required checks, review threads, and new target-branch commits.
3. If the branch has an unambiguous merge conflict, resolve it while preserving both the tool's intent and upstream changes, run affected tests plus required validation, commit the resolution, and push it to the existing PR.
4. Never resolve a conflict by deleting unrelated upstream functionality, recordings, tests, documentation, or registration. Never use destructive reset/checkout operations or shared-worktree `git stash`.
5. If a conflict exposes incompatible API contracts, generated-code changes, test fixtures, or product behavior, stop and ask the developer to choose between the competing intents.
6. Handle Agency/Copilot/bot review comments autonomously: verify the finding, implement valid fixes, test, push, and respond with evidence. Explain and close invalid automated findings with evidence.
7. For a human review comment, prepare the fix locally, run validation, and show the draft diff and proposed response. Wait for developer approval before pushing that fix to the existing PR.
8. Re-check mergeability after every push, target-branch update, automated review fix, human-approved review fix, and CI completion.
9. If checks fail, inspect logs, fix failures caused by the PR, rerun the narrow check, and push the repair. Report unrelated infrastructure failures without changing unrelated code.

Do not use fixed-interval terminal polling or keep a sleeping process alive. During an active session, check at workflow transitions. After a session ends, resume when invoked again and begin with the mergeability/comment/check inspection above. True unattended periodic monitoring must be implemented separately with repository automation.

The repository workflow `.github/workflows/resilience-merge-conflict-monitor.yml` provides that unattended detection every three hours. It scans only open PRs containing the delivery manifest marker, applies the `resilience-merge-conflict` label, maintains one status comment, and removes the label when the conflict clears. It detects conflicts but does not edit PR branches; resume this skill to perform semantic conflict resolution. Scheduled workflows run only from the repository default branch and must be enabled in the repository.

### State 5: Completion

The workflow is complete only after required checks pass, required approvals are present, no unresolved review threads or merge conflicts remain, and a human approver merges the PR. Report the merged PR and final validation state. The skill must not perform the merge itself.

## Patterns for Future Drill Tools

Choose the implementation and result contract for a new drill tool from the operation category below. Use the existing command named in the second column as a local reference.

| Area | Reference commands | Required contract pattern |
|---|---|---|
| Drill definition | `get`, `create`, `update`, `delete` | List-or-get, completed create/update/delete |
| Drill preparation | `check-resync-readiness`, `validate-for-execution` | Accepted asynchronous operation with operation ID |
| Drill execution | `start`, `end` | Accepted asynchronous operation with operation ID and `Accepted` status |
| Drill resources | `resource get`, `resource add-or-update` | List-or-get, validated JSON mutation |
| Drill runs | `run get`, `run add-notes`, `run failover`, `run resume`, `run mark-complete`, `run reprotect` | List-or-get and state-gated accepted actions |
| Drill-run resources | `run resource get` | List-or-get under the full drill/run hierarchy |

Implement future drill tools in these paths:

- Commands: `tools/Azure.Mcp.Tools.ResilienceManagement/src/Commands/Drills/`
- Options: `tools/Azure.Mcp.Tools.ResilienceManagement/src/Options/Drills/`
- Typed results: `tools/Azure.Mcp.Tools.ResilienceManagement/src/Models/`
- Service contract and implementation: `IResilienceManagementService.cs` and `ResilienceManagementService.cs`
- Unit and recorded tests: `tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/`

## Future Tool Checklist

A Resilience Management command is not complete when only its command class exists. Inspect and update every applicable surface:

1. Add a flat options class with `[Option]` attributes. Use `subscription`, `resource-group`, `service-group`, `recoveryplan`, `drill`, and `drill-run` consistently with released tools.
2. Add a sealed command class using a primary constructor and the correct generic command base.
3. Add or update the service interface and implementation. Keep Azure SDK calls out of the command.
4. Add a typed result model or nested command result and register it in `ResilienceManagementJsonContext` for AOT serialization.
5. Register the command for dependency injection and in the correct command-group hierarchy in `ResilienceManagementSetup`.
6. Add focused command unit tests for constructor metadata, required options, semantic validation, success serialization, exact service arguments, cancellation propagation, and sanitized service errors.
7. Add a successful recorded test that proves the real Azure contract. A not-found test proves invocation only, not lifecycle correctness.
8. Update test resources and post-deployment setup when the operation needs a new state, identity, resource, role assignment, failed job, paused job, or drill stage.
9. Update `servers/Azure.Mcp.Server/docs/azmcp-commands.md`, the toolset/server README where applicable, and at least two prompts in `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`.
10. Inspect `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`. Map the exact registered tool name and make the aggregate `destructive`, `idempotent`, and `readOnly` metadata compatible with every command in the group.
11. Update consolidated-mode tests when registration or consolidated grouping changes. A metadata mismatch can prevent consolidated-mode startup even when command unit tests pass.
12. Add the required changelog entry and record ToolDescriptionEvaluator evidence in the pull request.

Do not hand-edit generated Azure SDK code. If the generated SDK cannot express a required service contract, isolate the narrow workaround in the service layer or an Azure pipeline policy and explain why it exists.

## Command and Result Design

- Use `AuthenticatedCommand<TOptions, TResult>` for service-group-scoped drill, drill-resource, drill-run, and drill-run-resource tools. The `subscription` used to locate supporting resources does not make a drill command subscription-scoped.
- Keep commands transport-agnostic, stateless, and thread-safe. Validate and map options in the command; perform Azure calls in the service.
- Use the established list-or-get shape for `get` tools: omit `name` to return summaries, or provide `name` to return one detailed resource. Do not list first when a caller already supplied a name.
- Return typed, stable output. Accepted operations must return their generated operation ID. Use status words such as `Accepted` only when the method returns before completion.
- Do not report an accepted operation as completed. `ArmOperation.HasCompleted` immediately after `WaitUntil.Started` is informational and is normally `false`.
- Log only known-safe identifiers. Do not log an entire options object or user payload. Convert Azure exceptions through `HandleException` and return actionable, sanitized messages for conflict, forbidden, not found, and generic request failures.

## Validate Before Azure Calls

Validation must reject malformed input before constructing an SDK request. Unit tests for every rejection must assert that the service was not called.

- Validate every ARM path name as a non-empty single path segment. Apply the stricter service limits when known; do not rely only on slash checks.
- Validate enum-like values against the exact wire values. Important drill values include `Failover`, `TestFailover`, `Success`, `Failed`, `Enable`, `Disable`, `FaultInjection`, `Failover`, and `Reprotect`.
- Validate physical source locations as `<region>-az<positive-zone-number>`, for example `westus2-az1`. A normal Azure location such as `westus2` is not a physical zone selector.
- Validate cross-field rules explicitly. Drill update requires at least one change, and `subscription` and `region` must be supplied together.
- Parse ARM IDs with `ResourceIdentifier` and verify the complete expected hierarchy and resource types. For selected recovery-resource IDs, verify service group ownership, recovery-plan parentage, `recoveryResources` type, and that the leaf `parsed.Name` is a GUID. A syntactically valid ARM ID with a leaf such as `resource1` is invalid for this SDK contract.
- Validate JSON with `JsonDocument`/`Utf8JsonWriter` and deserialize with `ModelReaderWriter`; do not assemble payloads by string concatenation. Enforce object/array shape, required fields, non-empty IDs, enum values, positive durations, and payload-size limits before service invocation.
- For optional selector arrays, distinguish omitted from present-but-empty according to the service contract. Require at least one selector when the operation cannot safely infer a target.

## Azure SDK and Service-Layer Patterns

### Resource identifiers and models

- Use SDK `CreateResourceIdentifier` methods for service-group extension resources instead of concatenating ARM IDs.
- Resolve a subscription name to its ID through `AzureService.GetSubscription`; preserve an already-valid subscription ID.
- Use SDK model factories when generated models expose no public constructor or setter for required values. Use `ArmResilienceManagementModelFactory.RecoveryPlanPropertiesOfDrill` for drill recovery-plan association.
- A Zonal drill create request needs all service-required fields, not merely those enforced by generated model constructors: top-level system-assigned identity, drill type, RBAC setup mode, drill asset subscription/region, chaos identities, and recovery-plan identity/ID when associated. Missing fields can surface only as generic `ResourceCreationValidateFailed` errors.
- After completed create or update, perform a GET and map the hydrated resource. The create LRO value can be a request echo without server-generated properties.
- When serializing an Azure SDK model to map a response, use full JSON format (`new ModelReaderWriterOptions("J")`) rather than wire format (`"W"`). Wire format can omit read-only fields including `id`, `name`, `type`, `systemData`, `provisioningState`, chaos IDs, and monitoring properties.
- When generated model coverage is incomplete, parse the raw response only inside the service and map it to a typed MCP result. Do not leak `JsonDocument`, mutable SDK models, or transport details into commands.

### Long-running operations

Choose waiting behavior from the MCP result contract, not merely from the SDK return type:

| Operation kind | Service behavior | MCP response |
|---|---|---|
| List/get | Await the SDK request | Resource or summaries |
| Drill create/update/delete | Start with `WaitUntil.Started`, then use the shared bounded LRO helper; re-GET after create/update | Completed result |
| Readiness, validation, and resource mutation | Start with `WaitUntil.Started` | Operation ID and current completion flag |
| Drill start/end and drill-run actions | Start with `WaitUntil.Started` and do not poll in the command | Operation ID plus accepted status/context |

Use the shared bounded LRO helper when waiting is part of the contract. Propagate caller cancellation and the defined timeout. Do not use `CancellationToken.None`, custom polling loops in command classes, or configurable Azure SDK retry policies.

### Operation IDs

Generate an operation ID once per accepted action, pass that exact value to the SDK, and return the same value in the MCP result. Tests must arrange a known operation ID and assert exact serialization, not merely non-empty output.

The Drills backend uses operation IDs for asynchronous work. Before implementing a new asynchronous action, inspect the generated SDK request and service contract to determine whether the ID is required in the `operation-id` header, the `operationId` query parameter, or both. When both are required but the SDK emits only the header, use `CreateArmClientOptionsWithOperationIdPolicy` and `OperationIdQueryParameterPolicy`. The policy mirrors the existing header value into the query only when absent. Do not apply the policy without verifying the new operation's generated request.

Never generate independent header and query IDs in MCP code. Preserve the returned ID for diagnostics and subsequent status correlation.

## Drill Lifecycle and State Gates

Successful HTTP acceptance is not proof that a drill action finished. Diagnose each action through its own asynchronous operation status and then inspect the drill run for the next allowed verb.

Use this sequence when a future tool participates in the manual failover/reprotect lifecycle:

1. Create and prepare an isolated drill and include its resources.
2. Run readiness/resync and validation as required.
3. Start the drill in `Failover` mode and preserve its operation ID.
4. Start drill-run failover with physical source locations and `auto-failover` disabled when testing manual stage progression.
5. Wait until `FaultInjection` offers `MarkAsComplete`; call mark-complete with the exact wire value `FaultInjection`.
6. Wait until `Failover` offers `Start`; call resume.
7. Wait until `Failover` offers `MarkAsComplete`; call mark-complete with `Failover`.
8. Wait until `Reprotect` offers `Start` or `Retry`; call reprotect.
9. End the drill in cleanup if the scenario leaves an active run.

Do not use `Fault` as the mark-complete stage. The service can accept the outer request while the action's operation status later reports an invalid stage; resume then correctly fails because `Failover: Start` was never unlocked.

Do not rely on the drill run's `errorDetails` alone to diagnose an action. Poll or inspect the specific operation-status URL represented by that action's LRO headers, and check each preceding action's own status before attributing a failure to Azure.

`CallToolAsync` returns an error-shaped JSON value for non-success MCP responses; it does not necessarily throw. Retry helpers for transient `InvalidResourceOperation` conflicts must inspect the response shape/status and must only retry operations that are safe to repeat. A `try/catch` around `CallToolAsync` alone does not implement a conflict retry.

For add-notes, an `Accepted` response proves only submission. The recorded test must poll drill-run GET until the note appears to prove persistence.

## Test Fixture Design

- Deploy Zonal drill fixtures to an availability-zone-capable region such as `westus2`. The repository default `westus` cannot host the fixture's zonal managed disk.
- Use `-Unique` when changing regions or creating a clean fixture. Reusing a deterministic resource group created for another region causes deployment conflicts.
- Give each service group/recovery plan/drill lifecycle fixture a dedicated resource group and dedicated resources. Do not share a resource across service-group fixtures; cross-discovery can invalidate recovery qualification.
- Do not re-PUT an existing drill with a different supporting `resource-group`; supporting resources such as the automation account may already exist in the original group.
- Account for the service group's concurrent Zonal drill limit. Use deterministic cleanup and isolated lifecycle service groups so one test cannot block another.
- New service-group role assignments can appear in ARM before authorization is effective. Treat early nested-resource `403` responses as potentially transient, but verify the intended role and scope rather than hiding persistent authorization failures.
- Before the first fault-injection action, verify that required managed-identity role assignments exist and have propagated. Retrying the same action after an initial permission failure can leave a run stuck. End the drill and start a clean run instead of indefinitely replaying an orphaned action.
- Drill resource inclusion and recovery-plan resource inclusion are distinct operations. Build the drill mutation payload from IDs returned by the current drill-resource GET contract; do not invent or infer an ID from an unrelated ARM resource.
- Assert only the readiness conditions required by the operation. Do not make a future tool depend on unrelated monitoring signals unless the service contract requires them.
- Design recorded tests with cleanup in `finally` for mutations that can consume concurrency or block later tests.

## Unit and Recorded Test Expectations

Focused command unit tests should cover:

- command name, description, and metadata;
- every required option and conditional option combination;
- path traversal, malformed enums, invalid physical zones, malformed JSON, wrong ARM hierarchy, wrong service group, and non-GUID recovery-resource leaves;
- exact SDK-facing service arguments, including selected resource IDs, automatic failover choice, tenant, and cancellation token;
- exact operation ID in the typed command result;
- `DidNotReceive` for validation failures;
- conflict, forbidden, not-found, generic request failure, and unexpected exception handling without provider-detail leakage.

Recorded coverage for a new action must exercise a successful service state, assert meaningful response fields, and prove any persisted state where applicable. Use list tests for collection shape and get tests for detailed shape. For create/update, assert hydrated server fields such as ID, provisioning state, drill type, RBAC mode, or recovery-plan association rather than echoing input values.

Do not weaken request-body matching to make playback pass. `[CustomMatcher(compareBody: false)]` is acceptable only when an intentionally unstable body has been investigated and response/state assertions still prove the contract.

## Tool Description Evaluation Override

For every new or behaviorally changed Resilience Management tool:

1. Add at least two distinct natural-language prompts to `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`.
2. Run `ToolDescriptionEvaluator` against every added prompt.
3. Require the expected tool to rank `#1` with a score of at least `0.6` for every prompt.
4. Revise the command description and rerun evaluation until every prompt passes both thresholds.
5. Record the final score and representative prompts in the pull request description.

This gate intentionally overrides the generic top-three and `0.4` acceptance threshold. A Resilience Management tool is not ready for review when it has fewer than two prompts or any prompt misses rank `#1` or score `0.6`.

## Architecture Requirements

- Choose the command base by resource scope. Use `SubscriptionCommand<TOptions, TResult>` and `SubscriptionCommandUnitTestsBase` for subscription-scoped usage plans and enrollments. Use `AuthenticatedCommand<TOptions, TResult>` and `CommandUnitTestsBase` for tenant/service-group-scoped goals, drills, recovery plans, recovery resources, and recovery jobs. A supporting-resource `subscription` option does not make a command subscription-scoped.
- Create ARM clients through `CreateArmClientAsync`, and propagate `tenant` and the caller's cancellation token on every request.
- Start SDK long-running operations with `WaitUntil.Started`, then follow the command contract. Acceptance-only operations, including drill start and drill end, must return the operation ID with `Accepted` status without waiting for completion. Operations whose contract returns the completed ARM result must wait through the shared bounded helper, linking a defined timeout with caller cancellation.
- Register every response type in `ResilienceManagementJsonContext` and every command in `ResilienceManagementSetup`.
- Return explicit typed response contracts. Do not expose raw `JsonElement`, `JsonDocument`, `BinaryData`, `object`, or unstructured dictionaries when the response schema is known.
- Validate ManualAction, CustomRunbook, Azure Site Recovery, action timeout, JSON structure, ARM resource ID, selector, identity, tenant, and subscription requirements before service calls. Use [payloads.md](./payloads.md) for service-specific schemas and conditional fields.
- Preserve the established `recoveryplan` option spelling and released tool names, result properties, metadata IDs, and recorded test identifiers unless an explicit migration updates every dependent mapping, document, prompt, and recording.
- Use Azure SDK retry defaults. Do not expose `RetryPolicyOptions`, construct ad hoc credentials, use `CancellationToken.None`, access `HttpContext`, branch on transport, or retain request state in command fields.
- Test the real lifecycle state required by retry and resume. Retry requires a failed job; resume requires a paused `ManualAction` job.

When implementation depends on SDK contracts, service behavior, or portal workflows, add only the relevant Azure SDK, service, or portal repositories to the Visual Studio Code multi-root workspace.

## Build and Recorded Tests

Use [recorded-tests.md](../../../../docs/recorded-tests.md) for the canonical recording workflow. Run these Resilience Management commands from the repository root.

Authenticate with Azure PowerShell using the tenant that contains the test subscription and service group:

```powershell
Connect-AzAccount
```

If the active tenant is not the test tenant, use `Connect-AzAccount -TenantId <tenant-id>`. Ensure the local test settings use the same tenant.

Build the server and referenced toolset:

```powershell
dotnet build servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj
```

Deploy live-test resources to an availability-zone-capable region:

```powershell
./eng/scripts/Deploy-TestResources.ps1 -Paths ResilienceManagement -Location westus2 -Unique
```

The fixture deploys a zonal managed disk, so the default `westus` region fails with `LocationNotSupportAvailabilityZones`. Use `-Unique` to avoid reusing a deterministic resource group from another region. The signed-in principal must have `Microsoft.Authorization/roleAssignments/write` at service-group scope.

Set `TestMode` to `Record` in `tools/Azure.Mcp.Tools.ResilienceManagement/tests/.testsettings.json`, then run:

```powershell
dotnet test --project tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/Azure.Mcp.Tools.ResilienceManagement.Tests.csproj
```

This repository uses .NET SDK 10, Microsoft Testing Platform, and xUnit v3. Use `--project`, and pass MTP options without a `--` separator.

The failover, finalize, reprotect, retry, and resume not-found theory cases verify invocation and missing-resource handling only. They do not replace successful lifecycle coverage with qualified recovery resources, a failed job for retry, and a paused `ManualAction` job for resume.

Before publishing, locate and inspect recordings:

```powershell
./.proxy/Azure.Sdk.Tools.TestProxy.exe config locate -a tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json
```

Return `TestMode` to `Playback`, rerun the tests, and publish only after playback passes:

```powershell
./.proxy/Azure.Sdk.Tools.TestProxy.exe push -a tools/Azure.Mcp.Tools.ResilienceManagement/tests/Azure.Mcp.Tools.ResilienceManagement.Tests/assets.json
```

Commit the updated `assets.json`. Never commit `.proxy/` or `.assets/`.

## Recording Safeguards

- Preserve LRO polling paths by disabling the default `AZSDK2003` sanitizer in `ResilienceManagementCommandTests`; replacing the entire `Location` header breaks playback.
- Sanitize only signed `t`, `c`, `s`, and `h` query values in LRO locations while retaining the path and operation identifier used for request matching.
- Sanitize `operation-id` and `x-ms-operation-identifier` headers because they can contain per-run or identity data.
- Normalize the fresh `operationId` query value with a `UriRegexSanitizer`. Header sanitization alone does not prevent a playback URI mismatch, and recordings captured before URI normalization must be re-recorded.
- Register generated names, operation IDs, and job IDs as playback variables when later requests depend on the same value.
- Preserve recording lineage when publishing a replacement asset tag. Start from the current complete asset set, add or replace the intended recordings, and run the entire Resilience Management suite in playback. Publishing a tag produced from only a focused recording run can silently omit unrelated recordings and break otherwise unchanged tests.
- Treat a test-proxy 404 stating that no recording matched as a recording, sanitizer, matcher, or asset-lineage defect. Do not remove unrelated tests or Azure functionality to make playback green.
- Use a custom matcher only for intentionally irrelevant request-body differences. Do not hide contract or payload regressions by disabling body comparison broadly.
- Inspect recordings for credentials, tenant or object identifiers, signed query values, unstable timestamps, and environment-specific resource paths before publishing.