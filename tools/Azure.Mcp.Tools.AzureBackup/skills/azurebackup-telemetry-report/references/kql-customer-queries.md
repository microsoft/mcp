# Azure Backup MCP — Customer & Adoption KQL Queries

All queries use the `getAzureMcpEvents_ToolCalls` function from the Azure MCP telemetry cluster.

- **Cluster URI:** `https://ddazureclients.kusto.windows.net`
- **Database:** `AzureDevExp`
- **Cross-cluster:** `mabprod1.kusto.windows.net` / `AzureBackup` and `icmdataro.centralus` / `Customer`

Replace `{DAYS}` with the desired time range (e.g., `15` for 15 days).

> **Note on field casing:** Lowercase bag access (e.g., `customDimensions.toolname`) works because
> `getAzureMcpEvents_ToolCalls` normalizes keys. If queries return empty results, try PascalCase
> variants (`customDimensions.ClientName`, `customDimensions.Version`). See the full note in
> [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md).

> **Critical:** Always use `P360_CustomerName` instead of `CustomerName` in P360. The `CustomerName`
> field contains generic tenant names (e.g., "Denis" for all Prometeia SpA subs). `P360_CustomerName`
> has the actual company name.

---

## 14. Overall Summary

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize
    TotalCalls = count(),
    SuccessCount = countif(success == true),
    FailedCount = countif(success == false),
    DistinctUsers = dcount(tostring(customDimensions.devdeviceid)),
    CallsWithSubTag = countif(isnotempty(tostring(customDimensions.azsubscriptionguid)))
```

## 15. Subscription Breakdown

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname),
         SubId = tostring(customDimensions.azsubscriptionguid),
         DeviceId = tostring(customDimensions.devdeviceid)
| where ToolName contains "azurebackup"
| where isnotempty(SubId)
| summarize Calls = count(), Users = dcount(DeviceId), ToolsUsed = dcount(ToolName)
    by SubId
| order by Calls desc
```

## 16. Customer Name Resolution (P360 — use P360_CustomerName)

```kql
// Replace <subscription_ids> with dynamic array of sub IDs from query 15
cluster('mabprod1.kusto.windows.net').database('AzureBackup').P360CustomerSubscriptions
| where SubscriptionId in (<subscription_ids>)
| distinct P360_CustomerName, CustomerName, SubscriptionName, SubscriptionId, UsageType, OfferType
```

## 17. External vs Internal Classification (C360)

```kql
// External customers only
cluster('icmdataro.centralus').database('Customer').C360_CustomerSubscriptions
| where SubscriptionId in (<subscription_ids>)
| where UsageType != "Internal"
| where OfferType != "Internal"
| distinct CustomerName, SubscriptionId, UsageType, OfferType
```

```kql
// Internal (Microsoft) customers
cluster('icmdataro.centralus').database('Customer').C360_CustomerSubscriptions
| where SubscriptionId in (<subscription_ids>)
| where UsageType == "Internal" or OfferType == "Internal"
| distinct CustomerName, SubscriptionId, UsageType, OfferType
```

## 18. Non-GUID Subscription Name Resolution

If any `azsubscriptionguid` values are not GUIDs (e.g., "ERM-BPER"), resolve via SubscriptionName:

```kql
cluster('mabprod1.kusto.windows.net').database('AzureBackup').P360CustomerSubscriptions
| where SubscriptionName == "<sub_name_value>"
| distinct P360_CustomerName, SubscriptionName, SubscriptionId
```

## 19. Tool Usage Ranking

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname),
         DeviceId = tostring(customDimensions.devdeviceid)
| where ToolName contains "azurebackup"
| summarize Calls = count(), Succeeded = countif(success == true),
            Failed = countif(success == false), Users = dcount(DeviceId)
    by ToolName
| extend SuccessRate = round(100.0 * Succeeded / Calls, 1)
| order by Calls desc
```

## 20. Per-Customer Tool Breakdown

```kql
// Replace <customer_subs> with dynamic array of subscription IDs for a specific customer
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname),
         SubId = tostring(customDimensions.azsubscriptionguid)
| where ToolName contains "azurebackup"
| where SubId in (<customer_subs>) or SubId == "<sub_name>"
| summarize Calls = count() by ToolName
| order by Calls desc
```

## 21. Client / Editor Distribution

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname),
         ClientName = tostring(customDimensions.clientname)
| where ToolName contains "azurebackup"
| summarize Calls = count(), Users = dcount(tostring(customDimensions.devdeviceid))
    by ClientName
| order by Calls desc
```

## 22. MCP Server Version Distribution

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname),
         Version = tostring(customDimensions.version)
| where ToolName contains "azurebackup"
| summarize Calls = count(), Users = dcount(tostring(customDimensions.devdeviceid))
    by Version
| order by Calls desc
```

## 23. Daily Trend (for timechart dashboards)

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname)
| where ToolName contains "azurebackup"
| summarize Total = count(), Errors = countif(success == false),
    Users = dcount(tostring(customDimensions.devdeviceid))
    by bin(timestamp, 1d)
| extend FailureRate = round(100.0 * Errors / Total, 2)
| order by timestamp asc
```

## 24. Version by Client Matrix

```kql
getAzureMcpEvents_ToolCalls(ago({DAYS}d), now())
| extend ToolName = tostring(customDimensions.toolname),
         Version = tostring(customDimensions.version),
         ClientName = tostring(customDimensions.clientname)
| where ToolName contains "azurebackup"
| summarize Calls = count(), Users = dcount(tostring(customDimensions.devdeviceid))
    by Version, ClientName
| order by Calls desc
```

---

## Customer Aggregation Rules

When building the final customer table from queries 15–18:

1. **Group by `P360_CustomerName`**, not by subscription ID — one row per customer
2. **Merge multi-sub customers**: e.g., Prometeia SpA may have 15+ subscriptions
3. **Merge non-GUID entries**: If a non-GUID value resolves to an existing customer's subscription via query 18, merge it
4. **Categorize**: External (C360 UsageType=External) vs Internal (UsageType=Internal)
5. **~74% of calls lack `azsubscriptionguid`** (pre-beta.14 clients) — note this in the report

## Known Data Quirks

- `azsubscriptionguid` only emitted since beta.14 (~May 28, 2026)
- `P360.CustomerName` is unreliable (e.g., "Denis" = Prometeia SpA) — always use `P360_CustomerName`
- Non-GUID subscription values occur when clients pass subscription name instead of GUID
- Namespace-mode proxy tools (e.g., `get_azure_backup_details_azurebackup_vault_get`) have `toolarea=get_azure_backup_details` but `contains 'azurebackup'` catches them correctly
