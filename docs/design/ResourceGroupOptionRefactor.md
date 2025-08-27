# Resource Group Option Refactor (Final Design)

## Objective
Provide a single, optional global `--resource-group` option and a simple declarative way for commands to express whether they merely accept it or logically require it.

## Final Design Summary
1. A single global option: `OptionDefinitions.Common.ResourceGroup` (always parser-optional).
2. Two helpers on `BaseCommand`:
   * `UseResourceGroup()` – register the option (optional semantics).
   * `RequireResourceGroup()` – register the option and mark it as logically required.
3. Central logical validation in `BaseCommand.ValidateLogicalRequirements` produces a uniform `400` error with message: `resource-group is required for this command.` when a required resource group is missing.
4. Commands bind the value via `GetResourceGroup(parseResult)` (returns `null` if not registered / not provided).
5. No area-specific optional variants; all previous `OptionalResourceGroup` definitions removed.

## Rationale
* Eliminates duplicated option declarations and inconsistent help text.
* Keeps parser configuration simple (no dynamic required flags or custom builders).
* Shifts requirement enforcement to a business rule making optional cross-RG or subscription-scoped operations easy.
* Produces uniform error messaging and lowers boilerplate.

## Helper API (Implemented)
```csharp
// In BaseCommand
protected void UseResourceGroup();      // adds global option if not yet added
protected void RequireResourceGroup();  // adds and marks as logically required
protected string? GetResourceGroup(ParseResult parseResult);

// Central validation (excerpt)
if (result.IsValid && _requiresResourceGroup) {
   var rg = commandResult.GetValueForOption(OptionDefinitions.Common.ResourceGroup);
   if (string.IsNullOrWhiteSpace(rg)) { /* set 400 + message */ }
}
```

## Command Usage Examples
```csharp
protected override void RegisterOptions(Command command)
{
   base.RegisterOptions(command);
   RequireResourceGroup();        // RG now logically required
   command.Options.Add(_nameOption); // other options
}

protected override void RegisterOptions(Command command)
{
   base.RegisterOptions(command);
   UseResourceGroup();            // RG optional filter
   command.Options.Add(_filterOption);
}

protected override MyOptions BindOptions(ParseResult parseResult)
{
   var o = base.BindOptions(parseResult);
   o.ResourceGroup = GetResourceGroup(parseResult); // null if omitted / not used
   return o;
}
```

## Current Implementation Status
Implemented:
* Global option made optional.
* Helpers added to `BaseCommand`.
* Central logical validation active.
* Area-specific optional variants (ACR, Monitor Metrics, Extension Azqr) removed.
* Migrated representative commands and base classes (AKS, Workbooks, ACR registry, Metrics, LoadTesting, Postgres, Redis, SQL, Storage create, Extension Azqr, Monitor health / tables, Foundry, AzureIsv).

Pending / Follow‑ups (if desired):
* Ensure every remaining command uses `UseResourceGroup()` / `RequireResourceGroup()` instead of direct `command.Options.Add(_resourceGroupOption)`.
* Update `docs/new-command.md` to reference helpers (remove any legacy guidance).
* Add / adjust tests:
  - Required command without RG -> 400 + standard message.
  - Optional command without RG -> succeeds (or broader listing) and does not parse-fail.
* Update CHANGELOG.

## Semantics Cheat Sheet
| Scenario | Helper | Parser Requires? | Runtime Requires? | Behavior When Missing |
|----------|--------|------------------|-------------------|-----------------------|
| RG truly needed | `RequireResourceGroup()` | No | Yes | 400 + message |
| RG optional filter | `UseResourceGroup()` | No | No | Command executes with broader scope |
| RG irrelevant | (omit both) | No | No | Option not exposed |

## Error Message Contract
Missing required RG always yields: `resource-group is required for this command.` (HTTP 400 in tool response).

## Testing Guidelines
1. Do not assert parser-level failures for missing RG; assert logical validation (response status/message).
2. Optional commands: omit RG and assert success + expected broader result set.
3. Include at least one negative test per area where RG is required.

## Extensibility Pattern
If another option needs a conditional requirement (e.g., `--location`): introduce parallel helpers (`UseLocation()`, `RequireLocation()`) and replicate the minimal flag + central validation approach. Avoid generic abstraction until a second concrete need exists.

**Note**: This approach provides an interim improvement to the design. A fully extensible solution for "included but conditionally required" options will be implemented as part of https://github.com/Azure/azure-mcp/issues/84. The current design addresses immediate maintainability concerns while establishing patterns that can be generalized in the future.

## Migration Summary
Removed: All `OptionalResourceGroup` definitions and per-command duplicate validations.
Added: Two helper methods + central validation branch.
Modified: Commands now call a single declarative helper and bind via `GetResourceGroup` (or optional direct parse if already using it).

## Changelog (Planned Entry)
```
### Changed
* Made global --resource-group parser-optional.
* Removed area-specific optional resource group option definitions (ACR, Monitor Metrics, Extension Azqr).
* Added helper methods UseResourceGroup / RequireResourceGroup with centralized logical validation.
```

## Contributor Guidance
When adding a new command:
1. Decide if the command needs, optionally filters by, or ignores resource group.
2. Call exactly one of: `RequireResourceGroup()`, `UseResourceGroup()`, or neither.
3. Bind with `GetResourceGroup(parseResult)` if used.
4. Rely on central validation—do not add custom RG presence checks.

This document reflects the final design; earlier exploratory approaches (builder patterns, marker interfaces) have been superseded and intentionally omitted.
