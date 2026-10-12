# Azure Resilience Management MCP Tool Reference

<!-- cspell:words reprotect reprotection -->

This reference lists all 47 tools registered under the `resiliency` namespace. Parameters marked **required** must be collected before invocation. `tenant` is optional for every tool unless the active environment requires it. The `mcp_azure_mcp_ser_` prefix is the client display prefix; registered tool names start at `resiliency_`. Verb hyphens are preserved.

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

The GA API is `2026-10-01`; SDK package `1.0.0-alpha.20261012.1` remains a NuGet
alpha and requires Azure.Core `1.63.0` and System.ClientModel `1.16.0`. Public-feed
checking confirmed this exact package is absent, blocking clean CI restore.
Live SDK PATCH probes with a true zonal flag plus empty SLI list and with a false
flag both returned 400 `RHUBUserErrorInvalidRequestParameter`. A controlled probe
also sent only `{"properties":{"serviceLevelResources":[]}}` directly to the GA
endpoint, bypassing SDK serialization. Both the minimal request and SDK request
returned the same 400 immediately and after a 120-second settling interval.
Independent GETs before and after PATCH returned `Succeeded`, the unchanged
true zonal requirement, and an empty SLI list; this is not proof of an update.
The service source accepts explicit empty SLI arrays, but the deployed rejection's
exact validation gate is unconfirmed. Do not add a payload-stripping policy,
blind retry, PUT fallback, or success-shaped clear result based on these probes.
Those probes used the default ARM route. A subsequent explicit Canary retry used
`https://eastus2euap.management.azure.com` with
`x-ms-rhub-routing-location: eastus2euap`, as documented by the service API toolkit.
The unchanged true-plus-empty-list PATCH returned 202, reached `Succeeded`, and
an independent GET confirmed the true requirement and empty list. This establishes
ECY acceptance of `[]`, not `null`, and not substantive nonempty SLI replacement.
The MCP default route remains unchanged. Debug-only, opt-in ECY recording now
exercises the real MCP assignment lifecycle, including terminal PATCH success and
independent GET; the old default-route failure is archived outside the active recordings.
Do not silently route production tools to Canary. A real SLI fixture is still required
for the complete replacement/persistence lifecycle. Evaluation is blocked without
`AOAI_ENDPOINT`; do not treat blockers as skipped or passing checks.

## Drills

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resiliency_drill_get` | List drills or get one definition | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_create` | Create/configure a drill | **`service-group`**, **`drill`**, **`subscription`**, **`region`**, **`drill-type`** (`Zonal` or `Regional`), **`rbac-setup-mode`** (`AutomatedCustomRole`, `AutomatedBuiltinRoles`, or `Manual`), `resource-group?`, `recoveryplan?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_update` | Update drill RBAC, recovery plan association, or relocate supporting resources by changing subscription and region | **`service-group`**, **`drill`**, `subscription?` + `region?` together, `rbac-setup-mode?`, `recoveryplan?`, `tenant?`. Supply at least one change. |
| `mcp_azure_mcp_ser_resiliency_drill_delete` | Permanently delete a drill definition | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_start` | Start a drill execution | **`service-group`**, **`drill`**, **`mode`** (`Failover` or `TestFailover`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_end` | End a running drill and attest outcome | **`service-group`**, **`drill`**, **`attestation`** (`Success` or `Failed`), **`attestation-notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_check-resync-readiness` | Resync drill configuration and start a readiness check | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_validate-for-execution` | Validate drill execution from physical source locations | **`service-group`**, **`drill`**, **`source-locations`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_resource_get` | List drill targets or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_resource_add-or-update` | Include, update, or exclude drill resources and fault settings | **`service-group`**, **`drill`**, **`fault-duration-minutes`**, `include-resources?`, `update-resources?`, `exclude-resources?`, `force-inclusion-and-update?` (`Enable` or `Disable`), `tenant?`. Supply at least one include, update, or exclude payload. |
| `mcp_azure_mcp_ser_resiliency_drill_run_get` | List drill runs or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_add-notes` | Append notes to a drill run | **`service-group`**, **`drill`**, **`drill-run`**, **`notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_failover` | Start drill-run failover from physical zones | **`service-group`**, **`drill`**, **`drill-run`**, **`source-locations`** (such as `eastus-az1`), `selected-resource-ids?`, `auto-failover?` (default `false`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_resume` | Resume a paused drill run after fault injection | **`service-group`**, **`drill`**, **`drill-run`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_reprotect` | Reprotect failed-over drill-run resources | **`service-group`**, **`drill`**, **`drill-run`**, `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_mark-complete` | Mark a drill-run stage complete and disable further retries | **`service-group`**, **`drill`**, **`drill-run`**, **`stage`** (`FaultInjection`, `Failover`, `Reprotect`, `FailoverReverse`, or `ReprotectReverse`), `tenant?` |
| `mcp_azure_mcp_ser_resiliency_drill_run_resource_get` | List run targets or get one | **`service-group`**, **`drill`**, **`drill-run`**, `name?`, `tenant?` |

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
