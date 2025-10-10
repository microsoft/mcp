// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Foundry.UnitTests;

public class ResourceGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;
    private readonly ILogger<ResourceGetCommand> _logger;

    public ResourceGetCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();
        _logger = Substitute.For<ILogger<ResourceGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);
        collection.AddSingleton(_logger);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = new ResourceGetCommand(_logger);

        Assert.Equal("get", command.Name);
        Assert.NotEmpty(command.Description);
        Assert.NotNull(command.Metadata);
        Assert.True(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.Destructive);
    }

    [Fact]
    public async Task ExecuteAsync_ListsAllResources_WhenNoResourceNameProvided()
    {
        var expectedResources = new List<AiResourceInformation>
        {
            new()
            {
                ResourceName = "resource1",
                ResourceGroup = "rg1",
                SubscriptionName = "sub1",
                Location = "eastus",
                Endpoint = "https://resource1.openai.azure.com/",
                Kind = "OpenAI",
                SkuName = "S0",
                Deployments = new List<DeploymentInformation>()
            },
            new()
            {
                ResourceName = "resource2",
                ResourceGroup = "rg1",
                SubscriptionName = "sub1",
                Location = "westus",
                Endpoint = "https://resource2.openai.azure.com/",
                Kind = "AIServices",
                SkuName = "S0",
                Deployments = new List<DeploymentInformation>()
            }
        };

        _foundryService.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResources);

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FoundryJsonContext.Default.ResourceGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Resources);
        Assert.Equal(expectedResources.Count, result.Resources.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ListsResourcesInResourceGroup_WhenResourceGroupProvided()
    {
        var expectedResources = new List<AiResourceInformation>
        {
            new()
            {
                ResourceName = "resource1",
                ResourceGroup = "test-rg",
                SubscriptionName = "sub1",
                Location = "eastus",
                Endpoint = "https://resource1.openai.azure.com/",
                Kind = "OpenAI",
                SkuName = "S0",
                Deployments = new List<DeploymentInformation>()
            }
        };

        _foundryService.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Is("test-rg"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResources);

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "test-sub", "--resource-group", "test-rg"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FoundryJsonContext.Default.ResourceGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Resources);
        Assert.Single(result.Resources);
        Assert.Equal("test-rg", result.Resources[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_GetsSpecificResource_WhenResourceNameAndGroupProvided()
    {
        var expectedResource = new AiResourceInformation
        {
            ResourceName = "test-resource",
            ResourceGroup = "test-rg",
            SubscriptionName = "sub1",
            Location = "eastus",
            Endpoint = "https://test-resource.openai.azure.com/",
            Kind = "OpenAI",
            SkuName = "S0",
            Deployments = new List<DeploymentInformation>
            {
                new()
                {
                    DeploymentName = "gpt-4o",
                    ModelName = "gpt-4o",
                    ModelVersion = "2024-11-20",
                    ModelFormat = "OpenAI",
                    SkuName = "Standard",
                    SkuCapacity = 100,
                    ProvisioningState = "Succeeded"
                }
            }
        };

        _foundryService.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Is("test-rg"),
            Arg.Is("test-resource"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResource);

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FoundryJsonContext.Default.ResourceGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Resources);
        Assert.Single(result.Resources);
        Assert.Equal("test-resource", result.Resources[0].ResourceName);
        Assert.Equal("test-rg", result.Resources[0].ResourceGroup);
        Assert.NotEmpty(result.Resources[0].Deployments!);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResourcesExist()
    {
        _foundryService.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FoundryJsonContext.Default.ResourceGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesListException()
    {
        var expectedError = "Failed to list resources";

        _foundryService.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGetException()
    {
        var expectedError = "Resource not found";

        _foundryService.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription", "test-sub")]
    [InlineData("--subscription", "test-sub", "--resource-group", "test-rg")]
    [InlineData("--subscription", "test-sub", "--resource-group", "test-rg", "--resource-name", "test-resource")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(params string[] args)
    {
        _foundryService.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns([]);

        _foundryService.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new AiResourceInformation());

        var command = new ResourceGetCommand(_logger);
        var parsedArgs = command.GetCommand().Parse(args);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, parsedArgs);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var resourceWithDeployments = new AiResourceInformation
        {
            ResourceName = "test-resource",
            ResourceGroup = "test-rg",
            SubscriptionName = "Test Subscription",
            Location = "eastus",
            Endpoint = "https://test-resource.openai.azure.com/",
            Kind = "OpenAI",
            SkuName = "S0",
            Deployments = new List<DeploymentInformation>
            {
                new()
                {
                    DeploymentName = "gpt-4o",
                    ModelName = "gpt-4o",
                    ModelVersion = "2024-11-20",
                    ModelFormat = "OpenAI",
                    SkuName = "GlobalStandard",
                    SkuCapacity = 450,
                    ScaleType = "Standard",
                    ProvisioningState = "Succeeded"
                },
                new()
                {
                    DeploymentName = "text-embedding-ada-002",
                    ModelName = "text-embedding-ada-002",
                    ModelVersion = "2",
                    ModelFormat = "OpenAI",
                    SkuName = "Standard",
                    SkuCapacity = 120,
                    ProvisioningState = "Succeeded"
                }
            }
        };

        _foundryService.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(resourceWithDeployments);

        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource"
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        // Serialize and deserialize to validate JSON context
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, FoundryJsonContext.Default.ResourceGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Resources);
        Assert.Single(result.Resources);

        var resource = result.Resources[0];
        Assert.Equal("test-resource", resource.ResourceName);
        Assert.Equal("test-rg", resource.ResourceGroup);
        Assert.Equal("Test Subscription", resource.SubscriptionName);
        Assert.Equal("eastus", resource.Location);
        Assert.Equal("https://test-resource.openai.azure.com/", resource.Endpoint);
        Assert.Equal("OpenAI", resource.Kind);
        Assert.Equal("S0", resource.SkuName);

        Assert.NotNull(resource.Deployments);
        Assert.Equal(2, resource.Deployments.Count);

        var firstDeployment = resource.Deployments[0];
        Assert.Equal("gpt-4o", firstDeployment.DeploymentName);
        Assert.Equal("gpt-4o", firstDeployment.ModelName);
        Assert.Equal("2024-11-20", firstDeployment.ModelVersion);
        Assert.Equal("OpenAI", firstDeployment.ModelFormat);
        Assert.Equal("GlobalStandard", firstDeployment.SkuName);
        Assert.Equal(450, firstDeployment.SkuCapacity);
        Assert.Equal("Succeeded", firstDeployment.ProvisioningState);

        var secondDeployment = resource.Deployments[1];
        Assert.Equal("text-embedding-ada-002", secondDeployment.DeploymentName);
        Assert.Equal("text-embedding-ada-002", secondDeployment.ModelName);
        Assert.Equal("2", secondDeployment.ModelVersion);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var command = new ResourceGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource",
            "--tenant", "test-tenant"
        ]);

        var context = new CommandContext(_serviceProvider);
        // We can't directly access BindOptions, but we can verify the command parses correctly
        Assert.Empty(args.Errors);
    }
}
