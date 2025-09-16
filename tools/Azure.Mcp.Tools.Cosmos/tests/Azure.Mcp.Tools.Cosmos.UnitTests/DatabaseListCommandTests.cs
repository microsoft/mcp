// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
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
using static Azure.Mcp.Tools.Cosmos.Commands.DatabaseListCommand;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class DatabaseListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<DatabaseListCommand> _logger;
    private readonly DatabaseListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DatabaseListCommandTests()
    {
        _cosmosService = Substitute.For<ICosmosService>();
        _logger = Substitute.For<ILogger<DatabaseListCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_cosmosService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabases_WhenDatabasesExist()
    {
        // Arrange
        var expectedDatabases = new List<string> { "database1", "database2" };
        _cosmosService.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedDatabases);

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DatabaseListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
        Assert.Equal(expectedDatabases.Count, result.Databases.Count);
        Assert.Equal(expectedDatabases, result.Databases);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDataBaseExists()
    {
        // Arrange
        _cosmosService.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<DatabaseListCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
        Assert.Empty(result.Databases);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        var expectedError = "Test error";

        _cosmosService.ListDatabases(
            Arg.Is("account123"),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "account123",
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
    [InlineData("--account", "account123")]
    [InlineData("--subscription", "sub123")]
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act 
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
