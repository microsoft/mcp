// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.FoundryExtensions.Tests;

public class OpenAiEmbeddingsCreateCommandTests : SubscriptionCommandUnitTestsBase<OpenAiEmbeddingsCreateCommand, IFoundryExtensionsService>
{
    [Fact]
    public async Task ExecuteAsync_CreatesEmbeddings_WhenValidOptionsProvided()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "text-embedding-ada-002";
        var inputText = "Hello world";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        var expectedResult = new EmbeddingResult("list", [new("embedding", 0, [0.1f, 0.2f, 0.3f, 0.4f, 0.5f])], deploymentName, new(2, 2));

        Service.CreateEmbeddingsAsync(
            Arg.Is(resourceName),
            Arg.Is(deploymentName),
            Arg.Is(inputText),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--input-text", inputText);

        // Assert
        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.OpenAiEmbeddingsCreateCommandResult);

        Assert.Equal(expectedResult.Object, result.EmbeddingResult.Object);
        Assert.Equal(expectedResult.Model, result.EmbeddingResult.Model);
        Assert.Equal(resourceName, result.ResourceName);
        Assert.Equal(deploymentName, result.DeploymentName);
        Assert.Equal(inputText, result.InputText);
        Assert.Single(result.EmbeddingResult.Data);
        Assert.Equal(expectedResult.Data[0].Embedding.Length, result.EmbeddingResult.Data[0].Embedding.Length);
    }

    [Fact]
    public void Command_DoesNotExposeUnsupportedOptions()
    {
        var options = Command.GetCommand().Options;

        Assert.DoesNotContain(options, option => option.Name == "--user");
        Assert.DoesNotContain(options, option => option.Name == "--encoding-format");
        Assert.DoesNotContain(options, option => option.Name == "--dimensions");
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

        Service.CreateEmbeddingsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--input-text", inputText);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, (int)response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Command_HasCorrectName()
    {
        Assert.Equal("embeddings-create", Command.Name);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
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

        // Act
        var response = await ExecuteCommandAsync(parseArgs.ToArray());

        // Assert
        Assert.NotNull(response);
        // The command should handle validation and return appropriate error
        if (string.IsNullOrEmpty(inputText))
        {
            Assert.NotEqual(200, (int)response.Status);
        }
    }
}
