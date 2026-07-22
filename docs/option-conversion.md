# Converting Commands to Strongly-Typed Options (Two-Generic Pattern)

This guide describes how to convert commands from the legacy **one-generic** `BaseCommand<TOptions>` pattern to the **two-generic** `BaseCommand<TOptions, TResult>` pattern with attribute-driven option binding.

> **Cardinal rule: We're not changing the API, just removing boilerplate.**
>
> The `tools list` output and `--help` for every converted command must be identical before and after conversion. Same option names, same descriptions, same required flags. If a diff shows any change, it's either a bug to fix or an intentional cleanup that needs team review.

## Why Convert?

The old pattern requires manual `RegisterOptions` / `BindOptions` overrides at every level of the command hierarchy. Each base command class adds its options imperatively and binds them by hand:

```csharp
// OLD: every level manually adds and binds options
protected override void RegisterOptions(Command command)
{
    base.RegisterOptions(command);
    command.Options.Add(StorageOptionDefinitions.Account);
}

protected override TOptions BindOptions(ParseResult parseResult)
{
    var options = base.BindOptions(parseResult);
    options.Account = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Account.Name);
    return options;
}
```

The new pattern uses `[Option]` attributes on a flat options POCO. `OptionBinder` handles registration and binding automatically:

```csharp
// NEW: options are just a POCO with attributes
public class BlobUploadOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Storage account.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = "The blob name/path within the container.")]
    public required string Blob { get; set; }

    [Option(Description = "The local file path to read content from.")]
    public required string LocalFilePath { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
```

## Conversion Steps

### Step 1: Identify the Full Hierarchy

Before conversion, generate a baseline of user facing tools and options. Run `eng/scripts/New-ToolsListFile.ps1 -Suffix before` to generate clean `*-tools-before.json` files in `.work/`

Also generate `.work/tool-conversion-checklist.json` by running the `CommandTypeInventoryTests` test. For each command you'll see:

```json
{
  "commandClass": "BlobUploadCommand",
  "pattern": "one-generic",
  "commandHierarchy": [
    "BlobUploadCommand",
    "BaseBlobCommand<BlobUploadOptions>",
    "BaseContainerCommand<BlobUploadOptions>",
    "BaseStorageCommand<BlobUploadOptions>",
    "SubscriptionCommand<BlobUploadOptions>",
    "GlobalCommand<BlobUploadOptions>",
    "BaseCommand<BlobUploadOptions>"
  ],
  "optionsHierarchy": [
    "BlobUploadOptions",
    "BaseBlobOptions",
    "BaseContainerOptions",
    "BaseStorageOptions",
    "SubscriptionOptions",
    "GlobalOptions"
  ],
  "exposedOptions": [
    { "name": "--tenant", "required": false },
    { "name": "--subscription", "required": false },
    { "name": "--account", "required": true },
    { "name": "--container", "required": true },
    { "name": "--blob", "required": true },
    { "name": "--local-file-path", "required": true }
  ]
}
```

Each level in the old hierarchy adds options. You need to flatten them all into a single options class.

### Step 2: Create the New Flat Options Class

Replace the options class hierarchy with a single flat POCO that implements `ISubscriptionOption` (if the command needs a subscription). Use `[Option]` attributes to declare each option.

> **Important: Reproduce the exposed API, not the inherited properties.**
>
> Old options classes inherited many properties from base classes (`GlobalOptions`, `SubscriptionOptions`, etc.) that were never registered or bound for a given command. The new flat options class should contain **only the properties that correspond to actually exposed CLI options** for that specific command — i.e., the `exposedOptions` from the checklist.
>
> This also means nullability may change. A property like `Account` was `string?` in the old model because `BaseStorageOptions` was shared across commands that registered it as both required and optional. In the new model, if every use of `--account` for a given command is required, declare it as `required string Account` — no `?`. **Favor accuracy per-command over reusability across commands.**

**Conventions:**
- **Name**: Derived automatically from the property name in kebab-case (e.g., `LocalFilePath` → `--local-file-path`). Only use `[Option(Name = "...")]` when the convention doesn't produce the desired name (e.g., `RetryPolicy` → `--retry` instead of `--retry-policy`). **Do not** specify `Name =` when it matches the default.
- **Required**: Driven by the `required` keyword (`RequiredMemberAttribute`). Use `required` on required options; use nullable types (`?`) for optional options.
- **Description**: Always required, passed using attribute properties: `[Option(Description = "description")]`.
- **Shared descriptions**: Use constants from `OptionDescriptions` (e.g., `OptionDescriptions.Subscription`, `OptionDescriptions.Tenant`).
- **Nested objects**: Use `[OptionContainer(Prefix = "prefix")]` on a property of a complex type. Its child properties become `--prefix-child-name`. Example: `RetryPolicyOptions` with `[OptionContainer(Prefix = "retry")]` produces `--retry-delay`, `--retry-max-retries`, etc.
- **Property ordering**: List command-specific options first, then sink common/infrastructure options to the bottom in this order: `ResourceGroup`, `Subscription`, `Tenant`, `AuthMethod`, `RetryPolicy`. This keeps the most relevant options visible at a glance.

**Before (hierarchy of 6 classes):**
```
GlobalOptions                → Tenant, AuthMethod, RetryPolicy
  └─ SubscriptionOptions     → Subscription
       └─ BaseStorageOptions  → Account
            └─ BaseContainerOptions → Container
                 └─ BaseBlobOptions     → Blob
                      └─ BlobUploadOptions  → LocalFilePath
```

**After (single flat class):**
```csharp
public class BlobUploadOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Storage account.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = "The blob name/path within the container.")]
    public required string Blob { get; set; }

    [Option(Description = "The local file path to read content from.")]
    public required string LocalFilePath { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
```

> **Tip**: Copy option descriptions from the `option` array in `.work/azure-tools-before.json` or from the old `StorageOptionDefinitions` / `OptionDefinitions.Common` static fields to keep them identical.

### Step 3: Change the Command Base Class

**Old:** Chain of `BaseCommand<T>` → `GlobalCommand<T>` → `SubscriptionCommand<T>` → `BaseXxxCommand<T>`

**New:** `SubscriptionCommand<TOptions, TResult>` (which extends `AuthenticatedCommand<TOptions, TResult>` → `BaseCommand<TOptions, TResult>`)

```csharp
// OLD
public sealed class BlobUploadCommand : BaseBlobCommand<BlobUploadOptions>

// NEW
public sealed class BlobUploadCommand(
    ILogger<BlobUploadCommand> logger,
    IStorageService storageService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BlobUploadOptions, BlobUploadCommand.BlobUploadCommandResult>(subscriptionResolver)
```

Key changes:
- Add `ISubscriptionResolver` to the constructor and pass it to the base.
- Specify the `TResult` type — this can be an existing model (e.g., `BlobUploadResult`) or a new nested record defined in the command class.
- Remove all `RegisterOptions` and `BindOptions` overrides — `OptionBinder` handles this automatically.

### Step 4: Update ExecuteAsync Signature

**Old:** `ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)`

**New:** `ExecuteAsync(CommandContext context, TOptions options, CancellationToken cancellationToken)`

Options are pre-bound and pre-validated. No need to call `BindOptions(parseResult)` or `Validate(parseResult.CommandResult, context.Response)`.

```csharp
// OLD
public override async Task<CommandResponse> ExecuteAsync(
    CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
{
    if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        return context.Response;

    var options = BindOptions(parseResult);
    // ...
}

// NEW
public override async Task<CommandResponse> ExecuteAsync(
    CommandContext context, BlobUploadOptions options, CancellationToken cancellationToken)
{
    // options are already bound and validated — just use them
    // ...
}
```

### Step 5: Convert Intermediate Base Command Classes Using Interface Constraints

The old pattern uses intermediate base command classes (e.g., `BaseStorageCommand<T>`, `BaseContainerCommand<T>`) both for option registration **and** for shared behavior. In the new pattern, option registration is gone — but shared validation, error handling, or post-bind logic may still be valuable.

**If a base command class only existed to add `RegisterOptions` / `BindOptions`**, remove it entirely. The concrete command directly extends `SubscriptionCommand<TOptions, TResult>`.

**If a base command class provides real shared behavior**, keep it and convert it using the **interface constraint pattern**. This mirrors how `SubscriptionCommand<TOptions, TResult>` constrains `TOptions : ISubscriptionOption` to access `options.Subscription`.

#### Define option interfaces per shared concern

Each layer defines a small interface for the options it needs access to:

```csharp
// Defined in the tool's Options/ folder

public interface IStorageAccountOption
{
    string Account { get; }
}

public interface IContainerOption : IStorageAccountOption
{
    string Container { get; }
}

public interface IBlobOption : IContainerOption
{
    string Blob { get; }
}
```

Interfaces can inherit when it makes sense (every container operation also needs an account), but they don't have to — keep them independent if the concerns are truly separate.

#### Constrain base commands to their interface

```csharp
// Base command can access options.Account with type safety
public abstract class BaseStorageCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, IStorageAccountOption
{
    // ValidateOptions receives the options instance — interface gives type-safe access
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        // Shared validation using options.Account
    }
}

// Deeper layer adds its own interface constraint
public abstract class BaseContainerCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : BaseStorageCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, IContainerOption
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        // Shared container-level validation using options.Container
    }
}
```

#### Concrete options implement the interfaces

The options class stays flat — no inheritance. It just implements the interfaces its command chain requires:

```csharp
public class BlobUploadOptions : ISubscriptionOption, IBlobOption
{
    [Option(Description = "The name of the Azure Storage account.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = "The blob name/path within the container.")]
    public required string Blob { get; set; }

    [Option(Description = "The local file path to read content from.")]
    public required string LocalFilePath { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
```

#### Why interfaces instead of class inheritance for options?

| Approach | Options hierarchy | Constraint pattern |
|---|---|---|
| **Old (class inheritance)** | `BlobUploadOptions : BaseBlobOptions : BaseContainerOptions : ...` | `where T : BaseStorageOptions` |
| **New (interface constraint)** | `BlobUploadOptions` (flat, no base class) | `where TOptions : IStorageAccountOption` |

Benefits of the interface approach:
- **Options stay flat** — every `[Option]` attribute is visible in one file, no hunting through a class chain
- **Composable** — a command can require `IStorageAccountOption + ISomeOtherConcern` without a rigid single-inheritance tree
- **Consistent** — same pattern as `SubscriptionCommand` using `ISubscriptionOption`
- **OptionBinder-friendly** — `OptionBinder` reflects over the concrete class, so all properties are discovered regardless of which interfaces they satisfy

### Step 6: Update Validation

**Old:** `command.Validators.Add(...)` in `RegisterOptions` + `Validate()` call in `ExecuteAsync`

**New:** Override `ValidateOptions(TOptions options, ValidationResult validationResult)` on the command class:

```csharp
public override void ValidateOptions(BlobUploadOptions options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);  // checks --subscription

    if (!File.Exists(options.LocalFilePath))
    {
        validationResult.Errors.Add($"File not found: {options.LocalFilePath}");
    }
}
```

For `required` properties, `OptionBinder` automatically validates presence and throws `CommandValidationException` — no explicit check needed.

### Step 7: Verify Option Parity

After conversion, compare the tool's exposed options against the pre-conversion baseline. Run `eng/scripts/New-ToolsListFile.ps1` to generate clean `*-tools-after.json` files, then diff against the originals (e.g., `.work/azure-tools-before.json`):

```powershell
$before = (Get-Content .work/azure-tools-before.json -Raw | ConvertFrom-Json).results |
    Where-Object { $_.command -like '*blob upload*' } | ConvertTo-Json -Depth 5
$after = (Get-Content .work/azure-tools-after.json -Raw | ConvertFrom-Json).results |
    Where-Object { $_.command -like '*blob upload*' } | ConvertTo-Json -Depth 5
Compare-Object ($before -split "`n") ($after -split "`n")
```

You can also use `--help` on a specific command:
```
dotnet run --project servers/Azure.Mcp.Server/src -- storage blob upload --help
```

**Option names and required status must not change** — this is a public API.

#### Known pre-existing inconsistencies

Some commands on `main` already have minor description differences due to older one-generic commands using locally instantiated `RetryPolicyOptions` with slightly different description text. These show up in before/after diffs but are **not** caused by conversion:

| Option | Command | Description on `main` |
|---|---|---|
| `--retry-max-retries` | `appconfig kv lock set` | "Maximum number of retry attempts before giving up." |
| `--retry-max-retries` | All other commands (240+) | "Maximum number of retry attempts for failed operations before giving up." |

If your diff flags only these known discrepancies, it's safe to proceed. The inconsistency lives in the unconverted command's own `RetryPolicyOptions` instance (not in shared infrastructure) and will be resolved when that command is converted to the two-generic pattern.

#### Distinguishing unused options from removable inherited properties

During comparison you may find two categories of discrepancies:

- **Unused options** — The option was registered, bound, and visible to the user in the old command, but the command never actually consumed the value (e.g., `--auth-method` on Storage commands — `GlobalCommand` registered it, but `StorageService` never reads it). **Preserve these** in the new options class with a `// TODO: Remove unused option` comment. They are part of the exposed API and removing them requires team review.

- **Removable inherited properties** — The property existed on the old options class via inheritance (e.g., a field on `GlobalOptions`) but was **never registered** on the command and never appeared in `--help` or `tools list`. These are not part of the exposed API. **Do not carry them forward** — they were implementation artifacts of the class hierarchy, not user-facing options.

### Step 8: Update Tests

Tests for commands that extend `SubscriptionCommand<TOptions, TResult>` must inherit from `SubscriptionCommandUnitTestsBase<TCommand, TService>` instead of the plain `CommandUnitTestsBase`. This base class automatically registers a mock `ISubscriptionResolver` in DI. The existing `ExecuteCommandAsync(params string[] args)` continues to work — the two-generic base class internally binds options from `ParseResult`.

```csharp
public class AccountGetCommandTests : SubscriptionCommandUnitTestsBase<AccountGetCommand, IStorageService>
{
    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Tests still use string args — this exercises option registration AND binding
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert...
    }
}
```

`SubscriptionCommandUnitTestsBase` lives in `Azure.Mcp.Tests.Commands` and exposes a `SubscriptionResolver` property if you need to configure custom resolution behavior.

> **Without `SubscriptionCommandUnitTestsBase`**, DI will fail at runtime with "Unable to resolve service for type `ISubscriptionResolver`". The plain `CommandUnitTestsBase` only registers the service mock and logger.

> **Prefer string args over constructing options directly.** Using `ExecuteCommandAsync("--account", ...)` tests the full pipeline: `[Option]` attribute registration, `OptionBinder` parsing, and `SubscriptionResolver` post-processing. Constructing `TOptions` by hand only tests `ExecuteAsync` logic.

## Quick Reference: Old vs New

| Aspect | Old (one-generic) | New (two-generic) |
|---|---|---|
| Base class | `BaseCommand<TOptions>` | `BaseCommand<TOptions, TResult>` |
| Options constraint | `TOptions : class, new()` | `TOptions : class` |
| Subscription base | `SubscriptionCommand<TOptions>` inherits `GlobalCommand` inherits `BaseCommand` | `SubscriptionCommand<TOptions, TResult>` inherits `AuthenticatedCommand` inherits `BaseCommand` |
| Options class | Inherits from `SubscriptionOptions` → `GlobalOptions` | Flat POCO implementing `ISubscriptionOption` |
| Option registration | Manual `RegisterOptions(Command)` overrides at each level | Automatic via `[Option]` attributes on TOptions |
| Option binding | Manual `BindOptions(ParseResult)` overrides at each level | Automatic via `OptionBinder.BindOptions<TOptions>()` |
| ExecuteAsync args | `(CommandContext, ParseResult, CancellationToken)` | `(CommandContext, TOptions, CancellationToken)` |
| Validation | `command.Validators.Add(...)` + `Validate()` | Override `ValidateOptions(TOptions, ValidationResult)` |
| Subscription resolution | `CommandHelper.GetSubscription(parseResult)` | `ISubscriptionResolver` injected, called in base `BindOptions` |

## Checklist per Command

- [ ] Flatten options hierarchy into single POCO with `[Option]` attributes
- [ ] Implement `ISubscriptionOption` (if command needs subscription)
- [ ] Match all option names, descriptions, and required flags exactly
- [ ] Change base class to `SubscriptionCommand<TOptions, TResult>`
- [ ] Add `ISubscriptionResolver` constructor parameter
- [ ] Identify or define `TResult` type (existing model or new nested record)
- [ ] Update `ExecuteAsync` signature to take `TOptions` instead of `ParseResult`
- [ ] Remove manual `Validate()` call — use `ValidateOptions()` override if needed
- [ ] Remove `RegisterOptions` / `BindOptions` overrides
- [ ] Convert or remove intermediate base command classes (use interface constraints if keeping)
- [ ] **Re-parent test classes to `SubscriptionCommandUnitTestsBase<TCommand, TService>`** (add `using Azure.Mcp.Tests.Commands;`) — without this, tests fail with "Unable to resolve service for type `ISubscriptionResolver`"
- [ ] Verify option parity via `tools list` output
- [ ] Build and run tests
- [ ] Run dotnet format from the repo root

> **Remember: We're not changing the API, just removing boilerplate.** If the `tools list` diff shows any difference, stop and investigate before merging.
