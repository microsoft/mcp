# Resilience Operation Workflow Recipes

<!-- cspell:words reprotect reprotection -->

Use these recipes after resolving the exact service group and target names. The [tool reference](./tools.md) remains authoritative for parameter spelling.

## Usage Plan Enrollment

1. Get or create the usage plan with `mcp_azure_mcp_ser_resilience_usageplan_get` or `mcp_azure_mcp_ser_resilience_usageplan_create`.
2. If creation times out after 10 minutes, get the usage plan and verify its provisioning state before retrying or creating an enrollment.
3. Verify the target service group name.
4. Create the association with `mcp_azure_mcp_ser_resilience_usageplan_enrollment_create`.
5. Get the enrollment and report its provisioning state and errors.

## Delete a Usage Plan Enrollment

1. Get the enrollment with `mcp_azure_mcp_ser_resilience_usageplan_enrollment_get` and confirm the exact resource group, usage plan, and enrollment name.
2. Call `mcp_azure_mcp_ser_resilience_usageplan_enrollment_delete` only for an explicit, unambiguous request; deletion permanently removes the service group association but does not delete the usage plan.
3. Report `deleted: true` when the enrollment was removed and `deleted: false` when it was already absent.

## Delete a Usage Plan

1. Get the usage plan with `mcp_azure_mcp_ser_resilience_usageplan_get` and confirm the exact resource group and name.
2. Call `mcp_azure_mcp_ser_resilience_usageplan_delete`. A request to delete the parent plan does not authorize deleting its enrollments.
3. If deletion is blocked by dependent enrollments, list them with `mcp_azure_mcp_ser_resilience_usageplan_enrollment_get`, report every exact enrollment name, and ask for explicit confirmation before removing any association. Do not delete enrollments until confirmation is received.
4. After confirmation, delete only the listed enrollments by exact name with `mcp_azure_mcp_ser_resilience_usageplan_enrollment_delete`. Stop on any failure and report which associations were removed and which remain; do not claim the plan was deleted.
5. When all confirmed enrollment deletions succeed, retry the idempotent parent deletion once with `mcp_azure_mcp_ser_resilience_usageplan_delete`.
6. Get the usage plan again. Treat `ResourceNotFound` as confirmation that the requested parent plan is gone; do not report success based only on enrollment deletion.
7. Report `deleted: true` when the plan was removed and `deleted: false` when it was already absent.

The mention of a service group supplies context for resolving a usage plan; it does not change the target from the parent plan to an enrollment. Use the enrollment-only workflow only when the user asks to unenroll a service group, remove an association, or explicitly retain the usage plan.

## Inspect Goals and Participation

1. List templates with `mcp_azure_mcp_ser_resilience_goal_template_get`.
2. List assignments with `mcp_azure_mcp_ser_resilience_goal_assignment_get`.
3. For an assignment, list members with `mcp_azure_mcp_ser_resilience_goal_resource_get`.
4. Get individual members when full HA/DR attestation, participation, or exclusion details are needed.

These tools do not create or mutate goals.

## Create and Configure a Drill

1. Collect drill type, region, supporting-resource subscription, RBAC setup mode, and optional recovery plan.
2. Call `mcp_azure_mcp_ser_resilience_drill_create`.
3. Get the drill and verify provisioning state.
4. List targets with `mcp_azure_mcp_ser_resilience_drill_resource_get`.
5. Include, update, or exclude targets with `mcp_azure_mcp_ser_resilience_drill_resource_add-or-update`; require a positive fault duration and at least one resource payload.
6. Re-read affected targets and verify their inclusion and fault configuration.
7. Do not start until provisioning and target state permit execution.

## Execute a Drill

1. Get the drill and confirm it is not already running.
2. Run `mcp_azure_mcp_ser_resilience_drill_check-resync-readiness` when configuration may be stale or a current readiness result is required; retain the operation ID and do not treat acceptance as readiness.
3. Validate with `mcp_azure_mcp_ser_resilience_drill_validate-for-execution` using the intended physical source locations.
4. Stop and report blockers when validation does not qualify the drill for execution.
5. Ask for `Failover` versus `TestFailover` if not explicit.
6. Start with `mcp_azure_mcp_ser_resilience_drill_start` and retain the operation ID.
7. Inspect runs with `mcp_azure_mcp_ser_resilience_drill_run_get` when status is requested.
8. Inspect run targets with `mcp_azure_mcp_ser_resilience_drill_run_resource_get` when per-resource results are needed.
9. Use `mcp_azure_mcp_ser_resilience_drill_run_mark-complete` only when the user explicitly wants to stop retries for the named active stage and allow the run to proceed.
10. End only a running drill with `mcp_azure_mcp_ser_resilience_drill_end`; require outcome and notes.

## Create a Recovery Plan

1. Require plan name, `Zonal` plan type, description, and identity choice.
2. For `UserAssigned` or `SystemAndUserAssigned`, require the full identity ARM ID.
3. Build group/action JSON using [payload reference](./payloads.md).
4. Create the plan with `mcp_azure_mcp_ser_resilience_recoveryplan_create`.
5. Get it and verify provisioning state, identity, group ordering, and actions.
6. Configure resource membership separately.

For an existing plan, read it first and preserve omitted values. If it retains a user-assigned identity, reuse the exact current identity resource ID.

## Include or Reconfigure Recovery Resources

1. List plan members with `mcp_azure_mcp_ser_resilience_recoveryplan_resource_get`.
2. Get every intended target and capture its full recovery-resource ID, unique ID, inclusion state, existing solution type, and settings.
3. Build sparse resource updates. For first inclusion/re-inclusion, supply the selected protection solution and complete required settings.
4. Call `mcp_azure_mcp_ser_resilience_recoveryplan_resource_update`.
5. Get each affected resource and verify inclusion and protection configuration.
6. Distinguish:
   - Include: participates in recovery.
   - Exclude: remains a plan member but does not participate.
   - Remove: no longer a plan member.

## Check Readiness

1. Call `mcp_azure_mcp_ser_resilience_recoveryplan_checkreadiness`.
2. Preserve the operation/job identifiers.
3. If results are asynchronous, inspect recovery jobs.
4. Report readiness per resource, including permission, identity, runbook, protection, and configuration blockers.
5. Do not treat an accepted check as a ready plan.

## Validate and Start Failover

1. Require source locations, selected recovery-resource IDs, or both. Never derive selectors from plan metadata.
2. Check readiness if no current readiness result exists.
3. Call `mcp_azure_mcp_ser_resilience_recoveryplan_validateforfailover` with the exact selectors and consent intended for execution.
4. Partition the result into qualified and unqualified resources and report every blocking reason.
5. If any user-selected resource is unqualified, stop and ask whether the user wants to correct blockers or explicitly change the selector set.
6. If selectors change, validate again.
7. Call `mcp_azure_mcp_ser_resilience_recoveryplan_failover` with the validated selectors.
8. Return operation and recovery job IDs; monitor through recovery-job get when requested.

## Validate and Start Reprotect

1. Get the plan and relevant recovery resources after failover.
2. Call `mcp_azure_mcp_ser_resilience_recoveryplan_validateforreprotect` with selected IDs, or omit IDs to evaluate all qualified resources.
3. Report per-resource eligibility and blockers.
4. Stop when no resource qualifies.
5. Call `mcp_azure_mcp_ser_resilience_recoveryplan_reprotect` with the same selected IDs.
6. Return operation and recovery job IDs.

## Validate Another Recovery Operation

1. Require one of `Failover`, `FailoverCommit`, `Reprotect`, `TestFailover`, or `TestFailoverCleanup`.
2. Call `mcp_azure_mcp_ser_resilience_recoveryplan_validateforoperation`.
3. Report state, readiness, and permission blockers.
4. This tool validates only; it does not execute the operation.

## Finalize Current Plan Operation

1. Get the plan and verify that an operation is awaiting finalization.
2. Call `mcp_azure_mcp_ser_resilience_recoveryplan_finalize`.
3. Return the operation ID and terminal plan state. A successful result is completed because the tool waits for the ARM operation and validates the terminal plan state before returning.
4. Do not describe finalize as failover commit.

## Monitor, Retry, or Resume a Recovery Job

1. Get the job with `mcp_azure_mcp_ser_resilience_recoveryjob_get`.
2. For target-level details, use `mcp_azure_mcp_ser_resilience_recoveryjob_resource_get`.
3. Retry only when state is `Failed` using `mcp_azure_mcp_ser_resilience_recoveryjob_retry`.
4. Resume only when state is `Paused` using `mcp_azure_mcp_ser_resilience_recoveryjob_resume`.
5. Collect paused-action input instead of inventing a resume description.
6. Return the new operation ID and preserve the recovery job identity.

## Delete a Drill or Recovery Plan

1. Get the exact named target.
2. Check active execution state and dependent resources.
3. End a running drill before deletion; drill delete does not stop execution.
4. Do not delete a plan with active recovery operations.
5. Invoke the matching delete tool only after the user has clearly requested deletion of that target.
6. Report whether the target existed and the returned deletion result.
