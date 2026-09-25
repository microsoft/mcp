# Resilience Management Implementation Guide

## Choose the Command Base

| Scope | Base | Options contract | Unit-test base | Existing anchors |
|---|---|---|---|---|
| Tenant or service group | `AuthenticatedCommand<TOptions, TResult>` | Usually `Tenant`, `ServiceGroup`, then resource hierarchy | `CommandUnitTestsBase<TCommand, IResilienceManagementService>` | `DrillGetCommand`, `DrillCreateCommand`, `RecoveryPlanGetCommand` |
| Subscription and resource group | `SubscriptionCommand<TOptions, TResult>` | `ISubscriptionOption`; inject `ISubscriptionResolver` | `SubscriptionCommandUnitTestsBase<TCommand, IResilienceManagementService>` | `UsagePlanGetCommand`, `UsagePlanCreateCommand` |

Do not force service-group commands into `SubscriptionCommand` merely because an operation body contains a subscription ID. The command's lookup/authorization scope controls the base class.

## File and Naming Pattern

- Command: `src/Commands/<Hierarchy>/<Resource><Operation>Command.cs`.
- Options class: `<Resource><Operation>Options`; local filename convention is `<Resource><Operation>Option.cs`.
- Models and enums: one type per file under `src/Models`.
- Service contract: `src/Services/IResilienceManagementService.cs`.
- Service implementation: `src/Services/ResilienceManagementService.cs`.
- JSON source generation: `src/Commands/ResilienceManagementJsonContext.cs`.
- Registration: `src/ResilienceManagementSetup.cs`.
- Unit test: mirror the command hierarchy under the test project.
- Live test: add a focused method to `ResilienceManagementCommandTests.cs`.

Use a new GUID for `CommandMetadata.Id`. Command groups are singular concatenated lowercase names; operation names may use dashes, such as `mark-complete`.

## Validation and Security

- Call `base.ValidateOptions` first.
- Validate each service-group/resource name against its actual contract, including path-segment restrictions.
- Cap JSON payload byte size before parsing and reject malformed or incompatible shapes.
- Prefer enums for closed value sets such as drill type, RBAC mode, or run stage.
- Never log an options object. Log only known-safe identifiers.
- Always call `HandleException(context, ex)`.
- Preserve `CancellationToken` as the last async parameter and propagate it to every SDK/REST call.
- Construct ARM IDs with `ResourceIdentifier` or verified SDK collection methods where possible.
- Keep commands stateless and transport agnostic.

## Service and Response Patterns

The service currently inherits `BaseAzureResourceService`, which also provides the `BaseAzureService` capabilities needed for direct ARM operations. Use `CreateArmClientAsync` with the request tenant for tenant-scoped resources.

For list/get commands, preserve the local dual-result convention: a list of `ResourceSummary` values when the resource name is omitted and one detailed model when it is supplied.

For LROs:

1. Start with `WaitUntil.Started`.
2. Await `WaitForLroCompletionAsync`.
3. If the RP returns the request echo or incomplete `operation.Value.Data`, issue a follow-up collection `GetAsync`.
4. Map the hydrated resource.

Generated ARM models can omit read-only server properties in wire format. The confirmed drill-create failure was caused by serializing `ResilienceManagementDrillData` with `ModelReaderWriterOptions("W")`; full JSON format `"J"` preserves `id`, `name`, `type`, `systemData`, provisioning state, chaos resource ID, and monitoring properties. Treat empty IDs after a successful PUT as a hydration/mapping problem before blaming the recording harness.

## Metadata Semantics

- GET/list: `ReadOnly = true`, `Destructive = false`, normally `Idempotent = true`.
- PUT create-or-update: mutating and usually idempotent, but verify service semantics.
- POST action: usually mutating and non-idempotent unless the backend contract proves otherwise.
- DELETE marked idempotent must treat an already-absent resource as success; do not return 404 as failure while advertising `Idempotent = true`.
- Ensure consolidated tool metadata is compatible with every mapped command. A mismatched destructive/read-only aggregate can prevent registration or fail discovery tests.

## Registration and Discovery

Complete all of these in the same change:

1. Add the command as a singleton in `ConfigureServices`.
2. Add it at the correct `CommandGroup` node in `RegisterCommands`.
3. Register result/model types in `ResilienceManagementJsonContext` in sorted order.
4. Map the emitted tool name in `consolidated-tools.json` under the intent that matches its metadata.
5. Update the consolidated description when adding a new capability.
6. Verify the real emitted name from the built server; do not infer it only from class names.

## Test Expectations

Unit tests should cover:

- Command name and description.
- Required and optional argument binding through command-line strings.
- Semantic validation boundaries and malformed payloads.
- Correct service arguments, including tenant and cancellation token.
- Response deserialization through `ResilienceManagementJsonContext`.
- Service errors and meaningful 403/404/409 behavior where applicable.
- Idempotent repeat behavior for delete/update operations.

Recorded tests should assert stable domain behavior: returned IDs/names, provisioning state, operation IDs, selected run stage, or list membership. Structural presence alone is insufficient when a stable value can be asserted.