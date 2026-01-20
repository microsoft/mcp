// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Quota.LiveTests;

public sealed class QuotaCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    /// <summary>
    /// Disable the default sanitizer that redacts all JSON properties named "name". We need this
    /// to stick around so that we can verify that the expected resource names are present in the responses.
    /// Names are not unique and can be shared publicly, so there is no security risk in leaving them in the recordings.
    /// AZSDK3493 = $..name
    /// </summary>
    public override List<string> DisabledDefaultSanitizers =>
    [
        ..base.DisabledDefaultSanitizers,
        "AZSDK3493"
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers => [
        .. base.BodyKeySanitizers,
        // the default sanitizers take care of everything for the most part, but roleDefinitionId
        // is one value that ISN'T cleaned out by the default sanitizers. No reason to share these ids in recordings.
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..roleDefinitionId")
        {
            Value = "00000000-0000-0000-0000-000000000000"
        }),
    ];

    [Fact]
    public async Task Should_check_azure_quota()
    {
        // act
        var resourceTypes = "Microsoft.CognitiveServices, Microsoft.Compute, Microsoft.Storage, Microsoft.App, Microsoft.Network, Microsoft.MachineLearningServices, Microsoft.DBforPostgreSQL, Microsoft.HDInsight, Microsoft.Search, Microsoft.ContainerInstance";
        JsonElement? result = await CallToolAsync(
            "quota_usage_check",
            new() {
                { "subscription", Settings.SubscriptionId },
                { "region", "eastus" },
                { "resource-types", resourceTypes }
            });
        // assert
        var quotas = result.AssertProperty("usageInfo");
        Assert.Equal(JsonValueKind.Object, quotas.ValueKind);
        var appQuotas = quotas.AssertProperty("Microsoft.App");
        Assert.Equal(JsonValueKind.Array, appQuotas.ValueKind);
        Assert.NotEmpty(appQuotas.EnumerateArray());
        var storageQuotas = quotas.AssertProperty("Microsoft.Storage");
        Assert.Equal(JsonValueKind.Array, storageQuotas.ValueKind);
        Assert.NotEmpty(storageQuotas.EnumerateArray());
        var cognitiveQuotas = quotas.AssertProperty("Microsoft.CognitiveServices");
        Assert.Equal(JsonValueKind.Array, cognitiveQuotas.ValueKind);
        Assert.NotEmpty(cognitiveQuotas.EnumerateArray());
        var computeQuotas = quotas.AssertProperty("Microsoft.Compute");
        Assert.Equal(JsonValueKind.Array, computeQuotas.ValueKind);
        Assert.NotEmpty(computeQuotas.EnumerateArray());
        var networkQuotas = quotas.AssertProperty("Microsoft.Network");
        Assert.Equal(JsonValueKind.Array, networkQuotas.ValueKind);
        Assert.NotEmpty(networkQuotas.EnumerateArray());
        var mlQuotas = quotas.AssertProperty("Microsoft.MachineLearningServices");
        Assert.Equal(JsonValueKind.Array, mlQuotas.ValueKind);
        Assert.NotEmpty(mlQuotas.EnumerateArray());
        var postgresqlQuotas = quotas.AssertProperty("Microsoft.DBforPostgreSQL");
        Assert.Equal(JsonValueKind.Array, postgresqlQuotas.ValueKind);
        Assert.NotEmpty(postgresqlQuotas.EnumerateArray());
        var hdInsightQuotas = quotas.AssertProperty("Microsoft.HDInsight");
        Assert.Equal(JsonValueKind.Array, hdInsightQuotas.ValueKind);
        Assert.NotEmpty(hdInsightQuotas.EnumerateArray());
        var searchQuotas = quotas.AssertProperty("Microsoft.Search");
        Assert.Equal(JsonValueKind.Array, searchQuotas.ValueKind);
        Assert.NotEmpty(searchQuotas.EnumerateArray());
        var containerInstanceQuotas = quotas.AssertProperty("Microsoft.ContainerInstance");
        Assert.Equal(JsonValueKind.Array, containerInstanceQuotas.ValueKind);
        Assert.NotEmpty(containerInstanceQuotas.EnumerateArray());

    }

    [Fact]
    public async Task Should_check_azure_regions()
    {
        // act
        var result = await CallToolAsync(
            "quota_region_availability_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-types", "Microsoft.Web/sites, Microsoft.Storage/storageAccounts, microsoft.dbforpostgresql/flexibleServers" },
            });

        // assert
        var availableRegions = result.AssertProperty("availableRegions");
        Assert.Equal(JsonValueKind.Array, availableRegions.ValueKind);
        Assert.NotEmpty(availableRegions.EnumerateArray());
        var actualRegions = availableRegions.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToHashSet();
        // only available for test subscription
        // var expectedRegions = new HashSet<string>
        // {
        //     "southafricanorth","westus","australiaeast","brazilsouth","southeastasia",
        //     "centralus","japanwest","centralindia","uksouth","koreacentral","francecentral",
        //     "northeurope","australiacentral","uaenorth","swedencentral","switzerlandnorth",
        //     "northcentralus","ukwest","australiasoutheast","koreasouth","canadacentral",
        //     "westeurope","southindia","westcentralus","westus3","eastasia","japaneast",
        //     "jioindiawest","polandcentral","italynorth","israelcentral","spaincentral",
        //     "mexicocentral","newzealandnorth","indonesiacentral","malaysiawest","chilecentral",
        //     "eastus2euap","norwaywest","norwayeast","germanynorth","brazilsoutheast",
        //     "swedensouth","switzerlandwest"
        // };
        // Assert.Equal(expectedRegions, actualRegions);
    }

    [Fact]
    public async Task Should_check_regions_with_cognitive_services()
    {
        // act
        var result = await CallToolAsync(
            "quota_region_availability_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-types", "Microsoft.CognitiveServices/accounts" },
                { "cognitive-service-model-name", "gpt-4o" },
                { "cognitive-service-deployment-sku-name", "Standard" }
            });

        // assert
        var availableRegions = result.AssertProperty("availableRegions");
        Assert.Equal(JsonValueKind.Array, availableRegions.ValueKind);
        Assert.NotEmpty(availableRegions.EnumerateArray());
        var actualRegions = availableRegions.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToHashSet();

        // only available for test subscription
        // var expectedRegions = new HashSet<string>
        // {
        //     "australiaeast", "westus", "southcentralus", "eastus", "eastus2",
        //     "japaneast", "uksouth", "francecentral", "northcentralus",
        //     "swedencentral", "switzerlandnorth", "norwayeast", "westus3",
        //     "canadaeast", "southindia"
        // };
        // Assert.Equal(expectedRegions, actualRegions);
    }
}
