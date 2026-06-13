// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests;

/// <summary>
/// Unit tests for <see cref="MonitorService.BuildQuery"/>, focusing on correctness of
/// limit injection and locale-safe string comparison (MON-02).
/// </summary>
public class MonitorServiceBuildQueryTests
{
    // ── Limit injection ──────────────────────────────────────────────────────

    [Fact]
    public void BuildQuery_NoExistingLimit_AppendsLimitClause()
    {
        var result = MonitorService.BuildQuery("Heartbeat", "Heartbeat", 50);

        Assert.Contains("| limit 50", result);
    }

    [Fact]
    public void BuildQuery_AlreadyContainsLimitLowercase_DoesNotAppendDuplicate()
    {
        const string query = "Heartbeat\n| limit 100";

        var result = MonitorService.BuildQuery(query, "Heartbeat", 50);

        // Only one "| limit" clause should appear
        Assert.Equal(1, CountOccurrences(result, "| limit"));
    }

    [Fact]
    public void BuildQuery_AlreadyContainsLimitUppercase_DoesNotAppendDuplicate()
    {
        // MON-02: OrdinalIgnoreCase must be used so "LIMIT" is recognised in all locales
        const string query = "Heartbeat\n| LIMIT 100";

        var result = MonitorService.BuildQuery(query, "Heartbeat", 50);

        Assert.Equal(1, CountOccurrences(result, "limit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildQuery_AlreadyContainsLimitMixedCase_DoesNotAppendDuplicate()
    {
        const string query = "Heartbeat\n| Limit 100";

        var result = MonitorService.BuildQuery(query, "Heartbeat", 50);

        Assert.Equal(1, CountOccurrences(result, "limit", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildQuery_NullLimit_DoesNotAppendLimitClause()
    {
        const string query = "Heartbeat";

        var result = MonitorService.BuildQuery(query, "Heartbeat", null);

        Assert.DoesNotContain("| limit", result);
    }

    // ── Predefined queries ───────────────────────────────────────────────────

    [Fact]
    public void BuildQuery_PredefinedQueryRecent_ExpandsWithTableName()
    {
        var result = MonitorService.BuildQuery("recent", "Heartbeat", null);

        Assert.Contains("Heartbeat", result);
        Assert.Contains("order by TimeGenerated desc", result);
    }

    [Fact]
    public void BuildQuery_PredefinedQueryErrors_ExpandsWithTableName()
    {
        var result = MonitorService.BuildQuery("errors", "AppExceptions", null);

        Assert.Contains("AppExceptions", result);
        Assert.Contains("where Level == \"ERROR\"", result);
    }

    [Fact]
    public void BuildQuery_PredefinedQueryWithLimit_AppendLimit()
    {
        var result = MonitorService.BuildQuery("recent", "Heartbeat", 25);

        Assert.Contains("| limit 25", result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static int CountOccurrences(string source, string search,
        StringComparison comparison = StringComparison.Ordinal)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(search, index, comparison)) >= 0)
        {
            count++;
            index += search.Length;
        }
        return count;
    }
}
