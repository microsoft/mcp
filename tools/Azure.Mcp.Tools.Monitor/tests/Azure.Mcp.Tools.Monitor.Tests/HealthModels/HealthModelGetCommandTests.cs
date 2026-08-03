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

public class HealthModelGetCommandTests : SubscriptionCommandUnitTestsBase<HealthModelGetCommand, IMonitorHealthModelService>
{
    private const string TestSubscription = "sub123";
    private const string TestResourceGroup = "rg1";
    private const string TestHealthModel = "hm-one";

    [Fact]
    public async Task ExecuteAsync_ReturnsHealthModel_WhenItExists()
    {
        Service.GetHealthModel(TestSubscription, TestResourceGroup, TestHealthModel, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
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
}
