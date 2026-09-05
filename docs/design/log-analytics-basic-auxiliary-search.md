<!-- cspell:ignore externaldata SRCH -->

# Basic and Auxiliary log search

`monitor workspace log search` (`monitor_workspace_log_search`) queries one primary Basic or Auxiliary table in a Log Analytics workspace. Existing Analytics query tools are unchanged.

## Why a separate tool

Analytics queries use `/v1/workspaces/{workspaceId}/query`. Basic and Auxiliary queries use `/v1/workspaces/{workspaceId}/search`, with workspace-only scope and a narrower KQL language.

Keeping separate tools avoids guessing the primary table from arbitrary KQL or adding metadata requests to existing Analytics queries. There is no automatic fallback between endpoints.

## Inputs and limits

| Option | Meaning |
| --- | --- |
| `subscription` | ID or name. If omitted, the standard resolver uses the configured default. |
| `resource-group` | Required to identify the workspace unambiguously. |
| `workspace` | Required workspace name; ARM supplies its customer GUID. |
| `table` | Required Basic or Auxiliary table name, validated as an ASCII identifier. |
| `query` | Required KQL pipeline beginning with `\|`, without the primary table name. |
| `timespan` | Required positive ISO 8601 duration or RFC 3339 `start/end` interval, at most 30 days. Calendar years and months are not accepted. |
| `limit` | Maximum returned rows: 1-100, default 20. |
| `tenant` | Optional tenant ID or name. |

Basic queries cannot start more than 30 days ago. Auxiliary supports older retained data, but this tool limits each call to a 30-day interval. Query cost depends on data scanned across that interval, not the returned row limit.

## Query construction

The service builds `<table> <pipeline> | take <limit>`. It always appends the final `take`, including when the supplied pipeline already has a row limit.

The validator rejects multiple statements, comments, source functions, nested tabular pipelines, and unsupported operators such as `join`, `find`, `search`, `externaldata`, and `invoke`. It is not a KQL parser; Azure checks the remaining syntax and semantics. Azure-supported `union` and `lookup` enrichment from Analytics tables remains available. The result's `table` and `plan` fields identify the primary table, not every enrichment source.

## Table-plan checks

Before querying logs, the service reads the workspace and table through the ARM SDK:

1. Missing resources return 404.
2. Analytics or other unsupported plans return 409 with guidance to use `monitor_workspace_log_query`, even if their plan-change timestamp is absent.
3. A missing plan, or missing or invalid transition metadata on a Basic or Auxiliary table, returns 502.
4. Ranges starting before `LastPlanModifiedDate` return 409 with the supported boundary. This prevents a query spanning different access behavior from appearing complete.

If the plan changes after the metadata read, the service error is returned without retrying through another endpoint.

## Results and failures

Results contain typed `columns`, positional `rows`, `rowCount`, `limit`, `isPartial`, and `error`. Numbers, booleans, nulls, and dynamic JSON keep their types.

An Azure `PartialError` retains usable rows with `isPartial: true` and sanitized error details. Fatal errors, malformed responses, invalid row shapes, and responses exceeding 1 MiB return an error rather than silently dropping data. HTTP 204 returns an empty result. `isPartial` reflects service-reported incompleteness; the row limit still applies to successful results.

There are no application retries, pagination, caching, or parallel queries. Throttling returns 429 with retry guidance. A linked cancellation token bounds both the HTTP request and response-body read.

## Authentication and cloud support

The service uses the repository's per-request Azure credential provider in both stdio and HTTP modes. It requires workspace read, table metadata read, and log query permissions. Callers cannot supply tokens, endpoints, or workspace customer GUIDs.

Only the documented public-cloud `/search` endpoint is enabled. Other clouds return an unsupported-cloud error before network access. Endpoint fallback is intentionally absent: retrying a query on another host could repeat a billable scan.

## Server discovery modes

| Server start configuration | Exposed tool | Routed command |
| --- | --- | --- |
| `--mode all --namespace monitor` | `monitor_workspace_log_search` | Direct tool call |
| `--tool monitor_workspace_log_search` | `monitor_workspace_log_search` | Direct tool call |
| `--namespace monitor` (default namespace mode) | `monitor` | `monitor_workspace_log_search` |
| `--mode namespace --namespace monitor` | `monitor` | `monitor_workspace_log_search` |
| `--mode single --namespace monitor` | `azure` | Tool `monitor`, command `monitor_workspace_log_search` |
| `--mode consolidated --namespace monitor` | `get_azure_resource_and_app_health_status` | `get_azure_resource_and_app_health_status_monitor_workspace_log_search` |

`--tool` and `--namespace` cannot be combined. Routers accept the child arguments under `parameters`; `learn: true` lists their exact command names.

Structured output is opt-in with `--structured-output-mode duplicated` or `compact`. Direct mode advertises `WorkspaceLogSearchResult`; namespace and consolidated modes use the shared `tool-result` envelope. Single mode wraps the complete downstream MCP call result and forwards the output setting to its child server. See [output-schema conventions](https://github.com/microsoft/mcp/blob/main/docs/output-schema-migration.md).

## Search jobs are separate

Search jobs create persistent `*_SRCH` tables and can run for up to 24 hours. They require write permissions, incur ingestion costs, and need polling and cleanup. This synchronous read-only tool does not start search jobs.

## References

- [Query data in a Basic and Auxiliary table](https://learn.microsoft.com/azure/azure-monitor/logs/basic-logs-query)
- [Access the Azure Monitor Log Analytics API](https://learn.microsoft.com/azure/azure-monitor/logs/api/access-api)
- [Log Analytics API response format](https://learn.microsoft.com/azure/azure-monitor/logs/api/response-format)
- [Azure Monitor service limits: log queries and language](https://learn.microsoft.com/azure/azure-monitor/fundamentals/service-limits#log-queries-and-language)
- [Configure a table plan](https://learn.microsoft.com/azure/azure-monitor/logs/logs-table-plans)
- [Run search jobs in Azure Monitor](https://learn.microsoft.com/azure/azure-monitor/logs/search-jobs)
