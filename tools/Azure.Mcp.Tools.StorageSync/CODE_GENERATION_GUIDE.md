# Azure Storage Sync - Code Generation Guide

This guide provides detailed instructions for implementing all 24 Storage Sync commands following the new-command.md guidelines.

## Quick Start

1. **Review Requirements**
   - Read `new-command.md` completely
   - Review `IMPLEMENTATION_GUIDE.md`
   - Study existing command implementations (StorageSyncServiceListCommand example)

2. **Use Templates**
   - Copy `COMMAND_TEMPLATE.cs` for each new command
   - Copy `OPTIONS_TEMPLATE.cs` for each options class
   - Copy test template for unit tests

3. **Validate**
   - Run `dotnet build`
   - Run `dotnet test`
   - Run `./eng/scripts/Build-Local.ps1 -BuildNative` for AOT compatibility

## Command Implementation Details

### 1. Storage Sync Service Commands

#### StorageSyncServiceListCommand
- **File**: `src/Commands/StorageSyncService/StorageSyncServiceListCommand.cs`
- **Options**: `StorageSyncServiceListOptions`
- **Service Method**: `ListStorageSyncServicesAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription (required), resourceGroup (optional)
- **Returns**: List<StorageSyncServiceData>

#### StorageSyncServiceGetCommand
- **File**: `src/Commands/StorageSyncService/StorageSyncServiceGetCommand.cs`
- **Options**: `StorageSyncServiceGetOptions`
- **Service Method**: `GetStorageSyncServiceAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName (all required)
- **Returns**: StorageSyncServiceData

#### StorageSyncServiceCreateCommand
- **File**: `src/Commands/StorageSyncService/StorageSyncServiceCreateCommand.cs`
- **Options**: `StorageSyncServiceCreateOptions`
- **Service Method**: `CreateStorageSyncServiceAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, location (required); tags (optional)
- **Returns**: StorageSyncServiceData

#### StorageSyncServiceUpdateCommand
- **File**: `src/Commands/StorageSyncService/StorageSyncServiceUpdateCommand.cs`
- **Options**: `StorageSyncServiceUpdateOptions`
- **Service Method**: `UpdateStorageSyncServiceAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName (required); incomingTrafficPolicy, tags (optional)
- **Returns**: StorageSyncServiceData

#### StorageSyncServiceDeleteCommand
- **File**: `src/Commands/StorageSyncService/StorageSyncServiceDeleteCommand.cs`
- **Options**: `StorageSyncServiceDeleteOptions`
- **Service Method**: `DeleteStorageSyncServiceAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=true, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName (all required)
- **Returns**: void

---

### 2. Sync Group Commands

#### SyncGroupListCommand
- **File**: `src/Commands/SyncGroup/SyncGroupListCommand.cs`
- **Options**: `SyncGroupListOptions`
- **Service Method**: `ListSyncGroupsAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName (all required)
- **Returns**: List<SyncGroupData>

#### SyncGroupGetCommand
- **File**: `src/Commands/SyncGroup/SyncGroupGetCommand.cs`
- **Options**: `SyncGroupGetOptions`
- **Service Method**: `GetSyncGroupAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName (all required)
- **Returns**: SyncGroupData

#### SyncGroupCreateCommand
- **File**: `src/Commands/SyncGroup/SyncGroupCreateCommand.cs`
- **Options**: `SyncGroupCreateOptions`
- **Service Method**: `CreateSyncGroupAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, groupName (all required)
- **Returns**: SyncGroupData

#### SyncGroupDeleteCommand
- **File**: `src/Commands/SyncGroup/SyncGroupDeleteCommand.cs`
- **Options**: `SyncGroupDeleteOptions`
- **Service Method**: `DeleteSyncGroupAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=true, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, groupName (all required)
- **Returns**: void

---

### 3. Cloud Endpoint Commands

#### CloudEndpointListCommand
- **File**: `src/Commands/CloudEndpoint/CloudEndpointListCommand.cs`
- **Options**: `CloudEndpointListOptions`
- **Service Method**: `ListCloudEndpointsAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName (all required)
- **Returns**: List<CloudEndpointData>

#### CloudEndpointGetCommand
- **File**: `src/Commands/CloudEndpoint/CloudEndpointGetCommand.cs`
- **Options**: `CloudEndpointGetOptions`
- **Service Method**: `GetCloudEndpointAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName (all required)
- **Returns**: CloudEndpointData

#### CloudEndpointCreateCommand
- **File**: `src/Commands/CloudEndpoint/CloudEndpointCreateCommand.cs`
- **Options**: `CloudEndpointCreateOptions`
- **Service Method**: `CreateCloudEndpointAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName, storageAccountResourceId, azureFileShareName (required)
- **Returns**: CloudEndpointData

#### CloudEndpointDeleteCommand
- **File**: `src/Commands/CloudEndpoint/CloudEndpointDeleteCommand.cs`
- **Options**: `CloudEndpointDeleteOptions`
- **Service Method**: `DeleteCloudEndpointAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=true, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName (all required)
- **Returns**: void

#### CloudEndpointChangeDetectionCommand
- **File**: `src/Commands/CloudEndpoint/CloudEndpointChangeDetectionCommand.cs`
- **Options**: `CloudEndpointChangeDetectionOptions`
- **Service Method**: `TriggerChangeDetectionAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName (required); directoryPath, filePaths, recursive (optional)
- **Returns**: void

---

### 4. Server Endpoint Commands

#### ServerEndpointListCommand
- **File**: `src/Commands/ServerEndpoint/ServerEndpointListCommand.cs`
- **Options**: `ServerEndpointListOptions`
- **Service Method**: `ListServerEndpointsAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName (all required)
- **Returns**: List<ServerEndpointData>

#### ServerEndpointGetCommand
- **File**: `src/Commands/ServerEndpoint/ServerEndpointGetCommand.cs`
- **Options**: `ServerEndpointGetOptions`
- **Service Method**: `GetServerEndpointAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName (all required)
- **Returns**: ServerEndpointData

#### ServerEndpointCreateCommand
- **File**: `src/Commands/ServerEndpoint/ServerEndpointCreateCommand.cs`
- **Options**: `ServerEndpointCreateOptions`
- **Service Method**: `CreateServerEndpointAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName, serverResourceId, serverLocalPath (required); cloudTiering, volumeFreeSpacePercent, tierFilesOlderThanDays (optional)
- **Returns**: ServerEndpointData

#### ServerEndpointUpdateCommand
- **File**: `src/Commands/ServerEndpoint/ServerEndpointUpdateCommand.cs`
- **Options**: `ServerEndpointUpdateOptions`
- **Service Method**: `UpdateServerEndpointAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName (required); cloudTiering, volumeFreeSpacePercent, tierFilesOlderThanDays (optional)
- **Returns**: ServerEndpointData

#### ServerEndpointDeleteCommand
- **File**: `src/Commands/ServerEndpoint/ServerEndpointDeleteCommand.cs`
- **Options**: `ServerEndpointDeleteOptions`
- **Service Method**: `DeleteServerEndpointAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=true, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, groupName, endpointName (all required)
- **Returns**: void

---

### 5. Registered Server Commands

#### RegisteredServerListCommand
- **File**: `src/Commands/RegisteredServer/RegisteredServerListCommand.cs`
- **Options**: `RegisteredServerListOptions`
- **Service Method**: `ListRegisteredServersAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName (all required)
- **Returns**: List<RegisteredServerData>

#### RegisteredServerGetCommand
- **File**: `src/Commands/RegisteredServer/RegisteredServerGetCommand.cs`
- **Options**: `RegisteredServerGetOptions`
- **Service Method**: `GetRegisteredServerAsync`
- **ToolMetadata**: ReadOnly=true, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, serverId (all required)
- **Returns**: RegisteredServerData

#### RegisteredServerRegisterCommand
- **File**: `src/Commands/RegisteredServer/RegisteredServerRegisterCommand.cs`
- **Options**: `RegisteredServerRegisterOptions`
- **Service Method**: `RegisterServerAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, serverId (all required)
- **Returns**: RegisteredServerData

#### RegisteredServerUpdateCommand
- **File**: `src/Commands/RegisteredServer/RegisteredServerUpdateCommand.cs`
- **Options**: `RegisteredServerUpdateOptions`
- **Service Method**: `UpdateServerAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=false, Idempotent=true
- **Parameters**: subscription, resourceGroup, serviceName, serverId (required); properties (optional)
- **Returns**: RegisteredServerData

#### RegisteredServerUnregisterCommand
- **File**: `src/Commands/RegisteredServer/RegisteredServerUnregisterCommand.cs`
- **Options**: `RegisteredServerUnregisterOptions`
- **Service Method**: `UnregisterServerAsync`
- **ToolMetadata**: ReadOnly=false, Destructive=true, Idempotent=false
- **Parameters**: subscription, resourceGroup, serviceName, serverId (all required)
- **Returns**: void

---

## File Structure Summary

```
24 Command Classes (5 resource types × ~4.8 commands each)
24 Options Classes (one per command)
24 Unit Test Classes (one per command)
1 StorageSyncJsonContext (aggregates all result types)
1 IStorageSyncService (service interface)
1 StorageSyncService (service implementation)
1 StorageSyncSetup (command registration)
1 BaseStorageSyncCommand (base class)
1 BaseStorageSyncOptions (base options)
1 StorageSyncOptionDefinitions (static option definitions)
```

## Key Guidelines

1. **CancellationToken**: Always include as final parameter in async methods
2. **Option Naming**: Follow pattern `{Resource}{Operation}Options`
3. **Command Naming**: Follow pattern `{Resource}{Operation}Command`
4. **ToolMetadata**: Set all properties, even if using defaults
5. **Error Handling**: Override GetErrorMessage and GetStatusCode
6. **JSON Serialization**: Register all result types in StorageSyncJsonContext
7. **Tests**: Create unit tests for initialization, validation, and execution
8. **Documentation**: Add XML comments to all public members

## Testing Checklist

- [ ] Unit tests for all commands
- [ ] Integration tests with live resources
- [ ] Error handling tests
- [ ] Parameter validation tests
- [ ] AOT compatibility testing
- [ ] Performance testing for large result sets
