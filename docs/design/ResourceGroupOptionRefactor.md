# Resource Group Option Refactor (Current Design)

## Objective

Let each command declare whether `--resource-group` is required, optional, conditionally required, or not exposed, using the same attributed options model as every other command option.

## Current Design

`--resource-group` is not a global `Option<T>` instance. A command exposes it by adding an attributed property to its flat options POCO:

```csharp
public sealed class ResourceGetOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
```

`OptionBinder` registers and binds the property automatically. The C# `required` modifier controls unconditional parser requiredness; nullability alone does not.

## Semantics

| Scenario | Property | Additional validation | Missing behavior |
|----------|----------|-----------------------|------------------|
| Always required | `public required string ResourceGroup { get; set; }` | None | `OptionBinder` returns HTTP 400 |
| Optional filter | `public string? ResourceGroup { get; set; }` | None | Command runs at subscription scope |
| Conditionally required | `public string? ResourceGroup { get; set; }` | `ValidateOptions` | Command returns HTTP 400 when the condition applies |
| Irrelevant | No property | None | Option is not exposed |

For a conditional requirement:

```csharp
public override void ValidateOptions(ResourceGetOptions options, ValidationResult validationResult)
{
    base.ValidateOptions(options, validationResult);

    if (!string.IsNullOrEmpty(options.Resource) && string.IsNullOrEmpty(options.ResourceGroup))
    {
        validationResult.Errors.Add("--resource-group is required when --resource is specified.");
    }
}
```

Do not add `RegisterOptions`, `BindOptions`, `UseResourceGroup`, `RequireResourceGroup`, or `GetResourceGroup` methods to new commands. Those APIs belong to superseded command patterns.

## Shared Descriptions

Use `[Option(Description = OptionDescriptions.ResourceGroup)]` for the standard help text. A toolset-specific description constant is appropriate only when the option has additional service-specific semantics.

## Testing Guidelines

Use the command test base and string arguments so tests exercise attribute registration, parsing, binding, and validation together:

1. Required resource group: omit `--resource-group` and assert HTTP 400.
2. Optional resource group: omit it and assert the expected subscription-scoped result.
3. Conditional resource group: test both the triggering and non-triggering option combinations.
4. Verify successful calls pass `options.ResourceGroup` to the service unchanged.

Subscription commands use `SubscriptionCommandUnitTestsBase<TCommand, TService>`.
