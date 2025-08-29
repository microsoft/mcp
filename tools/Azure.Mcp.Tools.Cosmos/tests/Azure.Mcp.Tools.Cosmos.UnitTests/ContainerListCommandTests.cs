// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Azure.Mcp.Tools.Cosmos.Commands.ContainerListCommand;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class ContainerListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<ContainerListCommand> _logger;
    private readonly ContainerListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public ContainerListCommandTests()
    {
        _cosmosService = Substitute.For<ICosmosService>();
        _logger = Substitute.For<ILogger<ContainerListCommand>>();
        _command = new(_logger);
        _parser = new(_command.GetCommand());
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_cosmosService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsContainers_WhenContainersExist()
    {
        // Arrange
        var expectedContainers = new List<string> { "container1", "container2" };
        _cosmosService.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedContainers);

        var args = _parser.Parse([
            "--account", "account123",
            "--database", "database123",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ContainerListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
        Assert.Equal(expectedContainers.Count, result.Containers.Count);
        Assert.Equal(expectedContainers, result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoContainersExist()
    {
        // Arrange
        _cosmosService.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(new List<string>());

        var args = _parser.Parse([
            "--account", "account123",
            "--database", "database123",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        var expectedError = "Test error";

        _cosmosService.ListContainers(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _parser.Parse([
            "--account", "account123",
            "--database", "database123",
            "--subscription", "sub123"
        ]);

        // Act 
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--account", "account123", "--database", "database123")] // Missing subscription
    [InlineData("--subscription", "sub123", "--database", "database123")] // Missing account-name
    [InlineData("--subscription", "sub123", "--account", "account123")] // Missing database-name
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act 
        var response = await _command.ExecuteAsync(_context, _parser.Parse(args));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
