// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
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

[Trait("Area", "Foundry")]
public class OpenAiCompletionsCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFoundryService _foundryService;
    private readonly OpenAiCompletionsCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public OpenAiCompletionsCreateCommandTests()
    {
        _foundryService = Substitute.For<IFoundryService>();

        var collection = new ServiceCollection().AddSingleton(_foundryService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new();
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
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

        _foundryService.CreateCompletionAsync(
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

        var parseResult = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment-name", deploymentName,
            "--prompt-text", promptText,
            "--max-tokens", maxTokens.ToString(),
            "--temperature", temperature.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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
        var parseResult = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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

        _foundryService.CreateCompletionAsync(
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

        var parseResult = _commandDefinition.Parse([
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--resource-name", resourceName,
            "--deployment-name", deploymentName,
            "--prompt-text", promptText
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

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

        _foundryService.CreateCompletionAsync(
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

        var parseResult = _commandDefinition.Parse(argsList.ToArray());

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        await _foundryService.Received(1).CreateCompletionAsync(
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