// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.Optimization.Services;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests;

public sealed class OptimizationCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const int DefaultTop = 100;

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

        AssertRecommendationListResult(result, DefaultTop);
    }

    [Fact]
    public async Task Should_list_cost_saving_recommendations_with_top()
    {
        const int top = 5;
        var result = await CallToolAsync(
            "optimization_recommendation_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "top", top }
            });

        AssertRecommendationListResult(result, top);
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

    private static void AssertRecommendationListResult(JsonElement? result, int maxCount)
    {
        var recommendations = result.AssertProperty("recommendations");
        Assert.Equal(JsonValueKind.Array, recommendations.ValueKind);
        Assert.InRange(recommendations.GetArrayLength(), 0, maxCount);

        var truncated = result.AssertProperty("areResultsTruncated");
        Assert.True(truncated.ValueKind is JsonValueKind.True or JsonValueKind.False);

        string? scopedSubscriptionId = null;
        foreach (var recommendation in recommendations.EnumerateArray())
        {
            var id = AssertNonEmptyString(recommendation, "id");
            Assert.Contains("/providers/microsoft.advisor/recommendations/", id, StringComparison.OrdinalIgnoreCase);
            AssertNonEmptyString(recommendation, "resourceId");

            // A subscription-scoped query must only return recommendations from that one subscription.
            var subscriptionId = AssertNonEmptyString(recommendation, "subscriptionId");
            scopedSubscriptionId ??= subscriptionId;
            Assert.Equal(scopedSubscriptionId, subscriptionId, ignoreCase: true);

            // Unattached disks are reported through diskRecommendationSummary, never as individual rows.
            var recommendationTypeId = AssertNonEmptyString(recommendation, "recommendationTypeId");
            Assert.True(Guid.TryParse(recommendationTypeId, out _), $"recommendationTypeId '{recommendationTypeId}' is not a GUID.");
            Assert.False(
                string.Equals(recommendationTypeId, OptimizationKqlQueries.UnattachedDiskRecommendationTypeId, StringComparison.OrdinalIgnoreCase),
                "Unattached-disk recommendations must not be listed individually.");

            AssertOptionalNonNegativeNumber(recommendation, "savingsAmount");
            AssertOptionalNonNegativeNumber(recommendation, "annualSavingsAmount");
        }

        // The summary is only returned when unattached-disk recommendations exist.
        if (result!.Value.TryGetProperty("diskRecommendationSummary", out var summary) && summary.ValueKind != JsonValueKind.Null)
        {
            var count = summary.AssertProperty("unattachedDiskCount").GetInt32();
            Assert.True(count > 0, "diskRecommendationSummary must only be returned when unattached disks exist.");
            Assert.Contains(count.ToString(CultureInfo.InvariantCulture), AssertNonEmptyString(summary, "message"));

            var subscriptionIds = summary.AssertProperty("subscriptionIds");
            Assert.Equal(JsonValueKind.Array, subscriptionIds.ValueKind);
            Assert.NotEqual(0, subscriptionIds.GetArrayLength());
            Assert.All(subscriptionIds.EnumerateArray(), e => Assert.True(Guid.TryParse(e.GetString(), out _)));

            var actionUrl = AssertNonEmptyString(summary, "actionUrl");
            Assert.StartsWith("https://", actionUrl, StringComparison.Ordinal);
            Assert.Contains(OptimizationKqlQueries.UnattachedDiskRecommendationTypeId, actionUrl, StringComparison.Ordinal);
        }
    }

    private static string AssertNonEmptyString(JsonElement element, string propertyName)
    {
        var property = element.AssertProperty(propertyName);
        Assert.Equal(JsonValueKind.String, property.ValueKind);
        var value = property.GetString();
        Assert.False(string.IsNullOrWhiteSpace(value), $"'{propertyName}' must not be empty.");
        return value!;
    }

    private static void AssertOptionalNonNegativeNumber(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null)
        {
            Assert.Equal(JsonValueKind.Number, property.ValueKind);
            Assert.True(property.GetDouble() >= 0, $"'{propertyName}' must not be negative.");
        }
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

        var resourceId = RegisterOrRetrieveVariable("vmResourceId", id.GetString()!);

        // The recorded variable retains the record-time subscription id, but recorded request bodies
        // are sanitized to Settings.SubscriptionId. Rewrite the subscription segment so the KQL query
        // built from this resource id matches the recording during playback (no-op during recording).
        return NormalizeSubscriptionId(resourceId);
    }

    private string NormalizeSubscriptionId(string resourceId)
    {
        const string prefix = "/subscriptions/";
        if (!resourceId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return resourceId;
        }

        var rest = resourceId[prefix.Length..];
        var slashIndex = rest.IndexOf('/');
        if (slashIndex < 0)
        {
            return resourceId;
        }

        return prefix + Settings.SubscriptionId + rest[slashIndex..];
    }
}
