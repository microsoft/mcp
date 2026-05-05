# Command-Result Naming Cleanup

## Background

The dominant convention across the MCP codebase is that every command's
`ResponseResult.Create<T>` call site writes a single record root nested in the
command class and named `*CommandResult`:

```csharp
public sealed class FooGetCommand
    : SubscriptionCommand<FooGetOptions, FooGetCommand.FooGetCommandResult>
{
    protected override JsonTypeInfo<FooGetCommandResult> ResultTypeInfo
        => FooJsonContext.Default.FooGetCommandResult;

    // ...

    public record FooGetCommandResult(List<FooInfo> Foos);
}
```

This is the rule codified by `docs/mcp-tool-schemas-rfc.md` and followed by
~85% of commands today (~150+ records). A meaningful minority of older
toolsets still use the bare `*Result` suffix, sometimes nested in the command
and sometimes living in a separate file under `Models/`.

This document tracks the cleanup needed to align the remaining outliers.

## Scope

Out of scope (these records are **payloads** carried inside a `*CommandResult`
root and may stay as-is — they're domain models, not command-result roots):

- `Azure.Mcp.Tools.Storage.Models.StorageAccountResult` (wrapped by `AccountCreateCommandResult`)
- `Azure.Mcp.Tools.Communication` `EmailSendResult` (wrapped by `EmailSendCommandResult`)
- `Fabric.Mcp.Tools.OneLake.Models` payload records (`BlobPutResult`, `BlobGetResult`, `BlobDeleteResult`, `TableConfigurationResult`, `TableGetResult`, `TableListResult`, `TableNamespaceGetResult`, `TableNamespaceListResult`) — each is wrapped by a `*CommandResult`
- Non-command result types (`AppLensModels.AppLensArgQueryResult`, `DiagnosticResult`, `BicepSchema.TypesDefinitionResult`, `Speech` model records, `OnboardingSpecValidator.ValidationResult`, `Monitor.InstrumentationResult`, `FileSharesService.FileShareNameAvailabilityResult`)

In scope: every record used as the `TResult` root of a command (i.e., referenced
by `ResultTypeInfo`) that is **not** named `*CommandResult`.

## Per-command checklist

For each entry below, the cleanup is:

1. Rename the record `*Result` → `*CommandResult`.
2. If it lives in a separate file (only `BlobUploadResult` today), move it to be a nested type of the command class and delete the standalone file.
3. Update the `ResultTypeInfo` property and the `[JsonSerializable]` registration in the toolset's `*JsonContext.cs`.
4. Update unit tests that deserialize the response (`*JsonContext.Default.*Result` → `*JsonContext.Default.*CommandResult`).
5. Update any references in `azmcp-commands.md` / e2e prompts if the type name appears there (rare).
6. Build the toolset's `src` and run its unit tests.

### `Azure.Mcp.Tools.AppService`

| File | Current | Target |
|---|---|---|
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/WebappGetCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/WebappGetCommand.cs) | `WebappGetResult` | `WebappGetCommandResult` |
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Settings/AppSettingsGetCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Settings/AppSettingsGetCommand.cs) | `AppSettingsGetResult` | `AppSettingsGetCommandResult` |
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Settings/AppSettingsUpdateCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Settings/AppSettingsUpdateCommand.cs) | `AppSettingsUpdateResult` | `AppSettingsUpdateCommandResult` |
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Diagnostic/DetectorListCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Diagnostic/DetectorListCommand.cs) | `DetectorListResult` | `DetectorListCommandResult` |
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Diagnostic/DetectorDiagnoseCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Diagnostic/DetectorDiagnoseCommand.cs) | `DetectorDiagnoseResult` | `DetectorDiagnoseCommandResult` |
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Deployment/DeploymentGetCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Webapp/Deployment/DeploymentGetCommand.cs) | `DeploymentGetResult` | `DeploymentGetCommandResult` |
| [tools/Azure.Mcp.Tools.AppService/src/Commands/Database/DatabaseAddCommand.cs](tools/Azure.Mcp.Tools.AppService/src/Commands/Database/DatabaseAddCommand.cs) | `DatabaseAddResult` | `DatabaseAddCommandResult` |

### `Azure.Mcp.Tools.Advisor`

| File | Current | Target |
|---|---|---|
| [tools/Azure.Mcp.Tools.Advisor/src/Commands/Recommendation/RecommendationListCommand.cs](tools/Azure.Mcp.Tools.Advisor/src/Commands/Recommendation/RecommendationListCommand.cs) | `RecommendationListResult` | `RecommendationListCommandResult` |

### `Azure.Mcp.Tools.AzureIsv`

| File | Current | Target |
|---|---|---|
| [tools/Azure.Mcp.Tools.AzureIsv/src/Commands/Datadog/MonitoredResourcesListCommand.cs](tools/Azure.Mcp.Tools.AzureIsv/src/Commands/Datadog/MonitoredResourcesListCommand.cs) | `MonitoredResourcesListResult` | `MonitoredResourcesListCommandResult` |

### `Azure.Mcp.Tools.ManagedLustre`

Entire toolset still uses bare `*Result`, except for the three `*JobGetCommand`
records (`ImportJobGetCommandResult`, `AutoimportJobGetCommandResult`,
`AutoexportJobGetCommandResult`) which were consolidated before.

| File | Current | Target |
|---|---|---|
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/FileSystemCreateCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/FileSystemCreateCommand.cs) | `FileSystemCreateResult` | `FileSystemCreateCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/FileSystemListCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/FileSystemListCommand.cs) | `FileSystemListResult` | `FileSystemListCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/FileSystemUpdateCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/FileSystemUpdateCommand.cs) | `FileSystemUpdateResult` | `FileSystemUpdateCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/Sku/SkuGetCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/Sku/SkuGetCommand.cs) | `SkuGetResult` | `SkuGetCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/SubnetSize/SubnetSizeAskCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/SubnetSize/SubnetSizeAskCommand.cs) | `FileSystemSubnetSizeResult` | `SubnetSizeAskCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/SubnetSize/SubnetSizeValidateCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/SubnetSize/SubnetSizeValidateCommand.cs) | `FileSystemCheckSubnetResult` | `SubnetSizeValidateCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/ImportJob/ImportJobCancelCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/ImportJob/ImportJobCancelCommand.cs) | `ImportJobCancelResult` | `ImportJobCancelCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/ImportJob/ImportJobCreateCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/ImportJob/ImportJobCreateCommand.cs) | `ImportJobCreateResult` | `ImportJobCreateCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/ImportJob/ImportJobDeleteCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/ImportJob/ImportJobDeleteCommand.cs) | `ImportJobDeleteResult` | `ImportJobDeleteCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoimportJob/AutoimportJobCancelCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoimportJob/AutoimportJobCancelCommand.cs) | `AutoimportJobCancelResult` | `AutoimportJobCancelCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoimportJob/AutoimportJobCreateCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoimportJob/AutoimportJobCreateCommand.cs) | `AutoimportJobCreateResult` | `AutoimportJobCreateCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoimportJob/AutoimportJobDeleteCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoimportJob/AutoimportJobDeleteCommand.cs) | `AutoimportJobDeleteResult` | `AutoimportJobDeleteCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoexportJob/AutoexportJobCancelCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoexportJob/AutoexportJobCancelCommand.cs) | `AutoexportJobCancelResult` | `AutoexportJobCancelCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoexportJob/AutoexportJobCreateCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoexportJob/AutoexportJobCreateCommand.cs) | `AutoexportJobCreateResult` | `AutoexportJobCreateCommandResult` |
| [tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoexportJob/AutoexportJobDeleteCommand.cs](tools/Azure.Mcp.Tools.ManagedLustre/src/Commands/FileSystem/AutoexportJob/AutoexportJobDeleteCommand.cs) | `AutoexportJobDeleteResult` | `AutoexportJobDeleteCommandResult` |


### `Azure.Mcp.Tools.Sql`

| File | Current | Target |
|---|---|---|
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerCreateCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerCreateCommand.cs) | `ServerCreateResult` | `ServerCreateCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerDeleteCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Server/ServerDeleteCommand.cs) | `ServerDeleteResult` | `ServerDeleteCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseCreateCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseCreateCommand.cs) | `DatabaseCreateResult` | `DatabaseCreateCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseDeleteCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseDeleteCommand.cs) | `DatabaseDeleteResult` | `DatabaseDeleteCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseGetCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseGetCommand.cs) | `DatabaseGetListResult` | Rename to `DatabaseGetCommandResult` (the record already covers both single and list shapes; awkwardly named today) |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseRenameCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseRenameCommand.cs) | `DatabaseRenameResult` | `DatabaseRenameCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseUpdateCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/Database/DatabaseUpdateCommand.cs) | `DatabaseUpdateResult` | `DatabaseUpdateCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/ElasticPool/ElasticPoolListCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/ElasticPool/ElasticPoolListCommand.cs) | `ElasticPoolListResult` | `ElasticPoolListCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/EntraAdmin/EntraAdminListCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/EntraAdmin/EntraAdminListCommand.cs) | `EntraAdminListResult` | `EntraAdminListCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/FirewallRule/FirewallRuleCreateCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/FirewallRule/FirewallRuleCreateCommand.cs) | `FirewallRuleCreateResult` | `FirewallRuleCreateCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/FirewallRule/FirewallRuleDeleteCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/FirewallRule/FirewallRuleDeleteCommand.cs) | `FirewallRuleDeleteResult` | `FirewallRuleDeleteCommandResult` |
| [tools/Azure.Mcp.Tools.Sql/src/Commands/FirewallRule/FirewallRuleListCommand.cs](tools/Azure.Mcp.Tools.Sql/src/Commands/FirewallRule/FirewallRuleListCommand.cs) | `FirewallRuleListResult` | `FirewallRuleListCommandResult` |

### `Azure.Mcp.Tools.Storage` — `BlobUploadResult` outlier

`BlobUploadResult` is unique: it lives in
[tools/Azure.Mcp.Tools.Storage/src/Models/BlobUploadResult.cs](tools/Azure.Mcp.Tools.Storage/src/Models/BlobUploadResult.cs)
and is referenced as the root type by
[tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs](tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs)
(`ResultTypeInfo => StorageJsonContext.Default.BlobUploadResult`).

Cleanup:

1. Move the record into `BlobUploadCommand` as a nested type renamed `BlobUploadCommandResult`.
2. Delete `tools/Azure.Mcp.Tools.Storage/src/Models/BlobUploadResult.cs`.
3. Update the registration in `StorageJsonContext.cs`.
4. Update tests in `tools/Azure.Mcp.Tools.Storage/tests/.../Blob/BlobUploadCommandTests.cs`.

This was called out as **optional** in the RFC; track it here so it isn't forgotten.

### `Fabric.Mcp.Tools.Docs`

| File | Current | Target |
|---|---|---|
| [tools/Fabric.Mcp.Tools.Docs/src/Commands/BestPractices/GetExamplesCommand.cs](tools/Fabric.Mcp.Tools.Docs/src/Commands/BestPractices/GetExamplesCommand.cs) | `ExampleFileResult` | `GetExamplesCommandResult` |

### `Fabric.Mcp.Tools.OneLake`

| File | Current | Target |
|---|---|---|
| [tools/Fabric.Mcp.Tools.OneLake/src/Commands/File/PathListCommand.cs](tools/Fabric.Mcp.Tools.OneLake/src/Commands/File/PathListCommand.cs) | `PathListResult` | `PathListCommandResult` |

(All other OneLake command roots already follow `*CommandResult`. The remaining
`*Result` records under `Models/` are payloads carried inside `*CommandResult`
and stay as-is.)

## Suggested sequencing

Submit one PR per toolset, mirroring the "one tool per PR" guidance in
`AGENTS.md`. A reasonable order, smallest first:

1. `Advisor` (1 record)
2. `AzureIsv` (1 record)
3. `Fabric.Mcp.Tools.Docs` (1 record)
4. `Fabric.Mcp.Tools.OneLake` (1 record)
5. `Azure.Mcp.Tools.Storage` — `BlobUploadResult` (involves file move/delete)
6. `Azure.Mcp.Tools.AppService` (7 records)
7. `Azure.Mcp.Tools.Sql` (12 records)
8. `Azure.Mcp.Tools.ManagedLustre` (13 records)

## Notes

- These are pure renames with no behavior changes. JSON wire format is unchanged
  because the property names inside the records are preserved.
- After each PR, run the toolset's unit tests and confirm the count matches
  the pre-existing baseline; no new failures should appear.
