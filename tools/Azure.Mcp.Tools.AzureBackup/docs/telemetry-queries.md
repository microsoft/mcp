# Azure Backup MCP Telemetry Queries

> All queries use the `getAzureMcpEvents_ToolCalls` function from the Azure MCP telemetry cluster.
> Custom dimensions reference `customDimensions.*` fields emitted by OpenTelemetry.

---

## 1. Production Data Analysis

### 1.1 Error Summary by Tool and Exception Type (your original query, refined)

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == false
| extend
    ExceptionType    = tostring(customDimensions["exception.type"]),
    ExceptionMessage = tostring(customDimensions["exception.message"]),
    DevDeviceId      = tostring(customDimensions.devdeviceid)
| summarize
    ErrorCount    = count(),
    DistinctUsers = dcount(DevDeviceId),
    LastSeen      = max(timestamp)
    by ToolName, ExceptionType, ExceptionMessage
| order by ErrorCount desc
```

### 1.2 Success vs Failure Rate per Tool

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize
    Total     = count(),
    Succeeded = countif(success == true),
    Failed    = countif(success == false)
    by ToolName
| extend FailureRate = round(100.0 * Failed / Total, 2)
| order by Total desc
```

### 1.3 Top Exception Stack Traces (for debugging)

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == false
| extend
    ExceptionType       = tostring(customDimensions["exception.type"]),
    ExceptionMessage    = tostring(customDimensions["exception.message"]),
    ExceptionStackTrace = tostring(customDimensions["exception.stacktrace"])
| summarize Count = count() by ToolName, ExceptionType, ExceptionMessage, ExceptionStackTrace
| top 20 by Count desc
```

### 1.4 Daily Error Trend (spot regressions)

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize
    Total  = count(),
    Errors = countif(success == false)
    by bin(timestamp, 1d), ToolName
| extend FailureRate = round(100.0 * Errors / Total, 2)
| order by timestamp desc, ToolName asc
```

### 1.5 Duration Percentiles (P50/P95/P99) per Tool

```kql
getAzureMcpEvents_ToolCalls(ago(7d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == true
| summarize
    P50_ms = percentile(duration, 50),
    P95_ms = percentile(duration, 95),
    P99_ms = percentile(duration, 99),
    Calls  = count()
    by ToolName
| order by P95_ms desc
```

### 1.6 Unique Users per Tool (adoption tracking)

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName    = tostring(customDimensions.toolname),
    DevDeviceId = tostring(customDimensions.devdeviceid)
| where ToolName contains "azurebackup"
| summarize
    TotalCalls    = count(),
    DistinctUsers = dcount(DevDeviceId)
    by ToolName
| order by TotalCalls desc
```

### 1.7 Client Distribution (which editors/hosts are calling us)

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName      = tostring(customDimensions.toolname),
    ClientName    = tostring(customDimensions.ClientName),
    ClientVersion = tostring(customDimensions.ClientVersion),
    ServerMode    = tostring(customDimensions.ServerMode),
    Transport     = tostring(customDimensions.Transport)
| where ToolName contains "azurebackup"
| summarize Calls = count(), Users = dcount(tostring(customDimensions.devdeviceid))
    by ClientName, ClientVersion, ServerMode, Transport
| order by Calls desc
```

---

## 2. Weekly Report Queries

### 2.1 Weekly Executive Summary

```kql
// Paste this as the single-query weekly snapshot
let WeekStart = startofweek(ago(7d));
let WeekEnd   = startofweek(now());
getAzureMcpEvents_ToolCalls(WeekStart, WeekEnd)
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize
    TotalCalls    = count(),
    Succeeded     = countif(success == true),
    Failed        = countif(success == false),
    DistinctUsers = dcount(tostring(customDimensions.devdeviceid)),
    P50_ms        = percentile(duration, 50),
    P95_ms        = percentile(duration, 95)
    by ToolName
| extend FailureRate = round(100.0 * Failed / TotalCalls, 2)
| order by TotalCalls desc
```

### 2.2 Week-over-Week Comparison

```kql
let ThisWeek = startofweek(ago(0d));
let LastWeek = startofweek(ago(7d));
let TwoWeeksAgo = startofweek(ago(14d));
getAzureMcpEvents_ToolCalls(TwoWeeksAgo, ThisWeek)
| extend
    ToolName = tostring(customDimensions.toolname),
    Week     = iff(timestamp >= LastWeek, "ThisWeek", "LastWeek")
| where ToolName contains "azurebackup"
| summarize
    Calls  = count(),
    Errors = countif(success == false),
    Users  = dcount(tostring(customDimensions.devdeviceid))
    by Week, ToolName
| order by ToolName asc, Week desc
```

### 2.3 New Errors This Week (not seen last week)

```kql
let ThisWeekStart = startofweek(ago(0d));
let LastWeekStart = startofweek(ago(7d));
let thisWeekErrors =
    getAzureMcpEvents_ToolCalls(LastWeekStart, ThisWeekStart)
    | extend ToolName = tostring(customDimensions.toolname)
    | where ToolName contains "azurebackup" and success == false
    | extend ExKey = strcat(tostring(customDimensions["exception.type"]), "|", tostring(customDimensions["exception.message"]))
    | where timestamp >= ThisWeekStart
    | distinct ExKey;
let lastWeekErrors =
    getAzureMcpEvents_ToolCalls(LastWeekStart, ThisWeekStart)
    | extend ToolName = tostring(customDimensions.toolname)
    | where ToolName contains "azurebackup" and success == false
    | extend ExKey = strcat(tostring(customDimensions["exception.type"]), "|", tostring(customDimensions["exception.message"]))
    | where timestamp < ThisWeekStart
    | distinct ExKey;
thisWeekErrors
| join kind=leftanti lastWeekErrors on ExKey
```

### 2.4 Slowest Operations This Week

```kql
let WeekStart = startofweek(ago(7d));
getAzureMcpEvents_ToolCalls(WeekStart, now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == true
| top 25 by duration desc
| project
    timestamp,
    ToolName,
    Duration_sec = round(duration / 1000.0, 2),
    ClientName   = tostring(customDimensions.ClientName),
    DevDeviceId  = tostring(customDimensions.devdeviceid)
```

### 2.5 Error Hotspots by HTTP Status Code

```kql
let WeekStart = startofweek(ago(7d));
getAzureMcpEvents_ToolCalls(WeekStart, now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| where success == false
| extend
    ExceptionMessage = tostring(customDimensions["exception.message"]),
    ExceptionType    = tostring(customDimensions["exception.type"])
| extend StatusCode = extract(@"""StatusCode""\s*:\s*(\d+)", 1, ExceptionMessage)
| summarize ErrorCount = count() by ToolName, StatusCode, ExceptionType
| order by ErrorCount desc
```

---

## 3. Custom Dimension Queries (post-telemetry-instrumentation)

> These queries will produce results **after** the telemetry tags PR is merged and deployed.

### 3.1 Usage by Vault Type (RSV vs DPP)

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName  = tostring(customDimensions.toolname),
    VaultType = tostring(customDimensions["azurebackup/VaultType"])
| where ToolName contains "azurebackup"
| summarize Calls = count(), Errors = countif(success == false)
    by ToolName, VaultType
| order by Calls desc
```

### 3.2 Failure Rate by Vault Type

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName  = tostring(customDimensions.toolname),
    VaultType = tostring(customDimensions["azurebackup/VaultType"])
| where ToolName contains "azurebackup"
| where isnotempty(VaultType)
| summarize
    Total  = count(),
    Failed = countif(success == false)
    by VaultType
| extend FailureRate = round(100.0 * Failed / Total, 2)
```

### 3.3 Policy Create by Workload Type

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName     = tostring(customDimensions.toolname),
    WorkloadType = tostring(customDimensions["azurebackup/WorkloadType"])
| where ToolName == "azmcp_azurebackup_policy_create"
| summarize
    Calls  = count(),
    Errors = countif(success == false),
    Users  = dcount(tostring(customDimensions.devdeviceid))
    by WorkloadType
| order by Calls desc
```

### 3.4 Single vs List Operation Distribution

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName       = tostring(customDimensions.toolname),
    OperationScope = tostring(customDimensions["azurebackup/OperationScope"])
| where ToolName contains "azurebackup"
| where isnotempty(OperationScope)
| summarize Calls = count(), P50_ms = percentile(duration, 50)
    by ToolName, OperationScope
| order by ToolName asc, OperationScope asc
```

### 3.5 Protect Command by Datasource Type

```kql
getAzureMcpEvents_ToolCalls(ago(30d), now())
| extend
    ToolName       = tostring(customDimensions.toolname),
    DatasourceType = tostring(customDimensions["azurebackup/DatasourceType"])
| where ToolName == "azmcp_azurebackup_protecteditem_protect"
| summarize
    Calls  = count(),
    Errors = countif(success == false),
    P95_ms = percentile(duration, 95)
    by DatasourceType
| order by Calls desc
```

---

## 4. Alerting / SLA Queries

### 4.1 High Failure Rate Alert (>20% in last 4h)

```kql
getAzureMcpEvents_ToolCalls(ago(4h), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize Total = count(), Failed = countif(success == false) by ToolName
| where Total >= 5 // minimum sample size
| extend FailureRate = round(100.0 * Failed / Total, 2)
| where FailureRate > 20
```

### 4.2 P95 Latency Breach (>30s)

```kql
getAzureMcpEvents_ToolCalls(ago(1h), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize P95_ms = percentile(duration, 95), Calls = count() by ToolName
| where Calls >= 3 and P95_ms > 30000
```

---

## 5. Composite Weekly Report Query

> Run this single query to produce a full weekly report table.

```kql
let WeekStart = startofweek(ago(7d));
let WeekEnd   = now();
getAzureMcpEvents_ToolCalls(WeekStart, WeekEnd)
| extend
    ToolName    = tostring(customDimensions.toolname),
    DevDeviceId = tostring(customDimensions.devdeviceid),
    VaultType   = tostring(customDimensions["azurebackup/VaultType"]),
    WorkloadType = tostring(customDimensions["azurebackup/WorkloadType"]),
    OperationScope = tostring(customDimensions["azurebackup/OperationScope"])
| where ToolName contains "azurebackup"
| summarize
    TotalCalls    = count(),
    Succeeded     = countif(success == true),
    Failed        = countif(success == false),
    DistinctUsers = dcount(DevDeviceId),
    P50_ms        = round(percentile(duration, 50), 0),
    P95_ms        = round(percentile(duration, 95), 0),
    P99_ms        = round(percentile(duration, 99), 0),
    TopVaultType  = take_any(VaultType),
    TopWorkload   = take_any(WorkloadType)
    by ToolName
| extend
    FailureRate   = round(100.0 * Failed / TotalCalls, 2),
    SuccessRate   = round(100.0 * Succeeded / TotalCalls, 2)
| project
    ToolName,
    TotalCalls,
    SuccessRate,
    FailureRate,
    DistinctUsers,
    P50_ms,
    P95_ms,
    P99_ms,
    Failed
| order by TotalCalls desc
```
