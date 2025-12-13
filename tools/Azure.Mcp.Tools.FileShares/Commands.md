# Azure File Shares Commands Reference

This document provides a technical reference for all commands implemented in the Azure MCP FileShares toolset. Each command follows the patterns and guidelines defined in [new-command.md](../../servers/Azure.Mcp.Server/docs/new-command.md).

## File Share Management Commands

### FileShareListCommand
**Command Pattern:** `azmcp fileshares list`

**Description:** List all FileShare resources in a subscription.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. Supports both subscription IDs and names for flexible subscription resolution. |
| `resourceGroup` | `string` | ❌ No | Filter results by resource group name. If not provided, lists all file shares in the subscription. |
| `filter` | `string` | ❌ No | Optional filter expression to apply to the results. |

**Response:** Returns a list of FileShare resources with properties including name, location, provisioning state, and tags.

**ToolMetadata Settings:**
- `OpenWorld`: `false` (well-defined Azure resource domain)
- `ReadOnly`: `true` (read-only operation)
- `Destructive`: `false` (no resource modifications)
- `Idempotent`: `true` (repeatable with same results)
- `Secret`: `false` (returns non-sensitive data)
- `LocalRequired`: `false` (pure cloud API operation)

---

### FileShareGetCommand
**Command Pattern:** `azmcp fileshares get`

**Description:** Get a specific FileShare resource by name.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group containing the file share. |
| `name` | `string` | ✅ Yes | The resource name of the file share as seen through Azure Resource Manager. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |

**Response:** Returns detailed information about the specified FileShare resource including properties, provisioning state, location, and tags.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareCreateOrUpdateCommand
**Command Pattern:** `azmcp fileshares create` or `azmcp fileshares update`

**Description:** Create or update a file share resource. This is a long-running operation.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group where the file share will be created or updated. |
| `name` | `string` | ✅ Yes | The resource name of the file share. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `location` | `string` | ✅ Yes | Azure region where the file share will be deployed (e.g., 'eastus', 'westus2'). |
| `properties` | `object` | ❌ No | File share properties including provisioning parameters, quotas, and metadata. |
| `tags` | `object` | ❌ No | Resource tags as key-value pairs for organizing and billing purposes. |

**Response:** Returns the created or updated FileShare resource. Returns HTTP 201 on creation, 200 on update. This operation is asynchronous with long-running operation tracking.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `false` (creates/modifies resources)
- `Destructive`: `false`
- `Idempotent`: `false` (long-running operation may require polling)
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareDeleteCommand
**Command Pattern:** `azmcp fileshares delete`

**Description:** Delete a FileShare resource. This is a long-running destructive operation.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group containing the file share to delete. |
| `name` | `string` | ✅ Yes | The resource name of the file share to delete. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `force` | `boolean` | ❌ No | If true, forces deletion without waiting for graceful shutdown. Default: `false` |

**Response:** Returns HTTP 202 (Accepted) with location header for tracking the deletion operation. Returns HTTP 204 if the resource does not exist.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `false`
- `Destructive`: `true` (permanently deletes the file share)
- `Idempotent`: `false` (long-running operation)
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareCheckNameAvailabilityCommand
**Command Pattern:** `azmcp fileshares checkname`

**Description:** Check if a file share name is available in a specific location.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `location` | `string` | ✅ Yes | Azure region where name availability should be checked. |
| `name` | `string` | ✅ Yes | The proposed file share name to check for availability. |
| `type` | `string` | ❌ No | Resource type being checked. Default: `Microsoft.FileShares/fileShares` |

**Response:** Returns a CheckNameAvailabilityResponse indicating whether the name is available, and if not, provides a message and list of available alternatives.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

## File Share Snapshot Commands

### FileShareSnapshotListCommand
**Command Pattern:** `azmcp fileshares snapshots list`

**Description:** List all snapshots for a specific FileShare resource.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group containing the file share. |
| `fileShareName` | `string` | ✅ Yes | The resource name of the parent file share. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `filter` | `string` | ❌ No | Optional filter expression to apply to snapshots. |

**Response:** Returns a list of FileShareSnapshot resources with metadata including creation time, snapshot ID, and properties.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareSnapshotGetCommand
**Command Pattern:** `azmcp fileshares snapshots get`

**Description:** Get a specific FileShareSnapshot by name.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group containing the file share. |
| `fileShareName` | `string` | ✅ Yes | The resource name of the parent file share. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `snapshotName` | `string` | ✅ Yes | The name of the snapshot. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |

**Response:** Returns detailed information about the specified FileShareSnapshot including creation time, snapshot properties, and restoration details.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareSnapshotCreateCommand
**Command Pattern:** `azmcp fileshares snapshots create`

**Description:** Create a snapshot of a FileShare. This is a long-running operation.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group containing the file share. |
| `fileShareName` | `string` | ✅ Yes | The resource name of the parent file share. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `snapshotName` | `string` | ✅ Yes | The name for the new snapshot. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `properties` | `object` | ❌ No | Snapshot properties and configuration. |

**Response:** Returns HTTP 202 (Accepted) with Azure-AsyncOperation header for tracking the snapshot creation. Returns the created FileShareSnapshot resource upon completion.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `false`
- `Destructive`: `false`
- `Idempotent`: `false` (long-running operation)
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareSnapshotDeleteCommand
**Command Pattern:** `azmcp fileshares snapshots delete`

**Description:** Delete a FileShareSnapshot. This is a long-running operation.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `resourceGroup` | `string` | ✅ Yes | Name of the resource group containing the file share. |
| `fileShareName` | `string` | ✅ Yes | The resource name of the parent file share. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |
| `snapshotName` | `string` | ✅ Yes | The name of the snapshot to delete. Must match pattern: `^([a-z]\|[0-9])([a-z]\|[0-9]\|(-(?!-))){1,61}([a-z]\|[0-9])$` |

**Response:** Returns HTTP 202 (Accepted) with location header for tracking the deletion operation.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `false`
- `Destructive`: `true` (permanently deletes the snapshot)
- `Idempotent`: `false` (long-running operation)
- `Secret`: `false`
- `LocalRequired`: `false`

---

## Informational Operations Commands

### FileShareGetLimitsCommand
**Command Pattern:** `azmcp fileshares getlimits`

**Description:** Get file shares limits and quotas for a specific location.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `location` | `string` | ✅ Yes | Azure region for which to retrieve limits. |

**Response:** Returns FileShareLimitsResponse containing maximum provisioning parameters, quota limits, and regional capacity information.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareGetProvisioningRecommendationCommand
**Command Pattern:** `azmcp fileshares getprovisioningrecommendation`

**Description:** Get provisioning parameters recommendation for file shares based on workload requirements.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `location` | `string` | ✅ Yes | Azure region for which to get recommendations. |
| `workloadProfile` | `string` | ✅ Yes | Workload profile type (e.g., 'TransactionHeavy', 'SequentialRead', 'RandomMixed'). |
| `estimatedThroughput` | `integer` | ❌ No | Estimated required throughput in operations per second. |
| `estimatedSize` | `integer` | ❌ No | Estimated file share size in GB. |

**Response:** Returns FileShareProvisioningRecommendationResponse with recommended provisioning parameters, tier selection, and configuration details.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

### FileShareGetUsageDataCommand
**Command Pattern:** `azmcp fileshares getusagedata`

**Description:** Get aggregate usage data and metrics for file shares in a location.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `subscription` | `string` | ✅ Yes | Azure subscription ID or name. |
| `location` | `string` | ✅ Yes | Azure region for which to retrieve usage data. |
| `timeRange` | `string` | ❌ No | Time range for usage data ('Last7Days', 'Last30Days', 'Last90Days'). Default: 'Last30Days' |

**Response:** Returns FileShareUsageDataResponse containing aggregate metrics including total provisioned storage, used capacity, transaction counts, and throughput utilization.

**ToolMetadata Settings:**
- `OpenWorld`: `false`
- `ReadOnly`: `true`
- `Destructive`: `false`
- `Idempotent`: `true`
- `Secret`: `false`
- `LocalRequired`: `false`

---

## Implementation Guidelines

All commands in this toolset follow the patterns and requirements defined in [new-command.md](../../servers/Azure.Mcp.Server/docs/new-command.md):

1. **Parameter Naming**: Always use `subscription` (never `subscriptionId`) to support both IDs and names.
2. **Options Pattern**: Commands use static OptionDefinitions and extension methods (`.AsRequired()`, `.AsOptional()`).
3. **Error Handling**: Commands override `GetErrorMessage` and `GetStatusCode` for service-specific error handling.
4. **JSON Serialization**: All response models are registered in `FileSharesJsonContext` for AOT safety.
5. **Long-Running Operations**: Commands that create/update/delete resources handle async operation tracking.
6. **Testing**: Unit tests validate input validation, binding, and error handling. Live tests validate Azure resource interactions.

## Service Base Classes

- **Read Operations**: Inherit from `BaseAzureResourceService` for Resource Graph queries
- **Write Operations**: Inherit from `BaseAzureService` for ARM client operations

All service methods include `CancellationToken` parameters as the final argument for proper async cancellation support.
