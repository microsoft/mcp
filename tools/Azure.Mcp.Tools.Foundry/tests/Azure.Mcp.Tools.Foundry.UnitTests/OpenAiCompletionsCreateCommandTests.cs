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

public class OpenAiCompletionsCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;

    public OpenAiCompletionsCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
    }

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

        _foundryService.CreateCompletionAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == deploymentName),
                Arg.Is<string>(s => s == promptText),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Any<int?>(),
                Arg.Any<double?>(),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        // Act
        var command = new OpenAiCompletionsCreateCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--prompt-text", promptText
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<OpenAiCompletionsCreateCommandResult>(json);

        Assert.NotNull(result);
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

        _foundryService.CreateCompletionAsync(
                Arg.Is<string>(s => s == resourceName),
                Arg.Is<string>(s => s == deploymentName),
                Arg.Is<string>(s => s == promptText),
                Arg.Is<string>(s => s == subscriptionId),
                Arg.Is<string>(s => s == resourceGroup),
                Arg.Is<int?>(i => i == maxTokens),
                Arg.Is<double?>(d => d == temperature),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedResult);

        // Act
        var command = new OpenAiCompletionsCreateCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--prompt-text", promptText,
            "--max-tokens", maxTokens.ToString(),
            "--temperature", temperature.ToString()
        ]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        // Verify the service was called at least once with the core parameters
        await _foundryService.Received(1).CreateCompletionAsync(
            resourceName,
            deploymentName,
            promptText,
            subscriptionId,
            resourceGroup,
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string?>(),
            Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
            Arg.Any<RetryPolicyOptions?>());
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

        _foundryService.CreateCompletionAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<double?>(),
                Arg.Any<string?>(),
                Arg.Is<AuthMethod>(s => s == AuthMethod.Credential),
                Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var command = new OpenAiCompletionsCreateCommand();
        var args = command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment", deploymentName,
            "--prompt-text", promptText
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
        var command = new OpenAiCompletionsCreateCommand();

        // Assert
        Assert.Equal("create-completion", command.Name);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        // Arrange & Act
        var command = new OpenAiCompletionsCreateCommand();

        // Assert
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    private class OpenAiCompletionsCreateCommandResult
    {
        [JsonPropertyName("completionText")]
        public string CompletionText { get; set; } = string.Empty;

        [JsonPropertyName("usageInfo")]
        public CompletionUsageInfo UsageInfo { get; set; } = new CompletionUsageInfo(0, 0, 0);
    }
}
