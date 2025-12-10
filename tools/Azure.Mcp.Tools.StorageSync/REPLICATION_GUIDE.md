# Replication Guide: Generate All 23 Remaining Commands

## Overview

You now have a **complete, tested, working template** in `StorageSyncServiceCreateCommand`.

This guide shows how to systematically replicate this pattern for all 23 remaining commands.

---

## Phase 1: StorageSyncService Commands (4 remaining)

### Template: StorageSyncServiceCreateCommand ✅

### 1.1 StorageSyncServiceGetCommand

**Copy from**: StorageSyncServiceCreateCommand
**Location**: `src/Commands/StorageSyncService/StorageSyncServiceGetCommand.cs`

**Changes**:
```diff
- public sealed class StorageSyncServiceCreateCommand
+ public sealed class StorageSyncServiceGetCommand

- public override string Name => "storagesyncservice create";
- public override string Description => "Create a new Azure Storage Sync service";
+ public override string Name => "storagesyncservice get";
+ public override string Description => "Get a specific Azure Storage Sync service";

- public override ToolMetadata ToolMetadata => new()
- {
-     ReadOnly = false,
-     Destructive = false,
-     Idempotent = false,
+ public override ToolMetadata ToolMetadata => new()
+ {
+     ReadOnly = true,
+     Destructive = false,
+     Idempotent = true,

- protected override void RegisterOptions(Command command)
- {
-     base.RegisterOptions(command);
-     command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.ResourceGroup.AsRequired());
-     command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
-     command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Location.AsRequired());
-     command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Tags.AsOptional());
- }
+ protected override void RegisterOptions(Command command)
+ {
+     base.RegisterOptions(command);
+     command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.ResourceGroup.AsRequired());
+     command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
+ }

- var result = await _service.CreateStorageSyncServiceAsync(
-     options.Subscription,
-     options.ResourceGroup,
-     options.Name,
-     options.Location,
-     options.Tags,
+ var result = await _service.GetStorageSyncServiceAsync(
+     options.Subscription,
+     options.ResourceGroup,
+     options.Name,

- context.Response.Results = ResponseResult.Create(
-     new StorageSyncServiceCreateCommandResult(result),
-     StorageSyncJsonContext.Default.StorageSyncServiceCreateCommandResult);
+ context.Response.Results = ResponseResult.Create(
+     new StorageSyncServiceGetCommandResult(result),
+     StorageSyncJsonContext.Default.StorageSyncServiceGetCommandResult);

- internal record StorageSyncServiceCreateCommandResult(StorageSyncServiceData Result);
+ internal record StorageSyncServiceGetCommandResult(StorageSyncServiceData Result);
```

**Options**: Copy `StorageSyncServiceCreateOptions` → `StorageSyncServiceGetOptions`
```csharp
public class StorageSyncServiceGetOptions : BaseStorageSyncOptions
{
    public string? ResourceGroup { get; set; }
    public string? Name { get; set; }
    // Remove: Location, Tags
}
```

**JSON Context**: Add line
```csharp
[JsonSerializable(typeof(StorageSyncServiceGetCommand.StorageSyncServiceGetCommandResult))]
```

**Tests**: Copy tests, customize for Get operation (no creation parameters)

---

### 1.2 StorageSyncServiceListCommand

**Already exists**: `src/Commands/StorageSyncService/StorageSyncServiceListCommand.cs` ✅

---

### 1.3 StorageSyncServiceUpdateCommand

**Copy from**: StorageSyncServiceCreateCommand
**Location**: `src/Commands/StorageSyncService/StorageSyncServiceUpdateCommand.cs`

**Key changes**:
- ToolMetadata: `ReadOnly=false, Destructive=false, Idempotent=true`
- Name: "storagesyncservice update"
- RegisterOptions: ResourceGroup, Name (required); IncomingTrafficPolicy, Tags (optional)
- Service call: `UpdateStorageSyncServiceAsync()`
- Options: Add `IncomingTrafficPolicy` property

---

### 1.4 StorageSyncServiceDeleteCommand

**Copy from**: StorageSyncServiceCreateCommand
**Location**: `src/Commands/StorageSyncService/StorageSyncServiceDeleteCommand.cs`

**Key changes**:
- ToolMetadata: `ReadOnly=false, Destructive=true, Idempotent=false`
- Name: "storagesyncservice delete"
- RegisterOptions: Only ResourceGroup, Name (no Location, Tags)
- Service call: `DeleteStorageSyncServiceAsync()` returns Task (void)
- Result: Empty or confirmation message
- Options: Remove Location, Tags

---

## Phase 2: SyncGroup Commands (4 commands)

### 2.1-2.4 SyncGroup Operations

**Resource hierarchy**: StorageSyncService → SyncGroup

**Commands needed**:
- SyncGroupListCommand
- SyncGroupGetCommand
- SyncGroupCreateCommand
- SyncGroupDeleteCommand

**Key differences from StorageSyncService**:
- RegisterOptions includes: ResourceGroup (req), ServiceName (req), GroupName (req for Get/Delete)
- Service calls: `List/Get/Create/DeleteSyncGroupAsync()`
- ToolMetadata same pattern as StorageSyncService

**Template pattern**:
```csharp
// Same structure as StorageSyncService, but with:
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.ResourceGroup.AsRequired());
    command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.ServiceName.AsRequired());
    command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.GroupName.AsRequired());  // for Get/Delete
}
```

---

## Phase 3: CloudEndpoint Commands (5 commands)

### 3.1-3.5 CloudEndpoint Operations

**Resource hierarchy**: StorageSyncService → SyncGroup → CloudEndpoint

**Commands needed**:
- CloudEndpointListCommand
- CloudEndpointGetCommand
- CloudEndpointCreateCommand
- CloudEndpointDeleteCommand
- CloudEndpointChangeDetectionCommand (special operation)

**Unique aspects**:
- Create requires: StorageAccountResourceId, AzureFileShareName
- ChangeDetection: DirectoryPath, FilePaths (list), Recursive (bool)
- RegisterOptions will be more extensive

**Template**:
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.ResourceGroup.AsRequired());
    command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.ServiceName.AsRequired());
    command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.GroupName.AsRequired());
    command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.EndpointName.AsRequired());  // for Get/Delete
    command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.StorageAccountResourceId.AsRequired());  // for Create
    command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.AzureFileShareName.AsRequired());  // for Create
}
```

---

## Phase 4: ServerEndpoint Commands (5 commands)

### 4.1-4.5 ServerEndpoint Operations

**Resource hierarchy**: StorageSyncService → SyncGroup → ServerEndpoint

**Commands needed**:
- ServerEndpointListCommand
- ServerEndpointGetCommand
- ServerEndpointCreateCommand
- ServerEndpointUpdateCommand
- ServerEndpointDeleteCommand

**Unique aspects**:
- Create requires: ServerResourceId, ServerLocalPath
- Update allows: CloudTiering (bool?), VolumeFreeSpacePercent (int?), TierFilesOlderThanDays (int?)
- Cloud tiering configuration

---

## Phase 5: RegisteredServer Commands (5 commands)

### 5.1-5.5 RegisteredServer Operations

**Resource hierarchy**: StorageSyncService → RegisteredServer (no SyncGroup)

**Commands needed**:
- RegisteredServerListCommand (List all registered servers for service)
- RegisteredServerGetCommand
- RegisteredServerRegisterCommand (instead of Create)
- RegisteredServerUpdateCommand
- RegisteredServerUnregisterCommand (instead of Delete)

**Unique aspects**:
- Register/Unregister naming instead of Create/Delete
- ServerId parameter instead of Name
- Properties dict for update operations

---

## Batch Generation Strategy

### Option A: Sequential Implementation (Recommended for Learning)

1. Implement StorageSyncService commands (4) first
   - Get, Update, Delete (Create already done)
   - Validates pattern understanding
   - Small scope for testing

2. Implement SyncGroup commands (4)
   - Introduces hierarchy (needs ServiceName)
   - Same CRUD pattern

3. Implement CloudEndpoint commands (5)
   - More parameters
   - ChangeDetection special operation

4. Implement ServerEndpoint commands (5)
   - Update instead of Create
   - Cloud tiering options

5. Implement RegisteredServer commands (5)
   - Register/Unregister instead of Create/Delete

### Option B: Bulk Generation (After Pattern Mastery)

Once you've done 3-4 commands, use templating to generate all remaining.

Each command follows **exact same structure** with only method names and parameter lists changing.

---

## Verification Checklist After Each Command

```powershell
# Build single project
dotnet build "tools/Azure.Mcp.Tools.StorageSync/src" -v q

# Run specific test class
dotnet test "tools/Azure.Mcp.Tools.StorageSync/tests" --filter "FullyQualifiedName~{CommandName}Tests"

# Check for errors
dotnet build "tools/Azure.Mcp.Tools.StorageSync/src" 2>&1 | findstr "error"
```

---

## Expected Outcomes

After completing all 24 commands:

```
src/Commands/
├── StorageSyncService/
│   ├── StorageSyncServiceListCommand.cs        ✅
│   ├── StorageSyncServiceGetCommand.cs         (Todo)
│   ├── StorageSyncServiceCreateCommand.cs      ✅
│   ├── StorageSyncServiceUpdateCommand.cs      (Todo)
│   └── StorageSyncServiceDeleteCommand.cs      (Todo)
├── SyncGroup/
│   ├── SyncGroupListCommand.cs                 (Todo)
│   ├── SyncGroupGetCommand.cs                  (Todo)
│   ├── SyncGroupCreateCommand.cs               (Todo)
│   └── SyncGroupDeleteCommand.cs               (Todo)
├── CloudEndpoint/
│   ├── CloudEndpointListCommand.cs             (Todo)
│   ├── CloudEndpointGetCommand.cs              (Todo)
│   ├── CloudEndpointCreateCommand.cs           (Todo)
│   ├── CloudEndpointDeleteCommand.cs           (Todo)
│   └── CloudEndpointChangeDetectionCommand.cs  (Todo)
├── ServerEndpoint/
│   ├── ServerEndpointListCommand.cs            (Todo)
│   ├── ServerEndpointGetCommand.cs             (Todo)
│   ├── ServerEndpointCreateCommand.cs          (Todo)
│   ├── ServerEndpointUpdateCommand.cs          (Todo)
│   └── ServerEndpointDeleteCommand.cs          (Todo)
├── RegisteredServer/
│   ├── RegisteredServerListCommand.cs          (Todo)
│   ├── RegisteredServerGetCommand.cs           (Todo)
│   ├── RegisteredServerRegisterCommand.cs      (Todo)
│   ├── RegisteredServerUpdateCommand.cs        (Todo)
│   └── RegisteredServerUnregisterCommand.cs    (Todo)
└── StorageSyncJsonContext.cs                   (Updated for all commands)

src/Options/
├── StorageSyncService/
│   ├── StorageSyncServiceListOptions.cs        ✅
│   ├── StorageSyncServiceGetOptions.cs         ✅
│   ├── StorageSyncServiceCreateOptions.cs      ✅
│   ├── StorageSyncServiceUpdateOptions.cs      (Todo)
│   └── StorageSyncServiceDeleteOptions.cs      (Todo)
├── SyncGroup/                                  (20 options classes)
├── CloudEndpoint/                              (15 options classes)
├── ServerEndpoint/                             (15 options classes)
└── RegisteredServer/                           (15 options classes)

tests/
├── StorageSyncServiceCreateCommandTests.cs     ✅
└── ... 23 more test files (Todo)
```

---

## Time Estimate

- StorageSyncService commands (4): 1 hour
- SyncGroup commands (4): 1 hour
- CloudEndpoint commands (5): 1.5 hours
- ServerEndpoint commands (5): 1.5 hours
- RegisteredServer commands (5): 1.5 hours
- **Total: ~6-7 hours of implementation**

Once you complete the first 3-4 commands, the rest become very mechanical and can be accelerated significantly with copy-paste-modify approach.

---

## Key Files to Keep Open for Reference

1. `StorageSyncServiceCreateCommand.cs` - Your template
2. `StorageSyncServiceCreateOptions.cs` - Options template
3. `StorageSyncOptionDefinitions.cs` - All available options
4. `IStorageSyncService.cs` - All service method signatures
5. `QUICK_REFERENCE.md` - Quick copy-paste structures
6. `IMPLEMENTATION_PATTERN.md` - Detailed patterns

These are everything you need to successfully replicate all 23 remaining commands.
