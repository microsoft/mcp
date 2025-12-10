# Azure Storage Sync - Command Implementation Guide

This document provides the implementation structure for all Azure Storage Sync commands based on new-command.md guidelines.

## Commands Summary

### Storage Sync Services (5 commands)
1. **StorageSyncServiceListCommand** - List storage sync services
2. **StorageSyncServiceGetCommand** - Get a specific storage sync service
3. **StorageSyncServiceCreateCommand** - Create a new storage sync service
4. **StorageSyncServiceUpdateCommand** - Update storage sync service properties
5. **StorageSyncServiceDeleteCommand** - Delete a storage sync service

### Sync Groups (3 commands)
6. **SyncGroupListCommand** - List sync groups
7. **SyncGroupGetCommand** - Get a specific sync group
8. **SyncGroupCreateCommand** - Create a new sync group
9. **SyncGroupDeleteCommand** - Delete a sync group

### Cloud Endpoints (5 commands)
10. **CloudEndpointListCommand** - List cloud endpoints
11. **CloudEndpointGetCommand** - Get a specific cloud endpoint
12. **CloudEndpointCreateCommand** - Create a new cloud endpoint
13. **CloudEndpointDeleteCommand** - Delete a cloud endpoint
14. **CloudEndpointChangeDetectionCommand** - Trigger change detection

### Server Endpoints (5 commands)
15. **ServerEndpointListCommand** - List server endpoints
16. **ServerEndpointGetCommand** - Get a specific server endpoint
17. **ServerEndpointCreateCommand** - Create a new server endpoint
18. **ServerEndpointUpdateCommand** - Update server endpoint properties
19. **ServerEndpointDeleteCommand** - Delete a server endpoint

### Registered Servers (5 commands)
20. **RegisteredServerListCommand** - List registered servers
21. **RegisteredServerGetCommand** - Get a specific registered server
22. **RegisteredServerRegisterCommand** - Register a new server
23. **RegisteredServerUpdateCommand** - Update registered server
24. **RegisteredServerUnregisterCommand** - Unregister a server

## File Structure

```
src/
├── Commands/
│   ├── StorageSyncService/
│   │   ├── StorageSyncServiceListCommand.cs
│   │   ├── StorageSyncServiceGetCommand.cs
│   │   ├── StorageSyncServiceCreateCommand.cs
│   │   ├── StorageSyncServiceUpdateCommand.cs
│   │   └── StorageSyncServiceDeleteCommand.cs
│   ├── SyncGroup/
│   │   ├── SyncGroupListCommand.cs
│   │   ├── SyncGroupGetCommand.cs
│   │   ├── SyncGroupCreateCommand.cs
│   │   └── SyncGroupDeleteCommand.cs
│   ├── CloudEndpoint/
│   │   ├── CloudEndpointListCommand.cs
│   │   ├── CloudEndpointGetCommand.cs
│   │   ├── CloudEndpointCreateCommand.cs
│   │   ├── CloudEndpointDeleteCommand.cs
│   │   └── CloudEndpointChangeDetectionCommand.cs
│   ├── ServerEndpoint/
│   │   ├── ServerEndpointListCommand.cs
│   │   ├── ServerEndpointGetCommand.cs
│   │   ├── ServerEndpointCreateCommand.cs
│   │   ├── ServerEndpointUpdateCommand.cs
│   │   └── ServerEndpointDeleteCommand.cs
│   ├── RegisteredServer/
│   │   ├── RegisteredServerListCommand.cs
│   │   ├── RegisteredServerGetCommand.cs
│   │   ├── RegisteredServerRegisterCommand.cs
│   │   ├── RegisteredServerUpdateCommand.cs
│   │   ├── RegisteredServerUnregisterCommand.cs
│   │   └── StorageSyncJsonContext.cs
│
├── Options/
│   ├── StorageSyncOptionDefinitions.cs
│   ├── StorageSyncService/
│   │   ├── StorageSyncServiceListOptions.cs
│   │   ├── StorageSyncServiceGetOptions.cs
│   │   ├── StorageSyncServiceCreateOptions.cs
│   │   ├── StorageSyncServiceUpdateOptions.cs
│   │   └── StorageSyncServiceDeleteOptions.cs
│   ├── SyncGroup/
│   ├── CloudEndpoint/
│   ├── ServerEndpoint/
│   └── RegisteredServer/
│
├── Services/
│   ├── IStorageSyncService.cs
│   └── StorageSyncService.cs
│
├── Models/
│   ├── StorageSyncServiceModel.cs
│   ├── SyncGroupModel.cs
│   ├── CloudEndpointModel.cs
│   ├── ServerEndpointModel.cs
│   └── RegisteredServerModel.cs
│
├── Commands/BaseStorageSyncCommand.cs
└── StorageSyncSetup.cs

tests/
├── Azure.Mcp.Tools.StorageSync.UnitTests/
│   └── [Test files matching command structure]
├── Azure.Mcp.Tools.StorageSync.LiveTests/
│   └── StorageSyncCommandTests.cs
├── test-resources.bicep
└── test-resources-post.ps1
```

## Naming Conventions Applied

### Command Naming
- Pattern: `{Resource}{Operation}Command`
- Examples: `StorageSyncServiceListCommand`, `SyncGroupCreateCommand`, `CloudEndpointDeleteCommand`

### Options Naming
- Pattern: `{Resource}{Operation}Options`
- Examples: `StorageSyncServiceListOptions`, `SyncGroupCreateOptions`

### Test Naming
- Pattern: `{Resource}{Operation}CommandTests`
- Examples: `StorageSyncServiceListCommandTests`, `SyncGroupCreateCommandTests`

### Command Group Names (MCP)
- Format: lowercase concatenated (no dashes)
- Examples: `storagesyncservice`, `syncgroup`, `cloudendpoint`, `serverendpoint`, `registeredserver`

## ToolMetadata Settings by Command Type

### Read-Only Commands (List, Get)
```csharp
ReadOnly = true,
Destructive = false,
OpenWorld = false,
Idempotent = true,
Secret = false,
LocalRequired = false
```

### Create Commands
```csharp
ReadOnly = false,
Destructive = false,
OpenWorld = false,
Idempotent = false,  // Resource creation may fail if already exists
Secret = false,
LocalRequired = false
```

### Update Commands
```csharp
ReadOnly = false,
Destructive = false,
OpenWorld = false,
Idempotent = true,   // Setting config to specific values
Secret = false,
LocalRequired = false
```

### Delete Commands
```csharp
ReadOnly = false,
Destructive = true,  // Can cause data loss
OpenWorld = false,
Idempotent = false,  // Cannot delete non-existent resource
Secret = false,
LocalRequired = false
```

### Special Commands (ChangeDetection, Trigger)
```csharp
ReadOnly = false,
Destructive = false,
OpenWorld = false,
Idempotent = true,   // Can be run multiple times safely
Secret = false,
LocalRequired = false
```

## Implementation Checklist

- [ ] Create all command classes with proper ToolMetadata
- [ ] Create all options classes with inheritance from BaseStorageSyncOptions
- [ ] Create service interface IStorageSyncService with all methods
- [ ] Create service implementation StorageSyncService
- [ ] Create model classes for data representation
- [ ] Create StorageSyncJsonContext for AOT serialization
- [ ] Create unit tests for all commands
- [ ] Create integration tests with test fixtures
- [ ] Create test-resources.bicep for test infrastructure
- [ ] Create test-resources-post.ps1 for post-deployment setup
- [ ] Register all commands in StorageSyncSetup.cs
- [ ] Register toolset in Program.cs RegisterAreas()
- [ ] Validate CancellationToken usage in all async methods
- [ ] Run `dotnet format` to clean up code
- [ ] Test with `dotnet build` and `dotnet test`
- [ ] Validate with `./eng/scripts/Build-Local.ps1 -BuildNative` for AOT compatibility

## Key Implementation Notes

1. **BaseStorageSyncCommand**: All commands inherit from this base class which extends SubscriptionCommand
2. **StorageSyncOptionDefinitions**: Static class defining all command options
3. **IStorageSyncService**: Service interface with dependency injection
4. **StorageSyncService**: Inherits from BaseAzureResourceService for read operations or BaseAzureService for writes
5. **Cancellation Token**: All async service methods must have CancellationToken as final parameter
6. **JSON Serialization**: All response models must be registered in StorageSyncJsonContext
7. **Error Handling**: Override GetErrorMessage and GetStatusCode for service-specific errors
8. **Test Infrastructure**: Required Bicep template and post-deployment script for Azure resource operations
