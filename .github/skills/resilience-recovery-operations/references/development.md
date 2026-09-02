# Resilience Management Tool Development

<!-- cspell:words reprotect -->

Follow [add-azure-mcp-tools](../../add-azure-mcp-tools/SKILL.md) as the canonical command, options, service, security, registration, AOT, documentation, changelog, testing, recording, and pull-request workflow. This reference contains only Resilience Management-specific requirements and overrides.

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
- Start SDK long-running operations with `WaitUntil.Started` and wait through the shared bounded helper. Link a defined timeout with caller cancellation; do not rely on the caller to cancel an otherwise unbounded operation.
- Register every response type in `ResilienceManagementJsonContext` and every command in `ResilienceManagementSetup`.
- Return explicit typed response contracts. Do not expose raw `JsonElement`, `JsonDocument`, `BinaryData`, `object`, or unstructured dictionaries when the response schema is known.
- Validate ManualAction, CustomRunbook, Azure Site Recovery, action timeout, JSON structure, ARM resource ID, selector, identity, tenant, and subscription requirements before service calls. Use [payloads.md](./payloads.md) for service-specific schemas and conditional fields.
- Preserve the established `recoveryplan` option spelling and released tool names, result properties, metadata IDs, and recorded test identifiers unless an explicit migration updates every dependent mapping, document, prompt, and recording.
- Use Azure SDK retry defaults. Do not expose `RetryPolicyOptions`, construct ad hoc credentials, use `CancellationToken.None`, access `HttpContext`, branch on transport, or retain request state in command fields.
- Test the real lifecycle state required by retry and resume. Retry requires a failed job; resume requires a paused `ManualAction` job.

When implementation depends on SDK contracts, service behavior, or portal workflows, add only the relevant Azure SDK, service, or portal repositories to the Visual Studio Code multi-root workspace.

## Build and Recorded Tests

Use [recorded-tests.md](../../../../docs/recorded-tests.md) for the canonical recording workflow. Run these Resilience Management commands from the repository root.

Authenticate to the Microsoft tenant:

```powershell
Connect-AzAccount -TenantId 72f988bf-86f1-41af-91ab-2d7cd011db47
```

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
- Register generated names, operation IDs, and job IDs as playback variables when later requests depend on the same value.
- Use a custom matcher only for intentionally irrelevant request-body differences. Do not hide contract or payload regressions by disabling body comparison broadly.
- Inspect recordings for credentials, tenant or object identifiers, signed query values, unstable timestamps, and environment-specific resource paths before publishing.