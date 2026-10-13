# Resilience Payload Reference

<!-- cspell:words reprotect reprotection runbook -->

Use JSON strings only for parameters whose tool schema requires JSON. Do not serialize ordinary scalar or array parameters manually.

## Goal Resource Updates (GA)

`resiliency_goal_assignment_update-resources` takes `resources` as a JSON string
containing a non-empty array (maximum 1 MB). Each item requires
`properties.resourceArmId`, a unique complete underlying Azure resource ARM ID,
and `properties.zonalResiliency` with at least one change:

```json
[
  {
    "properties": {
      "resourceArmId": "/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/virtualMachines/{vm}",
      "zonalResiliency": {
        "goalParticipation": "Included",
        "attestationStatus": "NotAttested",
        "userConfirmation": [
          {
            "solutionDisplayName": "ZonePinnedVmWithZrsDisk",
            "confirmationStatus": "ApprovedByUser",
            "reasonForRequestingConfirmation": "ZonePinnedZrsDataDisksConditional"
          }
        ]
      }
    }
  }
]
```

The example's confirmation must reflect the user's actual decision; do not copy it
as a default approval. Omit fields that are not being changed.

| Field | Exact case-sensitive values |
|---|---|
| `goalParticipation` | `Included`, `Excluded` |
| `attestationStatus` | `NotAttested`, `ManuallyAttested` |
| `userConfirmation[].solutionDisplayName` | `ZonePinnedVmWithZrsDisk`, `VmInMultiZoneVmss` |
| `userConfirmation[].confirmationStatus` | `ApprovedByUser`, `ApprovalPending`, `ApprovalNotNeeded`, `RejectedByUser` |
| `userConfirmation[].reasonForRequestingConfirmation` (optional) | `ZonePinnedZrsDataDisksConditional`, `VmInMultiZoneScaleSetStatelessOnly` |

When present, `userConfirmation` must be non-empty and each entry requires a
solution name and status; solution names cannot repeat. Unknown or duplicate
properties, empty changes, duplicate resource IDs, and legacy flat HA/DR fields
are rejected. Manual attestation requires verified resilience.

## Goal Assignment SLI Associations

Assignment create/update `service-level-resource-ids` is an ordinary string array,
not a JSON string or the member-update payload above. Each unique complete ID must
refer to an existing `Microsoft.Monitor/slis` resource in the same service group,
with caller read access. Service-source validation permits at most one SLI per
category; resolve actual resources rather than inventing IDs or fixtures.

Create requires `require-zonal-resiliency=true`; it cannot replace an existing assignment.
Update takes exactly one of a non-empty `service-level-resource-ids` array or
`clear-service-level-resources: true`. The latter removes associations only, not
the indicators. Update GETs the assignment and preserves its zonal requirement
through SDK `UpdateAsync`; GA PATCH cannot change `requireZonalResiliency`.

Recommend-capacity `resource-ids` is also an ordinary ARM ID array, but identifies
underlying Azure resources, not SLIs. Resource IDs may contain subscription
segments, but Goals tools have no `subscription` option.

## Recovery Plan Actions

A manual action pauses the workflow for a person:

```json
[
  {
    "type": "ManualAction",
    "name": "Confirm-dependencies",
    "description": "Verify dependencies are ready",
    "timeoutInMinutes": 30
  }
]
```

A custom runbook action executes an Azure Automation runbook:

```json
[
  {
    "type": "CustomRunbook",
    "name": "Start-dependencies",
    "description": "Start application dependencies",
    "timeoutInMinutes": 30,
    "actionResourceId": "/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Automation/automationAccounts/{account}/runbooks/{runbook}",
    "parameters": {
      "environment": "production"
    }
  }
]
```

Rules:

- `type`: `ManualAction` or `CustomRunbook`.
- `name`: 3–24 characters; letters, numbers, and hyphens only.
- `description`: optional; 0–100 characters.
- `timeoutInMinutes`: positive whole number.
- `actionResourceId`: required only for `CustomRunbook`; use a full Automation runbook resource ID.
- `parameters`: optional or `null` for `CustomRunbook`; every value must be a string.
- Omit an action-list option to preserve it during update. Pass `[]` only to clear it intentionally.

### Group action behavior

Recovery group actions control orchestration around a group; they do not configure protection for an individual recovery resource.

| Action | Placement | Runtime behavior | Live-test prerequisite |
|---|---|---|---|
| `ManualAction` | `preActions` or `postActions` | Pauses the recovery job until a person resumes it or the action times out | Required for a successful resume-action test. The operation must reach this action so the job enters `Paused`. |
| `CustomRunbook` | `preActions` or `postActions` | Starts the referenced Azure Automation runbook and waits for its result | The Automation account, runbook, managed identity/RBAC, and any string-valued parameters must be usable by the execution identity. |

`preActions` execute before the recovery resources in that group. `postActions` execute after those resources. Place an action according to the boundary being tested; do not add both merely to obtain coverage.

## Additional Recovery Groups

```json
[
  {
    "orderId": 1,
    "description": "Application recovery group",
    "preActions": [],
    "postActions": []
  }
]
```

Populate `preActions` or `postActions` with the action objects defined in the preceding section.

Rules:

- `orderId`: unique whole number from 1 through 14.
- Additional groups must be sequential, beginning at 1.
- `description`: 5–50 characters.
- `groupUniqueId`: optional. Omit it to preserve the existing group ID at that order or generate one for a new group.
- Omit `preActions` or `postActions` to preserve that list; use `[]` to clear it.
- Omit `additional-groups` to preserve all groups; use `[]` to remove all additional groups.

## Recovery Resource Updates

`resources-to-update` is a JSON array. Every element requires `properties.recoveryResourceUniqueId`:

```json
[
  {
    "properties": {
      "recoveryResourceUniqueId": "{unique-id}",
      "inclusionState": "Included",
      "selectedProtectionSolutionType": "{solution-type}",
      "selectedProtectionSolutionSetting": {
        "{service-defined-setting}": "{value}"
      }
    }
  }
]
```

Use the resource's current response and the tool schema as the source of truth for service-defined settings. Never guess a solution-setting property.

### Sparse update rules

- Existing settings are preserved when omitted.
- First inclusion and re-inclusion require a matching solution type and setting.
- While a resource is excluded, update only `inclusionState`.
- `recoveryGroupId` and `associatedIdentity` are optional.
- The payload maximum is 1 MB.

### Custom runbook protection

Require full Automation runbook resource IDs for both failover and reprotect actions. Preserve any existing action fields not intentionally changed.

This is resource protection, not a recovery group `CustomRunbook` action. Its `selectedProtectionSolutionSetting` uses operation-specific objects:

- `failoverAction.resourceId` and `reprotectAction.resourceId` are mandatory.
- `failoverCommitAction.resourceId`, `testFailoverAction.resourceId`, and `testFailoverCleanupAction.resourceId` are optional unless those operations are being exercised.
- Every supplied resource ID must identify `Microsoft.Automation/automationAccounts/runbooks`.
- Use it for resources whose recovery lifecycle is implemented by runbooks rather than Azure Site Recovery.

### Azure Site Recovery protection

Use only for `Microsoft.Compute/virtualMachines` resources with healthy Azure Site Recovery protection. Collect rather than infer:

- Disk resource IDs under disk reprotection input details.
- Staging storage account resource ID.
- Test-failover virtual network resource ID.
- Any associated identity required by the service.

The VM must already have a discovered `AzureSiteRecovery` solution in `Protected` state. `selectedProtectionSolutionSetting` requires at least one `diskReprotectInputDetails` entry containing a `Microsoft.Compute/disks` ID and a `Microsoft.Storage/storageAccounts` staging ID, plus `testFailoverParams.networkResourceId` for a `Microsoft.Network/virtualNetworks` resource. Do not use this protection type for non-VM resources.

### Protection choice

| Protection type | Use when | Do not use when |
|---|---|---|
| `CustomRunbook` | Runbooks implement resource-level failover and reprotect | Only a group-level pre/post action is needed |
| `AzureSiteRecovery` | The resource is a VM with healthy ASR protection and complete reprotect/test-failover inputs | The resource is not a VM or ASR is not `Protected` |
| `AzureNative` | Never for an included resource in the current tool | Adding or re-including any resource |

## Live-Test Scenario Checklist

Use the smallest fixture that reaches the state required by the command under test:

- Create/update tests: cover `ManualAction` and group-level `CustomRunbook` parsing, preservation on omission, explicit clearing with `[]`, and invalid IDs/parameters.
- Resume success test: configure a `ManualAction` in the relevant recovery group, start an operation that reaches it, wait for the recovery job to become `Paused`, call resume for that same job, and verify it leaves `Paused`. A readiness job or a fabricated job ID cannot validate resume behavior.
- Retry success test: use a real job in `Failed` state and retry that job; do not reuse a completed, paused, or stale job.
- Resource-update tests: exercise inclusion, sparse update, exclusion, re-inclusion, removal, and update/remove conflicts.
- Custom-runbook protection tests: provide mandatory failover/reprotect runbooks and add optional operation runbooks only for operations covered by the test.
- ASR tests: provision an ASR-protected VM and supply valid disk, staging storage, and test-failover network IDs; include negative coverage for non-VM, unhealthy protection, and mistyped resource IDs.
- Recorded tests: preserve generated operation and job IDs through playback variables or sanitizers, inspect recordings for secrets, then verify playback before publishing assets.

## Recovery Resource Removal

`resources-to-remove` is a JSON array of full recovery-resource IDs:

```json
[
  "{full-recovery-resource-id}"
]
```

Removal is different from exclusion. Use removal only when the user wants membership deleted from the recovery plan.

## Array Selectors

Pass source locations and selected resource IDs as tool arrays, not JSON strings:

- `source-locations`: values such as `westus2` or `westus2-az3`, supplied by the user.
- `selected-resource-ids`: full recovery-resource IDs supplied or explicitly selected by the user.
