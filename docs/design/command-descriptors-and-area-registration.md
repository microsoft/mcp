# Command Descriptors and Area Registration

This document describes the architecture for defining MCP commands, registering
service dependencies, and building runtime command trees. It covers the
descriptor model, area registration pattern, keyed DI resolution, and the
consolidated tool discovery strategy.

## Overview

Commands in the MCP server are defined declaratively via **descriptor objects**
(`CommandGroupDescriptor`, `CommandDescriptor`, `OptionDescriptor`). These
descriptors are pure data — serializable to JSON — and carry no runtime
dependencies. At startup, each **area registration** provides a descriptor tree
and registers its command handlers into the DI container. The `CommandFactory`
then builds the runtime command tree by walking descriptors and resolving
handlers from DI.

```
┌──────────────────────┐   ┌──────────────────────────────────┐   ┌────────────────────┐
│ AreaRegistrationInfo │   │ CommandFactory                   │   │ System.CommandLine │
│ (per toolset)        │──>│                                  │──>│ (CLI execution)    │
│                      │   │ Builds command tree via          │   │                    │
│ CommandDescriptors   │   │ DescriptorCommandBuilder.Build() │   │  RootCommand       │
│ ConfigureServices()  │   │                                  │   │  └─ SubCommands    │
└──────────────────────┘   └──────────────────────────────────┘   └────────────────────┘
```

## Descriptor Model

### CommandGroupDescriptor

Represents a hierarchical grouping of commands (maps to a `System.CommandLine.Command`).

```csharp
public sealed record CommandGroupDescriptor
{
    public required string Name { get; init; }        // CLI verb: "storage", "keyvault"
    public required string Description { get; init; } // Help text
    public string? Title { get; init; }               // Display name
    public string? Category { get; init; }            // Help grouping: "Azure Services", "MCP"
    public CommandDescriptor[] Commands { get; init; } = [];
    public CommandGroupDescriptor[] SubGroups { get; init; } = [];
}
```

Groups nest arbitrarily: `keyvault` → `admin` → `settings` → `get`. The
top-level descriptor for an area carries `Name`, `Title`, and `Category` which
identify the area throughout the system (help output, command mapping, etc.).

### CommandDescriptor

Defines a single executable command within a group.

```csharp
public sealed record CommandDescriptor
{
    public required string Id { get; init; }          // Stable GUID
    public required string Name { get; init; }        // Verb: "list", "get", "create"
    public required string Description { get; init; }
    public required string Title { get; init; }
    public required string HandlerType { get; init; } // Type name for DI resolution
    public ToolAnnotations? Annotations { get; init; }
    public OptionDescriptor[] Options { get; init; } = [];
    public InheritOptions InheritOptions { get; init; } = InheritOptions.Basic;
    public bool Hidden { get; init; }
}
```

Key fields:

- **`HandlerType`** — the type name of the `IBaseCommand` implementation
  (e.g., `nameof(AccountGetCommand)`). Resolved at runtime via keyed DI.
- **`InheritOptions`** — controls automatic injection of shared options:
  - `Basic` — no inherited options
  - `Global` — tenant, auth method, retry policy options
  - `Subscription` — global options plus `--subscription` with validation
- **`Annotations`** — behavioral metadata (`Destructive`, `ReadOnly`,
  `Idempotent`, `Secret`, `LocalRequired`, `OpenWorld`). Used for filtering
  and MCP tool annotations.

### OptionDescriptor

Defines a command option (maps to a `System.CommandLine.Option`).

```csharp
public sealed record OptionDescriptor
{
    public required string Name { get; init; }        // Without "--" prefix
    public required string Description { get; init; }
    public required string TypeName { get; init; }    // "string", "int32", "boolean"
    public bool Required { get; init; }
    public string? DefaultValue { get; init; }        // JSON-serialized
    public bool Hidden { get; init; }
}
```

## Area Registration

### AreaRegistrationInfo

The base class that tool authors subclass to define an area. Each area provides
two things: a descriptor tree (what commands exist) and service registrations
(how to resolve handlers and their dependencies).

```csharp
public class AreaRegistrationInfo
{
    public virtual CommandGroupDescriptor CommandDescriptors =>
        throw new NotImplementedException();

    public virtual void ConfigureServices(IServiceCollection services) { }

    public CommandGroup? CommandGroupOverride { get; init; }
}
```

- **`CommandDescriptors`** — override to return the static descriptor tree.
- **`ConfigureServices`** — override to register command handlers and service
  dependencies into DI.
- **`CommandGroupOverride`** — when set, bypasses descriptor-based tree
  building and uses a pre-built `CommandGroup` directly. Used by
  [consolidated tool discovery](#consolidated-tool-discovery).

### Implementing an Area

Each toolset provides a sealed Setup class that inherits `AreaRegistrationInfo`:

```csharp
public sealed class StorageSetup : AreaRegistrationInfo
{
    public override CommandGroupDescriptor CommandDescriptors { get; } = new()
    {
        Name = "storage",
        Description = "Manage Azure Storage Account operations.",
        Title = "Manage Azure Storage Account",
        Category = "Azure Services",
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "account",
                Description = "Storage account operations.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "eb2363f1-f21f-45fc-ad63-bacfbae8c45c",
                        Name = "get",
                        Description = "Retrieves storage account details.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            ReadOnly = true,
                            Idempotent = true,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "account",
                                Description = "The storage account name.",
                                TypeName = "string",
                            },
                        ],
                        InheritOptions = InheritOptions.Subscription,
                        HandlerType = nameof(AccountGetCommand),
                    },
                ],
            },
            // ... more subgroups (blob, table, etc.)
        ],
    };

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IStorageService, StorageService>();
        services.AddCommand<AccountGetCommand>();
        services.AddCommand<AccountCreateCommand>();
        // ... more commands
    }
}
```

### Keyed DI Registration

The `AddCommand<T>()` extension method registers each command handler as a
keyed singleton, using the type name as the key:

```csharp
public static class AreaServiceCollectionExtensions
{
    public static IServiceCollection AddCommand<T>(
        this IServiceCollection services) where T : class, IBaseCommand
    {
        services.AddKeyedSingleton<IBaseCommand, T>(typeof(T).Name);
        return services;
    }
}
```

At resolution time, `CommandFactory` retrieves handlers by type name:

```csharp
_serviceProvider.GetRequiredKeyedService<IBaseCommand>(handlerTypeName)
```

This decouples the descriptor (which stores a string `HandlerType`) from the
concrete handler type, enabling future scenarios like JSON-serialized
descriptors loaded without handler assemblies.

## Startup Registration

Each MCP server defines a static array of area registrations and loops through
them at startup:

```csharp
// Program.cs
private static AreaRegistrationInfo[] DescriptorAreas =
[
    // Core areas
    new ServerSetup(),
    new ToolsSetup(),
    new GroupSetup(),
    new SubscriptionSetup(),

    // Service areas
    new StorageSetup(),
    new KeyVaultSetup(),
    // ... 50+ more
];

internal static void ConfigureServices(IServiceCollection services)
{
    // Infrastructure registrations...
    services.AddSingleton<ICommandFactory, CommandFactory>();

    // Area registrations
    foreach (var areaInfo in DescriptorAreas)
    {
        services.AddSingleton(areaInfo);       // Makes area available to CommandFactory
        areaInfo.ConfigureServices(services);  // Registers handlers + dependencies
    }
}
```

## Command Tree Building

When `CommandFactory` is constructed, it walks all registered areas and builds
the runtime command tree:

```
CommandFactory constructor
  └─ RegisterCommandGroup()
       └─ for each AreaRegistrationInfo:
            └─ RegisterDescriptorArea()
                 ├─ Validate unique area name (from CommandDescriptors.Name)
                 ├─ If CommandGroupOverride is set → use it directly
                 ├─ Otherwise → DescriptorCommandBuilder.Build()
                 │    ├─ Create CommandGroup for each CommandGroupDescriptor
                 │    ├─ Create DescriptorBackedCommand for each CommandDescriptor
                 │    ├─ Inject inherited options (subscription, tenant, etc.)
                 │    └─ Resolve handler via keyed DI
                 └─ Map tokenized command names to area name
```

`DescriptorCommandBuilder` recursively transforms the descriptor tree into
`CommandGroup` objects. Each `CommandDescriptor` becomes a
`DescriptorBackedCommand` that wraps the resolved `IBaseCommand` handler.
Inherited options are injected based on the `InheritOptions` enum.

Command names are tokenized with underscores for the flat command map:
`storage_account_get`, `keyvault_secret_list`, etc.

## Consolidated Tool Discovery

The `ConsolidatedToolDiscoveryStrategy` re-packages existing commands from the
primary `CommandFactory` into a new `CommandFactory` with different groupings.
This enables MCP tool consolidation — presenting many fine-grained commands as
fewer, coarser tool groups to AI agents.

### How It Works

1. **Load definitions** — reads a `consolidated-tools.json` manifest that maps
   consolidated tool names to lists of original command names.

2. **Filter commands** — applies runtime filters to the primary factory's
   commands:
   - Excludes `server` and `tools` command groups
   - If `ReadOnly` mode: only read-only commands
   - If HTTP mode: excludes `LocalRequired` commands
   - If `Namespace` filter: only commands from specified service areas

3. **Group by consolidated tool** — matches filtered commands to consolidated
   tool definitions via `MappedToolList`.

4. **Validate metadata** — in debug builds, ensures every command's
   `ToolMetadata` matches its consolidated tool's declared metadata. Throws on
   mismatch.

5. **Build consolidated areas** — for each consolidated tool, creates a
   lightweight `AreaRegistrationInfo` subclass with a `CommandGroupOverride`
   containing the pre-resolved commands.

6. **Create new CommandFactory** — constructs a separate `CommandFactory` from
   the consolidated areas. This factory has a flat command structure where each
   consolidated tool is a top-level namespace.

### Programmatic Area Creation

The consolidated strategy creates areas programmatically using a private nested
class, since these areas don't need service registration (commands are already
resolved):

```csharp
private static AreaRegistrationInfo CreateConsolidatedAreaRegistration(
    ConsolidatedToolDefinition consolidatedTool,
    Dictionary<string, IBaseCommand> matchingCommands)
{
    var name = consolidatedTool.Name ?? string.Empty;

    return new ConsolidatedArea(new CommandGroupDescriptor
    {
        Name = name,
        Description = name,
        Title = name,
        Category = "Azure Services",
        Commands = []
    })
    {
        // Bypass descriptor build — commands already resolved
        CommandGroupOverride = BuildConsolidatedCommandGroup(
            name, consolidatedTool, matchingCommands)
    };
}

private sealed class ConsolidatedArea(
    CommandGroupDescriptor descriptors) : AreaRegistrationInfo
{
    public override CommandGroupDescriptor CommandDescriptors => descriptors;
}
```

The `CommandGroupOverride` causes `CommandFactory.RegisterDescriptorArea` to
skip `DescriptorCommandBuilder.Build()` entirely and use the pre-built
`CommandGroup` as-is.

### Filter Pipeline

```
AllCommands (from primary CommandFactory)
  │
  ├─ Exclude ignored groups ("server", "tools")
  ├─ ReadOnly filter (if --read-only mode)
  ├─ LocalRequired filter (if HTTP transport)
  └─ Namespace filter (if --namespace specified)
  │
  ▼
FilteredCommands
  │
  ├─ Match to ConsolidatedToolDefinition.MappedToolList
  ├─ Validate ToolMetadata consistency (DEBUG only)
  └─ Build CommandGroup with matched commands
  │
  ▼
New CommandFactory (consolidated view)
```

## Design Principles

1. **Descriptors are data** — `CommandGroupDescriptor` and its children are
   sealed records with JSON serialization attributes. They carry no behavior
   and can be cached, serialized, or generated by tooling.

2. **Handlers are deferred** — `HandlerType` is a string, not a type
   reference. Handlers are resolved from DI at registration time, not at
   descriptor creation time. This enables future scenarios where descriptors
   are loaded from JSON without loading handler assemblies.

3. **One area, one Setup class** — each toolset defines exactly one
   `AreaRegistrationInfo` subclass that provides both the command tree and
   service registrations. This is the developer-facing API for tool authors.

4. **Keyed singletons** — command handlers are registered as keyed singletons
   (`AddCommand<T>()`), enabling resolution by type name string. This bridges
   the gap between the string-based descriptor model and the strongly-typed DI
   container.

5. **Override for pre-built trees** — `CommandGroupOverride` provides an escape
   hatch for scenarios (like consolidated tools) where the command tree is
   already built and descriptor-based building should be skipped.

6. **Category is metadata, not taxonomy** — the `Category` string on
   `CommandGroupDescriptor` is used only for help output grouping. It's a
   free-form string, not an enum, so new servers can define their own
   categories without modifying core.

## MCP Tool Generation

When the MCP server starts in `server start` mode, `CommandFactoryToolLoader`
converts the runtime command tree into MCP `Tool` objects for the `tools/list`
protocol response. Understanding this path is important because it currently
routes through System.CommandLine as an intermediary rather than generating MCP
tools directly from descriptors.

### Current Flow

```
CommandDescriptor                          System.CommandLine                    MCP Protocol
─────────────────                          ──────────────────                    ────────────
                                           DescriptorBackedCommand
OptionDescriptor[]   ──(CreateOption)──►   Command.Options[]     ──(GetTool)──► Tool.InputSchema
  .Name                                      Option.Name                          properties[name]
  .Description                               Option.Description                   properties[name].description
  .TypeName ("string")                       Option<string>                        properties[name].type
  .Required                                  Option.Required                       schema.required[]

Annotations          ──(ToToolMetadata)──► IBaseCommand.Metadata  ──(GetTool)──► Tool.Annotations
  .ReadOnly                                  .ReadOnly                             .ReadOnlyHint
  .Destructive                               .Destructive                          .DestructiveHint
  .Idempotent                                .Idempotent                           .IdempotentHint
  .OpenWorld                                 .OpenWorld                            .OpenWorldHint
  .Secret                                    .Secret                               Tool.Meta["SecretHint"]
  .LocalRequired                             .LocalRequired                        Tool.Meta["LocalRequiredHint"]

InheritOptions       ──(AddInherited)──►   Command.Options[]     ──(GetTool)──► Tool.InputSchema
  .Subscription                              + --subscription                      + subscription property
  .Global                                    + --tenant, etc.                      + tenant property, etc.
```

#### Step 1: Descriptor → System.CommandLine

`DescriptorBackedCommand.CreateCommand()` converts each `OptionDescriptor`
into a `System.CommandLine.Option<T>` by switching on `TypeName`:

```csharp
// DescriptorBackedCommand.cs
Option option = opt.TypeName.ToLowerInvariant() switch
{
    "string"              => CreateTypedOption<string>(opt),
    "int32" or "int"      => CreateTypedOption<int>(opt),
    "boolean" or "bool"   => CreateTypedOption<bool>(opt),
    // ...
};
```

Then `DescriptorCommandBuilder.AddInheritedOptions()` appends shared options
(subscription, tenant, retry policy) based on `InheritOptions` — these are
`OptionDefinitions` singletons, not descriptors.

#### Step 2: System.CommandLine → MCP Tool

`CommandFactoryToolLoader.GetTool()` reads the `System.CommandLine.Command`
back to build the MCP schema:

```csharp
// CommandFactoryToolLoader.cs
foreach (var option in command.GetCommand().Options)
{
    var propName = NameNormalization.NormalizeOptionName(option.Name);
    schema.Properties.Add(propName,
        TypeToJsonTypeMapper.CreatePropertySchema(option.ValueType, option.Description));
}
schema.Required = options.Where(p => p.Required)
    .Select(p => NameNormalization.NormalizeOptionName(p.Name)).ToArray();
```

`TypeToJsonTypeMapper.CreatePropertySchema()` inspects the CLR `Type` from
`Option.ValueType` (e.g., `typeof(string)`) and maps it to a JSON Schema type
string (`"string"`, `"integer"`, `"array"`, etc.).

### Why This Is Indirect

Every piece of information in the MCP `Tool` originates from the
`CommandDescriptor` and its `OptionDescriptor[]`:

| MCP Tool field | Comes from | Via S.CL? |
|---|---|---|
| `Name` | `CommandFactory` tokenized key | No |
| `Description` | `CommandDescriptor.Description` | Via `Command.Description` |
| `InputSchema.properties[x].type` | `OptionDescriptor.TypeName` | Via `Option<T>.ValueType` |
| `InputSchema.properties[x].description` | `OptionDescriptor.Description` | Via `Option.Description` |
| `InputSchema.required[]` | `OptionDescriptor.Required` | Via `Option.Required` |
| `Annotations.*Hint` | `CommandDescriptor.Annotations` | Via `ToolMetadata` |
| Inherited options (subscription, etc.) | `InheritOptions` enum | Via `OptionDefinitions` singletons |

The only information that **must** flow through System.CommandLine is the
inherited options — subscription, tenant, retry policy — because they come from
`OptionDefinitions` singletons that are `System.CommandLine.Option` instances,
not from descriptors.

### Future Direction: Direct Descriptor → MCP Tool

The descriptor model already contains all the information needed to produce MCP
tools without the System.CommandLine intermediary:

```
                          ┌──► System.CommandLine (CLI path)
                          │      Command + Options + Validators
CommandDescriptor ────────┤
                          │
                          └──► MCP Tool (server path)
                                 Tool + InputSchema + Annotations
```

To enable this:

1. **Inherited options in descriptors** — the `InheritOptions` enum could be
   expanded at descriptor build time to include the actual option definitions
   (subscription, tenant, etc.) in the `OptionDescriptor[]` array, so
   descriptors are self-contained.

2. **Direct schema generation** — a `DescriptorToMcpToolMapper` could convert
   `OptionDescriptor.TypeName` → JSON Schema type directly (the mapping is
   trivial: `"string"` → `"string"`, `"int32"` → `"integer"`, etc.) without
   going through CLR `Type` reflection.

3. **Descriptor-only startup** — MCP server mode could skip
   `DescriptorCommandBuilder.Build()` entirely and generate `Tool` objects
   straight from descriptors. Handler resolution would still happen at call
   time via keyed DI.

This would decouple the MCP server path from System.CommandLine entirely,
reducing startup cost and enabling scenarios like serving tools from
JSON-serialized descriptor files without loading tool assemblies.

## File Locations

| Component | Path |
|---|---|
| `AreaRegistrationInfo` | `core/Microsoft.Mcp.Core/src/Areas/AreaRegistrationInfo.cs` |
| `CommandGroupDescriptor` | `core/Microsoft.Mcp.Core/src/Commands/Descriptors/CommandGroupDescriptor.cs` |
| `CommandDescriptor` | `core/Microsoft.Mcp.Core/src/Commands/Descriptors/CommandDescriptor.cs` |
| `OptionDescriptor` | `core/Microsoft.Mcp.Core/src/Commands/Descriptors/OptionDescriptor.cs` |
| `DescriptorBackedCommand` | `core/Microsoft.Mcp.Core/src/Commands/DescriptorBackedCommand.cs` |
| `DescriptorCommandBuilder` | `core/Microsoft.Mcp.Core/src/Commands/DescriptorCommandBuilder.cs` |
| `AddCommand<T>()` | `core/Microsoft.Mcp.Core/src/Areas/AreaServiceCollectionExtensions.cs` |
| `CommandFactory` | `core/Microsoft.Mcp.Core/src/Commands/CommandFactory.cs` |
| `CommandFactoryToolLoader` | `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs` |
| `TypeToJsonTypeMapper` | `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/TypeToJsonTypeMapper.cs` |
| `ConsolidatedToolDiscoveryStrategy` | `core/Microsoft.Mcp.Core/src/Areas/Server/Commands/Discovery/ConsolidatedToolDiscoveryStrategy.cs` |
| Azure server startup | `servers/Azure.Mcp.Server/src/Program.cs` |
| Fabric server startup | `servers/Fabric.Mcp.Server/src/Program.cs` |
| Example Setup (Storage) | `tools/Azure.Mcp.Tools.Storage/src/StorageSetup.cs` |
| Example Setup (KeyVault) | `tools/Azure.Mcp.Tools.KeyVault/src/KeyVaultSetup.cs` |
