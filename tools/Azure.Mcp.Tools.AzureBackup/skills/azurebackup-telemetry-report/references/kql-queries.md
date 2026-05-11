# Azure Backup MCP — KQL Telemetry Queries

All queries use the `getAzureMcpEvents_ToolCalls` function from the Azure MCP telemetry cluster.

- **Cluster URI:** `https://ddazureclients.kusto.windows.net`
- **Database:** `AzureDevExp`

> **Note on field casing:** The telemetry source emits `ToolName` and `DevDeviceId` (PascalCase),
> but `customDimensions.toolname` (lowercase) works in practice because the `getAzureMcpEvents_ToolCalls`
> function normalizes bag key access. If queries return empty results, try PascalCase variants
> (`customDimensions.ToolName`, `customDimensions.DevDeviceId`).

---

## 1. Per-Tool Health with 3-Way Error Classification (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| extend
    ExMsg = tostring(customDimensions["exception.message"]),
    ExType = tostring(customDimensions["exception.type"])
| extend StatusCode = toint(extract(@"StatusCode.*?(\d+)", 1, ExMsg))
| extend ErrorCategory = case(
    success == true, "Success",
    StatusCode >= 400 and StatusCode < 500, "Customer",
    ExType == "System.Collections.Generic.KeyNotFoundException", "Customer",
    ExType == "Azure.RequestFailedException" and StatusCode >= 500, "AzureService",
    ExType == "System.AggregateException", "AzureService",
    ExType in ("System.FormatException", "System.ArgumentNullException", "System.ArgumentException", "System.InvalidOperationException"), "McpToolBug",
    "Unknown")
| summarize
    Total = count(),
    OK = countif(ErrorCategory == "Success"),
    Customer = countif(ErrorCategory == "Customer"),
    AzureSvc = countif(ErrorCategory == "AzureService"),
    McpBug = countif(ErrorCategory == "McpToolBug"),
    Users = dcount(tostring(customDimensions.devdeviceid)),
    P50_ms = round(percentile(duration, 50), 0),
    P95_ms = round(percentile(duration, 95), 0)
    by ToolName
| extend ToolRate = round(100.0 * (Total - McpBug) / Total, 1)
| order by Total desc
```

## 2. Aggregate KPI — This Week vs Last Week

```kql
let ThisWeek = startofweek(ago(0d));
let LastWeek = startofweek(ago(7d));
let TwoWeeksAgo = startofweek(ago(14d));
getAzureMcpEvents_ToolCalls(TwoWeeksAgo, now())
| extend
    ToolName = tostring(customDimensions.toolname),
    Week = iff(timestamp >= ThisWeek, "ThisWeek", iff(timestamp >= LastWeek, "LastWeek", "TwoWeeksAgo"))
| where ToolName contains "azurebackup"
| where Week in ("ThisWeek", "LastWeek")
| extend
    ExMsg = tostring(customDimensions["exception.message"]),
    ExType = tostring(customDimensions["exception.type"])
| extend StatusCode = toint(extract(@"StatusCode.*?(\d+)", 1, ExMsg))
| extend ErrorCategory = case(
    success == true, "Success",
    StatusCode >= 400 and StatusCode < 500, "Customer",
    ExType == "System.Collections.Generic.KeyNotFoundException", "Customer",
    ExType == "Azure.RequestFailedException" and StatusCode >= 500, "AzureService",
    ExType == "System.AggregateException", "AzureService",
    ExType in ("System.FormatException", "System.ArgumentNullException", "System.ArgumentException", "System.InvalidOperationException"), "McpToolBug",
    "Unknown")
| summarize
    Total = count(),
    OK = countif(ErrorCategory == "Success"),
    Customer = countif(ErrorCategory == "Customer"),
    AzureSvc = countif(ErrorCategory == "AzureService"),
    McpBug = countif(ErrorCategory == "McpToolBug"),
    Users = dcount(tostring(customDimensions.devdeviceid))
    by Week
| extend ToolRate = round(100.0 * (Total - McpBug) / Total, 1)
| order by Week desc
```

## 3. Error Summary by Tool and Exception Type (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == false
| extend
    ExceptionType = tostring(customDimensions["exception.type"]),
    ExceptionMessage = tostring(customDimensions["exception.message"]),
    DevDeviceId = tostring(customDimensions.devdeviceid)
| summarize
    ErrorCount = count(),
    DistinctUsers = dcount(DevDeviceId),
    LastSeen = max(timestamp)
    by ToolName, ExceptionType, ExceptionMessage
| order by ErrorCount desc
```

## 4. Error Hotspots by HTTP Status Code (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == false
| extend
    ExMsg = tostring(customDimensions["exception.message"]),
    ExType = tostring(customDimensions["exception.type"])
| extend StatusCode = toint(extract(@"StatusCode.*?(\d+)", 1, ExMsg))
| extend ErrorCategory = case(
    StatusCode >= 400 and StatusCode < 500, "Customer",
    ExType == "System.Collections.Generic.KeyNotFoundException", "Customer",
    ExType == "Azure.RequestFailedException" and StatusCode >= 500, "AzureService",
    ExType == "System.AggregateException", "AzureService",
    ExType in ("System.FormatException", "System.ArgumentNullException", "System.ArgumentException", "System.InvalidOperationException"), "McpToolBug",
    "Unknown")
| summarize Count = count()
    by ToolName, StatusCode, ErrorCategory, ExType
| order by ErrorCategory asc, Count desc
```

## 5. Daily Error Trend (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize
    Total = count(),
    Errors = countif(success == false),
    Users = dcount(tostring(customDimensions.devdeviceid))
    by bin(timestamp, 1d)
| extend FailureRate = round(100.0 * Errors / Total, 2)
| order by timestamp asc
```

## 6. Duration Percentiles for Successful Calls (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == true
| summarize
    P50_ms = round(percentile(duration, 50), 0),
    P95_ms = round(percentile(duration, 95), 0),
    P99_ms = round(percentile(duration, 99), 0),
    Calls = count()
    by ToolName
| order by P95_ms desc
```

## 7. Custom Telemetry Tags — Vault Type Usage (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend
    ToolName = tostring(customDimensions.toolname),
    VaultType = tostring(customDimensions["azurebackup/VaultType"])
| where ToolName contains "azurebackup"
| summarize Calls = count(), Errors = countif(success == false)
    by ToolName, VaultType
| order by Calls desc
```

## 8. Top Exception Stack Traces (7-day)

Use this when you need to investigate a specific error class in depth.

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == false
| extend
    ExType = tostring(customDimensions["exception.type"]),
    ExMsg = tostring(customDimensions["exception.message"]),
    ExStack = tostring(customDimensions["exception.stacktrace"])
| project timestamp, ToolName, ExType, ExMsg, ExStack
| order by timestamp desc
| take 10
```

## 9. Concentrated User Analysis

When a single user has many errors, drill into their full call timeline:

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend
    ToolName = tostring(customDimensions.toolname),
    DevDeviceId = tostring(customDimensions.devdeviceid)
| where ToolName == "<TOOL_NAME>"
| where DevDeviceId == "<DEVICE_ID>"
| project timestamp, success, duration, ExType = tostring(customDimensions["exception.type"])
| order by timestamp asc
```

## 10. 30-Day Overview

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize
    Total = count(),
    Succeeded = countif(success == true),
    Failed = countif(success == false),
    DistinctUsers = dcount(tostring(customDimensions.devdeviceid))
    by ToolName
| extend
    FailureRate = round(100.0 * Failed / Total, 2),
    SuccessRate = round(100.0 * Succeeded / Total, 2)
| order by Total desc
```

## 11. Custom Telemetry Tags — WorkloadType Usage (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend
    ToolName = tostring(customDimensions.toolname),
    WorkloadType = tostring(customDimensions["azurebackup/WorkloadType"])
| where ToolName contains "azurebackup"
| where isnotempty(WorkloadType)
| summarize Calls = count(), Errors = countif(success == false),
    Users = dcount(tostring(customDimensions.devdeviceid))
    by ToolName, WorkloadType
| order by Calls desc
```

## 12. Custom Telemetry Tags — DatasourceType Usage (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend
    ToolName = tostring(customDimensions.toolname),
    DatasourceType = tostring(customDimensions["azurebackup/DatasourceType"])
| where ToolName contains "azurebackup"
| where isnotempty(DatasourceType)
| summarize Calls = count(), Errors = countif(success == false),
    P95_ms = percentile(duration, 95)
    by ToolName, DatasourceType
| order by Calls desc
```

## 13. Custom Telemetry Tags — OperationScope (single vs list) (7-day)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend
    ToolName = tostring(customDimensions.toolname),
    OperationScope = tostring(customDimensions["azurebackup/OperationScope"])
| where ToolName contains "azurebackup"
| where isnotempty(OperationScope)
| summarize Calls = count(), P50_ms = percentile(duration, 50)
    by ToolName, OperationScope
| order by ToolName asc, OperationScope asc
```
