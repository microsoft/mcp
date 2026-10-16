# Azure Resilience Management MCP Tool Reference

<!-- cspell:words reprotect reprotection -->

This reference lists all 41 tools registered under the `resiliency` namespace. Parameters marked **required** must be collected before invocation. `tenant` is optional for every tool unless the active environment requires it.

## Usage Plans

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_usageplan_get` | List or get Azure Resilience Management usage plans; not quota, API Management, or load-testing plans | `subscription?`, `resource-group?`, `name?`, `tenant?`. A specific `name` requires `resource-group`. Omit both to list across the subscription. |
| `mcp_azure_mcp_ser_resiliency_usageplan_create` | Create or update a usage plan | **`resource-group`**, **`usage-plan`**, **`plan-type`** (`Basic` or `Standard`), `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_delete` | Delete the entire usage plan after all child enrollments are removed; returns whether the plan existed | **`resource-group`**, **`usage-plan`**, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_get` | List or get resilience usage-plan enrollments that associate service groups; not API Management subscriptions | **`resource-group`**, **`usage-plan`**, `name?`, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_create` | Create/update enrollment associating a service group | **`resource-group`**, **`usage-plan`**, **`enrollment`**, **`service-group`**, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_usageplan_enrollment_delete` | Delete only an enrollment/service-group association while retaining the usage plan; returns whether the enrollment existed | **`resource-group`**, **`usage-plan`**, **`enrollment`**, `subscription?`, `tenant?` |

Usage-plan and enrollment names are 3–24 characters containing letters, numbers, or hyphens.

## Goals

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_goal_assignment_get` | List goal assignments or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_goal_resource_get` | List assignment members or get one | **`service-group`**, **`goal-assignment`**, `name?`, `tenant?` |

These tools are read-only. Omit `name` to list IDs and names; provide it for full details.

With API `2026-10-01`, assignment details include `requireZonalResiliency` and member details include `zonalResiliency`. Preview-only goal fields are nullable when the service does not return them. Do not infer legacy HA/DR participation or attestation from zonal resiliency.

## Drills

All 17 existing Drills tools use SDK `Azure.ResourceManager.ResilienceManagement` 1.0.0 and API `2026-10-01`. GA configuration inputs, operation-selectable validation, optional failover requests, and selected-resource reprotection are supported. Validation requires sources except for explicit `Reprotect`. Get responses preserve returned GA fields; list responses remain ID/name summaries. Separate report-generation/download tools are not exposed.

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_drill_get` | List drills or get one definition | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_create` | Create/configure a drill, including GA goal and monitoring properties | **`service-group`**, **`drill`**, **`subscription`**, **`region`**, **`drill-type`** (`Zonal` or `Regional`), **`rbac-setup-mode`** (`AutomatedCustomRole`, `AutomatedBuiltinRoles`, or `Manual`), `resource-group?`, `recoveryplan?`, `goal-assignment-properties?`, `health-model-monitoring-properties?`, `sli-monitoring-properties?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_update` | Update drill RBAC, recovery plan, goal identity, monitoring, or supporting-resource subscription/region | **`service-group`**, **`drill`**, `subscription?` + `region?` together, `rbac-setup-mode?`, `recoveryplan?`, `goal-assignment-properties?`, `health-model-monitoring-properties?`, `sli-monitoring-properties?`, `tenant?`. Supply at least one change; see JSON omission/clearing rules below. |
| `mcp_azure_mcp_ser_resiliency_drill_delete` | Permanently delete a drill definition | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_start` | Start a drill execution | **`service-group`**, **`drill`**, **`mode`** (`Failover` or `TestFailover`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_end` | End a running drill and attest outcome | **`service-group`**, **`drill`**, **`attestation`** (`Success` or `Failed`), **`attestation-notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_check-resync-readiness` | Resync drill configuration and start a readiness check | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_validate-for-execution` | Validate drill execution with an optional operation selector | **`service-group`**, **`drill`**, `source-locations`, `operation-name?` (`Failover`, `Reprotect`, `FailoverReverse`, `ReprotectReverse`), `tenant?`. Sources may be omitted only for explicit `Reprotect`; default validation also requires sources. |
| `mcp_azure_mcp_ser_resiliency_drill_resource_get` | List drill targets or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_resource_add-or-update` | Include, update, or exclude drill resources and fault settings | **`service-group`**, **`drill`**, **`fault-duration-minutes`**, `include-resources?`, `update-resources?`, `exclude-resources?`, `force-inclusion-and-update?` (`Enable` or `Disable`), `tenant?`. Supply at least one payload; include/update IDs must identify drill resources under the requested drill, not underlying VMs. |
| `mcp_azure_mcp_ser_resiliency_drill_run_get` | List drill runs or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_add-notes` | Add notes to an existing drill run | **`service-group`**, **`drill`**, **`drill-run`**, **`notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_failover` | Start failover for qualified drill-run resources | **`service-group`**, **`drill`**, **`drill-run`**, `source-locations?`, `selected-resource-ids?`, `auto-failover?`, `tenant?`. Omit all three failover inputs to send no body and use service defaults. Explicit selected IDs or auto-failover require source locations; with sources, omitted auto-failover remains `false`. |
| `mcp_azure_mcp_ser_resiliency_drill_run_resume` | Resume the drill run when its current stage permits it | **`service-group`**, **`drill`**, **`drill-run`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_mark-complete` | Mark a drill-run stage complete and disable further retries | **`service-group`**, **`drill`**, **`drill-run`**, **`stage`** (`FaultInjection`, `Failover`, `Reprotect`, `FailoverReverse`, or `ReprotectReverse`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_reprotect` | Start reprotection after failover when the run permits it | **`service-group`**, **`drill`**, **`drill-run`**, `selected-resource-ids?`, `tenant?`. Omit selected IDs to process all qualified resources without a request body. |
| `mcp_azure_mcp_ser_resiliency_drill_run_resource_get` | List run targets or get one | **`service-group`**, **`drill`**, **`drill-run`**, `name?`, `tenant?` |

The three configuration options accept JSON strings; see [payloads](./payloads.md#drill-ga-configuration). On create, each supplied configuration requires identity, and monitoring requires its selection. Update accepts partial objects and preserves omitted fields. Pass the string `"null"` as the Health Model or SLI option value to clear that configuration. A supplied SLI array replaces the selection; empty arrays are invalid. `goalAssignmentId` is read-only.

## Recovery Plans

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_recoveryplan_get` | List plans or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_recoveryplan_create` | Create/update plan identity, groups, and actions | **`service-group`**, **`recoveryplan`**, **`plan-type`** (`Zonal`), **`identity-type`**, `plan-description?`, `user-assigned-identity?`, `default-group-description?`, `default-group-pre-actions?`, `default-group-post-actions?`, `additional-groups?`, `tenant?`. Creation requires a 5–50 character `plan-description`; update preserves it when omitted. |
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
