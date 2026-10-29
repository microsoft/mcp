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
    ];

    // Subscription display names are not covered by the default GUID sanitizers.
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new(new BodyKeySanitizerBody("$..displayName")),
    ];

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
