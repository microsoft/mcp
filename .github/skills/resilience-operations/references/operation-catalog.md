<!-- cspell:words reprotect -->

# Resilience Operation Catalog

This catalog organizes all Azure Resilience Management command surfaces. Verify
SDK signatures whenever the `Azure.ResourceManager.ResilienceManagement`
package version changes.

## Resource Hierarchy

| Domain | Command path | Resource levels | Current operation kinds |
|---|---|---|---|
| Goals | `resilience goal` | template, assignment, resource | get |
| Usage plans | `resilience usageplan` | plan, enrollment | get, create |
| Recovery | `resilience recoveryplan` | plan, resource | get, create, update, delete, validation, readiness |
| Recovery jobs | `resilience recoveryjob` | job, resource | get; execution extensions pending |
| Drills | `resilience drill` | drill, resource, run, run resource | get, create, update, delete |

When introducing a new level, add its `CommandGroup` in
`ResilienceManagementSetup` and retain singular, concatenated lowercase names.

## Operation Registry Fields

Every new operation row should record:

| Field | Purpose |
|---|---|
| Intent | Unambiguous user goal |
| MCP command | Exact command hierarchy and operation name |
| Kind | Read, validation, create/update, delete, or execution |
| Command class | Repository class name |
| SDK surface | Typed resource and async method |
| Request model | Typed SDK request or `None` |
| Preconditions/result | Required state and useful output/tracking ID |

Existing commands remain the source of truth for rows not yet written here. Add
a detailed row when implementing or materially changing an operation.

## Verified Recovery Execution Operations

| Intent | MCP command | Kind | Command class | SDK surface | Request model |
|---|---|---|---|---|---|
| Recovery plan failover | `resilience recoveryplan failover` | Execution | `RecoveryPlanFailoverCommand` | `RecoveryPlanResource.FailoverAsync` | `ResilienceManagementFailoverContent` |
| Recovery plan finalize | `resilience recoveryplan finalize` | Execution | `RecoveryPlanFinalizeCommand` | `RecoveryPlanResource.FinalizeAsync` | None |
| Recovery plan reprotect | `resilience recoveryplan reprotect` | Execution | `RecoveryPlanReprotectCommand` | `RecoveryPlanResource.ReprotectAsync` | `ReprotectContent` |
| Recovery job retry | `resilience recoveryjob retry` | Execution | `RecoveryJobRetryCommand` | `RecoveryJobResource.RetryAsync` | None |
| Recovery job resume | `resilience recoveryjob resume` | Execution | `RecoveryJobResumeCommand` | `RecoveryJobResource.ResumeAsync` | `RecoveryActionContent` |

All five SDK methods accept `WaitUntil`, a caller-generated operation ID, and a
`CancellationToken`. Methods with a request accept it between the operation ID
and cancellation token.

### Failover

`ResilienceManagementFailoverContent` requires `FailoverDirection` and may carry
`FailoverRequestProperties`. Verify the installed SDK types before exposing
fields. Require explicit direction, source locations, selected resources, and
consent whenever required by the chosen scenario.

Precondition: finalized, ready, qualified plan. Result: trackable operation or
recovery job.

### Finalize

Finalize has no request body. It validates configuration and transitions an
editable plan toward ready. It is not failover commit.

Precondition: configured editable plan. Result: trackable operation and updated
plan/readiness state or actionable blockers.

### Reprotect

`ReprotectContent` exposes `ReprotectRequestSelectedResourceIds`. Omitted or
empty selections must follow the service contract; never invent a selection from
prior output.

Precondition: qualified post-failover state. Result: trackable operation or
recovery job.

### Retry

Retry has no body and acts on one recovery job. Require service group, recovery
plan, and job. Report active, successful, paused, and otherwise non-retryable
states clearly.

Precondition: retryable failed job. Result: tracking information for the new
attempt.

### Resume

`RecoveryActionContent` has an optional `Description`; validate its service
length limit and never log it. Require service group, recovery plan, and job.

Precondition: job paused for user intervention. Result: tracking information for
the resumed attempt.

## Adding Any Operation

Append a registry row after verifying the installed SDK and service contract.
Document unique required fields, valid source states, state transition, tracking
result, and live-test setup. If those semantics match an existing operation kind,
reference the shared workflow instead of duplicating it here.