// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Nodes;
using Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

public class HealthModelGetCommandTests : SubscriptionCommandUnitTestsBase<HealthModelGetCommand, IMonitorHealthModelService>
{
    private const string TestSubscription = "sub123";
    private const string TestResourceGroup = "rg1";
    private const string TestHealthModel = "hm-one";

    [Fact]
    public async Task ExecuteAsync_ReturnsHealthModel_WhenItExists()
    {
        Service.GetHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(JsonNode.Parse("""{"name":"hm-one","type":"microsoft.cloudhealth/healthmodels","properties":{"provisioningState":"Succeeded"}}""")!);

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.JsonNode);
        Assert.Equal("hm-one", result!["name"]!.GetValue<string>());
        Assert.Equal("Succeeded", result["properties"]!["provisioningState"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("--subscription", TestSubscription)] // missing rg + health-model
    [InlineData("--subscription", TestSubscription, "--resource-group", TestResourceGroup)] // missing health-model
    public async Task ExecuteAsync_ReturnsBadRequest_WhenRequiredOptionsMissing(params string[] args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotFound_WhenModelMissing()
    {
        Service.GetHealthModel(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource not found"));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsInternalServerError_WhenServiceThrows()
    {
        Service.GetHealthModel(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("boom"));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
