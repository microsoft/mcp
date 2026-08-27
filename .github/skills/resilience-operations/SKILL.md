---
name: resilience-operations
description: 'Implement or extend any Azure Resilience Management MCP operation. USE WHEN: add resilience command, goal, goal assignment, usage plan, enrollment, recovery plan, recovery resource, recovery job, retry, resume, failover, finalize, reprotect, drill, drill run, validation, readiness, create, update, delete, list, or get operation.'
argument-hint: 'Describe one Resilience Management operation, for example "recovery plan failover" or "drill run get"'
---

<!-- cspell:words reprotect -->

# Resilience Operations

Implement one command at a time in
`tools/Azure.Mcp.Tools.ResilienceManagement`. This skill specializes the shared
[`add-azure-mcp-tools`](../add-azure-mcp-tools/SKILL.md) workflow for the
Resilience Management resource hierarchy and stateful recovery domain.

Start with the [operation catalog](./references/operation-catalog.md). It maps
the current command groups, operation kinds, and verified pending recovery
actions. The catalog is the extension point for future resources and actions.

## 1. Classify the Operation

Map the request along three axes before editing:

1. Domain: goal, usage plan, recovery, or drill.
2. Resource level: parent, child resource, job, run, or target resource.
3. Kind: read, validation, create/update, delete, or execution.

If the operation is absent from the catalog, verify the installed
`Azure.ResourceManager.ResilienceManagement` SDK resource, async method, request
model, response type, and service semantics before adding a row. Do not infer a
contract from a similarly named operation.

Follow the repository rule of one tool per pull request. A request for several
operations is a sequence of independently validated command slices.

## 2. Apply Safety Metadata

Derive command metadata from behavior, not from the resource family:

| Kind | Destructive | ReadOnly | Idempotent |
|---|---:|---:|---:|
| List/get | `false` | `true` | `true` |
| Validation/readiness | `false` | Verify contract | Usually `true` |
| Create/update | Verify contract | `false` | Verify PUT/action semantics |
| Delete | `true` | `false` | Usually `true` |
| Recovery execution/job control | `true` | `false` | Usually `false` |

Confirm every value against the service contract and neighboring commands.
Validation that creates an operation record is not automatically read-only.

For every kind:

- Treat all inputs as untrusted and use existing resource-specific validators.
- Never infer tenant, subscription, service group, parent resource, selected
  resources, source location, direction, consent, job, run, or action input.
- Pass tenant, retry options, and `CancellationToken` through every layer.
- Use typed SDK clients and models; never construct ARM action URLs.
- Log only individually named safe identifiers, never request objects or raw
  backend bodies.
- Return sanitized errors through `HandleException(context, ex)`.

For execution and job-control commands, never invoke the operation while
researching, generating tests, or evaluating descriptions. Live execution
requires explicit intent and dedicated disposable resources.

## 3. Select the Nearest Pattern

Use `ResilienceManagementSetup` to locate the owning command group, then read one
neighbor of the same operation kind:

- list/get: the nearest parent or child `GetCommand`
- create/update: the nearest command with the same ARM scope
- delete: `RecoveryPlanDeleteCommand` or `DrillDeleteCommand`
- validation/readiness: the nearest recovery-plan validation command
- recovery execution: the verified recovery-action catalog entry
- job/run action: the nearest job/run get command plus the SDK action contract

Also inspect `IResilienceManagementService`, `ResilienceManagementService`,
`ResilienceManagementJsonContext`, the matching unit tests, and the recorded
command tests. Read no broader surface unless the selected contract remains
ambiguous.

## 4. Implement the Vertical Slice

Create or update only what the selected operation needs:

1. A flat sealed options class under the matching `Options` hierarchy.
2. A dedicated result model when returning typed data or tracking identifiers.
3. A service interface method with `CancellationToken` last.
4. A service implementation using the verified SDK method and request model.
5. A sealed command with a new stable metadata GUID and operation-specific
   validation and sanitized errors.
6. JSON source-generation registrations for all response models.
7. DI registration and placement in the existing command group.

Prefer established model mappers and helpers. Add a shared helper only when at
least two commands have identical mapping, LRO, polling, or validation behavior.

For long-running actions, return a trackable operation, job, or run identifier
instead of holding the MCP request open for an unbounded workflow. Generate a
fresh operation GUID unless the contract explicitly requires a customer-supplied
idempotency value. Use bounded timeouts only for contractually bounded phases.

## 5. Test According to Risk

Command unit tests must cover valid binding, exact service arguments, required
identifiers, semantic validation, typed serialization, cancellation, and
operation-specific errors. Service tests must cover typed request mapping and
response/LRO mapping with behavioral assertions.

Scale recorded live coverage by kind:

- reads: list/get against deployed resources
- validation: qualified and blocked outcomes without executing recovery
- create/update/delete: isolated lifecycle with deterministic cleanup
- execution/job control: explicit prerequisite state, returned tracking ID, and
  status polling without depending on test order or arbitrary existing jobs

Use `RecordedCommandTestsBase`, update and publish `assets.json`, and prove
playback passes. Never weaken assertions to accommodate recordings.

After the first implementation edit, run the narrow command test immediately.
Then run the full ResilienceManagement test project in playback mode.

## 6. Complete Product Surfaces

For every new or changed command:

- update `servers/Azure.Mcp.Server/docs/azmcp-commands.md`
- update `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
- update the relevant README/discovery text
- update `servers/Azure.Mcp.Server/src/Resources/consolidated-tools.json`
- add a schema-valid changelog entry when user-facing
- evaluate changed tool descriptions with positive and neighboring negative
  prompts and meet the repository threshold

Descriptions must distinguish reads, validation, configuration, and execution.
State required identifiers, side effects, prerequisites, and tracking output.

## 7. Validate and Review

Run from narrowest to broadest:

1. selected command unit tests
2. ResilienceManagement playback tests
3. `dotnet build tools/Azure.Mcp.Tools.ResilienceManagement/src`
4. `./eng/scripts/Build-Local.ps1 -VerifyNpx`
5. `./eng/scripts/Build-Local.ps1 -BuildNative`
6. `./eng/common/spelling/Invoke-Cspell.ps1`

Review metadata, AOT registrations, cancellation, state-specific errors,
secret-safe logging, recording completeness, documentation, and generated-code
boundaries.

## Extending the Catalog

For a new operation or resource family:

1. Add its command group and resource level to the hierarchy table if new.
2. Add one operation row with SDK and request-model facts.
3. Add state-transition and live-test notes only when they differ from its kind.
4. Reuse this workflow unchanged.

Split into a separate skill only when a future domain requires materially
different safety, tooling, or lifecycle gates.