# Azure Resilience Management MCP Tool Reference

<!-- cspell:words recoveryplans reprotect reprotection -->

This reference lists all 32 tools registered under the `resilience` namespace. Parameters marked **required** must be collected before invocation. `tenant` is optional for every tool unless the active environment requires it.

## Usage Plans

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resilience_usageplan_get` | List usage plans or get one | `subscription?`, `resource-group?`, `name?`, `tenant?`. A specific `name` requires `resource-group`. Omit both to list across the subscription. |
| `mcp_azure_mcp_ser_resilience_usageplan_create` | Create or update a usage plan | **`resource-group`**, **`usage-plan`**, **`plan-type`** (`Basic` or `Standard`), `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_usageplan_enrollment_get` | List enrollments or get one | **`resource-group`**, **`usage-plan`**, `name?`, `subscription?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_usageplan_enrollment_create` | Create/update enrollment associating a service group | **`resource-group`**, **`usage-plan`**, **`enrollment`**, **`service-group`**, `subscription?`, `tenant?` |

Usage-plan and enrollment names are 3–24 characters containing letters, numbers, or hyphens.

## Goals

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resilience_goal_template_get` | List goal templates or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_goal_assignment_get` | List goal assignments or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_goal_resource_get` | List assignment members or get one | **`service-group`**, **`goal-assignment`**, `name?`, `tenant?` |

These tools are read-only. Omit `name` to list IDs and names; provide it for full details.

## Drills

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resilience_drill_get` | List drills or get one definition | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_create` | Create/configure a drill | **`service-group`**, **`drill`**, **`subscription`**, **`region`**, **`drill-type`** (`Zonal` or `Regional`), **`rbac-setup-mode`** (`AutomatedCustomRole`, `AutomatedBuiltinRoles`, or `Manual`), `resource-group?`, `recoveryplan?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_update` | Update drill RBAC, recoveryplan association, or supporting-resource location | **`service-group`**, **`drill`**, `subscription?` + `region?` together, `rbac-setup-mode?`, `recoveryplan?`, `tenant?`. Supply at least one change. |
| `mcp_azure_mcp_ser_resilience_drill_delete` | Permanently delete a drill definition | **`service-group`**, **`drill`**, `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_start` | Start a drill execution | **`service-group`**, **`drill`**, **`mode`** (`Failover` or `TestFailover`), `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_end` | End a running drill and attest outcome | **`service-group`**, **`drill`**, **`attestation`** (`Success` or `Failed`), **`attestation-notes`**, `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_resource_get` | List drill targets or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_run_get` | List drill runs or get one | **`service-group`**, **`drill`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_drill_run_resource_get` | List run targets or get one | **`service-group`**, **`drill`**, **`drill-run`**, `name?`, `tenant?` |

## Recoveryplans

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resilience_recoveryplan_get` | List plans or get one | **`service-group`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_create` | Create/update plan identity, groups, and actions | **`service-group`**, **`recoveryplan`**, **`plan-type`** (`Zonal`), **`identity-type`**, `plan-description?`, `user-assigned-identity?`, `default-group-description?`, `default-group-pre-actions?`, `default-group-post-actions?`, `additional-groups?`, `tenant?`. Creation requires a 5–50 character `plan-description`; update preserves it when omitted. |
| `mcp_azure_mcp_ser_resilience_recoveryplan_delete` | Permanently delete a plan | **`service-group`**, **`recoveryplan`**, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_checkreadiness` | Check plan/resource readiness | **`service-group`**, **`recoveryplan`**, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_validateforoperation` | Validate plan state/readiness/permissions for one operation | **`service-group`**, **`recoveryplan`**, **`operation-name`**, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_validateforfailover` | Validate failover qualification | **`service-group`**, **`recoveryplan`**, `source-locations?`, `selected-resource-ids?`, `user-consent?`, `tenant?`. Require at least one selector. |
| `mcp_azure_mcp_ser_resilience_recoveryplan_validateforreprotect` | Validate reprotect qualification | **`service-group`**, **`recoveryplan`**, `selected-resource-ids?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_failover` | Start failover | **`service-group`**, **`recoveryplan`**, `source-locations?`, `selected-resource-ids?`, `user-consent?`, `tenant?`. Require at least one selector. |
| `mcp_azure_mcp_ser_resilience_recoveryplan_reprotect` | Start reprotection after failover | **`service-group`**, **`recoveryplan`**, `selected-resource-ids?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_finalize` | Validate permissions and finalize current plan operation state | **`service-group`**, **`recoveryplan`**, `tenant?`. This does not commit a completed failover. |

### Recoveryplan identity values

- `SystemAssigned`
- `UserAssigned` — requires `user-assigned-identity`
- `SystemAndUserAssigned` — requires `user-assigned-identity`

For action, group, and resource-update JSON schemas and examples, use the [payload reference](./payloads.md).

## Recovery Resources

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resilience_recoveryplan_resource_get` | List plan members or get one | **`service-group`**, **`recoveryplan`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryplan_resource_update` | Include, configure, exclude, or remove members | **`service-group`**, **`recoveryplan`**, `resources-to-update?`, `resources-to-remove?`, `tenant?`. Supply at least one update/removal payload. |

## Recovery Jobs

| Tool | Purpose | Parameters |
|---|---|---|
| `mcp_azure_mcp_ser_resilience_recoveryjob_get` | List jobs or get one | **`service-group`**, **`recoveryplan`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryjob_resource_get` | List job targets or get one | **`service-group`**, **`recoveryplan`**, **`recovery-job`**, `name?`, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryjob_retry` | Retry a failed job | **`service-group`**, **`recoveryplan`**, **`recovery-job`**, `tenant?` |
| `mcp_azure_mcp_ser_resilience_recoveryjob_resume` | Resume a paused job | **`service-group`**, **`recoveryplan`**, **`recovery-job`**, `description?`, `tenant?`. Supply `description` when the paused action requires user input; maximum 100 characters. |

## Operation Values

- Operation names: `Failover`, `FailoverCommit`, `Reprotect`, `TestFailover`, `TestFailoverCleanup`
- Failover consent: `Unspecified` or `Allowed`
