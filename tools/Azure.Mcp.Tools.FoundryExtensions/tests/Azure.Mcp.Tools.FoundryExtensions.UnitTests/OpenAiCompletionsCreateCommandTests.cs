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

public class OpenAiCompletionsCreateCommandTests : CommandUnitTestsBase<OpenAiCompletionsCreateCommand, IFoundryExtensionsService>
{
    [Fact]
    public async Task ExecuteAsync_CreatesCompletion_WhenValidOptionsProvided()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "gpt-35-turbo";
        var promptText = "What is Azure?";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";

        var expectedUsage = new CompletionUsageInfo(10, 50, 60);
        var expectedResult = new CompletionResult("Azure is a cloud computing platform...", expectedUsage);

        Service.CreateCompletionAsync(
            Arg.Is(resourceName),
            Arg.Is(deploymentName),
            Arg.Is(promptText),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--prompt-text", promptText);

        // Assert
        var result = ValidateAndDeserializeResponse(response, FoundryExtensionsJsonContext.Default.OpenAiCompletionsCreateCommandResult);

        Assert.Equal(expectedResult.CompletionText, result.CompletionText);
        Assert.Equal(expectedUsage.PromptTokens, result.UsageInfo.PromptTokens);
        Assert.Equal(expectedUsage.CompletionTokens, result.UsageInfo.CompletionTokens);
        Assert.Equal(expectedUsage.TotalTokens, result.UsageInfo.TotalTokens);
    }

    [Fact]
    public async Task ExecuteAsync_OptionalParameters_PassedToService()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "gpt-35-turbo";
        var promptText = "What is Azure?";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";
        var maxTokens = 100;
        var temperature = 0.7;

        var expectedUsage = new CompletionUsageInfo(10, 50, 60);
        var expectedResult = new CompletionResult("Azure is a cloud computing platform...", expectedUsage);

        Service.CreateCompletionAsync(
            Arg.Is(resourceName),
            Arg.Is(deploymentName),
            Arg.Is(promptText),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(maxTokens),
            Arg.Is(temperature),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--prompt-text", promptText,
            "--max-tokens", maxTokens.ToString(),
            "--temperature", temperature.ToString());

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        // Verify the service was called at least once with the core parameters
        await Service.Received(1).CreateCompletionAsync(
            resourceName,
            deploymentName,
            promptText,
            subscriptionId,
            resourceGroup,
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var resourceName = "test-openai";
        var deploymentName = "gpt-35-turbo";
        var promptText = "What is Azure?";
        var subscriptionId = "test-subscription-id";
        var resourceGroup = "test-resource-group";
        var expectedError = "Test error";

        Service.CreateCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string?>(),
            Arg.Is(AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--prompt-text", promptText);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, (int)response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Command_HasCorrectName()
    {
        Assert.Equal("create-completion", Command.Name);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }
}
