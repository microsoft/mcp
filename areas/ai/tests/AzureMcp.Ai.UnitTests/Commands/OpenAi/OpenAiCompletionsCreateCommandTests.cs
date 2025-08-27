// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using AzureMcp.Ai.Commands.OpenAi;
using AzureMcp.Ai.Models;
using AzureMcp.Ai.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AzureMcp.Ai.UnitTests.Commands.OpenAi;

[Trait("Area", "Ai")]
public class OpenAiCompletionsCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAiService _aiService;
    private readonly ILogger<OpenAiCompletionsCreateCommand> _logger;
    private readonly OpenAiCompletionsCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public OpenAiCompletionsCreateCommandTests()
    {
        _aiService = Substitute.For<IAiService>();
        _logger = Substitute.For<ILogger<OpenAiCompletionsCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_aiService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsCompletion()
    {
        // Arrange
        var subscriptionId = "sub123";
        var resourceName = "test-openai";
        var resourceGroup = "test-rg";
        var deploymentName = "gpt-35-turbo";
        var promptText = "What is Azure?";
        var maxTokens = 100;
        var temperature = 0.7;

        var expectedUsage = new CompletionUsageInfo(10, 50, 60);
        var expectedResult = new CompletionResult("Azure is a cloud computing platform...", expectedUsage);

        _aiService.CreateCompletionAsync(
            Arg.Is(resourceName),
            Arg.Is(deploymentName),
            Arg.Is(promptText),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(maxTokens),
            Arg.Is(temperature),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _parser.Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment-name", deploymentName,
            "--prompt-text", promptText,
            "--max-tokens", maxTokens.ToString(),
            "--temperature", temperature.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        Assert.Contains("Azure is a cloud computing platform", json);
        Assert.Contains("promptTokens", json);
        Assert.Contains("completionTokens", json);
        Assert.Contains("totalTokens", json);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredParameters_ReturnsValidationError()
    {
        // Arrange
        var args = _parser.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsError()
    {
        // Arrange
        var subscriptionId = "sub123";
        var resourceName = "test-openai";
        var resourceGroup = "test-rg";
        var deploymentName = "gpt-35-turbo";
        var promptText = "What is Azure?";

        _aiService.CreateCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException("Resource not found"));

        var args = _parser.Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment-name", deploymentName,
            "--prompt-text", promptText
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Resource not found", response.Message);
    }

    [Theory]
    [InlineData("--max-tokens", "50")]
    [InlineData("--temperature", "0.5")]
    public async Task ExecuteAsync_WithOptionalParameters_PassesToService(string paramName, string paramValue)
    {
        // Arrange
        var subscriptionId = "sub123";
        var resourceName = "test-openai";
        var resourceGroup = "test-rg";
        var deploymentName = "gpt-35-turbo";
        var promptText = "What is Azure?";

        var expectedUsage = new CompletionUsageInfo(10, 50, 60);
        var expectedResult = new CompletionResult("Test response", expectedUsage);

        _aiService.CreateCompletionAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var argsList = new List<string>
        {
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment-name", deploymentName,
            "--prompt-text", promptText,
            paramName, paramValue
        };

        var args = _parser.Parse(argsList.ToArray());

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        await _aiService.Received(1).CreateCompletionAsync(
            Arg.Is(resourceName),
            Arg.Is(deploymentName),
            Arg.Is(promptText),
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Any<int?>(),
            Arg.Any<double?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }
}
