// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.Mcp.Tools.Monitor.Services;
using NSubstitute;
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
        Service.GetHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new HealthModelDetail
            {
                Id = null,
                Name = "hm-one",
                ResourceGroup = "rg1",
                Location = "eastus2",
                ProvisioningState = "Succeeded",
                HealthState = "Healthy",
                Tags = null,
            });

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.HealthModelGetCommandResult);
        Assert.Equal("hm-one", result!.HealthModel.Name);
        Assert.Equal("Succeeded", result.HealthModel.ProvisioningState);
        Assert.Equal("Healthy", result.HealthModel.HealthState);
    }

    [Theory]
    [InlineData(404, "ResourceGroupNotFound", "Resource group not found.")]
    [InlineData(404, "ResourceNotFound", "Health model not found.")]
    [InlineData(403, "AuthorizationFailed", "Authorization failed reading the health model. Details: Forbidden")]
    public async Task ExecuteAsync_MapsServiceFailures(int status, string errorCode, string message)
    {
        Service.GetHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<HealthModelDetail>(new RequestFailedException(status, "Forbidden\nHeaders:\nx-ms-request-id: 1", errorCode, null)));

        var response = await ExecuteCommandAsync("--subscription", TestSubscription, "--resource-group", TestResourceGroup, "--health-model", TestHealthModel);

        Assert.Equal((HttpStatusCode)status, response.Status);
        Assert.StartsWith(message, response.Message);
        Assert.DoesNotContain("x-ms-request-id", response.Message);
    }
}
