# StorageSync Implementation - Quick Reference

## Complete Template Example: StorageSyncServiceCreateCommand

```
┌─────────────────────────────────────────────────────────────┐
│  Command Class: StorageSyncServiceCreateCommand             │
│  ✅ File Created: src/Commands/StorageSyncService/...       │
│  ✅ Tests Created: tests/.../StorageSyncService/...         │
│  ✅ No Compilation Errors                                   │
└─────────────────────────────────────────────────────────────┘

COMMAND FLOW:
┌──────────────┐
│  CLI Input   │ --subscription sub1 --resource-group rg1 --name sync1 --location eastus --tags env=test
└──────┬───────┘
       │
       ▼
┌──────────────────────────┐
│  RegisterOptions()       │ Declare which CLI options are accepted
│  • Required options      │ • --subscription (base)
│  • Optional options      │ • --resource-group (required)
└──────┬───────────────────┘ • --name (required)
       │                      • --location (required)
       │                      • --tags (optional)
       ▼
┌──────────────────────────┐
│  BindOptions()           │ Map CLI args to typed options object
│  ParseResult → Options   │ Creates StorageSyncServiceCreateOptions
└──────┬───────────────────┘
       │
       ▼
┌──────────────────────────┐
│  ExecuteAsync()          │ Core business logic
│  Validate parameters     │ 1. Check subscription required
│  Call service method     │ 2. Check resourceGroup required
│  Return result           │ 3. Check name required
└──────┬───────────────────┘ 4. Check location required
       │                      5. Call _service.CreateAsync()
       │                      6. Return result
       ▼
┌──────────────────────────┐
│  Response               │ {
│  JSON-serialized result │   "result": {
└──────────────────────────┘     "id": "/subscriptions/...",
                                   "name": "sync1",
                                   "location": "eastus",
                                   "tags": {"env": "test"}
                                 }
                               }

FILE DEPENDENCIES:
─────────────────────────────────────────────────
Command Class (170 lines)
    ├── depends on → Options Class
    ├── depends on → IStorageSyncService
    ├── depends on → BaseStorageSyncCommand (abstract base)
    └── defines → StorageSyncServiceCreateCommandResult (record)
         └── depends on → StorageSyncServiceData model

Options Class (28 lines)
    └── extends → BaseStorageSyncOptions

JSON Context (updated)
    └── registers → StorageSyncServiceCreateCommandResult

Unit Tests (220 lines)
    ├── tests → Command initialization
    ├── tests → ToolMetadata
    ├── tests → Happy path execution
    ├── tests → Parameter validation (4 tests)
    └── tests → Exception handling

REPLICATION FOR OTHER COMMANDS:
──────────────────────────────

For StorageSyncServiceGetCommand:
1. Copy StorageSyncServiceCreateCommand
2. Replace "Create" → "Get"
3. Change ToolMetadata: ReadOnly=true, Destructive=false, Idempotent=true
4. Change RegisterOptions: Remove tags, make all required
5. Change service call: _service.GetStorageSyncServiceAsync()
6. Update result type
7. Copy and modify tests

For StorageSyncServiceDeleteCommand:
1. Copy StorageSyncServiceCreateCommand
2. Replace "Create" → "Delete"
3. Change ToolMetadata: ReadOnly=false, Destructive=true, Idempotent=false
4. Change RegisterOptions: Only subscription, resourceGroup, name
5. Change service call: _service.DeleteStorageSyncServiceAsync()
6. Return void/empty result
7. Copy and modify tests

TOOL METADATA REFERENCE TABLE:
────────────────────────────

Operation        ReadOnly  Destructive  Idempotent  Example
─────────────────────────────────────────────────────────────
List             true      false        true        StorageSyncServiceListCommand
Get              true      false        true        StorageSyncServiceGetCommand
Create           false     false        false       StorageSyncServiceCreateCommand ✅
Update           false     false        true        StorageSyncServiceUpdateCommand
Delete           false     true         false       StorageSyncServiceDeleteCommand

COMMAND NAMING CONVENTIONS:
──────────────────────────

Resource         Operation      Command Class Name              CLI Name
─────────────────────────────────────────────────────────────────────────
StorageSyncService  Create    StorageSyncServiceCreateCommand  storagesyncservice create
StorageSyncService  Get       StorageSyncServiceGetCommand     storagesyncservice get
SyncGroup          Create    SyncGroupCreateCommand            syncgroup create
CloudEndpoint      List      CloudEndpointListCommand          cloudendpoint list
ServerEndpoint     Update    ServerEndpointUpdateCommand       serverendpoint update
RegisteredServer   Register  RegisteredServerRegisterCommand   registeredserver register

PARAMETER BINDING EXAMPLE:
─────────────────────────

Options Class Property      → OptionDefinitions Reference      → CLI Argument
─────────────────────────────────────────────────────────────────────────────
options.Subscription        → (from base)                      → (from base)
options.ResourceGroup       → StorageSyncOptionDefinitions.StorageSyncService.ResourceGroup
                                                                → --resource-group
options.Name                → StorageSyncOptionDefinitions.StorageSyncService.Name
                                                                → --name
options.Location            → StorageSyncOptionDefinitions.StorageSyncService.Location
                                                                → --location
options.Tags                → StorageSyncOptionDefinitions.StorageSyncService.Tags
                                                                → --tags

VALIDATION PATTERN:
──────────────────

protected override async Task ExecuteAsync(CommandContext context, {Options} options)
{
    try
    {
        // Always validate required parameters first
        if (string.IsNullOrWhiteSpace(options.Subscription))
            throw new InvalidOperationException("Subscription is required.");

        if (string.IsNullOrWhiteSpace(options.ResourceGroup))
            throw new InvalidOperationException("Resource group is required.");

        // Call service
        var result = await _service.{Operation}{Resource}Async(
            options.Subscription,
            options.ResourceGroup,
            // ... other parameters
            options.Tenant,
            options.RetryPolicy,
            context.CancellationToken);

        // Return result
        context.Response.Results = ResponseResult.Create(
            new {Resource}{Operation}CommandResult(result),
            StorageSyncJsonContext.Default.{Resource}{Operation}CommandResult);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error: {@Options}", options);
        HandleException(context, ex);
    }
}

QUICK CHECKLIST FOR NEW COMMAND:
────────────────────────────────

□ Command class created
  □ Inherits from BaseStorageSyncCommand<{Operation}Options>
  □ Dependency injection: ILogger, IStorageSyncService
  □ Name and Description properties set
  □ ToolMetadata configured correctly
  □ RegisterOptions() calls base.RegisterOptions() first
  □ BindOptions() calls base.BindOptions() first
  □ ExecuteAsync() validates parameters
  □ ExecuteAsync() calls service method
  □ ExecuteAsync() catches exceptions with try/catch
  □ Error handling methods implemented

□ Options class created
  □ Inherits from BaseStorageSyncOptions
  □ All properties are nullable
  □ XML comments on all properties

□ JSON Context updated
  □ [JsonSerializable(...)] attribute added

□ Unit tests created
  □ Constructor test
  □ ToolMetadata test
  □ Happy path test
  □ Parameter validation tests
  □ Exception handling test

□ Compilation
  □ dotnet build succeeds
  □ No warnings

□ Tests pass
  □ dotnet test passes all tests
```

---

## Implementation Statistics

| Metric | Value |
|--------|-------|
| Command class size | 170 lines |
| Options class size | 28 lines |
| Unit tests | 8 test methods |
| Total new code | ~450 lines |
| Compilation errors | 0 |
| Test coverage | Command init, metadata, happy path, all validations, error handling |

---

## Files to Reference

1. **IMPLEMENTATION_PATTERN.md** - Detailed replication instructions
2. **IMPLEMENTATION_COMPLETE.md** - Full component breakdown
3. **CODE_GENERATION_GUIDE.md** - All 24 commands specification
4. **StorageSyncServiceCreateCommand.cs** - The working example
5. **StorageSyncServiceCreateCommandTests.cs** - Complete test pattern

---

## Next Steps

1. Review StorageSyncServiceCreateCommand.cs thoroughly
2. Review unit tests to understand test patterns
3. Implement StorageSyncServiceGetCommand using same pattern
4. Repeat for all 23 remaining commands
5. Create StorageSyncSetup.cs registration
6. Update Program.cs
7. Validate AOT compatibility
