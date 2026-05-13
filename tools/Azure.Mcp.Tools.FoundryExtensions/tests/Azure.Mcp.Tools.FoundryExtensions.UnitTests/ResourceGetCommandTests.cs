// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class ResourceGetCommandTests : CommandUnitTestsBase<ResourceGetCommand, IFoundryExtensionsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotNull(Command.Metadata);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
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
                Deployments = []
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
                Deployments = []
            }
        };

        Service.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResources);

        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);

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
                Deployments = []
            }
        };

        Service.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Is("test-rg"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResources);

        var response = await ExecuteCommandAsync("--subscription", "test-sub", "--resource-group", "test-rg");

        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);

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
            Deployments =
            [
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
            ]
        };

        Service.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Is("test-rg"),
            Arg.Is("test-resource"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResource);

        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource");

        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);

        Assert.NotNull(result.Resources);
        Assert.Single(result.Resources);
        Assert.Equal("test-resource", result.Resources[0].ResourceName);
        Assert.Equal("test-rg", result.Resources[0].ResourceGroup);
        Assert.NotEmpty(result.Resources[0].Deployments!);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResourcesExist()
    {
        Service.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);
        Assert.Empty(result.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesListException()
    {
        var expectedError = "Failed to list resources";

        Service.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGetException()
    {
        var expectedError = "Resource not found";

        Service.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource");

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
        Service.ListAiResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        Service.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new AiResourceInformation());

        var response = await ExecuteCommandAsync(args);

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
            Deployments =
            [
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
            ]
        };

        Service.GetAiResourceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(resourceWithDeployments);

        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource");

        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.ResourceGetCommandResult);

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
        var args = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--resource-name", "test-resource",
            "--tenant", "test-tenant"
        ]);

        // We can't directly access BindOptions, but we can verify the command parses correctly
        Assert.Empty(args.Errors);
    }
}
