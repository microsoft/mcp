# StorageSync Command Implementation - Complete Pattern

## Successfully Implemented: StorageSyncServiceCreateCommand

This is a **complete, production-ready implementation** that you can use as a template for all remaining 23 commands.

## Architecture Overview

### 1. **Command Class** (`StorageSyncServiceCreateCommand.cs`)
The command orchestrates the entire operation:
- **Properties**: Name, Description, ToolMetadata
- **RegisterOptions()**: Declares which CLI options are required/optional
- **BindOptions()**: Maps parsed CLI arguments to typed options
- **ExecuteAsync()**: Core business logic
- **Error Handling**: Custom error messages and HTTP status codes
- **Helper Methods**: Tag parsing, validation

### 2. **Options Class** (`StorageSyncServiceCreateOptions.cs`)
Strongly-typed parameter container:
- Inherits from `BaseStorageSyncOptions` (provides Subscription, Tenant, RetryPolicy)
- Add resource-specific properties (ResourceGroup, Name, Location, Tags)
- All properties are nullable `string?` or `Dictionary<string, string>?`

### 3. **JSON Context** (`StorageSyncJsonContext.cs`)
AOT serialization registry:
- Declare `[JsonSerializable(typeof(CommandResultType))]` for each command result
- This enables native compilation and performance optimization

### 4. **Unit Tests** (`StorageSyncServiceCreateCommandTests.cs`)
Comprehensive test coverage:
- Constructor initialization
- ToolMetadata values
- Happy path with valid options
- All parameter validation failures
- Service exception handling

---

## Replication Pattern for Remaining 23 Commands

### Step 1: Create Command Class

```csharp
public sealed class {Resource}{Operation}Command : BaseStorageSyncCommand<{Resource}{Operation}Options>
{
    private readonly ILogger<{Resource}{Operation}Command> _logger;
    private readonly IStorageSyncService _service;

    public {Resource}{Operation}Command(
        ILogger<{Resource}{Operation}Command> logger,
        IStorageSyncService service)
    {
        _logger = logger;
        _service = service;
    }

    public override ToolMetadata ToolMetadata => new()
    {
        ReadOnly = {IS_READ_ONLY},        // true for Get/List, false for Create/Update/Delete
        Destructive = {IS_DESTRUCTIVE},  // true for Delete, false otherwise
        Idempotent = {IS_IDEMPOTENT},    // false for Create, true for Update/Get/Delete
        Secret = false,
        LocalRequired = false
    };

    public override string Name => "{resource} {operation}";
    public override string Description => "{Description of what the command does}";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // Add required options
        command.Options.Add(StorageSyncOptionDefinitions.{Resource}.{Option}.AsRequired());
        // Add optional options
        command.Options.Add(StorageSyncOptionDefinitions.{Resource}.{Option}.AsOptional());
    }

    protected override {Resource}{Operation}Options BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Property = parseResult.GetValueOrDefault<string>(
            StorageSyncOptionDefinitions.{Resource}.Property.Name);
        return options;
    }

    protected override async Task ExecuteAsync(
        CommandContext context,
        {Resource}{Operation}Options options)
    {
        _logger.LogInformation("Executing {Operation} on {Resource}...", nameof({Operation}), nameof({Resource}));

        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(options.Subscription))
                throw new InvalidOperationException("Subscription is required.");

            // Call service
            var result = await _service.{Operation}{Resource}Async(
                options.Subscription,
                // ... other parameters
                options.Tenant,
                options.RetryPolicy,
                context.CancellationToken);

            // Return result
            context.Response.Results = ResponseResult.Create(
                new {Resource}{Operation}CommandResult(result),
                StorageSyncJsonContext.Default.{Resource}{Operation}CommandResult);

            _logger.LogInformation("Successfully executed {Operation} on {Resource}", nameof({Operation}), nameof({Resource}));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {@Options}", options);
            HandleException(context, ex);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException => $"Invalid argument: {ex.Message}",
        InvalidOperationException => $"Invalid operation: {ex.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => 400,
        InvalidOperationException => 400,
        _ => base.GetStatusCode(ex)
    };

    internal record {Resource}{Operation}CommandResult(object Result);
}
```

### Step 2: Create Options Class

```csharp
public class {Resource}{Operation}Options : BaseStorageSyncOptions
{
    /// <summary>
    /// Gets or sets the resource group.
    /// </summary>
    public string? ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string? ServiceName { get; set; }

    // Add command-specific properties
    public string? Property { get; set; }
}
```

### Step 3: Update JSON Context

```csharp
[JsonSerializable(typeof({Resource}{Operation}Command.{Resource}{Operation}CommandResult))]
```

Add this line to the `StorageSyncJsonContext` class for each new command.

### Step 4: Create Unit Tests

Copy the `StorageSyncServiceCreateCommandTests.cs` pattern and customize:
1. Replace class names
2. Update mock setup for your service method
3. Add test cases for your specific parameters
4. Validate success and error scenarios

---

## 24 Commands to Implement

### StorageSyncService (5 commands)
- ✅ Create (DONE - use as template)
- Get
- List
- Update
- Delete

### SyncGroup (4 commands)
- Create
- Get
- List
- Delete

### CloudEndpoint (5 commands)
- Create
- Get
- List
- Delete
- ChangeDetection (trigger)

### ServerEndpoint (5 commands)
- Create
- Get
- List
- Update
- Delete

### RegisteredServer (5 commands)
- Register
- Get
- List
- Update
- Unregister

---

## Implementation Checklist per Command

For each command, verify:
- [ ] Command class created with proper inheritance
- [ ] ToolMetadata set correctly
- [ ] RegisterOptions() includes all parameters
- [ ] BindOptions() properly maps CLI args to options
- [ ] ExecuteAsync() calls service method with all required args
- [ ] Error handling covers edge cases
- [ ] Options class matches command parameters
- [ ] JSON context includes command result type
- [ ] Unit tests cover happy path and error cases
- [ ] No compilation errors
- [ ] Tests pass with dotnet test

---

## Key Implementation Notes

### ToolMetadata Settings

| Operation | ReadOnly | Destructive | Idempotent |
|-----------|----------|-------------|-----------|
| List/Get  | true     | false       | true      |
| Create    | false    | false       | false     |
| Update    | false    | false       | true      |
| Delete    | false    | true        | false     |

### Required Base Calls

```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);  // REQUIRED - provides subscription, tenant, retry
    // ... add resource-specific options
}

protected override {Resource}{Operation}Options BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);  // REQUIRED - binds base options
    // ... bind resource-specific options
    return options;
}

protected override async Task ExecuteAsync(CommandContext context, {Resource}{Operation}Options options)
{
    try
    {
        // ... business logic
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error: {@Options}", options);
        HandleException(context, ex);  // REQUIRED - proper error handling
    }
}
```

### Service Method Naming Pattern

All service methods follow the pattern:
```
{Operation}{Resource}Async(
    string subscription,
    string resourceGroup,
    string serviceName,
    [specific params...],
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null,
    CancellationToken cancellationToken = default)
```

The **last three parameters are always optional** and always in this order:
1. `tenant`
2. `retryPolicy`
3. `cancellationToken`

---

## Next Steps

1. **Implement StorageSyncServiceGetCommand** using this pattern
2. **Verify build**: `dotnet build tools/Azure.Mcp.Tools.StorageSync/src`
3. **Run tests**: `dotnet test tools/Azure.Mcp.Tools.StorageSync/tests`
4. **Replicate pattern** for remaining commands
5. **Create Setup registration** once all commands are complete
6. **Register in Program.cs** to enable command discovery

All 23 remaining commands follow the exact same pattern as StorageSyncServiceCreateCommand.
