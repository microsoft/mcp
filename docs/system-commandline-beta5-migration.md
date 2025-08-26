# System.CommandLine 2.0.0-beta5 Migration Plan (Repo-Specific)

This document describes the exact steps and code changes to migrate this repository to System.CommandLine (SCL) 2.0.0-beta5. It complements the guidance in `docs/commandline/*` and tailors it to our servers, core, tools, and tests.

## Goals & Non-Goals
- Migrate all CLI projects to SCL `2.0.0-beta5` with minimal, scoped edits.
- Preserve current behavior: JSON output shape, exit code semantics, telemetry, and AOT safety.
- Follow repo engineering rules (primary constructors, static when possible, System.Text.Json, AOT-safe code, build after each change).
- Non-goals: Refactoring unrelated features or changing command UX beyond necessary API updates.

## Rollout Strategy (Phased)
1. Bump package versions centrally, then build to surface errors.
2. Update entrypoints in `servers/*/src/Program.cs` to the action model (remove middleware and `.UseDefaults()`).
3. Update handler wiring in `core` (CommandFactory) to use actions with telemetry and error handling.
4. Sweep registration/binding changes across `core` and all `tools/*` projects.
5. Update tests, run full build and verification scripts, and finalize docs/CHANGELOG.

## Version Pinning
- Update `Directory.Packages.props` (preferred) to pin SCL:
  - Add or update: `<PackageVersion Include="System.CommandLine" Version="2.0.0-beta5.25277.114" />`
  - Note: NuGet may resolve a nightly-suffixed build (e.g., `2.0.0-beta5.25277.114`). Pin to the exact resolved version to avoid NU1603 mismatches, or ensure feeds resolve the plain `2.0.0-beta5` tag consistently.
- If a project uses explicit references, ensure they align with the central version or remove explicit versions in favor of central pin.

## Current Status
- Central pin updated to `System.CommandLine 2.0.0-beta5.25277.114` and solution builds locally.
- Next: convert handler wiring to actions in `CommandFactory`, remove entrypoint middleware, and sweep API usages.

## API Changes Overview (Old → New)
- Command registration:
  - `command.AddOption(option)` → `command.Options.Add(option)`
  - `command.AddCommand(subcommand)` → `command.Subcommands.Add(subcommand)`
- Option requiredness/defaults:
  - `option.IsRequired = true` → `option.Required = true`
  - Default values: keep using default value factories; validate usage compiles under beta5.
- Binding from `ParseResult`:
  - `parseResult.GetValueForOption(option)` → `parseResult.GetValue(option)`
- Handler model → Action model:
  - Previous: `command.SetHandler(async (...) => { ... })`
  - New: `command.SetAction((ParseResult parseResult, CancellationToken ct) => Task<int>)`
- Parser setup:
  - Remove `new CommandLineBuilder(root).AddMiddleware(...).UseDefaults().Build()`
  - Prefer invoking the `RootCommand` directly: `await rootCommand.InvokeAsync(args, cancellationToken)`

## Entrypoints (servers/*/src/Program.cs)
For each server:
- Remove `CommandLineBuilder` usage, `.AddMiddleware(...)`, and `.UseDefaults()`.
- Do not rely on middleware to run validation; the per-command action will perform validation and output.
- Keep centralized JSON serialization helper (e.g., `WriteResponse`) to print `CommandResponse` using source-generated `ModelsJsonContext` (AOT-safe).
- Suggested Main pattern:
  - Build DI container, create `CommandFactory`, get `rootCommand`.
  - `return await rootCommand.InvokeAsync(args);` or create a `Parser` from the root and invoke.

Minimal before/after outline:
- Before: builder + middleware calling `baseCommand.Validate(...)` and writing response/exit code.
- After: remove builder + middleware; actions call `Validate(...)`, write response if invalid, and return status.

## Core Changes

### Concrete Edits
- `core/Azure.Mcp.Core/src/Commands/CommandFactory.cs`
  - Replace `SetHandler(async context => { ... })` with `SetAction(async (ParseResult parseResult, CancellationToken ct) => { ... return status; })` inside `ConfigureCommandHandler`.
  - Ensure JSON output and exit code mapping are preserved. Return the status code from the action.
  - Use `parseResult` directly instead of `context.ParseResult`.
  - Keep telemetry/activity around the action body.

- `core/Azure.Mcp.Core/src/Commands/CommandFactory.cs` (command registration)
  - In `CreateRootCommand`, replace `rootCommand.Add(subGroup.Command);` with `rootCommand.Subcommands.Add(subGroup.Command);`.
  - In `ConfigureCommands`, replace `group.Command.Add(cmd);` with `group.Command.Subcommands.Add(cmd);`.

- `core/Azure.Mcp.Core/src/Areas/Server/Commands/ToolLoading/CommandFactoryToolLoader.cs`
  - Replace any `option.IsRequired` usages with `option.Required` (e.g., `schema.Required = [.. options.Where(p => p.Required).Select(p => p.Name)];`).

- Any place reading options from `ParseResult`
  - Replace `parseResult.GetValueForOption(option)` with `parseResult.GetValue(option)`.

### CommandFactory (core/Azure.Mcp.Core)
- Where we previously set handlers (and possibly wrapped telemetry), replace with `SetAction((ParseResult, CancellationToken) => Task<int>)` on each command.
- Action responsibilities:
  - Start activity/telemetry; tag with command name/options.
  - Create `CommandContext` with DI `IServiceProvider` and `Activity.Current`.
  - Resolve the `IBaseCommand` implementation.
  - Perform validation via `baseCommand.Validate(parseResult.CommandResult, context.Response)`.
    - On invalid: write response JSON and return `context.Response.Status`.
  - Execute the command logic (existing call-site), catch exceptions and convert to `CommandResponse` consistently (maintain current error mapping), write JSON, and return status.
- Preserve current exit code mapping and logging strategy.

Example replacement for `ConfigureCommandHandler`:

```csharp
private void ConfigureCommandHandler(Command command, IBaseCommand implementation)
{
  command.SetAction(async (ParseResult parseResult, CancellationToken ct) =>
  {
    _logger.LogTrace("Executing '{Command}'.", command.Name);

    using var activity = await _telemetryService.StartActivity(ActivityName.CommandExecuted);
    var cmdContext = new CommandContext(_serviceProvider, activity);
    var startTime = DateTime.UtcNow;
    try
    {
      // Optional centralized validators go here (pre-flight)

      // Per-command validation if exposed on implementation; otherwise inside ExecuteAsync
      // var validation = implementation.Validate(parseResult.CommandResult, cmdContext.Response);
      // if (!validation.IsValid) { Console.WriteLine(JsonSerializer.Serialize(cmdContext.Response, _srcGenWithOptions.CommandResponse)); return cmdContext.Response.Status; }

      var response = await implementation.ExecuteAsync(cmdContext, parseResult);

      var endTime = DateTime.UtcNow;
      response.Duration = (long)(endTime - startTime).TotalMilliseconds;

      if (response.Status == 200 && response.Results == null)
      {
        response.Results = ResponseResult.Create(new List<string>(), JsonSourceGenerationContext.Default.ListString);
      }

      var isServiceStartCommand = implementation is Azure.Mcp.Core.Areas.Server.Commands.ServiceStartCommand;
      if (!isServiceStartCommand)
      {
        Console.WriteLine(JsonSerializer.Serialize(response, _srcGenWithOptions.CommandResponse));
      }

      var status = response.Status;
      if (status < 200 || status >= 300)
      {
        activity?.SetStatus(ActivityStatusCode.Error).AddTag(TagName.ErrorDetails, response.Error);
      }

      return status;
    }
    catch (Exception ex)
    {
      _logger.LogError("An exception occurred while executing '{Command}'. Exception: {Exception}", command.Name, ex);
      activity?.SetStatus(ActivityStatusCode.Error)?.AddTag(TagName.ErrorDetails, ex.Message);
      // Map exception to error response consistently if desired, write JSON, and return mapped status
      var errorResponse = BaseCommand.HandleException(ex);
      Console.WriteLine(JsonSerializer.Serialize(errorResponse, _srcGenWithOptions.CommandResponse));
      return errorResponse.Status;
    }
    finally
    {
      _logger.LogTrace("Finished running '{Command}'.", command.Name);
    }
  });
}
```

### BaseCommand and Helpers
- Replace any `parseResult.GetValueForOption(...)` with `parseResult.GetValue(...)`.
- Replace `Option.IsRequired` with `Option.Required` wherever used.
- Keep validation signature and usage the same if possible; ensure call sites in actions provide `parseResult.CommandResult` and `context.Response`.

### Extensions (SystemCommandLineExtensions)
- Review any extension methods referencing old APIs. Update to support `Options.Add`, `Subcommands.Add`, `SetAction`, and `GetValue` where applicable.

## Validation and Binding Pattern

Two-tier validation is recommended: centralized preflight plus per-command validation. Binding should occur once, after successful validation unless typed values are required for validation.

- Centralized validation (generic):
  - Purpose: checks common to all commands (e.g., auth/tenant/subscription presence when applicable, environment readiness, feature flags).
  - Location: in the `SetAction` wrapper before per-command validation. Implement as a chain of `ICommandValidator` services if helpful, each with `bool TryValidate(ParseResult, CommandContext, out CommandResponse?)`.
  - Outcome: on failure, write JSON response and return the associated status code without invoking the command.

- Per-command validation (specific):
  - Purpose: semantic checks unique to a command (e.g., mutually exclusive options, resource naming rules, required combinations).
  - Location: command’s existing `Validate(CommandResult, CommandResponse)` method.
  - Input: use `parseResult.CommandResult` and `parseResult.GetValue(option)` as needed. No need to bind a full options DTO just to validate unless you prefer that pattern.

- Binding options:
  - Recommendation: bind once after validation to a typed options record/class via `BindOptions(ParseResult)` for clarity and reuse in `ExecuteAsync`.
  - If per-command validation requires converted values, either read them directly with `parseResult.GetValue(option)` inside `Validate(...)` or call `BindOptions` early and reuse the same bound options for execution. Avoid binding twice.

- Suggested action flow:
```csharp
command.SetAction(async (ParseResult parseResult, CancellationToken ct) =>
{
    var ctx = new CommandContext(serviceProvider, Activity.Current);

    // 1) Centralized validators
    if (!GlobalValidators.TryValidate(parseResult, ctx.Response))
    {
        Console.WriteLine(JsonSerializer.Serialize(ctx.Response, ModelsJsonContext.Default.CommandResponse));
        return ctx.Response.Status;
    }

    // 2) Per-command validation
    var baseCommand = (IBaseCommand)command.Handler; // or resolve from DI
    var validation = baseCommand.Validate(parseResult.CommandResult, ctx.Response);
    if (!validation.IsValid)
    {
        Console.WriteLine(JsonSerializer.Serialize(ctx.Response, ModelsJsonContext.Default.CommandResponse));
        return ctx.Response.Status;
    }

    // 3) Bind options once (after validation)
    var options = baseCommand.BindOptions(parseResult); // uses parseResult.GetValue(...)

    // 4) Execute
    var status = await baseCommand.ExecuteAsync(options, ctx, ct);
    Console.WriteLine(JsonSerializer.Serialize(ctx.Response, ModelsJsonContext.Default.CommandResponse));
    return status;
});
```

- Syntactic vs semantic validation:
  - Use `option.Required = true` to let SCL enforce syntactic presence before the action is called.
  - Keep semantic/domain validation in your per-command `Validate(...)` and central preflight.

## Tools Changes (tools/Azure.Mcp.Tools.*/src)
For each command implementation across tools:
- Registration:
  - Use `command.Options.Add(option)` and `command.Subcommands.Add(subcommand)` (replace any `AddOption`/`AddCommand`).
- Required options:
  - Use `option.Required = true`.
- Binding:
  - Use `parseResult.GetValue(option)` in action execution.
- Handler wiring:
  - If commands were wired via `SetHandler`, switch to the action pattern through the central `CommandFactory` where possible. If a tool owns its own root or subcommand wiring, set actions at definition time.
- Execution:
  - Ensure outputs remain `CommandResponse` serialized via the shared source-generated contexts.

## Tests
- Update tests referencing `GetValueForOption` and any direct handler invocation patterns to reflect action-based invocation.
- Where tests invoke commands via `Parser`, you can now invoke `rootCommand.InvokeAsync(args)`.
- Verify expected JSON and exit codes remain unchanged.

## Mechanical Sweep: Searches and Replacements
Use these targeted searches to guide edits (verify each change compiles):
- Find: `AddOption(` → Change to: `.Options.Add(`
- Find: `AddCommand(` → Change to: `.Subcommands.Add(`
- Find: `GetValueForOption(` → Change to: `GetValue(`
- Find: `.IsRequired` → Change to: `.Required`
- Find usages of `CommandLineBuilder`/`.UseDefaults()` → Remove; rely on root command invocation and per-command actions.
- Find `SetHandler(` → Replace with `SetAction((ParseResult parseResult, CancellationToken ct) => Task<int>)` and move logic accordingly.

## Example Snippets (Representative)

Old middleware-based validation in `Program.cs`:
```csharp
var builder = new CommandLineBuilder(root);
builder.AddMiddleware(async (context, next) =>
{
    var commandContext = new CommandContext(serviceProvider, Activity.Current);
    var command = context.ParseResult.CommandResult.Command;
    if (command.Handler is IBaseCommand baseCommand)
    {
        var validation = baseCommand.Validate(context.ParseResult.CommandResult, commandContext.Response);
        if (!validation.IsValid)
        {
            WriteResponse(commandContext.Response);
            context.ExitCode = commandContext.Response.Status;
            return;
        }
    }
    await next(context);
});
```

New per-command action wiring in `CommandFactory`:
```csharp
command.SetAction(async (ParseResult parseResult, CancellationToken ct) =>
{
    var ctx = new CommandContext(serviceProvider, Activity.Current);
    using var activity = telemetry.StartActivity(command.Name);

    try
    {
        var baseCommand = (IBaseCommand)command.Handler; // or resolve from DI
        var validation = baseCommand.Validate(parseResult.CommandResult, ctx.Response);
        if (!validation.IsValid)
        {
            Console.WriteLine(JsonSerializer.Serialize(ctx.Response, ModelsJsonContext.Default.CommandResponse));
            return ctx.Response.Status;
        }

        var status = await baseCommand.ExecuteAsync(parseResult, ctx, ct); // existing logic
        Console.WriteLine(JsonSerializer.Serialize(ctx.Response, ModelsJsonContext.Default.CommandResponse));
        return status;
    }
    catch (Exception ex)
    {
        var response = BaseCommand.HandleException(ex); // preserve current mapping
        Console.WriteLine(JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse));
        return response.Status;
    }
});
```

Option binding change:
```csharp
// Before
var sub = parseResult.GetValueForOption(_subscriptionOption);

// After
var sub = parseResult.GetValue(_subscriptionOption);
```

Registration change:
```csharp
// Before
command.AddOption(_subscriptionOption);
command.AddCommand(_deployCommand);

// After
command.Options.Add(_subscriptionOption);
command.Subcommands.Add(_deployCommand);
```

Requiredness:
```csharp
// Before
_subscriptionOption.IsRequired = true;

// After
_subscriptionOption.Required = true;
```

Entrypoint invocation change:
```csharp
// Before
var parser = new CommandLineBuilder(root).UseDefaults().Build();
return await parser.InvokeAsync(args);

// After
return await root.InvokeAsync(args);
```

## Telemetry, JSON, and AOT Considerations
- Continue using source-generated `System.Text.Json` contexts for all response types.
- Keep telemetry/activity wrapping around action bodies; tag important properties (command name, options).
- Ensure any reflection-heavy code paths remain AOT-safe; prefer direct registration over runtime discovery where feasible.

## Validation & Build Steps
After each phase or significant set of changes:
- Build solution:
```pwsh
pwsh ./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx
```
- Or run the VS Code task:
  - Task: `build` (runs `dotnet build AzureMcp.sln`)
- Optional quick build:
```pwsh
dotnet build .\AzureMcp.sln /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
```
- Run unit tests for changed areas; validate exit codes and JSON outputs.
- Run spell-check:
```pwsh
.\eng\common\spelling\Invoke-Cspell.ps1
```

## Risks & Mitigations
- Middleware removal may hide validation if actions aren’t updated
  - Mitigation: Ensure every command’s action begins with `Validate(...)` and returns early on failure.
- Behavior drift in exit codes
  - Mitigation: Keep existing mapping logic; add targeted tests where coverage is thin.
- Missed mechanical changes across many projects
  - Mitigation: Use repo-wide searches; build iteratively; fix compile errors as a checklist.

## Comprehensive TODO Checklist

1) Package and Tooling
- [x] Update `Directory.Packages.props` to pin `System.CommandLine` to `2.0.0-beta5`.
- [ ] Remove/align any project-level SCL package versions with the central pin.
- [ ] Search and remove any legacy SCL add-ons (e.g., deprecated binders) if present.
- [ ] Run `./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx` to surface compile errors.

2) Servers (entrypoints)
- [x] `servers/Azure.Mcp.Server/src/Program.cs`: remove `CommandLineBuilder`, `.AddMiddleware(...)`, `.UseDefaults()`; invoke `rootCommand.InvokeAsync(args)`; keep JSON writer.
- [x] `servers/Fabric.Mcp.Server/src/Program.cs`: same as above.
- [x] `servers/Template.Mcp.Server/src/Program.cs`: same as above.

3) Core Wiring
- [x] `core/Azure.Mcp.Core` CommandFactory: replace handler wiring with `SetAction((ParseResult, CancellationToken) => Task<int>)`.
- [x] Start telemetry/activity around action, tag relevant properties.
- [x] Introduce centralized validation hook (e.g., `ICommandValidator` chain) executed before per-command validation.
- [x] Call per-command `Validate(CommandResult, CommandResponse)` and return early on failure.
- [x] Bind options once after validation via `BindOptions(ParseResult)` (create/standardize if needed).
- [x] Execute command, write JSON response, and return exit code. Centralize exception → response mapping.
- [x] `BaseCommand`: update to use `parseResult.GetValue(...)`; update `.IsRequired` → `.Required`; ensure `BindOptions(ParseResult)` is available and AOT-safe.
- [ ] `SystemCommandLineExtensions`: update helpers to new APIs (`Options.Add`, `Subcommands.Add`, `GetValue`, action model) as needed.
- [x] `SystemCommandLineExtensions`: update helpers to new APIs (`Options.Add`, `Subcommands.Add`, `GetValue`, action model) as needed. (Implemented via `core/Azure.Mcp.Core/src/Commands/CommandExtensions.cs`.)

4) Mechanical Sweeps (all projects)
- [ ] Replace `command.AddOption(...)` → `command.Options.Add(...)`.
- [ ] Replace `command.AddCommand(...)` → `command.Subcommands.Add(...)`.
- [x] Replace `parseResult.GetValueForOption(...)` → `parseResult.GetValue(...)`.
- [x] Replace `option.IsRequired` → `option.Required`.
- [x] Replace `SetHandler(...)` usages → `SetAction((ParseResult, CancellationToken) => Task<int>)`.
- [x] Remove `CommandLineBuilder` + `.UseDefaults()` usages.

5) Tools sweep (commands and wiring)
- [ ] Ensure each tool’s commands register options/subcommands with the new collections API.
- [ ] Ensure per-command validation remains in `Validate(...)` and called by action wrapper; remove reliance on middleware.
- [ ] Bind options once per invocation; confirm `BindOptions(ParseResult)` returns typed options for execution.
- [ ] Confirm JSON outputs still use source-generated contexts; add new types to contexts if needed.
- [ ] Project list to review (at minimum):
  - [ ] `tools/Azure.Mcp.Tools.Acr`
  - [ ] `tools/Azure.Mcp.Tools.Aks`
  - [ ] `tools/Azure.Mcp.Tools.AppConfig`
  - [ ] `tools/Azure.Mcp.Tools.Authorization`
  - [ ] `tools/Azure.Mcp.Tools.AzureBestPractices`
  - [ ] `tools/Azure.Mcp.Tools.AzureIsv`
  - [ ] `tools/Azure.Mcp.Tools.AzureManagedLustre`
  - [ ] `tools/Azure.Mcp.Tools.AzureTerraformBestPractices`
  - [ ] `tools/Azure.Mcp.Tools.BicepSchema`
  - [ ] `tools/Azure.Mcp.Tools.CloudArchitect`
  - [ ] `tools/Azure.Mcp.Tools.Cosmos`
  - [ ] `tools/Azure.Mcp.Tools.Deploy`
  - [ ] `tools/Azure.Mcp.Tools.Extension`
  - [ ] `tools/Azure.Mcp.Tools.Foundry`
  - [ ] `tools/Azure.Mcp.Tools.FunctionApp`
  - [ ] `tools/Azure.Mcp.Tools.Grafana`
  - [ ] `tools/Azure.Mcp.Tools.KeyVault`
  - [ ] `tools/Azure.Mcp.Tools.Kusto`
  - [ ] `tools/Azure.Mcp.Tools.LoadTesting`
  - [ ] `tools/Azure.Mcp.Tools.Marketplace`
  - [ ] `tools/Azure.Mcp.Tools.Monitor`
  - [ ] `tools/Azure.Mcp.Tools.MySql`
  - [ ] `tools/Azure.Mcp.Tools.Postgres`
  - [ ] `tools/Azure.Mcp.Tools.Quota`
  - [ ] `tools/Azure.Mcp.Tools.Redis`
  - [ ] `tools/Azure.Mcp.Tools.ResourceHealth`
  - [ ] `tools/Azure.Mcp.Tools.Search`
  - [ ] `tools/Azure.Mcp.Tools.ServiceBus`
  - [ ] `tools/Azure.Mcp.Tools.Sql`
  - [ ] `tools/Azure.Mcp.Tools.Storage`
  - [ ] `tools/Azure.Mcp.Tools.VirtualDesktop`
  - [ ] `tools/Azure.Mcp.Tools.Workbooks`

6) Tests
- [ ] Update tests that construct/consume `Parser` to instead use `RootCommand.InvokeAsync(args)` where applicable.
- [ ] Update usages of `GetValueForOption` in tests to `GetValue`.
- [ ] Ensure unit tests still validate JSON output and exit codes.
- [ ] Run test projects for each affected area.

7) Documentation & Samples
- [ ] Update any `docs/commandline/*` samples or guidance that reference legacy APIs.
- [ ] Finalize this migration document with any repo-specific gotchas discovered during implementation.
- [ ] Update `CHANGELOG.md` with migration notes and breaking change summary.
- [ ] Run spell check: `./eng/common/spelling/Invoke-Cspell.ps1`.

8) Build & AOT
- [ ] Run `./eng/scripts/Build-Local.ps1 -UsePaths -VerifyNpx` and ensure green.
- [ ] Optionally validate native/AOT build for impacted projects: `./eng/scripts/Build-Local.ps1 -BuildNative`.
- [ ] Verify no reflection-based paths break AOT; prefer source-generated JSON and explicit registrations.

9) Smoke Validation
- [ ] Run representative commands from each area/tool and verify:
  - [ ] Proper validation errors for missing/invalid inputs.
  - [ ] Expected JSON schema and content.
  - [ ] Exit codes preserved.
  - [ ] Telemetry/events captured with correct tags.

10) PR Hygiene
- [ ] Ensure all tests pass in CI.
- [ ] Include documentation updates (this file, samples) in the PR.
- [ ] Keep changes scoped to SCL migration (avoid unrelated refactors).
- [ ] Request reviews from owners of servers/core/tools.

## Checklists

Migration checklist (per project):
- [ ] Package reference resolves to `System.CommandLine 2.0.0-beta5`.
- [ ] Entrypoint no longer uses `CommandLineBuilder` or `.UseDefaults()`.
- [ ] All `AddOption` → `Options.Add`; `AddCommand` → `Subcommands.Add`.
- [x] All `GetValueForOption` → `GetValue`.
- [ ] All `.IsRequired` → `.Required`.
- [ ] Handlers replaced with action-based wiring; telemetry and validation preserved.
- [ ] JSON serialization uses source-generated contexts and matches previous shape.
- [ ] Tests updated and passing.

Repository wrap-up:
- [ ] Full build succeeds locally.
- [ ] `CHANGELOG.md` updated with migration notes.
- [ ] This doc reviewed and cross-checked with `docs/commandline/*` samples.

---

If you want, I can run the build after we bump the package to beta5 and then drive the fixes according to the compile errors to complete the migration.
