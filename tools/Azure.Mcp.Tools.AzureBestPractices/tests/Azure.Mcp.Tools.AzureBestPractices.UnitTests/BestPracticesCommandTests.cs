// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureBestPractices.UnitTests;

public class BestPracticesCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BestPracticesCommand> _logger;
    private readonly CommandContext _context;
    private readonly BestPracticesCommand _command;
    private readonly Parser _parser;

    public BestPracticesCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<BestPracticesCommand>>();
        _command = new(_logger);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public async Task ExecuteAsync_GeneralCodeGeneration_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Implement retry logic with exponential backoff for transient failures", result[0]);
        Assert.Contains("Managed Identity (Azure-hosted)", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_GeneralDeployment_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "deployment"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Your IaC files must include:", result[0]);
        Assert.Contains("Quality requirements for IaC files:", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsCodeGeneration_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "azurefunctions", "--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Use the latest programming models (v4 for JavaScript, v2 for Python)", result[0]);
        Assert.Contains("Azure Functions Core Tools for creating Function Apps", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsDeployment_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "azurefunctions", "--action", "deployment"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("flex consumption plan", result[0]);
        Assert.Contains("Always use Linux OS for Python", result[0]);
        Assert.Contains("Function authentication", result[0]);
        Assert.Contains("Application Insights", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_StaticWebAppAll_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "static-web-app", "--action", "all"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("Static Web Apps CLI", result[0]);
        Assert.Contains("npx swa init --yes", result[0]);
        Assert.Contains("npx swa build", result[0]);
        Assert.Contains("npx swa deploy --env production", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidResource_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "invalid", "--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid resource", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_StaticWebAppWithInvalidAction_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "static-web-app", "--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("The 'static-web-app' resource only supports 'all' action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidAction_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "invalid"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid action", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GeneralWithAllAction_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "general", "--action", "all"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        // Should contain content from both code-generation and deployment files
        Assert.Contains("Implement retry logic with exponential backoff for transient failures", result[0]);
        Assert.Contains("Managed Identity (Azure-hosted)", result[0]);
        Assert.Contains("Your IaC files must include:", result[0]);
        Assert.Contains("Quality requirements for IaC files:", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_AzureFunctionsWithAllAction_ReturnsAzureBestPractices()
    {
        var args = _parser.Parse(["--resource", "azurefunctions", "--action", "all"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        // Should contain content from both code-generation and deployment files
        Assert.Contains("Use the latest programming models (v4 for JavaScript, v2 for Python)", result[0]);
        Assert.Contains("Azure Functions Core Tools for creating Function Apps", result[0]);
        Assert.Contains("flex consumption plan", result[0]);
        Assert.Contains("Always use Linux OS for Python", result[0]);
        Assert.Contains("Function authentication", result[0]);
        Assert.Contains("Application Insights", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResource_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--action", "code-generation"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Both resource and action parameters are required", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingAction_ReturnsBadRequest()
    {
        var args = _parser.Parse(["--resource", "general"]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("Both resource and action parameters are required", response.Message);
    }
}
