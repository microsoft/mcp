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

public class OpenAiEmbeddingsCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public OpenAiEmbeddingsCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_CreatesEmbeddings_WhenValidOptionsProvided()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "text-embedding-ada-002";
        var inputText = "Hello world";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        var expectedEmbedding = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };
        var expectedUsage = new EmbeddingUsageInfo(2, 2);
        var expectedData = new List<EmbeddingData>
        {
            new EmbeddingData("embedding", 0, expectedEmbedding)
        };
        var expectedResult = new EmbeddingResult("list", expectedData, deploymentName, expectedUsage);

        _foundryService.CreateEmbeddingsAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == deploymentName),
                Arg.Is<string>(s => s == inputText),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Any<string?>(),
                Arg.Is<string>(s => s == "float"),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        // Act
        var command = new OpenAiEmbeddingsCreateCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--input-text", inputText
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<OpenAiEmbeddingsCreateCommandResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedResult.Object, result.EmbeddingResult.Object);
        Assert.Equal(expectedResult.Model, result.EmbeddingResult.Model);
        Assert.Equal(resourceName, result.ResourceName);
        Assert.Equal(deploymentName, result.DeploymentName);
        Assert.Equal(inputText, result.InputText);
        Assert.Single(result.EmbeddingResult.Data);
        Assert.Equal(expectedEmbedding.Length, result.EmbeddingResult.Data[0].Embedding.Length);
    }

    [Fact]
    public async Task ExecuteAsync_OptionalParameters_PassedToService()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "text-embedding-ada-002";
        var inputText = "Test embedding text";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";
        var user = "test-user";
        var dimensions = 1536;

        var expectedEmbedding = new float[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            expectedEmbedding[i] = 0.1f;
        }

        var expectedUsage = new EmbeddingUsageInfo(4, 4);
        var expectedData = new List<EmbeddingData>
        {
            new EmbeddingData("embedding", 0, expectedEmbedding)
        };
        var expectedResult = new EmbeddingResult("list", expectedData, deploymentName, expectedUsage);

        _foundryService.CreateEmbeddingsAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == deploymentName),
                Arg.Is<string>(s => s == inputText),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Is<string>(s => s == user),
                Arg.Is<string>(s => s == "float"),
                Arg.Is<int?>(i => i == dimensions),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        // Act
        var command = new OpenAiEmbeddingsCreateCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--input-text", inputText,
            "--user", user,
            "--dimensions", dimensions.ToString()
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        // Verify the service was called at least once with the core parameters
        await _foundryService.Received(1).CreateEmbeddingsAsync(
            resourceName,
            deploymentName,
            inputText,
            subscriptionId,
            resourceGroup,
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "text-embedding-ada-002";
        var inputText = "Test input";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";
        var expectedError = "Test embedding error";

        _foundryService.CreateEmbeddingsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var command = new OpenAiEmbeddingsCreateCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--input-text", inputText
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
        var command = new OpenAiEmbeddingsCreateCommand();

        // Assert
        Assert.Equal("embeddings-create", command.Name);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        // Arrange & Act
        var command = new OpenAiEmbeddingsCreateCommand();

        // Assert
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Idempotent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ExecuteAsync_RequiredParameterMissing_ReturnsValidationError(string? inputText)
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "text-embedding-ada-002";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        var command = new OpenAiEmbeddingsCreateCommand();
        var parseArgs = new List<string>
        {
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName
        };

        if (!string.IsNullOrEmpty(inputText))
        {
            parseArgs.AddRange(["--input-text", inputText]);
        }

        var args = command.GetCommand().Parse(parseArgs.ToArray());
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        // The command should handle validation and return appropriate error
        if (string.IsNullOrEmpty(inputText))
        {
            Assert.NotEqual(200, (int)response.Status);
        }
    }

    private class OpenAiEmbeddingsCreateCommandResult
    {
        [JsonPropertyName("embeddingResult")]
        public EmbeddingResult EmbeddingResult { get; set; } = new EmbeddingResult("", new List<EmbeddingData>(), "", new EmbeddingUsageInfo(0, 0));

        [JsonPropertyName("resourceName")]
        public string ResourceName { get; set; } = string.Empty;

        [JsonPropertyName("deploymentName")]
        public string DeploymentName { get; set; } = string.Empty;

        [JsonPropertyName("inputText")]
        public string InputText { get; set; } = string.Empty;
    }
}
