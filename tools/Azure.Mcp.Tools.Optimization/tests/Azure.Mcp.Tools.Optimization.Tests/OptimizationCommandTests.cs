// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests;

public sealed class OptimizationCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // These read-only tests query arbitrary existing resources across resource groups, so the base
    // ResourceBaseName/ResourceGroupName sanitizers don't apply; disable them and sanitize the
    // subscription id explicitly instead.
    public override bool EnableDefaultSanitizerAdditions => false;

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        new(new()
        {
            Regex = Settings.SubscriptionId,
            Value = "00000000-0000-0000-0000-000000000000",
        }),
    ];

    // Azure Monitor metric queries embed the record-time UTC window in the "timespan" query
    // parameter. Normalize it so playback matching succeeds regardless of when the tests run.
    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        new(new()
        {
            Regex = "timespan=([^&]+)",
            Value = "Sanitized",
            GroupForReplace = "1",
        })
    ];

    [Fact]
    public async Task Should_list_cost_saving_recommendations()
    {
        var result = await CallToolAsync(
            "optimization_recommendation_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var recommendations = result.AssertProperty("recommendations");
        Assert.Equal(JsonValueKind.Array, recommendations.ValueKind);
    }

    [Fact]
    public async Task Should_list_cost_saving_recommendations_with_top()
    {
        var result = await CallToolAsync(
            "optimization_recommendation_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "top", 5 }
            });

        var recommendations = result.AssertProperty("recommendations");
        Assert.Equal(JsonValueKind.Array, recommendations.ValueKind);
    }

    [Fact]
    public async Task Should_get_alternatives_for_vm()
    {
        var resourceId = await DiscoverVmResourceIdAsync();
        Assert.SkipWhen(string.IsNullOrWhiteSpace(resourceId), "No virtual machine available in the subscription to exercise alternatives.");

        var result = await CallToolAsync(
            "optimization_recommendation_alternatives",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-id", resourceId }
            });

        var returnedResourceId = result.AssertProperty("resourceId");
        Assert.Equal(JsonValueKind.String, returnedResourceId.ValueKind);

        var markdown = result.AssertProperty("markdown");
        Assert.Equal(JsonValueKind.String, markdown.ValueKind);
    }

    [Fact]
    public async Task Should_explain_recommendation_for_vm()
    {
        var resourceId = await DiscoverVmResourceIdAsync();
        Assert.SkipWhen(string.IsNullOrWhiteSpace(resourceId), "No virtual machine available in the subscription to exercise explain.");

        var result = await CallToolAsync(
            "optimization_recommendation_explain",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-id", resourceId }
            });

        var returnedResourceId = result.AssertProperty("resourceId");
        Assert.Equal(JsonValueKind.String, returnedResourceId.ValueKind);

        // recommendationCount is always returned; the current/target configuration is only present
        // when the resource actually has an Advisor right-size recommendation.
        var recommendationCount = result.AssertProperty("recommendationCount");
        Assert.Equal(JsonValueKind.Number, recommendationCount.ValueKind);
    }

    // Discovers an existing VM read-only (via the compute_vm_get tool) so the explain/alternatives
    // tools can be exercised without provisioning any resources.
    private async Task<string> DiscoverVmResourceIdAsync()
    {
        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        if (result is null || !result.Value.TryGetProperty("Vms", out var vms) || vms.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var firstVm = vms.EnumerateArray().FirstOrDefault();
        if (firstVm.ValueKind != JsonValueKind.Object
            || !firstVm.TryGetProperty("id", out var id)
            || id.ValueKind != JsonValueKind.String)
        {
            return string.Empty;
        }

        return RegisterOrRetrieveVariable("vmResourceId", id.GetString()!);
    }
}
