// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

/// <summary>
/// Live tests for the monitor traces toolset. These target a deployed Azure Monitor
/// Workspace with a traceContainers/default sub-resource (see test-resources.traces.module.bicep).
/// </summary>
public sealed class TracesCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Resource Graph returns the caller's tenant and object id in x-ms-operation-identifier, and
    // recordings are published to the public Azure/azure-sdk-assets repo. The base class adds its
    // own defaults to this same list, so overriding extends rather than replaces them.
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
        // Preserve valid GUID/trace-ID formats so sanitized values still replay in dependent requests.
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

    // Trace results can contain customer-defined attributes, span names, and identifiers.
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        .. new[]
        {
            "$..displayName",
            "$..owner",
            "$..createdBy",
            "$..lastModifiedBy",
            "$..trace_id",
            "$..traceId",
            "$..span_id",
            "$..spanId",
            "$..rootSpanResourceId",
            "$..uniqueRecordId",
            "$..['span.name']",
            "$..attribute",
            "$..attributeValue",
            "$..insightsMetadata",
            "$..dataQuality",
            "$..insightType",
            "$..issueType",
            "$..severity",
            "$..traceStartTime",
            "$..aggregationDimensions[*]",
        }.Select(static path => new BodyKeySanitizer(new BodyKeySanitizerBody(path)
        {
            Value = path is "$..trace_id" or "$..traceId"
                ? "00000000000000000000000000000000"
                : "Sanitized",
        })),
    ];

    [Fact]
    public async Task Should_compare_trace_cohorts()
    {
        var workspace = GetAnalysisWorkspace();
        var end = RegisterOrRetrieveVariable("compare_end", DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        var start = DateTimeOffset.Parse(end, System.Globalization.CultureInfo.InvariantCulture).AddMinutes(-30)
            .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        var result = await CallToolAsync("monitor_traces_query_analyze-top-attributes", new()
        {
            ["subscription"] = Settings.SubscriptionId,
            ["tenant"] = Settings.TenantId,
            ["resource-group"] = Settings.ResourceGroupName,
            ["workspace"] = workspace,
            ["overall-start-time"] = start,
            ["overall-end-time"] = end,
            ["selection-start-time"] = start,
            ["selection-end-time"] = end,
            ["selection-field"] = "span_count",
            ["selection-op"] = ">",
            ["selection-value"] = "10",
        });
        var analysis = result.AssertProperty("result");
        Assert.Equal("Succeeded", analysis.AssertProperty("status").GetString());
        Assert.Equal(JsonValueKind.Array, analysis.AssertProperty("payload").AssertProperty("value").ValueKind);
    }

    [Fact]
    public async Task Should_analyze_trace_critical_path()
    {
        var workspace = GetAnalysisWorkspace();
        var end = RegisterOrRetrieveVariable("critical_path_end", DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        var start = DateTimeOffset.Parse(end, System.Globalization.CultureInfo.InvariantCulture).AddMinutes(-30)
            .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        var traces = await CallToolAsync("monitor_traces_query_run", new()
        {
            ["subscription"] = Settings.SubscriptionId,
            ["tenant"] = Settings.TenantId,
            ["resource-group"] = Settings.ResourceGroupName,
            ["workspace"] = workspace,
            ["query"] = "Traces | project trace_id",
            ["start-time"] = start,
            ["end-time"] = end,
            ["max-results"] = 1,
            ["include-spans"] = false,
        });
        var rows = traces.AssertProperty("result").AssertProperty("payload").AssertProperty("value");
        var hasTraces = rows.GetArrayLength() > 0;
        var traceId = hasTraces ? rows[0].AssertProperty("trace_id").GetString() : "abcdef1234567890abcdef1234567890";
        var result = await CallToolAsync("monitor_traces_query_analyze-critical-path", new()
        {
            ["subscription"] = Settings.SubscriptionId,
            ["tenant"] = Settings.TenantId,
            ["resource-group"] = Settings.ResourceGroupName,
            ["workspace"] = workspace,
            ["trace-ids"] = new[] { traceId },
            ["start-time"] = start,
            ["end-time"] = end,
        }, resultProcessor: envelope => envelope.TryGetProperty("results", out var results) ? results : envelope);
        if (!hasTraces)
        {
            // Freshly deployed workspaces have no spans; analysis must report that explicitly.
            Assert.Contains("No traces found", result.AssertProperty("message").GetString());
            return;
        }
        var analysis = result.AssertProperty("result");
        Assert.Equal("Succeeded", analysis.AssertProperty("status").GetString());
        Assert.NotEmpty(analysis.AssertProperty("payload").AssertProperty("value").EnumerateArray());
    }

    private string GetAnalysisWorkspace()
    {
        var workspace = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");
        // The proxy sanitizes resource names, but not saved variables; save a template instead.
        var template = workspace.Replace(Settings.ResourceBaseName, "{base}", StringComparison.Ordinal);
        return RegisterOrRetrieveVariable("trace_workspace", template)
            .Replace("{base}", Settings.ResourceBaseName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_list_trace_accounts()
    {
        var traceAccountName = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");

        var result = await CallToolAsync(
            "monitor_traces_account_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var accounts = result.AssertProperty("accounts");
        Assert.Equal(JsonValueKind.Array, accounts.ValueKind);

        var found = accounts.EnumerateArray()
            .Any(a => string.Equals(a.GetProperty("name").GetString(), traceAccountName, StringComparison.OrdinalIgnoreCase));
        Assert.True(found, $"Expected trace account '{traceAccountName}' in the list.");
    }

    [Fact]
    public async Task Should_list_trace_fields()
    {
        var traceAccountName = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");

        var result = await CallToolAsync(
            "monitor_traces_field_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "workspace", traceAccountName }
            });

        var fields = result.AssertProperty("fields");
        var traceFields = fields.AssertProperty("traceFields");
        var spanFields = fields.AssertProperty("spanFields");
        Assert.Equal(JsonValueKind.Array, traceFields.ValueKind);
        Assert.Equal(JsonValueKind.Array, spanFields.ValueKind);
        // A fresh AMW always publishes the built-in schema even before any spans arrive.
        Assert.NotEmpty(traceFields.EnumerateArray());
        Assert.NotEmpty(spanFields.EnumerateArray());
    }

    [Fact]
    // The request body carries a start/end time defaulted to "now minus 30 minutes", so it differs
    // on every run and can never match a recording.
    [CustomMatcher(compareBody: false)]
    public async Task Should_run_trace_query_returning_empty()
    {
        var traceAccountName = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");

        var result = await CallToolAsync(
            "monitor_traces_query_run",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "workspace", traceAccountName },
                // Existence probe on a freshly deployed AMW with no ingested spans: expect
                // Succeeded with an empty result set, not Running or failed. Trace Store rejects
                // `take` ("'TakeOperator' is not supported in a query pipeline"), so bare `Traces`
                // is the minimal valid query; --max-results caps the page instead.
                { "query", "Traces" },
                { "max-results", "1" }
            });

        var resultObj = result.AssertProperty("result");
        var status = resultObj.AssertProperty("status").GetString();
        Assert.Equal("Succeeded", status);
        Assert.Empty(resultObj.AssertProperty("payload").AssertProperty("value").EnumerateArray());
    }

    [Fact]
    [CustomMatcher(compareBody: false)]
    public async Task Should_resume_trace_query_without_resubmitting()
    {
        var workspace = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");
        var started = await CallToolAsync(
            "monitor_traces_query_run",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "workspace", workspace },
                { "query", "Traces" },
                { "max-results", "1" }
            });
        var operationId = started.AssertProperty("result").AssertProperty("operationId").GetString();

        var resumed = await CallToolAsync(
            "monitor_traces_query_resume",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "workspace", workspace },
                { "operation-id", operationId! },
                { "page-size", "1" }
            });

        var result = resumed.AssertProperty("result");
        Assert.Equal(operationId, result.AssertProperty("operationId").GetString());
        Assert.Equal("Succeeded", result.AssertProperty("status").GetString());
        Assert.Empty(result.AssertProperty("payload").AssertProperty("value").EnumerateArray());
    }

    [Fact]
    public async Task Should_report_not_found_when_cancelling_an_unknown_operation()
    {
        var traceAccountName = Settings.DeploymentOutputs.GetValueOrDefault("traceAccountName", $"{Settings.ResourceBaseName}-traces");

        // Cancelling needs no in-flight query to cover what actually regresses here: the
        // operations/{id}:cancel URL shape and the 404 mapping. A well-formed id that was
        // never issued exercises both. resultProcessor returns the envelope because a failed
        // command has no "results" property.
        var response = await CallToolAsync(
            "monitor_traces_query_cancel",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "workspace", traceAccountName },
                { "operation-id", "00000000-0000-0000-0000-000000000000" }
            },
            resultProcessor: envelope => envelope);

        Assert.Equal(404, response.AssertProperty("status").GetInt32());
        Assert.StartsWith(
            "Operation '00000000-0000-0000-0000-000000000000' not found or has expired.",
            response.AssertProperty("message").GetString(),
            StringComparison.Ordinal);
    }
}
