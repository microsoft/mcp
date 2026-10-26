# Azure Resilience Management MCP Tool Reference

<!-- cspell:words reprotect reprotection -->

This reference lists all 49 tools registered under the `resiliency` namespace. Parameters marked **required** must be collected before invocation. `tenant` is optional for every tool unless the active environment requires it. The `mcp_azure_mcp_ser_` prefix is the client display prefix; registered tool names start at `resiliency_`. Verb hyphens are preserved.

## Usage Plans

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_usageplan_get` | List or get Azure Resilience Management usage plans; not quota, API Management, or load-testing plans | `subscription?`, `resource-group?`, `name?`, `tenant?`. A specific `name` requires `resource-group`. Omit both to list across the subscription. |
| `mcp_azure_mcp_ser_resiliency_usageplan_create` | Create or update a usage plan | **`resource-group`**, **`usage-plan`**, **`plan-type`** (`Standard`), `tags?` (JSON object of strings, applied to the plan after it is created or updated), `subscription?`, `tenant?`. Usage plans are always `global`. |
| `mcp_azure_mcp_ser_resiliency_usageplan_delete` | Delete the entire usage plan after all child enrollments are removed; returns whether the plan existed | **`resource-group`**, **`usage-plan`**, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_get` | List or get resilience usage-plan enrollments that associate service groups; not API Management subscriptions | **`resource-group`**, **`usage-plan`**, `name?`, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_create` | Create/update enrollment associating a service group | **`resource-group`**, **`usage-plan`**, **`enrollment`**, **`service-group`**, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_delete` | Delete only an enrollment/service-group association while retaining the usage plan; returns whether the enrollment existed | **`resource-group`**, **`usage-plan`**, **`enrollment`**, `subscription?`, `tenant?` |

Usage-plan and enrollment names are 3–24 characters containing letters, numbers, or hyphens.

## Goals

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_goal_assignment_get` | List goal assignments or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_goal_assignment_create` | Create an assignment; wait and return hydrated assignment | **`service-group`**, **`goal-assignment`**, **`require-zonal-resiliency`** (`true`, required by the service), `service-level-resource-ids?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_goal_assignment_update` | Replace or clear SLI associations, preserving the zonal requirement | **`service-group`**, **`goal-assignment`**, `service-level-resource-ids?` (non-empty string array), `clear-service-level-resources?` (boolean), `tenant?`. Exactly one of IDs or clear set to `true` is required. |
| `mcp_azure_mcp_ser_resiliency_goal_assignment_delete` | Delete assignment and tracked members, not underlying Azure resources | **`service-group`**, **`goal-assignment`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_goal_assignment_recommend-capacity` | Start zonal capacity recommendations; does not provision capacity | **`service-group`**, **`goal-assignment`**, `resource-ids?` (at most 50 unique underlying ARM IDs; omit for automatic discovery), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_goal_assignment_refresh-resources` | Rediscover tracked resources from current service-group membership | **`service-group`**, **`goal-assignment`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_goal_assignment_update-resources` | Change zonal participation, attestation, or user confirmation | **`service-group`**, **`goal-assignment`**, **`resources`** (non-empty GA JSON array, at most 1 MB), `tenant?`. See [payloads](./payloads.md#goal-resource-updates-ga). |
| `mcp_azure_mcp_ser_resiliency_goal_resource_get` | List assignment members or get one | **`service-group`**, **`goal-assignment`**, `name?`, `tenant?` |

Only the two GET tools are read-only. Omit `name` for ID/name summaries; provide it
for full details. Assignments expose `requireZonalResiliency` and
`serviceLevelResources`; members expose `resourceArmId` and nested
`zonalResiliency`. No Goals command takes `subscription` or a goal template.
Create/update/delete wait at most ten minutes; create/update then GET the hydrated
assignment. Delete reports whether it existed. Other mutations return an operation
with service operation ID, `Accepted`/`Succeeded`, and `hasCompleted`; acceptance
is not completion. Recommend-capacity and refresh-resources are non-idempotent.
Create/update/delete/update-resources are mutating, destructive, and idempotent.
Service-level resource arrays are ordinary unique ARM ID arrays, not JSON strings.
IDs must be existing `Microsoft.Monitor/slis` resources in the same service group,
readable by the caller; service-source validation allows at most one per category.
Assignment names must match `^[a-zA-Z0-9-]{3,24}$`. The GA service rejects longer
names and characters other than ASCII letters, digits, and hyphens.
Create sets initial targets (none when omitted); it cannot replace an existing assignment.
PUT may retry failed provisioning, so create retains conservative destructive metadata.
Update GETs the assignment and calls
SDK `UpdateAsync`, preserving its zonal requirement; GA PATCH cannot change that
flag. Clear removes associations, not SLI resources. Never invent SLI fixture IDs.

Goals target API `2026-10-01`. See [development](./development.md#goals-canary-recording)
for canary recording; production tools retain their normal ARM endpoint.

With API `2026-10-01`, assignment details include `requireZonalResiliency`, `serviceLevelResources`, and `errorDetails`, and member details include `resourceArmId` and `zonalResiliency`. Preview-only HA/DR goal fields are not returned. Do not infer HA/DR participation or attestation from zonal resiliency.

## Drills

Drills tools use `Azure.ResourceManager.ResilienceManagement` 1.0.0 and API `2026-10-01`. Get responses preserve returned GA fields; list responses remain ID/name summaries.

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_drill_get` | List drills or get one definition | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_create` | Create/configure a drill, including GA goal and monitoring properties | **`service-group`**, **`drill`**, **`subscription`**, **`region`**, **`drill-type`** (`Zonal` or `Regional`), **`rbac-setup-mode`** (`AutomatedCustomRole`, `AutomatedBuiltinRoles`, or `Manual`), `resource-group?`, `goal-assignment-properties?`, `health-model-monitoring-properties?`, `sli-monitoring-properties?`, `identity?`, `chaos-resource-properties?`, `recovery-plan-properties?`, `monitoring-properties?`, `tenant?`. Omitted identities default to `SystemAssigned`. The service links the service group's recovery plan automatically; `recoveryPlanId` is read-only. |
| `mcp_azure_mcp_ser_resiliency_drill_update` | Update drill RBAC, identities, goal identity, monitoring, or supporting-resource subscription/region | **`service-group`**, **`drill`**, `subscription?` + `region?` together (with optional `resource-group?`, which must match the creation-time resource group), `rbac-setup-mode?`, `goal-assignment-properties?`, `health-model-monitoring-properties?`, `sli-monitoring-properties?`, `identity?`, `chaos-resource-properties?`, `recovery-plan-properties?`, `monitoring-properties?`, `tenant?`. Supply at least one change; see JSON omission/clearing rules below. |
| `mcp_azure_mcp_ser_resiliency_drill_delete` | Permanently delete a drill definition | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_start` | Start a drill execution | **`service-group`**, **`drill`**, **`mode`** (`Failover` or `TestFailover`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_end` | End a running drill and attest outcome | **`service-group`**, **`drill`**, **`attestation`** (`Success` or `Failed`), **`attestation-notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_check-resync-readiness` | Resync drill configuration and start a readiness check | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_validate-for-execution` | Validate drill execution with an optional operation selector | **`service-group`**, **`drill`**, `source-locations`, `operation-name?` (`Failover`, `Reprotect`, `FailoverReverse`, `ReprotectReverse`), `tenant?`. Sources may be omitted only for explicit `Reprotect`; default validation also requires sources. |
| `mcp_azure_mcp_ser_resiliency_drill_resource_get` | List drill targets or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_resource_add-or-update` | Include, update, or exclude drill resources and fault settings | **`service-group`**, **`drill`**, **`fault-duration-minutes`**, `include-resources?`, `update-resources?`, `exclude-resources?`, `force-inclusion-and-update?` (`Enable` or `Disable`), `tenant?`. Supply at least one payload; include, update, and exclude IDs must identify drill resources under the requested drill, not underlying VMs. |
| `mcp_azure_mcp_ser_resiliency_drill_run_get` | List drill runs or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_add-notes` | Add notes to an existing drill run | **`service-group`**, **`drill`**, **`drill-run`**, **`notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_failover` | Start failover for qualified drill-run resources | **`service-group`**, **`drill`**, **`drill-run`**, **`source-locations`**, `selected-resource-ids?` (full drill-resource IDs under this service group and drill), `auto-failover?` (default `false`), `user-consent?` (`Unspecified` or `Allowed`; `Allowed` lets failover proceed when the recovery plan is not `Ready`), `tenant?`. |
| `mcp_azure_mcp_ser_resiliency_drill_run_resume` | Resume the drill run when its current stage permits it | **`service-group`**, **`drill`**, **`drill-run`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_mark-complete` | Mark a drill-run stage complete and disable further retries | **`service-group`**, **`drill`**, **`drill-run`**, **`stage`** (`FaultInjection`, `Failover`, `Reprotect`, `FailoverReverse`, or `ReprotectReverse`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_reprotect` | Start reprotection after failover when the run permits it | **`service-group`**, **`drill`**, **`drill-run`**, `selected-resource-ids?`, `tenant?`. Omit selected IDs to process all qualified resources. |
| `mcp_azure_mcp_ser_resiliency_drill_run_generate-report` | Generate or regenerate the drill-run report; returns the operation ID, `hasCompleted`, and the report summary when available | **`service-group`**, **`drill`**, **`drill-run`**, `tenant?`. Check progress with `drill_run_get` (`properties.report`). |
| `mcp_azure_mcp_ser_resiliency_drill_run_get-report-download-url` | Get a short-lived, read-only download URL for the generated report. The URL is a secret; do not log or share it. | **`service-group`**, **`drill`**, **`drill-run`**, `format?` (`Html`, the default), `tenant?`. Generate the report first. |
| `mcp_azure_mcp_ser_resiliency_drill_run_resource_get` | List run targets or get one | **`service-group`**, **`drill`**, **`drill-run`**, `name?`, `tenant?` |

The configuration options accept JSON strings; see [payloads](./payloads.md#drill-ga-configuration). On create, each supplied configuration requires identity, and monitoring requires its selection. Update accepts partial objects and preserves omitted fields. Pass the string `"null"` as the Health Model or SLI option value to clear that configuration. A supplied SLI array replaces the selection; empty arrays are invalid. `goalAssignmentId` is read-only. The identity options use `{"type":"...","userAssignedIdentity":"<ARM ID>"}` for each associated identity (`chaos-resource-properties` requires both `identity` and `chaosResourceIdentityForFaults` on create), and `identity` uses `{"type":"...","userAssignedIdentities":["<ARM ID>"]}`.

## Recovery Plans

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_recoveryplan_get` | List plans or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_create` | Create/update plan identity, groups, and actions | **`service-group`**, **`recoveryplan`**, **`plan-type`** (`Zonal`), **`identity-type`** (`SystemAssigned`, `UserAssigned`, `SystemAndUserAssigned`, or `None`), `plan-description?`, `user-assigned-identity?` (one per plan), `default-group-description?`, `default-group-unique-id?` (GUID, create only), `default-group-pre-actions?`, `default-group-post-actions?`, `additional-groups?`, `tenant?`. Creation requires a 5–50 character `plan-description`; update preserves it when omitted. CustomRunbook actions accept an optional `associatedIdentity`. |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_delete` | Permanently delete a plan | **`service-group`**, **`recoveryplan`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_checkreadiness` | Check plan/resource readiness | **`service-group`**, **`recoveryplan`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_validateforoperation` | Run operation-level plan state/readiness/permission validation; distinct from per-resource failover qualification | **`service-group`**, **`recoveryplan`**, **`operation-name`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_validateforfailover` | Validate failover qualification | **`service-group`**, **`recoveryplan`**, `source-locations?`, `selected-resource-ids?`, `user-consent?`, `tenant?`. Require at least one selector. |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_validateforreprotect` | Validate Azure Resilience Management recoveryplan reprotect qualification, not Azure Backup | **`service-group`**, **`recoveryplan`**, `selected-resource-ids?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_failover` | Start failover | **`service-group`**, **`recoveryplan`**, `source-locations?`, `selected-resource-ids?`, `user-consent?`, `tenant?`. Require at least one selector. |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_reprotect` | Start reprotection after failover | **`service-group`**, **`recoveryplan`**, `selected-resource-ids?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_finalize` | Validate permissions and finalize current plan operation state | **`service-group`**, **`recoveryplan`**, `tenant?`. This does not commit a completed failover. |

### Recovery plan identity values

- `SystemAssigned`
- `UserAssigned` — requires `user-assigned-identity`
- `SystemAndUserAssigned` — requires `user-assigned-identity`

For action, group, and resource-update JSON schemas and examples, use the [payload reference](./payloads.md).

## Recovery Resources

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_recoveryplan_resource_get` | List plan members or get one | **`service-group`**, **`recoveryplan`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_resource_update` | Include, configure, exclude, or remove members | **`service-group`**, **`recoveryplan`**, `resources-to-update?`, `resources-to-remove?`, `tenant?`. Supply at least one update/removal payload. |

## Recovery Jobs

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_recoveryjob_get` | List jobs or get one | **`service-group`**, **`recoveryplan`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryjob_resource_get` | List job targets or get one | **`service-group`**, **`recoveryplan`**, **`recovery-job`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryjob_retry` | Retry a failed job | **`service-group`**, **`recoveryplan`**, **`recovery-job`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryjob_resume` | Resume a paused job | **`service-group`**, **`recoveryplan`**, **`recovery-job`**, `description?`, `tenant?`. Supply `description` when the paused action requires user input; maximum 100 characters. |

## Operation Values

- Operation names: `Failover`, `FailoverCommit`, `Reprotect`, `TestFailover`, `TestFailoverCleanup`
- Failover consent: `Unspecified` or `Allowed`
