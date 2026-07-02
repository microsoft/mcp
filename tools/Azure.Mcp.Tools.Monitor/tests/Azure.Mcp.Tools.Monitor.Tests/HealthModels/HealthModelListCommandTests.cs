// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

public class HealthModelListCommandTests : SubscriptionCommandUnitTestsBase<HealthModelListCommand, IMonitorHealthModelService>
{
    private const string TestSubscription = "sub123";
    private const string TestResourceGroup = "rg1";

    [Fact]
    public async Task ExecuteAsync_ReturnsLeanSummaries_WhenTheyExist()
    {
        List<HealthModelSummary> summaries =
        [
            new() { Id = "/subscriptions/sub123/resourceGroups/rg1/providers/Microsoft.CloudHealth/healthmodels/hm-one", Name = "hm-one", ResourceGroup = "rg1", Location = "eastus2", ProvisioningState = "Succeeded" },
            new() { Id = "/subscriptions/sub123/resourceGroups/rg2/providers/Microsoft.CloudHealth/healthmodels/hm-two", Name = "hm-two", ResourceGroup = "rg2", Location = "westus2", ProvisioningState = "Provisioning" },
        ];
        Service.ListHealthModels(TestSubscription, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(summaries);

        var response = await ExecuteCommandAsync("--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.ListHealthModelSummary);
        Assert.Equal(2, result.Count);
        Assert.Equal("hm-one", result[0].Name);
        Assert.Equal("rg1", result[0].ResourceGroup);
        Assert.Equal("eastus2", result[0].Location);
        Assert.Equal("Succeeded", result[0].ProvisioningState);
        Assert.Equal("Provisioning", result[1].ProvisioningState);

        // Lean by construction: each serialized item carries ONLY the summary keys (no ARM envelope).
        var json = System.Text.Json.JsonSerializer.Serialize(result, MonitorJsonContext.Default.ListHealthModelSummary);
        var array = System.Text.Json.Nodes.JsonNode.Parse(json)!.AsArray();
        foreach (var item in array)
        {
            var keys = ((System.Text.Json.Nodes.JsonObject)item!).Select(kv => kv.Key).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            Assert.Equal(["id", "location", "name", "provisioningState", "resourceGroup"], keys);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsResourceGroup_WhenProvided()
    {
        Service.ListHealthModels(Arg.Any<string>(), Arg.Is(TestResourceGroup), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListHealthModels(Arg.Any<string>(), Arg.Is(TestResourceGroup), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
