// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models;
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

public class OpenAiModelsListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public OpenAiModelsListCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ListsModels_WhenValidOptionsProvided()
    {
        // Arrange
        var resourceName = "test-openai";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        var expectedModels = new List<OpenAiModelDeployment>
        {
            new OpenAiModelDeployment(
                DeploymentName: "gpt-4o",
                ModelName: "gpt-4o",
                ModelVersion: "2024-05-13",
                ScaleType: "Standard",
                Capacity: 30,
                ProvisioningState: "Succeeded",
                CreatedAt: DateTime.UtcNow.AddDays(-1),
                UpdatedAt: DateTime.UtcNow,
                Capabilities: new OpenAiModelCapabilities(true, false, true, false)
            ),
            new OpenAiModelDeployment(
                DeploymentName: "text-embedding-ada-002",
                ModelName: "text-embedding-ada-002",
                ModelVersion: "2",
                ScaleType: "Standard",
                Capacity: 120,
                ProvisioningState: "Succeeded",
                CreatedAt: DateTime.UtcNow.AddDays(-2),
                UpdatedAt: DateTime.UtcNow.AddHours(-1),
                Capabilities: new OpenAiModelCapabilities(false, true, false, false)
            )
        };

        var expectedResult = new OpenAiModelsListResult(expectedModels, resourceName);

        _foundryService.ListOpenAiModelsAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        // Act
        var command = new OpenAiModelsListCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<OpenAiModelsListCommandResult>(json);

        Assert.NotNull(result);
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

        var expectedResult = new OpenAiModelsListResult(new List<OpenAiModelDeployment>(), resourceName);

        _foundryService.ListOpenAiModelsAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        // Act
        var command = new OpenAiModelsListCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<OpenAiModelsListCommandResult>(json);

        Assert.NotNull(result);
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

        _foundryService.ListOpenAiModelsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var command = new OpenAiModelsListCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, (int)response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Command_HasCorrectName()
    {
        // Arrange & Act
        var command = new OpenAiModelsListCommand();

        // Assert
        Assert.Equal("models-list", command.Name);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        // Arrange & Act
        var command = new OpenAiModelsListCommand();

        // Assert
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Idempotent);
    }

    [Theory]
    [InlineData("--resource-name myresource --subscription sub --resource-group rg", true)]
    [InlineData("--subscription sub --resource-group rg", false)] // Missing resource-name
    [InlineData("--resource-name myresource --subscription sub", false)] // Missing resource-group  
    [InlineData("--resource-name myresource --resource-group rg", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        var command = new OpenAiModelsListCommand();
        var context = new CommandContext(_serviceProvider);

        var expectedResult = new OpenAiModelsListResult(new List<OpenAiModelDeployment>(), "myresource");

        if (shouldSucceed)
        {
            _foundryService.ListOpenAiModelsAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                    Arg.Any<RetryPolicyOptions?>())
                .Returns(expectedResult);
        }

        var parseResult = command.GetCommand().Parse(args.Split(' '));

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

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

        var expectedResult = new OpenAiModelsListResult(new List<OpenAiModelDeployment>(), resourceName);

        _foundryService.ListOpenAiModelsAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        var command = new OpenAiModelsListCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName
        ]);
        var context = new CommandContext(_serviceProvider);

        // Act
        await command.ExecuteAsync(context, args);

        // Assert - Verify the service was called with exact parameters
        await _foundryService.Received(1).ListOpenAiModelsAsync(
            resourceName,
            subscriptionId,
            resourceGroup,
            Arg.Any<string?>(),
            AuthMethod.Credential,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceAuthentication_Exception()
    {
        // Arrange
        var resourceName = "test-openai";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        _foundryService.ListOpenAiModelsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("Authentication failed"));

        // Act
        var command = new OpenAiModelsListCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(200, (int)response.Status);
        Assert.Contains("Authentication failed", response.Message);
    }

    private class OpenAiModelsListCommandResult
    {
        [JsonPropertyName("modelsListResult")]
        public OpenAiModelsListResult ModelsListResult { get; set; } = new OpenAiModelsListResult(new List<OpenAiModelDeployment>(), "");

        [JsonPropertyName("resourceName")]
        public string ResourceName { get; set; } = string.Empty;
    }
}
