// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.DocumentDb.Commands.Connection;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Connection;

public class ConnectDocumentDbCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<ConnectDocumentDbCommand> _logger;
    private readonly ConnectDocumentDbCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ConnectDocumentDbCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<ConnectDocumentDbCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenConnectionSucceeds()
    {
        // Arrange
        var connectionString = "mongodb://localhost:27017";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["message"] = "Connected successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["databaseCount"] = 2,
                ["databases"] = new List<string> { "test", "admin" }
            }
        };

        _documentDbService.ConnectAsync(
            Arg.Is(connectionString),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenConnectionSucceedsWithoutTestFlag()
    {
        // Arrange
        var connectionString = "mongodb://localhost:27017";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["message"] = "Connected successfully (not tested)",
            ["data"] = null
        };

        _documentDbService.ConnectAsync(
            Arg.Is(connectionString),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--test-connection", "false"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenConnectionFails()
    {
        // Arrange
        var connectionString = "mongodb://invalid:27017";
        var expectedError = "Failed to connect to DocumentDB";

        _documentDbService.ConnectAsync(
            Arg.Is(connectionString),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--connection-string", connectionString
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenConnectionStringIsMissing()
    {
        // Arrange & Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([]), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_Returns400_WhenConnectionStringIsEmpty(string connectionString)
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--connection-string", connectionString
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }
}
