// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.UnitTests;

public class OpenAiModelsListCommandTests : CommandUnitTestsBase<OpenAiModelsListCommand, IFoundryExtensionsService>
{
    [Fact]
    public async Task ExecuteAsync_ListsModels_WhenValidOptionsProvided()
    {
        // Arrange
        var resourceName = "test-openai";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        var expectedModels = new List<OpenAiModelDeployment>
        {
            new(
                DeploymentName: "gpt-4o",
                ModelName: "gpt-4o",
                ModelVersion: "2024-05-13",
                ScaleType: "Standard",
                Capacity: 30,
                ProvisioningState: "Succeeded",
                CreatedAt: DateTime.UtcNow.AddDays(-1),
                UpdatedAt: DateTime.UtcNow,
                Capabilities: new(true, false, true, false)),
            new(
                DeploymentName: "text-embedding-ada-002",
                ModelName: "text-embedding-ada-002",
                ModelVersion: "2",
                ScaleType: "Standard",
                Capacity: 120,
                ProvisioningState: "Succeeded",
                CreatedAt: DateTime.UtcNow.AddDays(-2),
                UpdatedAt: DateTime.UtcNow.AddHours(-1),
                Capabilities: new(false, true, false, false))
        };

        Service.ListOpenAiModelsAsync(
            Arg.Is(resourceName),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new OpenAiModelsListResult(expectedModels, resourceName));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.OpenAiModelsListCommandResult);

        Assert.Equal(resourceName, result.ResourceName);
        Assert.Equal(resourceName, result.ModelsListResult.ResourceName);
        Assert.Equal(2, result.ModelsListResult.Models.Count);

        // Verify GPT model
        var gptModel = result.ModelsListResult.Models.First(m => m.ModelName == "gpt-4o");
        Assert.Equal("gpt-4o", gptModel.DeploymentName);
        Assert.Equal(30, gptModel.Capacity);
        Assert.True(gptModel.Capabilities?.ChatCompletions);
        Assert.False(gptModel.Capabilities?.Embeddings);

        // Verify embedding model
        var embeddingModel = result.ModelsListResult.Models.First(m => m.ModelName == "text-embedding-ada-002");
        Assert.Equal("text-embedding-ada-002", embeddingModel.DeploymentName);
        Assert.Equal(120, embeddingModel.Capacity);
        Assert.True(embeddingModel.Capabilities?.Embeddings);
        Assert.False(embeddingModel.Capabilities?.ChatCompletions);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoModelsDeployed()
    {
        // Arrange
        var resourceName = "test-openai-empty";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        Service.ListOpenAiModelsAsync(
            Arg.Is(resourceName),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new OpenAiModelsListResult([], resourceName));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.OpenAiModelsListCommandResult);

        Assert.Equal(resourceName, result.ResourceName);
        Assert.Empty(result.ModelsListResult.Models);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var resourceName = "test-openai";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";
        var expectedError = "Test models list error";

        Service.ListOpenAiModelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, (int)response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Command_HasCorrectName()
    {
        Assert.Equal("models-list", Command.Name);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Theory]
    [InlineData("--resource-name myresource --subscription sub --resource-group rg", true)]
    [InlineData("--subscription sub --resource-group rg", false)] // Missing resource-name
    [InlineData("--resource-name myresource --subscription sub", false)] // Missing resource-group
    [InlineData("--resource-name myresource --resource-group rg", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.ListOpenAiModelsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Is(AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new OpenAiModelsListResult([], "myresource"));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
        else
        {
            // Should fail validation for missing required parameters
            Assert.NotEqual(200, (int)response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesServiceCall_WithCorrectParameters()
    {
        // Arrange
        var resourceName = "test-resource";
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";

        Service.ListOpenAiModelsAsync(
            Arg.Is(resourceName),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new OpenAiModelsListResult([], resourceName));

        // Act
        await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName);

        // Assert - Verify the service was called with exact parameters
        await Service.Received(1).ListOpenAiModelsAsync(
            resourceName,
            subscriptionId,
            resourceGroup,
            Arg.Any<string?>(),
            AuthMethod.Credential,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceAuthentication_Exception()
    {
        // Arrange
        var resourceName = "test-openai";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        Service.ListOpenAiModelsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Authentication failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(200, (int)response.Status);
        Assert.Contains("Authentication failed", response.Message);
    }
}
