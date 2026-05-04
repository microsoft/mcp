---
name: azurebackup-telemetry-report
description: 'Generate weekly telemetry reports for Azure Backup MCP tools. Runs KQL queries against the Kusto telemetry cluster, analyzes error patterns with 3-way classification (Customer/Azure Service/MCP Tool Bug), compares week-over-week metrics, correlates with merged PRs and releases, and produces an Outlook-compatible HTML report. USE WHEN: weekly telemetry report, Azure Backup MCP telemetry, error analysis, telemetry bugs, weekly report, MCP tool success rate, backup telemetry, error classification.'
argument-hint: 'Generate the Azure Backup MCP weekly telemetry report'
---

# Azure Backup MCP — Weekly Telemetry Report Generator

## Purpose

Generate a comprehensive weekly telemetry report for the Azure Backup MCP toolset by:
1. Querying live production telemetry from Kusto
2. Classifying errors into Customer / Azure Service / MCP Tool Bug
3. Correlating with merged PRs and releases
4. Producing an Outlook-compatible HTML report

## When to Use

- Weekly cadence (every Monday or Sunday) to produce the status report
- After a release to assess error rate impact
- When triaging production bugs from telemetry data
- When preparing stakeholder updates on Azure Backup MCP health

## Prerequisites

- Azure authentication configured (the Kusto MCP tool needs access to `ddazureclients.kusto.windows.net`)
- Access to the `AzureDevExp` database on the Kusto cluster
- Git access to `microsoft/mcp` repository (for PR/release correlation)

## Procedure

### Step 1: Query Telemetry Data

Run the following KQL queries against the Kusto cluster using the MCP Kusto tool.
All queries use:
- **Cluster URI:** `https://ddazureclients.kusto.windows.net`
- **Database:** `AzureDevExp`
- **Function:** `getAzureMcpEvents_ToolCalls`

#### 1a. Per-Tool Success/Failure with 3-Way Classification (7-day)

This is the primary query. It classifies every call into one of four categories:
- **Success**: Tool call completed without error
- **Customer (4xx)**: User error — wrong permissions (403), bad input (400), resource not found (404)
- **Azure Service**: Azure SDK/API/auth layer failed — not an MCP code bug (includes `AggregateException`, `RequestFailedException` with 5xx)
- **MCP Tool Bug**: Our tool code is wrong — `FormatException`, `ArgumentNullException`, `ArgumentException`, etc.

Refer to [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md) for the full query.

#### 1b. Aggregate KPI — This Week vs Last Week

Compare total calls, success rate, customer errors, Azure service errors, and MCP tool bugs between
the current week and the previous week. See [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md).

#### 1c. Error Details

Run the error summary query to get per-tool exception types, messages, and stack traces.
Also run the daily error trend query. See [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md).

#### 1d. Duration Percentiles

Get P50/P95/P99 latency for successful calls. See [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md).

#### 1e. Custom Dimension Tags

Query `azurebackup/VaultType`, `azurebackup/WorkloadType`, `azurebackup/DatasourceType`, `azurebackup/OperationScope`
dimensions to see if users have upgraded to versions with telemetry tags.
Use queries 7, 11, 12, and 13 from [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md).

### Step 2: Gather Git Context

Run these git commands against the `microsoft/mcp` repository:

```bash
# Merged PRs this week
git log upstream/main --oneline --since="<week-start>" -- tools/Azure.Mcp.Tools.AzureBackup/

# Releases shipped
git log upstream/main --oneline --since="<week-start>" --grep="Prepare MCP release"

# Open branches
git branch -a --list "*azurebackup*"

# Changes since last release
git diff <last-release-tag>..upstream/main --stat -- tools/Azure.Mcp.Tools.AzureBackup/
```

Also count tests on main (checkout `upstream/main` test files first):
```bash
# Runnable unit test count = [Fact] + [InlineData] (each InlineData is a separate test run)
$facts = (Select-String "\[Fact\]" tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.UnitTests/**/*.cs | Measure-Object).Count
$inlines = (Select-String "\[InlineData" tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.UnitTests/**/*.cs | Measure-Object).Count
# Total runnable = $facts + $inlines

# Live test count — count only [Fact] (each method has [Fact]; [LiveTestOnly] is an
# additional attribute on the same method, so counting both would double-count)
Select-String "\[Fact\]" tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/*.cs | Measure-Object

# Registered tools
Select-String "AddCommand" tools/Azure.Mcp.Tools.AzureBackup/src/AzureBackupSetup.cs | Measure-Object
```

> **Important:** Do NOT count `[Fact]` + `[Theory]` as the unit test count — that gives you
> method count, not runnable test count. Each `[InlineData]` on a `[Theory]` is a separate test.

### Step 3: Analyze and Classify

Apply the 3-way error classification:

| Category | Exception Types | What It Means |
|----------|----------------|---------------|
| **Customer (4xx)** | `RequestFailedException` with 400/403/404, `KeyNotFoundException` | User error — wrong input, missing permissions, resource doesn't exist |
| **Azure Service** | `AggregateException`, `RequestFailedException` with 5xx | Azure SDK/API/auth layer failure — tool correctly surfaces it |
| **MCP Tool Bug** | `FormatException`, `ArgumentNullException`, `ArgumentException`, `InvalidOperationException` (thrown by our code for unsupported operations) | Our tool code is wrong — needs a fix |

**MCP Tool Success Rate** = (Total - MCP Tool Bugs) / Total

For each error cluster, determine:
- Is this a **new** error or seen in previous weeks?
- Is there an existing **bug ID** (BUG-1 through BUG-8 from the original triage)?
- Has a **fix been merged**? If so, which PR and release?
- Is the user possibly on an **older version** (pre-fix)?

### Step 4: Generate the Report

Use the [`report-template.html`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/assets/report-template.html) as a starting point.

The report must include these sections in order:
1. **Header** — Title, date range, summary line
2. **Key Metrics** — KPI boxes with week-over-week deltas (Total Calls, MCP Tool Success %, Customer Errors, Azure Service Errors, MCP Tool Bugs, Distinct Users)
3. **Releases Shipped** — Table of releases with Azure Backup changes
4. **Per-Tool Health Table** — 3-way classification with MCP % column
5. **Error Deep Dive** — Analysis of top error patterns
6. **Error Hotspots** — Table by HTTP status code and classification
7. **Merged PRs** — Cards for each merged PR with bug references
8. **Open PRs** — Pending branches
9. **Bug Tracker** — Updated status of all known bugs
10. **Codebase Snapshot** — Tool count, test counts, lines changed
11. **Action Items** — Prioritized next steps

Save the report as `azure-backup-telemetry-report-<date>.html` (both the styled version and the Outlook-compatible version).

### Step 5: Outlook-Compatible Version

For the Outlook version, apply these rules:
- **All styles inline** (no `<style>` block or CSS classes)
- **Table-based layout** instead of flexbox/grid for KPI boxes
- **No** `border-radius`, gradients, or CSS pseudo-elements
- Use `cellpadding`/`cellspacing` attributes on tables
- Include MSO `PixelsPerInch` declaration in `<head>`
- Use `border-left` on table cells for callout boxes (instead of separate div)

## Error Classification Decision Tree

```
Is success == true?
  └─ YES → "Success"
  └─ NO →
      Is StatusCode 400/403/404?
        └─ YES → "Customer (4xx)"
        └─ NO →
            Is ExceptionType Azure.RequestFailedException with 5xx?
              └─ YES → "Azure Service"
            Is ExceptionType System.AggregateException?
              └─ YES → "Azure Service"
            Is ExceptionType FormatException / ArgumentNullException / ArgumentException / InvalidOperationException?
              └─ YES → "MCP Tool Bug"
            Otherwise → "Unknown" (investigate)
```

## Known Bug IDs

These are the bugs triaged from the original telemetry analysis (April 2026):

| Bug | Tool | Description | Fix PR | Release |
|-----|------|-------------|--------|---------|
| BUG-1 | backup_status | ArgumentNullException on null ResourceType | #2518 (code included, not in PR title) | beta.7 |
| BUG-2 | protecteditem_protect | VM pre-discovery loop timeout | #2470 | beta.6 |
| BUG-3 | protectableitem_list | Workload normalization ArgumentException | #2518 | beta.7 |
| BUG-4 | soft-delete | Deprecated BackupResourceVaultConfig API | #2518 | beta.7 |
| BUG-5 | enable-crr | Deprecated BackupResourceConfig API | #2518 | beta.7 |
| BUG-6 | vault_get | FormatException on subscription name (non-GUID) | #2518 | beta.7 |
| BUG-7 | protecteditem_protect | DPP vault missing managed identity | #2470 | beta.6 |
| BUG-8 | recoverypoint_get | Null container in resolution | #2518 | beta.7 |
| NEW-1 | vault_get | AggregateException — pre-existing auth race (Azure Service, not MCP bug) | Not filed | — |

> **Note:** PR #2518 title says "Fix 5 telemetry-triaged bugs (BUG-3,4,5,6,8)" but the
> actual merged code also includes the BUG-1 fix. Always verify the diff, not just the title.

When new errors appear, assign the next sequential ID (NEW-2, NEW-3, etc.) and add to this table.

## References

- [`kql-queries.md`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/references/kql-queries.md) — All Kusto queries used in the report
- [`report-template.html`](https://github.com/microsoft/mcp/blob/main/tools/Azure.Mcp.Tools.AzureBackup/skills/azurebackup-telemetry-report/assets/report-template.html) — Outlook-compatible HTML template
