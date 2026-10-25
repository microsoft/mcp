# Resilience Operation Workflow Recipes

<!-- cspell:words reprotect reprotection -->

Use these recipes after resolving the exact service group and target names. The [tool reference](./tools.md) remains authoritative for parameter spelling.

## Usage Plan Enrollment

1. Get or create the usage plan with `mcp_azure_mcp_ser_resiliency_usageplan_get` or `mcp_azure_mcp_ser_resiliency_usageplan_create`.
2. Verify the target service group name.
3. Create the association with `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_create`.
4. Get the enrollment and report its provisioning state and errors.

## Delete a Usage Plan Enrollment

1. Get the enrollment with `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_get` and confirm the exact resource group, usage plan, and enrollment name.
2. Call `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_delete` only for an explicit, unambiguous request; deletion permanently removes the service group association but does not delete the usage plan.
3. Report `deleted: true` when the enrollment was removed and `deleted: false` when it was already absent.

## Delete a Usage Plan

1. Get the usage plan with `mcp_azure_mcp_ser_resiliency_usageplan_get` and confirm the exact resource group and name.
2. Call `mcp_azure_mcp_ser_resiliency_usageplan_delete`. A request to delete the parent plan does not authorize deleting its enrollments.
3. If deletion is blocked by dependent enrollments, list them with `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_get`, report every exact enrollment name, and ask for explicit confirmation before removing any association. Do not delete enrollments until confirmation is received.
4. After confirmation, delete only the listed enrollments by exact name with `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_delete`. Stop on any failure and report which associations were removed and which remain; do not claim the plan was deleted.
5. When all confirmed enrollment deletions succeed, retry the idempotent parent deletion once with `mcp_azure_mcp_ser_resiliency_usageplan_delete`.
6. Get the usage plan again. Treat `ResourceNotFound` as confirmation that the requested parent plan is gone; do not report success based only on enrollment deletion.
7. Report `deleted: true` when the plan was removed and `deleted: false` when it was already absent.

The mention of a service group supplies context for resolving a usage plan; it does not change the target from the parent plan to an enrollment. Use the enrollment-only workflow only when the user asks to unenroll a service group, remove an association, or explicitly retain the usage plan.

## Inspect Goals and Participation

1. List assignments with `mcp_azure_mcp_ser_resiliency_goal_assignment_get`.
2. For an assignment, list members with `mcp_azure_mcp_ser_resiliency_goal_resource_get`.
3. Get individual members when nested `zonalResiliency` participation, attestation, exclusion reason, or user confirmations are needed. Use `properties.resourceArmId` to identify the underlying Azure resource.
4. Assignment GET with `name` returns its zonal resiliency requirement, service-level indicator targets, and provisioning state; without `name`, it returns summaries. Preview-only HA/DR goal fields are not returned; do not substitute zonal resiliency values for them.

## Manage Goal Assignments

All Goals operations use `service-group`, optional `tenant`, and no subscription
option. GET selects an assignment with `name`; mutations use `goal-assignment`.

1. Resolve the exact service group and assignment. Do not use goal templates.
2. For creation, explicitly set `require-zonal-resiliency` to `true` (required by the service) and choose any service-level indicator ARM IDs, then call `mcp_azure_mcp_ser_resiliency_goal_assignment_create`. Omitted targets mean no targets. Create cannot replace an existing assignment; do not use PUT as a fallback for a rejected PATCH.
3. To update, inspect the assignment and choose exactly one: replace associations with a non-empty `service-level-resource-ids` array, or clear associations with `clear-service-level-resources: true`. IDs must be existing `Microsoft.Monitor/slis` resources in the same service group, readable by the caller, with at most one SLI per category according to service-source validation. Do not invent fixtures. The implementation GETs the assignment and preserves its zonal flag through SDK `UpdateAsync`; GA PATCH cannot change that flag. Clear does not delete SLI resources.
4. On success, create/update wait up to ten minutes and return a hydrated GET result. Verify persisted associations and the unchanged zonal flag; report actual provisioning state. Timeout or cancellation leaves completion unknown.
5. For explicit deletion, inspect the exact assignment and call `mcp_azure_mcp_ser_resiliency_goal_assignment_delete`. It waits for deletion and returns `deleted: true` or `false` when already absent. It removes tracked goal members, not underlying Azure resources or the service group.

## Refresh Goal Resources and Recommend Capacity

1. Inspect the assignment. After service-group membership changes, use `mcp_azure_mcp_ser_resiliency_goal_assignment_refresh-resources`; this rediscovers members rather than directly changing inclusion or attestation.
2. To assess zonal capacity, use `mcp_azure_mcp_ser_resiliency_goal_assignment_recommend-capacity` with up to 50 unique underlying ARM IDs, or omit `resource-ids` to discover non-resilient resources automatically. This does not provision or resize resources.
3. Both tools start new work on each invocation and are non-idempotent. Preserve the service `operationId`, `status`, and `hasCompleted`. Say accepted for `Accepted`, not completed.
4. Re-read members or assignment state when appropriate. There is no Goals operation-status tool; do not substitute a recovery-job GET or invent a polling endpoint or completed recommendations.

## Update Goal Participation, Attestation, and Confirmation

1. Get the member and resolve its underlying `resourceArmId`.
2. Obtain the user's intended participation or confirmation decision. Set `ManuallyAttested` only for resilience actually verified by the user; use `NotAttested` to clear that attestation.
3. Build the nested GA payload from [payloads](./payloads.md#goal-resource-updates-ga). Never send flat HA/DR fields, a goal-member ARM ID in place of the underlying resource ID, or invented confirmation values.
4. Call `mcp_azure_mcp_ser_resiliency_goal_assignment_update-resources`. Excluding a resource does not delete it.
5. Report the returned operation ID and `Accepted`/`Succeeded` status accurately. Re-read the affected member to verify persisted settings; acceptance alone does not prove they changed.

## Create and Configure a Drill

1. Collect drill type, region, supporting-resource subscription, RBAC setup mode, and optional recovery plan. If GA goal/Health Model/SLI configuration is requested, collect its identity and selected resource IDs using the payload reference; do not supply the read-only goal assignment ID.
2. Call `mcp_azure_mcp_ser_resiliency_drill_create`.
3. Get the drill and verify provisioning state.
4. List targets with `mcp_azure_mcp_ser_resiliency_drill_resource_get`.
5. Include, update, or exclude targets with `mcp_azure_mcp_ser_resiliency_drill_resource_add-or-update`; require a positive fault duration and at least one resource payload.
6. Re-read affected targets and verify their inclusion and fault configuration.
7. Do not start until provisioning and target state permit execution.

## Execute a Drill

1. Get the drill and confirm it is not already running.
2. Run `mcp_azure_mcp_ser_resiliency_drill_check-resync-readiness` when configuration may be stale or a current readiness result is required; retain the operation ID and do not treat acceptance as readiness.
3. Validate with `mcp_azure_mcp_ser_resiliency_drill_validate-for-execution`. Supply source locations and optionally the intended operation (`Failover`, `Reprotect`, `FailoverReverse`, or `ReprotectReverse`). Sources may be omitted only for explicit `Reprotect`; omitting the operation still requires sources.
4. Stop and report blockers when validation does not qualify the drill for execution.
5. Ask for `Failover` versus `TestFailover` if not explicit.
6. Start with `mcp_azure_mcp_ser_resiliency_drill_start` and retain the operation ID.
7. Inspect runs with `mcp_azure_mcp_ser_resiliency_drill_run_get` when status is requested.
8. Inspect run targets with `mcp_azure_mcp_ser_resiliency_drill_run_resource_get` when per-resource results are needed.
9. Use `mcp_azure_mcp_ser_resiliency_drill_run_mark-complete` only when the user explicitly wants to stop retries for the named active stage and allow the run to proceed.
10. End only a running drill with `mcp_azure_mcp_ser_resiliency_drill_end`; require outcome and notes.
11. After a run finishes, generate its report with `mcp_azure_mcp_ser_resiliency_drill_run_generate-report`, check `properties.report.generationStatus` with `drill_run_get` until it is `Succeeded`, then use `mcp_azure_mcp_ser_resiliency_drill_run_get-report-download-url`. Treat the returned URL as a secret: give it to the user only, never log or store it, and note that it expires.

For drill-run failover, always collect the physical source zones; the service does not process a failover without them. Automatic failover defaults to `false` (pause after fault injection). Failover selections must be full drill-resource IDs under the requested service group and drill, obtained with drill resource get. Reprotect can restrict processing to selected recovery-resource IDs in the same service group; omission means all qualified resources. Neither request acceptance nor successful input validation proves that fault injection or reprotection has completed.

To update monitoring, send only the fields being changed. A supplied SLI array is a replacement, not an append. Clear Health Model or SLI monitoring only on explicit request, using the string `"null"` as the corresponding JSON option payload. Omission preserves the configuration.

## Create a Recovery Plan

1. Require plan name, `Zonal` plan type, description, and identity choice.
2. For `UserAssigned` or `SystemAndUserAssigned`, require the full identity ARM ID.
3. Build group/action JSON using [payload reference](./payloads.md).
4. Create the plan with `mcp_azure_mcp_ser_resiliency_recoveryplan_create`.
5. Get it and verify provisioning state, identity, group ordering, and actions.
6. Configure resource membership separately.

For an existing plan, read it first and preserve omitted values. If it retains a user-assigned identity, reuse the exact current identity resource ID.

## Include or Reconfigure Recovery Resources

1. List plan members with `mcp_azure_mcp_ser_resiliency_recoveryplan_resource_get`.
2. Get every intended target and capture its full recovery-resource ID, unique ID, inclusion state, existing solution type, and settings.
3. Build sparse resource updates. For first inclusion/re-inclusion, supply the selected protection solution and complete required settings.
4. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_resource_update`.
5. Get each affected resource and verify inclusion and protection configuration.
6. Distinguish:
   - Include: participates in recovery.
   - Exclude: remains a plan member but does not participate.
   - Remove: no longer a plan member.

## Check Readiness

1. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_checkreadiness`.
2. Preserve the operation/job identifiers.
3. If results are asynchronous, inspect recovery jobs.
4. Report readiness per resource, including permission, identity, runbook, protection, and configuration blockers.
5. Do not treat an accepted check as a ready plan.

## Validate and Start Failover

1. Require source locations, selected recovery-resource IDs, or both. Never derive selectors from plan metadata.
2. Check readiness if no current readiness result exists.
3. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_validateforfailover` with the exact selectors and consent intended for execution.
4. Partition the result into qualified and unqualified resources and report every blocking reason.
5. If any user-selected resource is unqualified, stop and ask whether the user wants to correct blockers or explicitly change the selector set.
6. If selectors change, validate again.
7. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_failover` with the validated selectors.
8. Return operation and recovery job IDs; monitor through recovery-job get when requested.

## Validate and Start Reprotect

1. Get the plan and relevant recovery resources after failover.
2. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_validateforreprotect` with selected IDs, or omit IDs to evaluate all qualified resources.
3. Report per-resource eligibility and blockers.
4. Stop when no resource qualifies.
5. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_reprotect` with the same selected IDs.
6. Return operation and recovery job IDs.

## Validate Another Recovery Operation

1. Require one of `Failover`, `FailoverCommit`, `Reprotect`, `TestFailover`, or `TestFailoverCleanup`.
2. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_validateforoperation`.
3. Report state, readiness, and permission blockers.
4. This tool validates only; it does not execute the operation.

## Finalize Current Plan Operation

1. Get the plan and verify that an operation is awaiting finalization.
2. Call `mcp_azure_mcp_ser_resiliency_recoveryplan_finalize`.
3. Return the operation ID and terminal plan state. A successful result is completed because the tool waits for the ARM operation and validates the terminal plan state before returning.
4. Do not describe finalize as failover commit.

## Monitor, Retry, or Resume a Recovery Job

1. Get the job with `mcp_azure_mcp_ser_resiliency_recoveryjob_get`.
2. For target-level details, use `mcp_azure_mcp_ser_resiliency_recoveryjob_resource_get`.
3. Retry only when state is `Failed` using `mcp_azure_mcp_ser_resiliency_recoveryjob_retry`.
4. Resume only when state is `Paused` using `mcp_azure_mcp_ser_resiliency_recoveryjob_resume`.
5. Collect paused-action input instead of inventing a resume description.
6. Return the new operation ID and preserve the recovery job identity.

## Delete a Drill or Recovery Plan

1. Get the exact named target.
2. Check active execution state and dependent resources.
3. End a running drill before deletion; drill delete does not stop execution.
4. Do not delete a plan with active recovery operations.
5. Invoke the matching delete tool only after the user has clearly requested deletion of that target.
6. Report whether the target existed and the returned deletion result.
