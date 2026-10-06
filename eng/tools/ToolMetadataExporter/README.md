# Tool Metadata Exporter

`ToolMetadataExporter` snapshots the tools exposed by an MCP server, compares them with the latest state in Azure Data Explorer (Kusto), and records created, updated, and deleted tool events.

Each run:

1. Invokes `server info` and `tools list` on the configured MCP executable.
1. Writes the complete tool catalog to JSON.
1. Queries Kusto for the latest known tool state.
1. Writes detected changes locally.
1. Ingests changes unless dry-run mode is enabled.

## Prerequisites

- The repository's configured .NET SDK.
- An MCP executable that supports `server info` and `tools list`.
- A Kusto cluster, database, and identity with query access. Ingestion also requires table ingestion access.

Run [`src\Resources\queries\CreateTable.kql`](src/Resources/queries/CreateTable.kql) against the target database before the first export.

> [!IMPORTANT]
> Dry-run mode skips ingestion but still queries Kusto and therefore still requires authentication and query access.

## Configuration

The project has a `UserSecretsId`. User secrets load automatically when `DOTNET_ENVIRONMENT` is `Development`:

```powershell
dotnet user-secrets --project .\src set "AppConfig:QueryEndpoint" "https://<cluster>.<region>.kusto.windows.net"
dotnet user-secrets --project .\src set "AppConfig:IngestionEndpoint" "https://ingest-<cluster>.<region>.kusto.windows.net"
dotnet user-secrets --project .\src set "AppConfig:DatabaseName" "<database>"
dotnet user-secrets --project .\src set "AppConfig:McpToolEventsTableName" "McpToolEvents"
```

Configuration can also be supplied through `src\appsettings.Development.json` or `AppConfig__*` environment variables.

| Setting | Purpose | Default |
|---|---|---|
| `QueryEndpoint` | Kusto query endpoint. | Required |
| `IngestionEndpoint` | Kusto ingestion endpoint. | Required |
| `DatabaseName` | Target database. | Required |
| `McpToolEventsTableName` | Event table. | `McpToolEvents` |
| `QueriesFolder` | Directory containing `GetAvailableTools.kql`. | `Resources\queries` |
| `WorkDirectory` | Generated JSON directory. | Repository `.work` for the repository executable; application directory for an explicit executable |
| `AzmcpExe` | MCP executable to inspect. | `eng\tools\Azmcp\azmcp.exe` |
| `IsDryRun` | Skip Kusto ingestion. | `false` |
| `UseAnalysisTime` | Use the run time instead of the executable's timestamp for events. | `false` |

## Run

Run from `src` so the default query path resolves correctly. `--no-launch-profile` avoids any developer-specific launch arguments:

```powershell
Push-Location .\src
$env:DOTNET_ENVIRONMENT = "Development"
dotnet run --no-launch-profile -- --AzmcpExe "C:\path\to\azmcp.exe" --IsDryRun true
Pop-Location
```

Remove `--IsDryRun true` to ingest detected events. The exporter only ingests when changes are found.

Authentication uses Azure CLI credentials or an interactive Microsoft Entra prompt:

```powershell
az login
```

The Azure MCP public release pipeline uses [`Invoke-ReleaseToolMetadataExport.ps1`](../../scripts/Invoke-ReleaseToolMetadataExport.ps1) after publishing the signed NuGet package. The script extracts the `win-x64` executable, builds the exporter, records its metadata, and publishes generated `*_tool_changes_*.json` files as the `tool_metadata_changes` artifact.

Release configuration comes from [`tool-metadata-exporter.yml`](../../pipelines/templates/variables/tool-metadata-exporter.yml):

| Variable | Exporter setting |
|---|---|
| `ToolMetadataIngestionEndpoint` | `AppConfig:IngestionEndpoint` |
| `ToolMetadataQueryEndpoint` | `AppConfig:QueryEndpoint` |
| `ToolMetadataDatabaseName` | `AppConfig:DatabaseName` |

## Export downloaded versions

[`src\Export-DownloadedVersions.ps1`](src/Export-DownloadedVersions.ps1) finds versioned package directories, orders them using semantic versioning, and runs the exporter against each `tools\any\win-x64\azmcp.exe`.

```powershell
.\src\Export-DownloadedVersions.ps1 `
    -PackagesDirectory "C:\downloads\azure.mcp" `
    -ExportUntil "3.0.0" `
    -IsDryRun
```

`-ExportUntil` is inclusive. For example, `3.0.0` includes `3.0.0-beta.1` and `3.0.0`.

| Option | Behavior |
|---|---|
| `-PackagesDirectory` | Directory containing versioned package folders. |
| `-ExportUntil <version>` | Process versions up to and including the semantic version. |
| `-IsDryRun` | Pass `--IsDryRun true` to every exporter invocation. Without it, changes may be ingested. |
| `-Force` | Delete the success ledger and restart from the earliest eligible version. |
| `-SuccessfulPathsFile <path>` | Override the success-ledger path. |
| `-WhatIf` | Preview protected operations without exporting or deleting the ledger. |

Successful executable paths are appended to:

```text
.work\ToolMetadataExporter\Export-DownloadedVersions.successful.txt
```

Subsequent runs skip those paths. A failed export is not recorded.

## Output

The exporter writes files to `WorkDirectory`:

```text
<version>_tool_<yyyyMMddHHmmss>.json
<version>_tool_changes_<yyyyMMddHHmmss>.json
```

The tool file contains the complete catalog. The change file is written only when created, updated, or deleted tools are detected.

## Troubleshooting

- **Unexpected organization-branded sign-in:** Cancel the prompt and verify the Kusto endpoints and tenant. Dry runs still authenticate.
- **`Resources\queries` not found:** Run from `src` or configure an absolute `AppConfig__QueriesFolder`.
- **Historical executable emits invalid JSON:** The exporter retries after escaping illegal C0 control characters, such as U+001A, in the response.
- **No change file:** The current catalog matches the latest Kusto state.
