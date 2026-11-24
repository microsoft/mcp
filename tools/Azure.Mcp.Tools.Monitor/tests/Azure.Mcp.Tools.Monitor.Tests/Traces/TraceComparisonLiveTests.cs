// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceComparisonLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override ValueTask InitializeAsync()
    {
        SetArguments("server", "start", "--mode", "all", "--namespace", "monitor",
            "--disable-caching", "--disable-proxy-tools");
        return base.InitializeAsync();
    }

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new(new()
        {
            Regex = "tenantId=[0-9a-fA-F-]+,objectId=[0-9a-fA-F-]+",
            Value = "Sanitized",
        }),
        // The default resource-group sanitizer covers URIs, not ARM IDs in JSON bodies.
        new(new()
        {
            Regex = @"(?i)/resourcegroups/(?<group>[\w\-.()]{1,90})",
            GroupForReplace = "group",
            Value = "Sanitized",
        }),
        new(new()
        {
            Regex = "\\b[0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}\\b",
            Value = "00000000-0000-0000-0000-000000000000",
        }),
        new(new()
        {
            Regex = "\\b[0-9a-fA-F]{32}\\b",
            Value = "00000000000000000000000000000000",
        }),
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        .. new[]
        {
            "$..displayName", "$..owner", "$..createdBy", "$..lastModifiedBy",
            "$..tags.*", "$..internalId", "$..values[*].value", "$..key.*",
            "$..span_id", "$..spanId", "$..rootSpanResourceId", "$..uniqueRecordId",
            "$..['span.name']", "$..root_span_name", "$..attribute", "$..attributeValue",
            "$..insightsMetadata", "$..dataQuality", "$..insightType", "$..issueType",
            "$..severity", "$..traceStartTime", "$..aggregationDimensions[*]",
            "$..['current.insightsMetadata']", "$..['compareTo.insightsMetadata']",
            "$..['current.dataQuality']", "$..['compareTo.dataQuality']",
        }.Select(static path => new BodyKeySanitizer(new BodyKeySanitizerBody(path))),
        new(new BodyKeySanitizerBody("$..trace_id") { Value = "00000000000000000000000000000000" }),
        new(new BodyKeySanitizerBody("$..traceId") { Value = "00000000000000000000000000000000" }),
        // Distinct synthetic IDs keep the recorded CPA populations distinct during playback.
        new(new BodyKeySanitizerBody("$.value[1].trace_id") { Value = "00000000000000000000000000000001" }),
        new(new BodyKeySanitizerBody("$.compareTo.traceId") { Value = "00000000000000000000000000000001" }),
    ];

    [Theory]
    [InlineData("statistics")]
    [InlineData("attributes")]
    [InlineData("critical-path")]
    public async Task Should_compare_independent_populations(string analysis)
    {
        string workspace = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");
        workspace = RegisterOrRetrieveVariable("trace_workspace",
            workspace.Replace(Settings.ResourceBaseName, "{base}", StringComparison.Ordinal))
            .Replace("{base}", Settings.ResourceBaseName, StringComparison.Ordinal);
        string end = RegisterOrRetrieveVariable("compare_end",
            DateTimeOffset.UtcNow.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        string start = RegisterOrRetrieveVariable("compare_start",
            DateTimeOffset.Parse(end, CultureInfo.InvariantCulture).AddMinutes(-30).UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        string comparisonStart = RegisterOrRetrieveVariable("compare_baseline_start",
            DateTimeOffset.Parse(start, CultureInfo.InvariantCulture).AddMinutes(-30).UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        var arguments = new Dictionary<string, object?>
        {
            ["subscription"] = Settings.SubscriptionId,
            ["tenant"] = Settings.TenantId,
            ["resource-group"] = Settings.ResourceGroupName,
            ["workspace"] = workspace,
            ["start-time"] = start,
            ["end-time"] = end,
            ["compare-start-time"] = comparisonStart,
            ["compare-end-time"] = start,
            ["max-results"] = 10,
        };
        if (analysis == "critical-path")
        {
            var discovered = await CallToolAsync("monitor_traces_query_run", new()
            {
                ["subscription"] = Settings.SubscriptionId,
                ["tenant"] = Settings.TenantId,
                ["resource-group"] = Settings.ResourceGroupName,
                ["workspace"] = workspace,
                ["query"] = "Traces | project trace_id",
                ["start-time"] = start,
                ["end-time"] = end,
                ["max-results"] = 2,
                ["include-spans"] = false,
            });
            var rows = discovered.AssertProperty("result").AssertProperty("payload").AssertProperty("value");
            var ids = rows.EnumerateArray().Select(row => row.AssertProperty("trace_id").GetString()!)
                .Select(id => id.Length < 32 && id.All(Uri.IsHexDigit) ? id.PadLeft(32, '0') : id)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (ids.Length < 2)
            {
                Assert.Skip("Critical-path comparison needs two distinct observed trace IDs.");
            }
            arguments["trace-ids"] = new[] { ids[0] };
            arguments["compare-trace-ids"] = new[] { ids[1] };
            arguments["compare-start-time"] = start;
            arguments["compare-end-time"] = end;
            arguments["include-samples"] = false;
        }
        else
        {
            string query = analysis == "statistics"
                ? "Traces | summarize count=count() by root_span_name"
                : "Traces";
            arguments["query"] = query;
            arguments["compare-query"] = query;
        }

        // Keep the "critical-path" case name so existing recordings still match.
        string tool = analysis == "critical-path" ? "paths" : analysis;
        var result = await CallToolAsync($"monitor_traces_query_analyze-compare-{tool}", arguments,
            resultProcessor: envelope => envelope.TryGetProperty("results", out var results) ? results : envelope);
        var envelope = result.AssertProperty("result");
        Assert.Equal("Succeeded", envelope.AssertProperty("status").GetString());
        var values = envelope.AssertProperty("payload").AssertProperty("value");
        Assert.Equal(JsonValueKind.Array, values.ValueKind);
        Assert.NotEmpty(values.EnumerateArray());
    }
}
