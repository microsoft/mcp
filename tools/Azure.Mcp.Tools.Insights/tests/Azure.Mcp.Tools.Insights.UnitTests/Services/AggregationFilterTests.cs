// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Tools.Insights.Services;
using Azure.Mcp.Tools.Insights.Services.Models;
using Xunit;

namespace Azure.Mcp.Tools.Insights.UnitTests.Services;

public class AggregationFilterTests
{
    [Fact]
    public void Filter_RemovesDiscardTypes()
    {
        var input = Aggregation(
            ("microsoft.advisor/recommendations", LocationLeaf("eastus")),
            ("microsoft.storage/storageaccounts", LocationLeaf("eastus")));

        var result = AggregationFilter.Filter(input);

        Assert.False(result.ResourceTypes.ContainsKey("microsoft.advisor/recommendations"));
        Assert.True(result.ResourceTypes.ContainsKey("microsoft.storage/storageaccounts"));
    }

    [Fact]
    public void Filter_RemovesArmChildTypes()
    {
        var input = Aggregation(
            ("microsoft.storage/storageaccounts/blobservices", LocationLeaf("eastus")),
            ("microsoft.storage/storageaccounts", LocationLeaf("eastus")));

        var result = AggregationFilter.Filter(input);

        Assert.False(result.ResourceTypes.ContainsKey("microsoft.storage/storageaccounts/blobservices"));
        Assert.True(result.ResourceTypes.ContainsKey("microsoft.storage/storageaccounts"));
    }

    [Theory]
    [InlineData("provisioningState")]
    [InlineData("connectionString")]
    [InlineData("etag")]
    [InlineData("password")]
    [InlineData("endTime")]
    public void Filter_DropsDeniedKeys(string deniedKey)
    {
        var props = new JsonObject
        {
            ["properties"] = new JsonObject
            {
                [deniedKey] = new JsonObject { ["whatever"] = 1.0 },
                ["minimumTlsVersion"] = new JsonObject { ["TLS1_2"] = 1.0 },
            }
        };
        var input = Aggregation(("microsoft.storage/storageaccounts", props));

        var result = AggregationFilter.Filter(input);
        var filtered = (JsonObject)result.ResourceTypes["microsoft.storage/storageaccounts"].PropertyAggregations["properties"]!;

        Assert.False(filtered.ContainsKey(deniedKey));
        Assert.True(filtered.ContainsKey("minimumTlsVersion"));
    }

    [Fact]
    public void Filter_DropsLeavesBelowMinCoverage()
    {
        var props = new JsonObject
        {
            ["properties"] = new JsonObject
            {
                ["someProp"] = new JsonObject { ["value"] = 0.05 },
            }
        };
        var input = Aggregation(("microsoft.test/widgets", props));

        var result = AggregationFilter.Filter(input);
        var filtered = result.ResourceTypes["microsoft.test/widgets"].PropertyAggregations;

        Assert.False(filtered.ContainsKey("properties"));
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("12345678-1234-1234-1234-1234567890ab")]
    [InlineData("2023-08-21T10:35:55Z")]
    [InlineData("https://example.com/foo")]
    public void Filter_DropsDeniedValues(string deniedValue)
    {
        var props = new JsonObject
        {
            ["properties"] = new JsonObject
            {
                ["someProp"] = new JsonObject
                {
                    [deniedValue] = 0.5,
                    ["allowed-value"] = 0.5,
                },
            }
        };
        var input = Aggregation(("microsoft.test/widgets", props));

        var result = AggregationFilter.Filter(input);
        var leaf = (JsonObject)result.ResourceTypes["microsoft.test/widgets"]
            .PropertyAggregations["properties"]!["someProp"]!;

        Assert.False(leaf.ContainsKey(deniedValue));
        Assert.True(leaf.ContainsKey("allowed-value"));
    }

    [Fact]
    public void Filter_RelationalIdKey_KeepsOnlyArmIdValues()
    {
        // Note: PropertyAggregator emits lower-cased property keys; AggregationFilter's
        // RelationalIdKeys set is matched ordinally against the lower-cased key.
        var props = new JsonObject
        {
            ["properties"] = new JsonObject
            {
                ["subnetid"] = new JsonObject
                {
                    ["/subscriptions/abc/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sub"] = 0.6,
                    ["not-an-arm-id"] = 0.4,
                },
            }
        };
        var input = Aggregation(("microsoft.test/widgets", props));

        var result = AggregationFilter.Filter(input);
        var leaf = (JsonObject)result.ResourceTypes["microsoft.test/widgets"]
            .PropertyAggregations["properties"]!["subnetid"]!;

        Assert.Single(leaf);
        Assert.True(leaf.ContainsKey(
            "/subscriptions/abc/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sub"));
    }

    [Fact]
    public void Filter_PreservesAllowedScalarLeaf()
    {
        var props = new JsonObject
        {
            ["location"] = new JsonObject
            {
                ["eastus"] = 0.6,
                ["westus"] = 0.4,
            }
        };
        var input = Aggregation(("microsoft.test/widgets", props));

        var result = AggregationFilter.Filter(input);
        var location = (JsonObject)result.ResourceTypes["microsoft.test/widgets"]
            .PropertyAggregations["location"]!;

        Assert.Equal(2, location.Count);
        Assert.True(location.ContainsKey("eastus"));
        Assert.True(location.ContainsKey("westus"));
    }

    [Fact]
    public void Filter_PreservesScopeMetadata()
    {
        var input = new SubscriptionAggregation(
            new Dictionary<string, ResourceTypeAggregation>(),
            SubscriptionCount: 3,
            ResourceGroupCount: 11);

        var result = AggregationFilter.Filter(input);

        Assert.Equal(3, result.SubscriptionCount);
        Assert.Equal(11, result.ResourceGroupCount);
    }

    private static JsonObject LocationLeaf(string value) =>
        new() { ["location"] = new JsonObject { [value] = 1.0 } };

    private static SubscriptionAggregation Aggregation(params (string Type, JsonObject Properties)[] types)
    {
        var dict = new Dictionary<string, ResourceTypeAggregation>(StringComparer.Ordinal);
        foreach (var (type, properties) in types)
        {
            dict[type] = new ResourceTypeAggregation(type, TotalCount: 1, properties);
        }
        return new SubscriptionAggregation(dict, SubscriptionCount: 1, ResourceGroupCount: 1);
    }
}
