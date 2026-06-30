// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

public class HealthModelListCommandTests : SubscriptionCommandUnitTestsBase<HealthModelListCommand, IMonitorHealthModelService>
{
    private const string TestSubscription = "sub123";
    private const string TestResourceGroup = "rg1";

    [Fact]
    public async Task ExecuteAsync_ReturnsHealthModels_WhenTheyExist()
    {
        List<JsonNode> models =
        [
            JsonNode.Parse("""{"name":"hm-one","location":"eastus2"}""")!,
            JsonNode.Parse("""{"name":"hm-two","location":"westus2"}""")!,
        ];
        Service.ListHealthModels(TestSubscription, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(models);

        var response = await ExecuteCommandAsync("--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.ListJsonNode);
        Assert.Equal(2, result.Count);
        Assert.Equal("hm-one", result[0]!["name"]!.GetValue<string>());
        Assert.Equal("hm-two", result[1]!["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoneExist()
    {
        Service.ListHealthModels(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.ListJsonNode);
        Assert.Empty(result);
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

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenSubscriptionMissing()
    {
        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInternalServerError_WhenServiceThrows()
    {
        Service.ListHealthModels(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
