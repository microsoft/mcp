// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.Insights.Services;
using Xunit;

namespace Azure.Mcp.Tools.Insights.UnitTests.Services;

public class PropertyAggregatorTests
{
    [Fact]
    public void Aggregate_GroupsRowsByType_LowerCasesTypeKey()
    {
        var rows = ParseRows("""
            [
              { "type": "Microsoft.Storage/StorageAccounts", "location": "eastus" },
              { "type": "microsoft.storage/storageaccounts", "location": "eastus" }
            ]
            """);

        var result = PropertyAggregator.Aggregate(rows);

        var entry = Assert.Single(result.ResourceTypes);
        Assert.Equal("microsoft.storage/storageaccounts", entry.Key);
        Assert.Equal(2, entry.Value.TotalCount);
    }

    [Fact]
    public void Aggregate_CountsResourceGroupsCaseInsensitive()
    {
        var rows = ParseRows("""
            [
              { "type": "microsoft.storage/storageaccounts", "resourceGroup": "RG1" },
              { "type": "microsoft.storage/storageaccounts", "resourceGroup": "rg1" },
              { "type": "microsoft.storage/storageaccounts", "resourceGroup": "rg2" }
            ]
            """);

        var result = PropertyAggregator.Aggregate(rows);

        Assert.Equal(2, result.ResourceGroupCount);
    }

    [Fact]
    public void Aggregate_WalksTagsSkuIdentityAndProperties()
    {
        var rows = ParseRows("""
            [
              {
                "type": "microsoft.storage/storageaccounts",
                "sku": { "name": "Standard_LRS" },
                "identity": { "type": "SystemAssigned" },
                "tags": { "env": "prod" },
                "properties": { "minimumTlsVersion": "TLS1_2" }
              }
            ]
            """);

        var result = PropertyAggregator.Aggregate(rows);
        var props = result.ResourceTypes["microsoft.storage/storageaccounts"].PropertyAggregations;

        Assert.NotNull(props["sku"]);
        Assert.NotNull(props["identity"]);
        Assert.NotNull(props["tags"]);
        Assert.NotNull(props["properties"]);

        var tls = props["properties"]!["minimumtlsversion"] as JsonObject;
        Assert.NotNull(tls);
        Assert.Equal(1.0, tls!["TLS1_2"]!.GetValue<double>(), precision: 6);
    }

    [Fact]
    public void Aggregate_RespectsMaxDepthCap()
    {
        // Nested beyond MaxPropertyDepth should not be emitted.
        var rows = ParseRows("""
            [
              {
                "type": "microsoft.test/widgets",
                "properties": { "a": { "b": { "c": { "d": { "e": { "tooDeep": "x" } } } } } }
              }
            ]
            """);

        var result = PropertyAggregator.Aggregate(rows);
        var props = result.ResourceTypes["microsoft.test/widgets"].PropertyAggregations;

        Assert.DoesNotContain("tooDeep", props.ToJsonString());
    }

    [Fact]
    public void Aggregate_TopValuesPerLeaf_LimitsToThree()
    {
        var rows = ParseRows("""
            [
              { "type": "microsoft.test/widgets", "location": "eastus" },
              { "type": "microsoft.test/widgets", "location": "eastus" },
              { "type": "microsoft.test/widgets", "location": "eastus" },
              { "type": "microsoft.test/widgets", "location": "westus" },
              { "type": "microsoft.test/widgets", "location": "centralus" },
              { "type": "microsoft.test/widgets", "location": "northeurope" }
            ]
            """);

        var result = PropertyAggregator.Aggregate(rows);
        var location = result.ResourceTypes["microsoft.test/widgets"].PropertyAggregations["location"] as JsonObject;

        Assert.NotNull(location);
        Assert.Equal(PropertyAggregator.TopValuesPerLeaf, location!.Count);
        Assert.True(location.ContainsKey("eastus"));
    }

    [Fact]
    public void Aggregate_IgnoresNonObjectRowsAndRowsMissingType()
    {
        var rows = ParseRows("""
            [
              "not-an-object",
              { "name": "missing-type" },
              { "type": "microsoft.test/widgets", "location": "eastus" }
            ]
            """);

        var result = PropertyAggregator.Aggregate(rows);

        var entry = Assert.Single(result.ResourceTypes);
        Assert.Equal(1, entry.Value.TotalCount);
    }

    [Fact]
    public void Aggregate_RawNamePreserved()
    {
        var rows = ParseRows("""
            [ { "type": "microsoft.test/widgets", "name": "MyResource-001" } ]
            """);

        var result = PropertyAggregator.Aggregate(rows);
        var name = result.ResourceTypes["microsoft.test/widgets"].PropertyAggregations["name"] as JsonObject;

        Assert.NotNull(name);
        Assert.True(name!.ContainsKey("MyResource-001"));
    }

    [Fact]
    public void Aggregate_PassesThroughSubscriptionCount()
    {
        var rows = ParseRows("""[ { "type": "microsoft.test/widgets" } ]""");

        var result = PropertyAggregator.Aggregate(rows, subscriptionCount: 7);

        Assert.Equal(7, result.SubscriptionCount);
    }

    private static List<JsonElement> ParseRows(string json)
    {
        var doc = JsonDocument.Parse(json);
        return [.. doc.RootElement.EnumerateArray()];
    }
}
