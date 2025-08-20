// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class ModelDeploymentCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public ModelDeploymentCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_DeploysModel_WhenValidOptionsProvided()
    {
        var deploymentName = "test-deployment";
        var modelName = "test-model";
        var modelFormat = "OpenAI";
        var aiServicesName = "test-ai-services";
        var resourceGroup = "test-resource-group";
        var subscriptionId = "test-subscription-id";

        var expectedResponse = new ModelDeploymentResult
        {
            HasData = true
        };

        _foundryService.DeployModel(
                Arg.Is<string>(s => s == deploymentName),
                Arg.Is<string>(s => s == modelName),
                Arg.Is<string>(s => s == modelFormat),
                Arg.Is<string>(s => s == aiServicesName),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResponse);

        var command = new ModelDeploymentCommand();
        var args = command.GetCommand().Parse(["--deployment", deploymentName, "--model-name", modelName, "--model-format", modelFormat, "--azure-ai-services", aiServicesName, "--resource-group", resourceGroup, "--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_OptionalParameters_PassedToService()
    {
        var deploymentName = "test-deployment";
        var modelName = "test-model";
        var modelFormat = "OpenAI";
        var aiServicesName = "test-ai-services";
        var resourceGroup = "test-resource-group";
        var subscriptionId = "test-subscription-id";
        var modelVersion = "1.0";
        var modelSource = "AzureOpenAI";
        var skuName = "Standard";
        var skuCapacity = 1;
        var scaleType = "Standard";
        var scaleCapacity = 2;

        var expectedResponse = new ModelDeploymentResult
        {
            HasData = true
        };

        _foundryService.DeployModel(
                Arg.Is<string>(s => s == deploymentName),
                Arg.Is<string>(s => s == modelName),
                Arg.Is<string>(s => s == modelFormat),
                Arg.Is<string>(s => s == aiServicesName),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string?>(s => s == modelVersion),
                Arg.Is<string?>(s => s == modelSource),
                Arg.Is<string?>(s => s == skuName),
                Arg.Is<int?>(s => s == skuCapacity),
                Arg.Is<string?>(s => s == scaleType),
                Arg.Is<int?>(s => s == scaleCapacity),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResponse);

        var command = new ModelDeploymentCommand();
        var args = command.GetCommand().Parse(["--deployment", deploymentName, "--model-name", modelName, "--model-format", modelFormat, "--azure-ai-services", aiServicesName, "--resource-group", resourceGroup, "--subscription", subscriptionId, "--model-version", modelVersion, "--model-source", modelSource, "--sku", skuName, "--sku-capacity", skuCapacity.ToString(), "--scale-type", scaleType, "--scale-capacity", scaleCapacity.ToString()]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var deploymentName = "test-deployment";
        var modelName = "test-model";
        var modelFormat = "OpenAI";
        var aiServicesName = "test-ai-services";
        var resourceGroup = "test-resource-group";
        var subscriptionId = "test-subscription-id";
        var expectedError = "Test error";

        _foundryService.DeployModel(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new ModelDeploymentCommand();
        var args = command.GetCommand().Parse(["--deployment", deploymentName, "--model-name", modelName, "--model-format", modelFormat, "--azure-ai-services", aiServicesName, "--resource-group", resourceGroup, "--subscription", subscriptionId]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

}
