<!-- Copyright (c) Microsoft Corporation.
<!-- Licensed under the MIT License. -->

# Implementing a New Command in Azure MCP

This document is the authoritative guide for adding new commands ("toolset commands") to Azure MCP. Follow it exactly to ensure consistency, testability, AOT safety, and predictable user experience.

## Toolset Pattern: Organizing code by toolset

All new Azure services and their commands should use the Toolset pattern:

- **Toolset code** goes in `tools/Azure.Mcp.Tools.{Toolset}/src` (e.g., `tools/Azure.Mcp.Tools.Storage/src`)
- **Tests** go in `tools/Azure.Mcp.Tools.{Toolset}/tests`, divided into UnitTests and LiveTests:
  -  `tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.UnitTests` (e.g., `tools/Azure.Mcp.Tools.Storage/tests/Azure.Mcp.Tools.Storage.UnitTests`)
  -  `tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.LiveTests` (e.g., `tools/Azure.Mcp.Tools.Storage/tests/Azure.Mcp.Tools.Storage.LiveTests`)

This keeps all code, options, models, JSON serialization contexts, and tests for a toolset together. See `tools/Azure.Mcp.Tools.Storage` for a reference implementation.

## ⚠️ Test Infrastructure Requirements

**CRITICAL DECISION POINT**: Does your command interact with Azure resources?

### **Azure Service Commands (REQUIRES Test Infrastructure)**
If your command interacts with Azure resources (storage accounts, databases, VMs, etc.):
- ✅ **MUST create** `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`
- ✅ **MUST create** `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1` (required even if basic template)
- ✅ **MUST include** RBAC role assignments for test application
- ✅ **MUST validate** with `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`
- ✅ **MUST test deployment** with `./eng/scripts/Deploy-TestResources.ps1 -Tool 'Azure.Mcp.Tools.{Toolset}'`

### **Non-Azure Commands (No Test Infrastructure Needed)**
If your command is a wrapper/utility (CLI tools, best practices, documentation):
- ❌ **Skip** Bicep template creation
- ❌ **Skip** live test infrastructure
- ✅ **Focus on** unit tests and mock-based testing

**Examples of each type**:
- **Azure Service Commands**: ACR Registry List, SQL Database Show, Storage Account List
- **Non-Azure Commands**: Azure CLI wrapper, Best Practices guidance, Documentation tools

## Command Architecture

### Command Design Principles

1. **Command Interface**
   - `IBaseCommand` serves as the root interface with core command capabilities:
     - `Name`: Command name for CLI display
     - `Description`: Detailed command description
     - `Title`: Human-readable command title
     - `Metadata`: Behavioral characteristics of the command
     - `GetCommand()`: Retrieves System.CommandLine command definition
     - `ExecuteAsync()`: Executes command logic
     - `Validate()`: Validates command inputs

2. **Command Hierarchy**
    All commands implement the layered hierarchy:
     ```
     IBaseCommand
     └── BaseCommand
         └── GlobalCommand<TOptions>
             └── SubscriptionCommand<TOptions>
                 └── Service-specific base commands (e.g., BaseSqlCommand)
                     └── Resource-specific commands (e.g., SqlIndexRecommendCommand)
     ```

   IMPORTANT:
   - Commands use primary constructors with ILogger injection
   - Classes are always sealed unless explicitly intended for inheritance
   - Commands inheriting from `SubscriptionCommand` must handle subscription parameters
   - Service-specific base commands should add service-wide options
   - Commands return `ToolMetadata` property to define their behavioral characteristics

3. **Command Pattern**
    Commands follow the Model-Context-Protocol (MCP) pattern with this execution naming convention:
   ```
   azmcp <azure service> <resource> <operation>
   ```
   Example: `azmcp storage container get`

   Where:
   - `azure service`: Azure service name (lowercase, e.g., storage, cosmos, kusto)
   - `resource`: Resource type (singular noun, lowercase)
   - `operation`: Action to perform (verb, lowercase)

   Each command is:
   - In code, to avoid ambiguity between service classes and Azure services, we refer to Azure services as Toolsets
   - Registered in the `RegisterCommands` method of its toolset's `tools/Azure.Mcp.Tools.{Toolset}/src/{Toolset}Setup.cs` file
   - Organized in a hierarchy of command groups
   - Documented with a title, description, and examples
   - Validated before execution
   - Returns a standardized response format

   **IMPORTANT**: Command group names use concatenated names or dash separated names. Do not use underscores:
   - ✅ Good: `new CommandGroup("entraadmin", "Entra admin operations")`
   - ✅ Good: `new CommandGroup("resourcegroup", "Resource group operations")`
   - ✅ Good:`new CommandGroup("entra-admin", "Entra admin operations")`
   - ❌ Bad: `new CommandGroup("entra_admin", "Entra admin operations")`

   **AVOID ANTI-PATTERNS**: When designing commands, keep resource names separated from operation names. Use proper command group hierarchy:
   - ✅ Good: `azmcp postgres server param set` (command groups: server → param, operation: set)
   - ❌ Bad: `azmcp postgres server setparam` (mixed operation `setparam` at same level as resource operations)
   - ✅ Good: `azmcp storage blob upload permission set`
   - ❌ Bad: `azmcp storage blobupload`

   This pattern improves discoverability, maintains consistency, and allows for better grouping of related operations.

### Required Files

Every new command (whether purely computational or Azure-resource backed) requires the following elements:

1. OptionDefinitions static class: `tools/Azure.Mcp.Tools.{Toolset}/src/Options/{Toolset}OptionDefinitions.cs`
2. Options class: `tools/Azure.Mcp.Tools.{Toolset}/src/Options/{Resource}/{Operation}Options.cs`
3. Command class: `tools/Azure.Mcp.Tools.{Toolset}/src/Commands/{Resource}/{Resource}{Operation}Command.cs`
4. Service interface: `tools/Azure.Mcp.Tools.{Toolset}/src/Services/I{ServiceName}Service.cs`
5. Service implementation: `tools/Azure.Mcp.Tools.{Toolset}/src/Services/{ServiceName}Service.cs`
    - Most toolsets have one primary service; some may have multiple where domain boundaries justify separation
6. Unit test: `tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.UnitTests/{Resource}/{Resource}{Operation}CommandTests.cs`
7. Integration test: `tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.LiveTests/{Toolset}CommandTests.cs`
8. Command registration in RegisterCommands(): `tools/Azure.Mcp.Tools.{Toolset}/src/{Toolset}Setup.cs`
9. Toolset registration in RegisterAreas(): `servers/Azure.Mcp.Server/src/Program.cs`
10. **Live test infrastructure** (for Azure service commands):
   - Bicep template: `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`
   - Post-deployment script: `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1` (required, even if basic template)

### File and Class Naming Convention

Primary pattern: **{Resource}{SubResource?}{Operation}Command**

Where:
- Resource = top-level domain entity (e.g., `Server`, `Database`, `FileSystem`)
- SubResource (optional) = nested concept (e.g., `Config`, `Param`, `SubnetSize`)
- Operation = action or computed intent (e.g., `List`, `Get`, `Set`, `Recommend`, `Calculate`, `SubnetSize`)

Acceptable Operation Forms:
- Standard verbs (`List`, `Get`, `Set`, `Show`, `Delete`)
- Domain-calculation nouns treated as operations when producing computed output (e.g., `SubnetSize` in `FileSystemSubnetSizeCommand` producing required size calculation)

Examples:
- ✅ `ServerListCommand`
- ✅ `ServerConfigGetCommand`
- ✅ `ServerParamSetCommand`
- ✅ `TableSchemaGetCommand`
- ✅ `DatabaseListCommand`
- ✅ `FileSystemSubnetSizeCommand` (computational operation on a resource)

Avoid:
- ❌ `GetConfigCommand` (missing resource)
- ❌ `ListServerCommand` (verb precedes resource)
- ❌ `FileSystemRequiredSubnetSizeCommand` (overly verbose – prefer concise subresource `SubnetSize`)

Apply pattern consistently to:
- Command classes & filenames: `FileSystemListCommand.cs`
- Options classes: `FileSystemListOptions.cs`
- Unit test classes: `FileSystemListCommandTests.cs`

Rationale:
- Predictable discovery in IDE
- Natural grouping by resource
- Supports both CRUD and compute-style operations

**IMPORTANT**: If implementing a new toolset, you must also ensure:
- Required packages are added to `Directory.Packages.props` first
- Models, base commands, and option definitions follow the established patterns
- JSON serialization context includes all new model types
- Service registration in the toolset setup ConfigureServices method
- **Live test infrastructure**: Add Bicep template to `tools/Azure.Mcp.Tools.{Toolset}/tests`
- **Test resource deployment**: Ensure resources are properly configured with RBAC for test application
- **Resource naming**: Follow consistent naming patterns - many services use just `baseName`, while others may need suffixes for disambiguation (e.g., `{baseName}-suffix`)
- **Solution file integration**: Add new projects to `AzureMcp.sln` with proper GUID generation to avoid conflicts
- **Program.cs registration**: Register the new toolset in `Program.cs` `RegisterAreas()` method in alphabetical order (see `Program.cs` `IAreaSetup[] RegisterAreas()`)

## Implementation Guidelines

### 1. Azure Resource Manager Integration

When creating commands that interact with Azure services, you'll need to:

**Package Management:**

For **Resource Graph queries** (using `BaseAzureResourceService`):
- No additional packages required - `Azure.ResourceManager.ResourceGraph` is already included in the core project
- Only add toolset-specific packages if you need direct ARM operations beyond Resource Graph queries
- Example: `<PackageReference Include="Azure.ResourceManager.Sql" />` (only if needed for direct ARM operations)

For **Direct ARM operations** (using `BaseAzureService`):
- Add the appropriate Azure Resource Manager package to `Directory.Packages.props`
  - Example: `<PackageVersion Include="Azure.ResourceManager.Sql" Version="1.3.0" />`
- Add the package reference in `Azure.Mcp.Tools.{Toolset}.csproj`
  - Example: `<PackageReference Include="Azure.ResourceManager.Sql" />`
- **Version Consistency**: Ensure the package version in `Directory.Packages.props` matches across all projects
- **Build Order**: Add the package to `Directory.Packages.props` first, then reference it in project files to avoid build errors

**Service Base Class Selection:**
Choose the appropriate base class for your service based on the operations needed:

1. **For Azure Resource Graph queries** (recommended for resource management operations):
   - Inherit from `BaseAzureResourceService` for services that need to query Azure Resource Graph
   - Automatically provides `ExecuteResourceQueryAsync<T>()` and `ExecuteSingleResourceQueryAsync<T>()` methods
   - Handles subscription resolution, tenant lookup, and Resource Graph query execution
   - Example:
   ```csharp
   public class MyService(ISubscriptionService subscriptionService, ITenantService tenantService)
       : BaseAzureResourceService(subscriptionService, tenantService), IMyService
   {
       public async Task<List<MyResource>> ListResourcesAsync(
           string resourceGroup,
           string subscription,
           RetryPolicyOptions? retryPolicy)
       {
           return await ExecuteResourceQueryAsync(
               "Microsoft.MyService/resources",
               resourceGroup,
               subscription,
               retryPolicy,
               ConvertToMyResourceModel);
       }

       public async Task<MyResource?> GetResourceAsync(
           string resourceName,
           string resourceGroup,
           string subscription,
           RetryPolicyOptions? retryPolicy)
       {
           return await ExecuteSingleResourceQueryAsync(
               "Microsoft.MyService/resources",
               resourceGroup,
               subscription,
               retryPolicy,
               ConvertToMyResourceModel,
               $"name =~ '{resourceName}'");
       }

       private static MyResource ConvertToMyResourceModel(JsonElement item)
       {
           var data = MyResourceData.FromJson(item);
           return new MyResource(
               Name: data.ResourceName,
               Id: data.ResourceId,
               // Map other properties...
           );
       }
   }
   ```

2. **For direct Azure Resource Manager operations**:
   - Inherit from `BaseAzureService` for services that use ARM clients directly
   - Use when you need direct ARM resource manipulation (create, update, delete)
   - Example:
   ```csharp
   public class MyService(ISubscriptionService subscriptionService, ITenantService tenantService)
       : BaseAzureService(tenantService), IMyService
   {
       private readonly ISubscriptionService _subscriptionService = subscriptionService;

       public async Task<MyResource> GetResourceAsync(string subscription, ...)
       {
           var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);
           // Use subscriptionResource for direct ARM operations
       }
   }
   ```

**BaseAzureResourceService Benefits:**
- Eliminates duplicate Resource Graph query code across services
- Provides consistent error handling and validation
- Handles subscription resolution and tenant lookup automatically
- Supports additional KQL filter parameters for complex queries
- Maintains AOT compatibility

**API Pattern Discovery:**
- Study existing services (e.g., Sql, Postgres, Redis) to understand resource access patterns
- Use resource collections correctly
   - ✅ Good: `.GetSqlServers().GetAsync(serverName)`
   - ❌ Bad: `.GetSqlServerAsync(serverName, cancellationToken)`
- Check Azure SDK documentation for correct method signatures and property names

**Common Azure Resource Manager Patterns:**
```csharp
// Resource Graph pattern (via BaseAzureResourceService)
var resources = await ExecuteResourceQueryAsync(
    "Microsoft.Sql/servers/databases",
    resourceGroup,
    subscription,
    retryPolicy,
    ConvertToSqlDatabaseModel,
    "name =~ 'mydb*'");  // Optional KQL filter

// Direct ARM pattern (via BaseAzureService)
var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

var resourceGroupResource = await subscriptionResource
    .GetResourceGroupAsync(resourceGroup, cancellationToken);

var sqlServerResource = await resourceGroupResource.Value
    .GetSqlServers()
    .GetAsync(serverName);

var databaseResource = await sqlServerResource.Value
    .GetSqlDatabases()
    .GetAsync(databaseName);
```

**Property Access Issues:**
- Azure SDK property names may differ from expected names (e.g., `CreatedOn` not `CreationDate`)
- Check actual property availability using IntelliSense or SDK documentation
- Some properties are objects that need `.ToString()` conversion (e.g., `Location.ToString()`)
- Be aware of nullable properties and use appropriate null checks

**Compilation Error Resolution:**
- When you see `cannot convert from 'System.Threading.CancellationToken' to 'string'`, check method parameter order
- For `'SqlDatabaseData' does not contain a definition for 'X'`, verify property names in the actual SDK types
- Use existing service implementations as reference for correct property access patterns

### 2. Options Class

```csharp
public class {Resource}{Operation}Options : Base{Toolset}Options
{
    // Only add properties not in base class
    public string? NewOption { get; set; }
}
```

IMPORTANT:
- Inherit from appropriate base class (Base{Toolset}Options, GlobalOptions, etc.)
- Only define properties that aren't in the base classes
- Make properties nullable if not required
- Use consistent parameter names across services:
  - **CRITICAL**: Always use `subscription` (never `subscriptionId`) for subscription parameters - this allows the parameter to accept both subscription IDs and subscription names, which are resolved internally by `ISubscriptionService.GetSubscription()`
  - Use `resourceGroup` instead of `resourceGroupName`
  - Use singular nouns for resource names (e.g., `server` not `serverName`)
  - **Remove unnecessary "-name" suffixes**: Use `--account` instead of `--account-name`, `--container` instead of `--container-name`, etc. Only keep "-name" when it provides necessary disambiguation (e.g., `--subscription-name` to distinguish from global `--subscription`)
  - Keep parameter names consistent with Azure SDK parameters when possible
  - If services share similar operations (e.g., ListDatabases), use the same parameter order and names

### Option Handling Pattern

Commands explicitly register options as required or optional using extension methods. This pattern provides explicit, per-command control over option requirements.

**Extension Methods (available on any `OptionDefinition<T>` or `Option<T>`):**

```csharp
.AsRequired()    // Makes the option required for this command
.AsOptional()    // Makes the option optional for this command
```

**Key principles:**
- Commands explicitly register options when needed using extension methods
- Each command controls whether each option is required or optional
- Binding is explicit using `parseResult.GetValueOrDefault<T>()`
- No shared state between commands - each gets its own option instance

**Usage patterns:**

**For commands that require specific options:**
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    // Make commonly optional options required for this command
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    command.Options.Add(ServiceOptionDefinitions.Account.AsRequired());
    // Use default requirement from definition
    command.Options.Add(ServiceOptionDefinitions.Database);
}

protected override MyCommandOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);
    // Use ??= for options that might be set by base classes
    options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
    // Direct assignment for command-specific options
    options.Account = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Account.Name);
    options.Database = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Database.Name);
    return options;
}
```

**For commands that use options optionally:**
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    // Make typically required options optional for this command
    command.Options.Add(ServiceOptionDefinitions.Account.AsOptional());
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
}

protected override MyCommandOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);
    options.Account = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Account.Name);
    options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
    return options;
}
```

**Important binding patterns:**
- Use `??=` assignment for options that might be set by base classes (like global options)
- Use direct assignment for command-specific options
- Use `parseResult.GetValueOrDefault<T>(optionName)` instead of holding Option<T> references
- The extension methods handle the required/optional logic at the parser level

**Benefits of the new pattern:**
- **Explicit**: Clear what options each command uses
- **Flexible**: Each command controls option requirements independently
- **No shared state**: Extension methods create new option instances
- **Consistent**: Same pattern works for all options
- **Maintainable**: Easy to see option dependencies in RegisterOptions method

### Option Extension Methods Pattern

The option pattern is built on extension methods that provide flexible, per-command control over option requirements. This eliminates shared state issues and makes option dependencies explicit.

**Available Extension Methods:**

```csharp
// For OptionDefinition<T> instances
.AsRequired()              // Creates a required option instance
.AsOptional()              // Creates an optional option instance

// For existing Option<T> instances
.AsRequired()              // Creates a new required version
.AsOptional()              // Creates a new optional version
```

**Usage Examples:**

```csharp
// Using OptionDefinitions with extension methods
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);

    // Global option - required for this command
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());

    // Service account - optional for this command
    command.Options.Add(ServiceOptionDefinitions.Account.AsOptional());

    // Database - required (override default from definition)
    command.Options.Add(ServiceOptionDefinitions.Database.AsRequired());

    // Filter - use default requirement from definition
    command.Options.Add(ServiceOptionDefinitions.Filter);
}
```

**Name-Based Binding Pattern:**

With the new pattern, option binding uses the name-based `GetValueOrDefault<T>()` method:

```csharp
protected override MyCommandOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);

    // Use ??= for options that might be set by base classes
    options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

    // Use direct assignment for command-specific options
    options.Account = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Account.Name);
    options.Database = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Database.Name);
    options.Filter = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.Filter.Name);

    return options;
}
```

**Key Benefits:**
- **Type Safety**: Generic `GetValueOrDefault<T>()` provides compile-time type checking
- **No Field References**: Eliminates need for readonly option fields in commands
- **Flexible Requirements**: Each command controls which options are required/optional
- **Clear Dependencies**: All option usage visible in `RegisterOptions` method
- **No Shared State**: Extension methods create new option instances per command

### 3. Command Class

```csharp
public sealed class {Resource}{Operation}Command(ILogger<{Resource}{Operation}Command> logger)
    : Base{Toolset}Command<{Resource}{Operation}Options>
{
    private const string CommandTitle = "Human Readable Title";
    private readonly ILogger<{Resource}{Operation}Command> _logger = logger;

    public override string Name => "operation";

    public override string Description =>
        """
        Detailed description of what the command does.
        Returns description of return format.
          Required options:
        - list required options
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,    // Set to true for tools that modify resources
        OpenWorld = true,       // Set to false for tools whose domain of interaction is closed and well-defined
        Idempotent = true,      // Set to false for tools that are not idempotent
        ReadOnly = true,        // Set to false for tools that modify resources
        Secret = false,         // Set to true for tools that may return sensitive information
        LocalRequired = false   // Set to true for tools requiring local execution/resources
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // Add options as needed (use AsRequired() or AsOptional() to override defaults)
        command.Options.Add({Toolset}OptionDefinitions.RequiredOption.AsRequired());
        command.Options.Add({Toolset}OptionDefinitions.OptionalOption.AsOptional());
        // Use default requirement from OptionDefinitions
        command.Options.Add({Toolset}OptionDefinitions.StandardOption);
    }

    protected override {Resource}{Operation}Options BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        // Bind options using GetValueOrDefault<T>(optionName)
        options.RequiredOption = parseResult.GetValueOrDefault<string>({Toolset}OptionDefinitions.RequiredOption.Name);
        options.OptionalOption = parseResult.GetValueOrDefault<string>({Toolset}OptionDefinitions.OptionalOption.Name);
        options.StandardOption = parseResult.GetValueOrDefault<string>({Toolset}OptionDefinitions.StandardOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        // Required validation step
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.WithSubscriptionTag(options);

            // Get the appropriate service from DI
            var service = context.GetService<I{Toolset}Service>();

            // Call service operation(s) with required parameters
            var results = await service.{Operation}(
                options.RequiredParam!,  // Required parameters end with !
                options.OptionalParam,   // Optional parameters are nullable
                options.Subscription!,   // From SubscriptionCommand
                options.RetryPolicy);    // From GlobalCommand

            // Set results if any were returned
            // For enumerable returns, coalesce null into an empty enumerable.
            context.Response.Results = ResponseResult.Create(new(results ?? []), {Toolset}JsonContext.Default.{Operation}CommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error in {Operation}. Required: {Required}, Optional: {Optional}, Options: {@Options}",
                Name, options.RequiredParam, options.OptionalParam, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling, only implement if this differs from base class behavior
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Resource not found. Verify the resource exists and you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the resource. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    // Implementation-specific status code retrieval, only implement if this differs from base class behavior
    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result records
    internal record {Resource}{Operation}CommandResult(List<ResultType> Results);
}
```

### ToolMetadata Properties

The `ToolMetadata` class provides behavioral characteristics that help MCP clients understand how commands operate. Set these properties carefully based on your command's actual behavior:

#### OpenWorld Property
- **`true`**: Command may interact with an "open world" of external entities where the domain is unpredictable or dynamic
- **`false`**: Command's domain of interaction is closed and well-defined

**Examples:**
- **Open World (`true`)**: Commands that query Azure resources, list storage accounts, search databases - the set of possible results is unpredictable and changes over time
- **Closed World (`false`)**: Commands that return schema definitions, best practices guides, static documentation, or predefined samples - the domain is well-defined and predictable

```csharp
// Open world - Azure resource queries
OpenWorld = true,    // Storage account list, database queries, resource discovery

// Closed world - Static/predictable content
OpenWorld = false,   // Bicep schemas, best practices, design patterns, predefined samples
```

#### Destructive Property
- **`true`**: Command may delete, modify, or destructively alter resources in a way that could cause data loss or irreversible changes
- **`false`**: Command is safe and will not cause destructive changes to resources

**Examples:**
- **Destructive (`true`)**: Commands that delete resources, modify configurations, reset passwords, purge data, or perform destructive operations
- **Non-Destructive (`false`)**: Commands that only read data, list resources, show configurations, or perform safe operations

```csharp
// Destructive operations
Destructive = true,     // Delete database, reset keys, purge storage, modify critical settings

// Safe operations
Destructive = false,    // List resources, show configuration, query data, get status
```

#### Idempotent Property
- **`true`**: Command can be safely executed multiple times with the same parameters and will produce the same result without unintended side effects
- **`false`**: Command may produce different results or side effects when executed multiple times

**Examples:**
- **Idempotent (`true`)**: Commands that set configurations to specific values, create resources with fixed names (when "already exists" is handled gracefully), or perform operations that converge to a desired state
- **Non-Idempotent (`false`)**: Commands that create resources with generated names, append data, increment counters, or perform operations that accumulate effects

```csharp
// Idempotent operations
Idempotent = true,      // Set configuration value, create named resource (with proper handling), list resources

// Non-idempotent operations
Idempotent = false,     // Generate new keys, create resources with auto-generated names, append logs
```

#### ReadOnly Property
- **`true`**: Command only reads or queries data without making any modifications to resources or state
- **`false`**: Command may modify, create, update, or delete resources or change system state

**Examples:**
- **Read-Only (`true`)**: Commands that list resources, show configurations, query databases, get status information, or retrieve data
- **Not Read-Only (`false`)**: Commands that create, update, delete resources, modify settings, or change any system state

```csharp
// Read-only operations
ReadOnly = true,        // List accounts, show database schema, query data, get resource properties

// Write operations
ReadOnly = false,       // Create resources, update configurations, delete items, modify settings
```

#### Secret Property
- **`true`**: Command may return sensitive information such as credentials, keys, connection strings, or other confidential data that should be handled with care
- **`false`**: Command returns non-sensitive information that is safe to log or display

**Examples:**
- **Secret (`true`)**: Commands that retrieve access keys, connection strings, passwords, certificates, or other credentials
- **Non-Secret (`false`)**: Commands that return public information, resource lists, configurations without sensitive data, or status information

```csharp
// Commands returning sensitive data
Secret = true,          // Get storage account keys, show connection strings, retrieve certificates

// Commands returning public data
Secret = false,         // List public resources, show non-sensitive configuration, get resource status
```

#### LocalRequired Property
- **`true`**: Command requires local execution environment, local resources, or tools that must be installed on the client machine
- **`false`**: Command can execute remotely and only requires network access to Azure services

**Examples:**
- **Local Required (`true`)**: Commands that use local tools (Azure CLI, Docker, npm), access local files, or require specific local environment setup
- **Remote Capable (`false`)**: Commands that only make API calls to Azure services and can run in any environment with network access

```csharp
// Commands requiring local resources
LocalRequired = true,   // Azure CLI wrappers, local file operations, tools requiring local installation

// Pure cloud API commands
LocalRequired = false,  // Azure Resource Manager API calls, cloud service queries, remote operations
```

Guidelines:
- Commands returning array payloads return an empty array (`[]`) if the service returned a null or empty array.
- Fully declare `ToolMetadata` properties even if they are using the default value.
- Only override `GetErrorMessage` and `GetStatusCode` if the logic differs from the base class definition.

### 4. Service Interface and Implementation

Each toolset has its own service interface that defines the methods that commands will call. The interface will have an implementation that contains the actual logic.

```csharp
public interface I<Toolset>Service
{
    ...
}
```

```csharp
public class <Toolset>Service(ISubscriptionService subscriptionService, ITenantService tenantService, ICacheService cacheService) : BaseAzureService(tenantService), I<Toolset>Service
{
   ...
}
```

### Method Signature Consistency

All interface methods should follow consistent formatting with proper line breaks and parameter alignment:

```csharp
// Correct formatting - parameters aligned with line breaks
Task<List<string>> GetStorageAccounts(
    string subscription,
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null);

// Incorrect formatting - all parameters on single line
Task<List<string>> GetStorageAccounts(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null);
```

**Formatting Rules:**
- Parameters indented and aligned
- Add blank lines between method declarations for visual separation
- Maintain consistent indentation across all methods in the interface

### 5. Base Service Command Classes

Each toolset has its own hierarchy of base command classes that inherit from `GlobalCommand` or `SubscriptionCommand`. Service classes that work with Azure resources should inject `ISubscriptionService` for subscription resolution. For example:

```csharp
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.{Toolset}.Options;

namespace Azure.Mcp.Tools.{Toolset}.Commands;

// Base command for all service commands (if no members needed, use concise syntax)
public abstract class Base{Toolset}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : Base{Toolset}Options, new();

// Base command for all service commands (if members are needed, use full syntax)
public abstract class Base{Toolset}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : Base{Toolset}Options, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // Register common options for all toolset commands
        command.Options.Add({Toolset}OptionDefinitions.CommonOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        // Bind common options using GetValueOrDefault<T>()
        options.CommonOption = parseResult.GetValueOrDefault<string>({Toolset}OptionDefinitions.CommonOption.Name);
        return options;
    }
}

// Example: Resource-specific base command with common options
public abstract class Base{Resource}Command<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : Base{Toolset}Command<TOptions> where TOptions : Base{Resource}Options, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // Add resource-specific options that all resource commands need
        command.Options.Add({Toolset}OptionDefinitions.{Resource}Name);
        command.Options.Add({Toolset}OptionDefinitions.{Resource}Type.AsOptional());
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        // Bind resource-specific options
        options.{Resource}Name = parseResult.GetValueOrDefault<string>({Toolset}OptionDefinitions.{Resource}Name.Name);
        options.{Resource}Type = parseResult.GetValueOrDefault<string>({Toolset}OptionDefinitions.{Resource}Type.Name);
        return options;
    }
}

// Service implementation example with subscription resolution
public class {Toolset}Service(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), I{Toolset}Service
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<{Resource}> GetResourceAsync(string subscription, string resourceGroup, string resourceName, RetryPolicyOptions? retryPolicy)
    {
        // Always use subscription service for resolution
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

        var resourceGroupResource = await subscriptionResource
            .GetResourceGroupAsync(resourceGroup, cancellationToken);
        // Continue with resource access...
    }
}
```

### 6. Unit Tests

Unit tests follow a standardized pattern that tests initialization, validation, and execution:

```csharp
public class {Resource}{Operation}CommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly I{Toolset}Service _service;
    private readonly ILogger<{Resource}{Operation}Command> _logger;
    private readonly {Resource}{Operation}Command _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public {Resource}{Operation}CommandTests()
    {
        _service = Substitute.For<I{Toolset}Service>();
        _logger = Substitute.For<ILogger<{Resource}{Operation}Command>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("operation", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--required value", true)]
    [InlineData("--optional-param value --required value", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _service.{Operation}(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns([]);
        }

        // Build args from a single string in tests using the test-only splitter
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _service.{Operation}(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse({argsArray});

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, {Toolset}JsonContext.Default.{Operation}CommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _service.{Operation}(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<ResultType>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse(["--required", "value"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _parser.Parse(["--subscription", "test-sub", "--required", "value"]);

        // Act
        var options = _command.BindOptions(parseResult);

        // Assert
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("value", options.RequiredParam);
    }
}
```

Guidelines:
- Use `{Toolset}JsonContext.Default.{Operation}CommandResult` when deserializing JSON to a response result model. Do not define custom models for serialization.
   - ✅ Good: `JsonSerializer.Deserialize(json, {Toolset}JsonContext.Default.{Operation}CommandResult)`
   - ❌ Bad: `JsonSerializer.Deserialize<TestModel>(json)`
- When using argument matchers for a specific value use `Arg.Is(<Value>)` or use the value directly as it is cleaner than `Arg.Is<T>(Predicate<T>)`.
   - ✅ Good: `_service.{Operation}(Arg.Is(value)).Returns(return)`
   - ✅ Good: `_service.{Operation}(value).Returns(return)`
   - ❌ Bad: `_service.{Operation}(Arg.Is<T>(t => t == value)).Returns(return)`
### 7. Integration Tests

Integration tests inherit from `CommandTestsBase` and use test fixtures:

```csharp
public class {Toolset}CommandTests(ITestOutputHelper output)
    : CommandTestsBase( output)
{
    [Theory]
    [InlineData(AuthMethod.Credential)]
    [InlineData(AuthMethod.Key)]
    public async Task Should_{Operation}_{Resource}_WithAuth(AuthMethod authMethod)
    {
        // Arrange
        var result = await CallToolAsync(
            "azmcp_{Toolset}_{resource}_{operation}",
            new()
            {
                { "subscription", Settings.Subscription },
                { "resource-group", Settings.ResourceGroup },
                { "auth-method", authMethod.ToString().ToLowerInvariant() }
            });

        // Assert
        var items = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);

        // Check results format
        foreach (var item in items.EnumerateArray())
        {
            // When JSON properties are expected, use AssertProperty.
            // It provides more failure information than asserting TryGetProperty returns true.
            item.AssertProperty("name");
            item.AssertProperty("type");

            // Conditionally validate optional properties.
            if (item.TryGetProperty("optional", out var optionalProp))
            {
                Assert.Equal(JsonValueKind.String, optionalProp.ValueKind);
            }
        }
    }

    [Theory]
    [InlineData("--invalid-param")]
    [InlineData("--subscription invalidSub")]
    public async Task Should_Return400_WithInvalidInput(string args)
    {
        var result = await CallToolAsync(
            $"azmcp_{Toolset}_{resource}_{operation} {args}");

        Assert.Equal(400, result.GetProperty("status").GetInt32());
        Assert.Contains("required",
            result.GetProperty("message").GetString()!.ToLower());
    }
}
```

Guidelines:
- When validating JSON for an expected property use `JsonElement.AssertProperty`.
- When validating JSON for a conditional property use `JsonElement.TryGetProperty` in an if-clause. 

### 8. Command Registration

```csharp
private void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
{
    var service = new CommandGroup(
        "{Toolset}",
        "{Toolset} operations");
    rootGroup.AddSubGroup(service);

    var resource = new CommandGroup(
        "{resource}",
        "{Resource} operations");
    service.AddSubGroup(resource);

    resource.AddCommand("{operation}", new {Resource}{Operation}Command(
        loggerFactory.CreateLogger<{Resource}{Operation}Command>()));
}
```

**IMPORTANT**: Use lowercase concatenated or dash-separated names. Command group names cannot contain underscores.
- ✅ Good: `"entraadmin"`, `"resourcegroup"`, `"storageaccount"`, `"entra-admin"`
- ❌ Bad: `"entra_admin"`, `"resource_group"`, `"storage_account"`

### 9. Toolset Registration
```csharp
private static IToolsetSetup[] RegisterAreas()
{
    return [
        // Register core toolsets
        new Azure.Mcp.Tools.AzureBestPractices.AzureBestPracticesSetup(),
        new Azure.Mcp.Tools.Extension.ExtensionSetup(),

        // Register Azure service toolsets
        new Azure.Mcp.Tools.{Toolset}.{Toolset}Setup(),
        new Azure.Mcp.Tools.Storage.StorageSetup(),
    ];
}
```

The area/toolset list in `RegisterAreas()` must remain alphabetically sorted (excluding the fixed conditional AOT exclusion block guarded by `#if !BUILD_NATIVE`).

### 10. JSON Serialization Context

All models and command result record types returned in `Response.Results` must be registered in a source-generated JSON context for AOT safety and performance.

Create (or update) a `{Toolset}JsonContext` file (common location: `src/Commands/{Toolset}JsonContext.cs` or within `Commands` folder) containing:

```csharp
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.{Toolset}.Commands.{Resource};
using Azure.Mcp.Tools.{Toolset}.Models;

[JsonSerializable(typeof({Resource}{Operation}Command.{Resource}{Operation}CommandResult))]
[JsonSerializable(typeof(YourModelType))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class {Toolset}JsonContext : JsonSerializerContext;
```

Usage inside a command when assigning results:

```csharp
context.Response.Results = ResponseResult.Create(new(results), {Toolset}JsonContext.Default.{Resource}{Operation}CommandResult);
```

Guidelines:
- Only include types actually serialized as top-level result payloads
- Keep attribute list minimal but complete
- Use one context per toolset (preferred) unless size forces logical grouping
- Ensure filename matches class for navigation (`{Toolset}JsonContext.cs`)
- Keep `JsonSerializable` sorted based on the `typeof` model name.

## Error Handling

Commands in Azure MCP follow a standardized error handling approach using the base `HandleException` method inherited from `BaseCommand`. Here are the key aspects:

### 1. Status Code Mapping
The base implementation returns 500 for all exceptions by default:
```csharp
protected virtual int GetStatusCode(Exception ex) => 500;
```

Commands should override this to provide appropriate status codes:
```csharp
protected override int GetStatusCode(Exception ex) => ex switch
{
    Azure.RequestFailedException reqEx => reqEx.Status,  // Use Azure-reported status
    Azure.Identity.AuthenticationFailedException => 401,   // Unauthorized
    ValidationException => 400,    // Bad request
    _ => base.GetStatusCode(ex) // Fall back to 500
};
```

### 2. Error Message Formatting
The base implementation returns the exception message:
```csharp
protected virtual string GetErrorMessage(Exception ex) => ex.Message;
```

Commands should override this to provide user-actionable messages:
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    Azure.Identity.AuthenticationFailedException authEx =>
        $"Authentication failed. Please run 'az login' to sign in. Details: {authEx.Message}",
    Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
        "Resource not found. Verify the resource name and that you have access.",
    Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
        $"Access denied. Ensure you have appropriate RBAC permissions. Details: {reqEx.Message}",
    Azure.RequestFailedException reqEx => reqEx.Message,
    _ => base.GetErrorMessage(ex)
};
```

### 3. Response Format
The base `HandleException` method in BaseCommand handles the response formatting:
```csharp
protected virtual void HandleException(CommandContext context, Exception ex)
{
    context.Activity?.SetStatus(ActivityStatusCode.Error)?.AddTag(TagName.ErrorDetails, ex.Message);

    var response = context.Response;
    var result = new ExceptionResult(
        Message: ex.Message,
        StackTrace: ex.StackTrace,
        Type: ex.GetType().Name);

    response.Status = GetStatusCode(ex);
    response.Message = GetErrorMessage(ex) + ". To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
    response.Results = ResponseResult.Create(result, JsonSourceGenerationContext.Default.ExceptionResult);
}
```

Commands should call `HandleException(context, ex)` in their catch blocks.

### 4. Service-Specific Errors
Commands should override error handlers to add service-specific mappings:
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    // Add service-specific cases
    ResourceNotFoundException =>
        "Resource not found. Verify name and permissions.",
    ServiceQuotaExceededException =>
        "Service quota exceeded. Request quota increase.",
    _ => base.GetErrorMessage(ex) // Fall back to base implementation
};
```

### 5. Error Context Logging
Always log errors with relevant context information:
```csharp
catch (Exception ex)
{
    _logger.LogError(ex,
        "Error in {Operation}. Resource: {Resource}, Options: {@Options}",
        Name, resourceId, options);
    HandleException(context, ex);
}
```

### 6. Common Error Scenarios to Handle

1. **Authentication/Authorization**
   - Azure credential expiry
   - Missing RBAC permissions
   - Invalid connection strings

2. **Validation**
   - Missing required parameters
   - Invalid parameter formats
   - Conflicting options

3. **Resource State**
   - Resource not found
   - Resource locked/in use
   - Invalid resource state

4. **Service Limits**
   - Throttling/rate limits
   - Quota exceeded
   - Service capacity

5. **Network/Connectivity**
   - Service unavailable
   - Request timeouts
   - Network failures

## Testing Requirements

### Unit Tests
Core test cases for every command:
```csharp
[Theory]
[InlineData("", false, "Missing required options")]  // Validation
[InlineData("--param invalid", false, "Invalid format")] // Input format
[InlineData("--param value", true, null)]  // Success case
public async Task ExecuteAsync_ValidatesInput(
    string args, bool shouldSucceed, string expectedError)
{
    var response = await ExecuteCommand(args);
    Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
    if (!shouldSucceed)
        Assert.Contains(expectedError, response.Message);
}

[Fact]
public async Task ExecuteAsync_HandlesServiceError()
{
    // Arrange
    _service.Operation()
        .Returns(Task.FromException(new ServiceException("Test error")));

    // Act
    var response = await ExecuteCommand("--param value");

    // Assert
    Assert.Equal(500, response.Status);
    Assert.Contains("Test error", response.Message);
    Assert.Contains("troubleshooting", response.Message);
}
```

**Running Tests Efficiently:**
When developing new commands, run only your specific tests to save time:
```bash
# Run all tests from the test project directory:
pushd ./tools/Azure.Mcp.Tools.YourToolset/tests/Azure.Mcp.Tools.YourToolset.UnitTests  #or .LiveTests

# Run only tests for your specific command class
dotnet test --filter "FullyQualifiedName~YourCommandNameTests" --verbosity normal

# Example: Run only SQL AD Admin tests
dotnet test --filter "FullyQualifiedName~EntraAdminListCommandTests" --verbosity normal

# Run all tests for a specific toolset
dotnet test --verbosity normal
```

### Integration Tests
Azure service commands requiring test resource deployment must add a bicep template, `tests/test-resources.bicep`, to their toolset directory. Additionally, all Azure service commands must include a `test-resources-post.ps1` file in the same directory, even if it contains only the basic template without custom logic. See `/tools/Azure.Mcp.Tools.Storage/tests/test-resources.bicep` and `/tools/Azure.Mcp.Tools.Storage/tests/test-resources-post.ps1` for canonical examples.

#### Live Test Resource Infrastructure

**1. Create Toolset Bicep Template (`/tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`)**

Follow this pattern for your toolset's infrastructure:

```bicep
targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)  // Adjust based on service naming limits
@description('The base resource name. Service names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

// The test infrastructure will only provide baseName and testApplicationOid.
// Any additional parameters are for local deployments only and require default values.

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Main service resource
resource serviceResource 'Microsoft.{Provider}/{resourceType}@{apiVersion}' = {
  name: baseName
  location: location
  properties: {
    // Service-specific properties
  }

  // Child resources (databases, containers, etc.)
  resource testResource 'childResourceType@{apiVersion}' = {
    name: 'test{resource}'
    properties: {
      // Test resource properties
    }
  }
}

// Role assignment for test application
resource serviceRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Use appropriate built-in role for your service
  // See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles
  name: '{role-guid}'
}

resource appServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceRoleDefinition.id, testApplicationOid, serviceResource.id)
  scope: serviceResource
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: serviceRoleDefinition.id
    description: '{Role Name} for testApplicationOid'
  }
}

// Outputs for test consumption
output serviceResourceName string = serviceResource.name
output testResourceName string = serviceResource::testResource.name
// Add other outputs as needed for tests
```

**Key Bicep Template Requirements:**
- Use `baseName` parameter with appropriate length restrictions
- Include `testApplicationOid` for RBAC assignments
- Deploy test resources (databases, containers, etc.) needed for integration tests
- Assign appropriate built-in roles to the test application
- Output resource names and identifiers for test consumption

**Cost and Resource Considerations:**
- Use minimal SKUs (Basic, Standard S0, etc.) for cost efficiency
- Deploy only resources needed for command testing
- Consider using shared resources where possible
- Set appropriate retention policies and limits
- Use resource naming that clearly identifies test purposes

**Common Resource Naming Patterns:**
- Deployments are on a per-toolset basis. Name collisions should not occur across toolset templates.
- Main service: `baseName` (most common, e.g., `mcp12345`) or `{baseName}{suffix}` if disambiguation needed
- Child resources: `test{resource}` (e.g., `testdb`, `testcontainer`)
- Follow Azure naming conventions and length limits
- Ensure names are unique within resource group scope
- Check existing `test-resources.bicep` files for consistent patterns

**2. Required: Post-Deployment Script (`tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1`)**

All Azure service commands must include this script, even if it contains only the basic template. Create with the standard template and add custom setup logic if needed:

```powershell
#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#Requires -Version 6.0
#Requires -PSEdition Core

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [hashtable] $DeploymentOutputs,

    [Parameter(Mandatory)]
    [hashtable] $AdditionalParameters
)

Write-Host "Running {Toolset} post-deployment setup..."

try {
    # Extract outputs from deployment
    $serviceName = $DeploymentOutputs['{Toolset}']['serviceResourceName']['value']
    $resourceGroup = $AdditionalParameters['ResourceGroupName']

    # Perform additional setup (e.g., create sample data, configure settings)
    Write-Host "Setting up test data for $serviceName..."

    # Example: Run Azure CLI commands for additional setup
    # az {service} {operation} --name $serviceName --resource-group $resourceGroup

    Write-Host "{Toolset} post-deployment setup completed successfully."
}
catch {
    Write-Error "Failed to complete {Toolset} post-deployment setup: $_"
    throw
}
```

**4. Update Live Tests to Use Deployed Resources**

Integration tests should use the deployed infrastructure:

```csharp
[Trait("Toolset", "{Toolset}")]
[Trait("Category", "Live")]
public class {Toolset}CommandTests( ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_Get{Resource}_Successfully()
    {
        // Use the deployed test resources
        var serviceName = Settings.ResourceBaseName;
        var resourceName = "test{resource}";

        var result = await CallToolAsync(
            "azmcp_{Toolset}_{resource}_show",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "service-name", serviceName },
                { "resource-name", resourceName }
            });

        // Verify successful response
        var resource = result.AssertProperty("{resource}");
        Assert.Equal(JsonValueKind.Object, resource.ValueKind);

        // Verify resource properties
        var name = resource.GetProperty("name").GetString();
        Assert.Equal(resourceName, name);
    }

    [Theory]
    [InlineData("--invalid-param", new string[0])]
    [InlineData("--subscription", new[] { "invalidSub" })]
    [InlineData("--subscription", new[] { "sub", "--resource-group", "rg" })]  // Missing required params
    public async Task Should_Return400_WithInvalidInput(string firstArg, string[] remainingArgs)
    {
        var allArgs = new[] { firstArg }.Concat(remainingArgs);
        var argsString = string.Join(" ", allArgs);

        var result = await CallToolAsync(
            "azmcp_{Toolset}_{resource}_show",
            new()
            {
                { "args", argsString }
            });

        // Should return validation error
        Assert.NotEqual(200, result.Status);
    }
}
```

**5. Deploy and Test Resources**

Use the deployment script with your toolset:

```powershell
# Deploy test resources for your toolset
./eng/scripts/Deploy-TestResources.ps1 -Tools "{Toolset}"

# Run live tests
pushd 'tools/Azure.Mcp.Tools.{Toolset}/tests/Azure.Mcp.Tools.{Toolset}.LiveTests'
dotnet test
```

Live test scenarios should include:
```csharp
[Theory]
[InlineData(AuthMethod.Credential)]  // Default auth
[InlineData(AuthMethod.Key)]         // Key based auth
public async Task Should_HandleAuth(AuthMethod method)
{
    var result = await CallCommand(new()
    {
        { "auth-method", method.ToString() }
    });
    // Verify auth worked
    Assert.Equal(200, result.Status);
}

[Theory]
[InlineData("--invalid-value")]    // Bad input
[InlineData("--missing-required")] // Missing params
public async Task Should_Return400_ForInvalidInput(string args)
{
    var result = await CallCommand(args);
    Assert.Equal(400, result.Status);
    Assert.Contains("validation", result.Message.ToLower());
}
```

If your live test class needs to implement `IAsyncLifetime` or override `Dispose`, you must call `Dispose` on your base class:
```cs
public class MyCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output), IAsyncLifetime
{
    public ValueTask DisposeAsync()
    {
        base.Dispose();
        return ValueTask.CompletedTask;
    }
}
```

Failure to call `base.Dispose()` will prevent request and response data from `CallCommand` from being written to failing test results.

## Code Quality and Unused Using Statements

### Preventing Unused Using Statements

Unused `using` statements are a common issue that clutters code and can lead to unnecessary dependencies. Here are strategies to prevent and detect them:

#### 1. **Use Minimal Using Statements When Creating Files**

When creating new C# files, start with only the using statements you actually need:

```csharp
// Start minimal - only add what you actually use
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.Logging;

// Add more using statements as you implement the code
// Don't copy-paste using blocks from other files
```

#### 2. **Leverage ImplicitUsings**

The project already has `<ImplicitUsings>enable</ImplicitUsings>` in `Directory.Build.props`, which automatically includes common using statements for .NET 9:

**Implicit Using Statements (automatically included):**
- `using System;`
- `using System.Collections.Generic;`
- `using System.IO;`
- `using System.Linq;`
- `using System.Net.Http;`
- `using System.Threading;`
- `using System.Threading.Tasks;`

**Don't manually add these - they're already included!**

#### 3. **Detection and Cleanup Commands**

Use these commands to detect and remove unused using statements:

```powershell
# Format specific toolset files (recommended during development)
dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs" --verbosity normal

# Format entire solution (use sparingly - takes longer)
dotnet format ./AzureMcp.sln --verbosity normal

# Check for analyzer warnings including unused usings
dotnet build --verbosity normal | Select-String "warning"
```

#### 4. **Common Unused Using Patterns to Avoid**

✅ **Start minimal and add as needed:**
```csharp
// Only what's actually used in this file
using Azure.Mcp.Tools.Acr.Services;
using Microsoft.Extensions.Logging;
```

✅ **Add using statements for better readability:**
```csharp
using Azure.ResourceManager.ContainerRegistry.Models;

// Clean and readable - even if used only once
public ContainerRegistryResource Resource { get; set; }

// This is much better than:
// public Azure.ResourceManager.ContainerRegistry.Models.ContainerRegistryResource Resource { get; set; }
```

❌ **Don't copy using blocks from other files:**
```csharp
// Copied from another file but not all are needed
using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Tools.Acr.Commands;         // ← May not be needed
using Azure.Mcp.Tools.Acr.Options;          // ← May not be needed
using Azure.Mcp.Tools.Acr.Options.Registry; // ← May not be needed
using Azure.Mcp.Tools.Acr.Services;
// ... 15 more using statements
```

#### 6. **Integration with Build Process**

The project checklist already includes cleaning up unused using statements:

- [ ] **Remove unnecessary using statements from all C# files** (use IDE cleanup or `dotnet format`)

**Make this part of your development workflow:**
1. Write code with minimal using statements
2. Add using statements only as you need them
3. Run `dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs"` before committing
4. Use IDE features to clean up automatically

### Build Verification and AOT Compatibility

After implementing your commands, verify that your implementation works correctly with both regular builds and AOT (Ahead-of-Time) compilation:

**1. Regular Build Verification:**
```powershell
# Build the solution
dotnet build

# Run specific tests
dotnet test --filter "FullyQualifiedName~YourCommandTests"
```

**2. AOT Compilation Verification:**

AOT (Ahead-of-Time) compilation is required for all new toolsets to ensure compatibility with native builds:

```powershell
# Test AOT compatibility - this is REQUIRED for all new toolsets
./eng/scripts/Build-Local.ps1 -BuildNative
```

**Expected Outcome**: If your toolset is properly implemented, the build should succeed. However, if AOT compilation fails (which is very likely for new toolsets), follow these steps:
**3. AOT Compilation Issue Resolution:**

When AOT compilation fails for your new toolset, you need to exclude it from native builds:

**Step 1: Move toolset setup under BuildNative condition in Program.cs**
```csharp
// Find your toolset setup call in Program.cs
// Move it inside the #if !BUILD_NATIVE block

#if !BUILD_NATIVE
    // ... other toolset setups ...
    builder.Services.Add{YourToolset}Setup();  // ← Move this line here
#endif
```

**Step 2: Add ProjectReference-Remove condition in Azure.Mcp.Server.csproj**
```xml
<!-- Add this to servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj -->
<ItemGroup Condition="'$(BuildNative)' == 'true'">
  <ProjectReference Remove="..\..\tools\Azure.Mcp.Tools.{Toolset}\src\Azure.Mcp.Tools.{Toolset}.csproj" />
</ItemGroup>
```

**Step 3: Verify the fix**
```powershell
# Test that AOT compilation now succeeds
./eng/scripts/Build-Local.ps1 -BuildNative

# Verify regular build still works
dotnet build
```

**Why AOT Compilation Often Fails:**
- Azure SDK libraries may not be fully AOT-compatible
- Reflection-based operations in service implementations
- Third-party dependencies that don't support AOT
- Dynamic JSON serialization without source generators

**Important**: This is a common and expected issue for new Azure service toolsets. The exclusion pattern is the standard solution and doesn't impact regular builds or functionality.

## Common Implementation Issues and Solutions

### Service Method Design

**Issue: Inconsistent method signatures across services**
- **Solution**: Follow established patterns for method signatures with proper parameter alignment
- **Pattern**:
```csharp
// Correct - parameters aligned with line breaks
Task<List<ResourceModel>> GetResources(
    string subscription,
    string? resourceGroup = null,
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null);
```

**Issue: Wrong subscription resolution pattern**
- **Solution**: Always use `ISubscriptionService.GetSubscription()` instead of manual ARM client creation
- **Pattern**:
```csharp
// Correct pattern
var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);
```

### Command Option Patterns

**Issue: Using readonly option fields in commands**
- **Problem**: Commands define readonly `Option<T>` fields and use `parseResult.GetValue()` without type parameters.
- **Solution**: Remove readonly fields; use `OptionDefinitions` directly in `RegisterOptions` and name-based binding in `BindOptions`.
- **Pattern**:
```csharp
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    // Use extension methods for flexible requirements
    command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    command.Options.Add(ServiceOptionDefinitions.ServiceOption);
}

protected override MyOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);
    // Use name-based binding with generic type parameters
    options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
    options.ServiceOption = parseResult.GetValueOrDefault<string>(ServiceOptionDefinitions.ServiceOption.Name);
    return options;
}
```

### Error Handling Patterns

**Issue: Generic error handling without service-specific context**
- **Solution**: Override base error handling methods for better user experience
- **Pattern**:
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
        "Resource not found. Verify the resource exists and you have access.",
    Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
        $"Authorization failed. Details: {reqEx.Message}",
    _ => base.GetErrorMessage(ex)
};
```

**Issue: Missing HandleException call**
- **Solution**: Always call `HandleException(context, ex)` in command catch blocks
- **Pattern**:
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error in {Operation}", Name);
    HandleException(context, ex);
}
```

## Best Practices

1. Command Structure:
   - Make command classes sealed
   - Use primary constructors
   - Follow exact namespace hierarchy
   - Register all options in RegisterOptions
   - Handle all exceptions

2. Error Handling:
   - Return 400 for validation errors
   - Return 401 for authentication failures
   - Return 500 for unexpected errors
   - Return service-specific status codes from RequestFailedException
   - Add troubleshooting URL to error messages
   - Log errors with context information
   - Override GetErrorMessage and GetStatusCode for custom error handling

3. Response Format:
   - Always set Results property for success
   - Set Status and Message for errors
   - Use consistent JSON property names
   - Follow existing response patterns

4. Documentation:
   - Clear command description without repeating the service name (e.g., use "List and manage clusters" instead of "AKS operations - List and manage AKS clusters")
   - List all required options
   - Describe return format
   - Include examples in description
   - **Maintain alphabetical sorting in e2eTestPrompts.md**: Insert new test prompts in correct alphabetical position by Tool Name within each service section

5. Tool Description Quality Validation:
    - Test your command descriptions for quality using the validation tool located at `eng/tools/ToolDescriptionEvaluator` before submitting:

      - **Single prompt validation** (test one description against one prompt):

        ```bash
        dotnet run -- --validate --tool-description "Your command description here" --prompt "typical user request"
        ```

      - **Multiple prompt validation** (test one description against multiple prompts):

        ```bash
        dotnet run -- --validate \
        --tool-description "Lists all storage accounts in a subscription" \
        --prompt "show me my storage accounts" \
        --prompt "list storage accounts" \
        --prompt "what storage do I have"
        ```

      - **Custom tools and prompts files** (use your own files for comprehensive testing):

        ```bash
        # Prompts:
        # Use markdown format (same as docs/e2eTestPrompts.md):
        dotnet run -- --prompts-file my-prompts.md

        # Use JSON format:
        dotnet run -- --prompts-file my-prompts.json

        # Tools:
        # Use JSON format (same as eng/tools/ToolDescriptionEvaluator/tools.json):
        dotnet run -- --tools-file my-tools.json

        # Combine both:
        # Use custom tools and prompts files together:
        dotnet run -- --tools-file my-tools.json --prompts-file my-prompts.md
        ```

    - Quality assessment guidelines:

      - Aim for your description to rank in the top 3 results (GOOD or EXCELLENT rating)
      - Test with multiple different prompts that users might use
      - Consider common synonyms and alternative phrasings in your descriptions
      - If validation shows POOR results or a confidence score of < 0.4, refine your description and test again

    - Custom prompts file formats:
      - **Markdown format**: Use same table format as `docs/e2eTestPrompts.md`:

        ```markdown
        | Tool Name | Test Prompt |
        |:----------|:----------|
        | azmcp-your-command | Your test prompt |
        | azmcp-your-command | Another test prompt |
        ```

      - **JSON format**: Tool name as key, array of prompts as value:

        ```json
        {
            "azmcp-your-command": [
            "Your test prompt",
            "Another test prompt"
            ]
        }
        ```

    - Custom tools file format:
      - Use the JSON format returned by calling the server command `azmcp-tools-list` or found in `eng/tools/ToolDescriptionEvaluator/tools.json`.

6. Live Test Infrastructure:
   - Use minimal resource configurations for cost efficiency
   - Follow naming conventions: `baseName` (most common) or `{baseName}-{Toolset}` if needed
   - Include proper RBAC assignments for test application
   - Output all necessary identifiers for test consumption
   - Use appropriate Azure service API versions
   - Consider resource location constraints and availability

## Common Pitfalls to Avoid

1. Do not:
   - **CRITICAL**: Use `subscriptionId` as parameter name - Always use `subscription` to support both IDs and names
   - **CRITICAL**: Define readonly option fields in commands - Use `OptionDefinitions` directly in `RegisterOptions` and `BindOptions`
   - **CRITICAL**: Use the old `UseResourceGroup()` or `RequireResourceGroup()` pattern - These methods no longer exist. Use extension methods like `.AsRequired()` or `.AsOptional()` instead
   - **CRITICAL**: Skip live test infrastructure for Azure service commands - Create `test-resources.bicep` template early in development
   - **CRITICAL**: Use `parseResult.GetValue()` without the generic type parameter - Use `parseResult.GetValueOrDefault<T>(optionName)` instead
   - Redefine base class properties in Options classes
   - Skip base.RegisterOptions() call
   - Skip base.Dispose() call
   - Use hardcoded option strings
   - Return different response formats
   - Leave command unregistered
   - Skip error handling
   - Miss required tests
   - Deploy overly expensive test resources
   - Forget to assign RBAC permissions to test application
   - Hard-code resource names in live tests
   - Use dashes in command group names

2. Always:
   - Create a static `{Toolset}OptionDefinitions` class for the toolset
   - **For option handling**: Use extension methods like `.AsRequired()` or `.AsOptional()` to control option requirements per command. Register explicitly in `RegisterOptions` and bind explicitly in `BindOptions`
   - **For option binding**: Use `parseResult.GetValueOrDefault<T>(optionDefinition.Name)` pattern for all options
   - **For Azure service commands**: Create test infrastructure (`test-resources.bicep`) before implementing live tests
   - Use OptionDefinitions for options
   - Follow exact file structure
   - Implement all base members
   - Add both unit and integration tests
   - Register in toolset setup RegisterCommands method
   - Handle all error cases
   - Use primary constructors
   - Make command classes sealed
   - Include live test infrastructure for Azure services
   - Use consistent resource naming patterns (check existing `test-resources.bicep` files)
   - Output resource identifiers from Bicep templates
   - Use concatenated all lowercase names for command groups (no dashes)

### Troubleshooting Common Issues

### Project Setup and Integration Issues

**Issue: Solution file GUID conflicts**
- **Cause**: Duplicate project GUIDs in the solution file causing build failures
- **Solution**: Generate unique GUIDs for new projects when adding to `AzureMcp.sln`
- **Fix**: Use Visual Studio or `dotnet sln add` command to properly add projects with unique GUIDs
- **Prevention**: Always check for GUID uniqueness when manually editing solution files

**Issue: Missing package references cause compilation errors**
- **Cause**: Azure Resource Manager package not added to `Directory.Packages.props` before being referenced
- **Solution**: Add package version to `Directory.Packages.props` first, then reference in project files
- **Fix**:
  1. Add `<PackageVersion Include="Azure.ResourceManager.{Service}" Version="{version}" />` to `Directory.Packages.props`
  2. Add `<PackageReference Include="Azure.ResourceManager.{Service}" />` to project file
- **Prevention**: Follow the two-step package addition process documented in Implementation Guidelines

**Issue: Missing live test infrastructure for Azure service commands**
- **Cause**: Forgetting to create `test-resources.bicep` template during development
- **Solution**: Create Bicep template early in development process, not as an afterthought
- **Fix**: Create `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep` following established patterns
- **Prevention**: Check "Test Infrastructure Requirements" section at top of this document before starting implementation
- **Validation**: Run `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep` to validate template

**Issue: Pipeline fails with "SelfContainedPostScript is not supported if there is no test-resources-post.ps1"**
- **Cause**: Missing required `test-resources-post.ps1` file for Azure service commands
- **Solution**: Create the post-deployment script file, even if it contains only the basic template
- **Fix**: Create `tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources-post.ps1` using the standard template from existing toolsets
- **Prevention**: All Azure service commands must include this file - it's required by the test infrastructure
- **Note**: The file is mandatory even if no custom post-deployment logic is needed

**Issue: Test project compilation errors with missing imports**
- **Cause**: Missing using statements for test frameworks and core libraries
- **Solution**: Add required imports for test projects:
  - `using System.Text.Json;` for JSON serialization
  - `using Xunit;` for test framework
  - `using NSubstitute;` for mocking
  - `using Azure.Mcp.Tests;` for test base classes
- **Fix**: Review test project template and ensure all necessary imports are included
- **Prevention**: Use existing test projects as templates for import statements

### Azure Resource Manager Compilation Errors

**Issue: Subscription not properly resolved**
- **Cause**: Using direct ARM client creation instead of subscription service
- **Solution**: Always inject and use `ISubscriptionService.GetSubscription()`
- **Fix**: Replace manual subscription resource creation with service call
- **Pattern**:
```csharp
// Correct - use service
var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

// Wrong - manual creation
var armClient = await CreateArmClientAsync(null, retryPolicy);
var subscriptionResource = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscription}"));
```

**Issue: `cannot convert from 'System.Threading.CancellationToken' to 'string'`**
- **Cause**: Wrong parameter order in resource manager method calls
- **Solution**: Check method signatures; many Azure SDK methods don't take CancellationToken as second parameter
- **Fix**: Use `.GetAsync(resourceName)` instead of `.GetAsync(resourceName, cancellationToken)`

**Issue: `'SqlDatabaseData' does not contain a definition for 'CreationDate'`**
- **Cause**: Property names in Azure SDK differ from expected/documented names
- **Solution**: Use IntelliSense to explore actual property names
- **Common fixes**:
  - `CreationDate` → `CreatedOn`
  - `EarliestRestoreDate` → `EarliestRestoreOn`
  - `Edition` → `CurrentSku?.Name`

**Issue: `Operator '?' cannot be applied to operand of type 'AzureLocation'`**
- **Cause**: Some Azure SDK types are structs, not nullable reference types
- **Solution**: Convert to string: `Location.ToString()` instead of `Location?.Name`

**Issue: Wrong resource access pattern**
- **Problem**: Using `.GetSqlServerAsync(name, cancellationToken)`
- **Solution**: Use resource collections: `.GetSqlServers().GetAsync(name)`
- **Pattern**: Always access through collections, not direct async methods

### Live Test Infrastructure Issues

**Issue: Bicep template validation fails**
- **Cause**: Invalid parameter constraints, missing required properties, or API version issues
- **Solution**: Use `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep` to validate template
- **Fix**: Check Azure Resource Manager template reference for correct syntax and required properties

**Issue: Live tests fail with "Resource not found"**
- **Cause**: Test resources not deployed or wrong naming pattern used
- **Solution**: Verify resource deployment and naming in Azure portal
- **Fix**: Ensure live tests use `Settings.ResourceBaseName` pattern for resource names (or appropriate service-specific pattern)

**Issue: Permission denied errors in live tests**
- **Cause**: Missing or incorrect RBAC assignments in Bicep template
- **Solution**: Verify role assignment scope and principal ID
- **Fix**: Check that `testApplicationOid` is correctly passed and role definition GUID is valid

**Issue: Deployment fails with template validation errors**
- **Cause**: Parameter constraints, resource naming conflicts, or invalid configurations
- **Solution**:
  - Review deployment logs and error messages
  - Use `./eng/scripts/Deploy-TestResources.ps1 -Toolset {Toolset} -Debug` for verbose deployment logs including resource provider errors.

### Live Test Project Configuration Issues

**Issue: Live tests fail with "MCP server process exited unexpectedly" and "azmcp.exe not found"**
- **Cause**: Incorrect project configuration in `Azure.Mcp.Tools.{Toolset}.LiveTests.csproj`
- **Common Problem**: Referencing the toolset project (`Azure.Mcp.Tools.{Toolset}`) instead of the CLI project
- **Solution**: Live test projects must reference `Azure.Mcp.Server.csproj` and include specific project properties
- **Required Configuration**:
  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>net9.0</TargetFramework>
      <ImplicitUsings>enable</ImplicitUsings>
      <Nullable>enable</Nullable>
      <IsPackable>false</IsPackable>
      <IsTestProject>true</IsTestProject>
      <OutputType>Exe</OutputType>
    </PropertyGroup>

    <ItemGroup>
      <ProjectReference Include="..\..\Azure.Mcp.Tools.{Toolset}\src\Azure.Mcp.Tools.{Toolset}.csproj" />
      <ProjectReference Include="..\..\..\..\servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj" />
    </ItemGroup>
  </Project>
  ```
- **Key Requirements**:
  - `OutputType=Exe` - Required for live test execution
  - `IsTestProject=true` - Marks as test project
  - Reference to `Azure.Mcp.Server.csproj` - Provides the executable for MCP server
  - Reference to toolset project - Provides the commands to test
- **Common fixes**:
  - Adjust `@minLength`/`@maxLength` for service naming limits
  - Ensure unique resource names within scope
  - Use supported API versions for resource types
  - Verify location support for specific resource types

**Issue: High deployment costs during testing**
- **Cause**: Using expensive SKUs or resource configurations
- **Solution**: Use minimal configurations for test resources
- **Best practices**:
  - SQL: Use Basic tier with small capacity
  - Storage: Use Standard LRS with minimal replication
  - Cosmos: Use serverless or minimal RU/s allocation
  - Always specify cost-effective options in Bicep templates

### Service Implementation Issues

**Issue: JSON Serialization Context missing new types**
- **Cause**: New model classes not included in `{Toolset}JsonContext` causing serialization failures
- **Solution**: Add all new model types to the JSON serialization context
- **Fix**: Update `{Toolset}JsonContext.cs` to include `[JsonSerializable(typeof(NewModelType))]` attributes
- **Prevention**: Always update JSON context when adding new model classes

**Issue: Toolset not registered in Program.cs**
- **Cause**: New toolset setup not added to `RegisterAreas()` method in `Program.cs`
- **Solution**: Add toolset registration to the array in alphabetical order
- **Fix**: Add `new Azure.Mcp.Tools.{Toolset}.{Toolset}Setup(),` to the `RegisterAreas()` return array
- **Prevention**: Follow the complete toolset setup checklist including Program.cs registration

**Issue: HandleException parameter mismatch**
- **Cause**: Confusion about the correct HandleException signature
- **Solution**: Always use `HandleException(context, ex)` - this is the correct signature in BaseCommand
- **Fix**: The method signature is `HandleException(CommandContext context, Exception ex)`, not `HandleException(context.Response, ex)`

**Issue: Missing AddSubscriptionInformation**
- **Cause**: Subscription commands need telemetry context
- **Solution**: Add `context.Activity?.WithSubscriptionTag(options);` or use `AddSubscriptionInformation(context.Activity, options);`

**Issue: Service not registered in DI**
- **Cause**: Forgot to register service in toolset setup
- **Solution**: Add `services.AddSingleton<IServiceInterface, ServiceImplementation>();` in ConfigureServices

### Base Command Class Issues

**Issue: Wrong logger type in base command constructor**
- **Example**: `ILogger<BaseSqlCommand<TOptions>>` in `BaseDatabaseCommand`
- **Solution**: Use correct generic type: `ILogger<BaseDatabaseCommand<TOptions>>`

**Issue: Missing using statements for TrimAnnotations**
- **Solution**: Add `using Azure.Mcp.Core.Commands;` for `TrimAnnotations.CommandAnnotations`

### AOT Compilation Issues

**Issue: AOT compilation fails with runtime dependencies**
- **Cause**: Some Azure SDK packages or dependencies are not AOT (Ahead-of-Time) compilation compatible
- **Symptoms**: Build errors when running `./eng/scripts/Build-Local.ps1 -BuildNative`
- **Solution**: Exclude non-AOT safe projects and packages for native builds
- **Fix Steps**:
  1. **Move toolset setup under conditional compilation** in `servers/Azure.Mcp.Server/src/Program.cs`:
     ```csharp
     #if !BUILD_NATIVE
         new Azure.Mcp.Tools.{Toolset}.{Toolset}Setup(),
     #endif
     ```
  2. **Add conditional project exclusion** in `servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj`:
     ```xml
     <ItemGroup Condition="'$(BuildNative)' == 'true'">
       <ProjectReference Remove="..\..\..\tools\Azure.Mcp.Tools.{Toolset}\src\Azure.Mcp.Tools.{Toolset}.csproj" />
     </ItemGroup>
     ```
  3. **Remove problematic package references** when building native (if applicable):
     ```xml
     <ItemGroup Condition="'$(BuildNative)' == 'true'">
       <PackageReference Remove="ProblematicPackage" />
     </ItemGroup>
     ```
- **Examples**: See Cosmos, Monitor, Postgres, Search, VirtualDesktop, and BicepSchema toolsets in Program.cs and Azure.Mcp.Server.csproj
-**Prevention**: Test AOT compilation early in development using `./eng/scripts/Build-Local.ps1 -BuildNative`
-**Note**: Toolsets excluded from AOT builds are still available in regular builds and deployments

## Checklist

Before submitting:

### Core Implementation
- [ ] Options class follows inheritance pattern
- [ ] Command class implements all required members
- [ ] Command uses proper OptionDefinitions
- [ ] Service interface and implementation complete
- [ ] Unit tests cover all paths
- [ ] Integration tests added
- [ ] Command registered in toolset setup RegisterCommands method
- [ ] Follows file structure exactly
- [ ] Error handling implemented
- [ ] Documentation complete

### **CRITICAL: Live Test Infrastructure (Required for Azure Service Commands)**

**⚠️ MANDATORY for any command that interacts with Azure resources:**

- [ ] **Live test infrastructure created** (`test-resources.bicep` template in `tools/Azure.Mcp.Tools.{Toolset}/tests`)
- [ ] **Post-deployment script created** (`test-resources-post.ps1` in `tools/Azure.Mcp.Tools.{Toolset}/tests` - required even if basic template)
- [ ] **Bicep template validated** with `az bicep build --file tools/Azure.Mcp.Tools.{Toolset}/tests/test-resources.bicep`
- [ ] **Live test resource template tested** with `./eng/scripts/Deploy-TestResources.ps1 -Toolset {Toolset}`
- [ ] **RBAC permissions configured** for test application in Bicep template (use appropriate built-in roles)
- [ ] **Live test project configuration correct**:
  - [ ] References `Azure.Mcp.Server.csproj` (not just the toolset project)
  - [ ] Includes `OutputType=Exe` property
  - [ ] Includes `IsTestProject=true` property
- [ ] **Live tests use deployed resources** via `Settings.ResourceBaseName` pattern
- [ ] **Resource outputs defined** in Bicep template for test consumption
- [ ] **Cost optimization verified** (use Basic/Standard SKUs, minimal configurations)

**This section is ONLY needed if your command interacts with Azure resources (e.g., Storage, KeyVault).**

### Package and Project Setup
- [ ] Azure Resource Manager package added to both `Directory.Packages.props` and `Azure.Mcp.Tools.{Toolset}.csproj`
- [ ] **Package version consistency**: Same version used in both `Directory.Packages.props` and project references
- [ ] **Solution file integration**: Projects added to `AzureMcp.sln` with unique GUIDs (no GUID conflicts)
- [ ] **Toolset registration**: Added to `Program.cs` `RegisterAreas()` method in alphabetical order
- [ ] JSON serialization context includes all new model types

### Build and Code Quality
- [ ] No compiler warnings
- [ ] Tests pass (run specific tests: `dotnet test --filter "FullyQualifiedName~YourCommandTests"`)
- [ ] Build succeeds with `dotnet build`
- [ ] Code formatting applied with `dotnet format`
- [ ] Spelling check passes with `.\eng\common\spelling\Invoke-Cspell.ps1`
- [ ] **AOT compilation verified** with `./eng/scripts/Build-Local.ps1 -BuildNative`
- [ ] **Clean up unused using statements**: Run `dotnet format --include="tools/Azure.Mcp.Tools.{Toolset}/**/*.cs"` to remove unnecessary imports and ensure consistent formatting
- [ ] Fix formatting issues with `dotnet format ./AzureMcp.sln` and ensure no warnings
- [ ] Identify unused properties for Azure Resource with `.\eng\scripts\Check-Unused-ResourceProperties.ps1`

### Azure SDK Integration
- [ ] All Azure SDK property names verified and correct
- [ ] Resource access patterns use collections (e.g., `.GetSqlServers().GetAsync()`)
- [ ] Subscription resolution uses `ISubscriptionService.GetSubscription()`
- [ ] Service constructor includes `ISubscriptionService` injection for Azure resources

### Documentation Requirements

**REQUIRED**: All new commands must update the following documentation files:

- [ ] **CHANGELOG.md**: Add entry under "Unreleased" section describing the new command(s)
- [ ] **docs/azmcp-commands.md**: Add command documentation with description, syntax, parameters, and examples
- [ ] **README.md**: Update the supported services table and add example prompts demonstrating the new command(s) in the appropriate toolset section
- [ ] **eng/vscode/README.md**: Update the VSIX README with new service toolset (if applicable) and add sample prompts to showcase new command capabilities
- [ ] **docs/e2eTestPrompts.md**: Add test prompts for end-to-end validation of the new command(s)
- [ ] **.github/CODEOWNERS**: Add new toolset to CODEOWNERS file for proper ownership and review assignments

**Documentation Standards**:
- Use consistent command paths in all documentation (e.g., `azmcp sql db show`, not `azmcp sql database show`)
- Organize example prompts by service in README.md under service-specific sections (e.g., `### 🗄️ Azure SQL Database`)
- Place new commands in the appropriate toolset section, or create a new toolset section if needed
- Provide clear, actionable examples that users can run with placeholder values
- Include parameter descriptions and required vs optional indicators in azmcp-commands.md
- Keep CHANGELOG.md entries concise but descriptive of the capability added
- Add test prompts to e2eTestPrompts.md following the established naming convention and provide multiple prompt variations
- **eng/vscode/README.md Updates**: When adding new services or commands, update the VSIX README to maintain accurate service coverage and compelling sample prompts for marketplace visibility
- **IMPORTANT**: Maintain alphabetical sorting in e2eTestPrompts.md:
  - Service sections must be in alphabetical order by service name
  - Tool Names within each table must be sorted alphabetically
  - When adding new tools, insert them in the correct alphabetical position to maintain sort order
