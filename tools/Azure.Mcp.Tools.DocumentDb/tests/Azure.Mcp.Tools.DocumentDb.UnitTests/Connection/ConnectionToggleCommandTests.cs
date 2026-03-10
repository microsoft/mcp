// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Connection;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Connection;

public class ConnectionToggleCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<ConnectionToggleCommand> _logger;
    private readonly ConnectionToggleCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ConnectionToggleCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<ConnectionToggleCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenConnectActionSucceeds()
    {
        var connectionString = "mongodb://localhost:27017";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Connected successfully",
            Data = new Dictionary<string, object?>
            {
                ["databaseCount"] = 2,
                ["databases"] = new List<string> { "test", "admin" }
            }
        };

        _documentDbService.ConnectAsync(connectionString, true, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--action", "connect",
            "--connection-string", connectionString
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenConnectActionDisablesConnectionTest()
    {
        var connectionString = "mongodb://localhost:27017";
        var expectedResult = new DocumentDbResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Connected successfully (not tested)",
            Data = null
        };

        _documentDbService.ConnectAsync(connectionString, false, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--action", "connect",
            "--connection-string", connectionString,
            "--test-connection", "false"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenDisconnectActionSucceeds()
    {
        _documentDbService.DisconnectAsync(Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Disconnected successfully",
                Data = new Dictionary<string, object?>
                {
                    ["isConnected"] = false
                }
            });

        var args = _commandDefinition.Parse([
            "--action", "disconnect"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenConnectActionIsMissingConnectionString()
    {
        var args = _commandDefinition.Parse([
            "--action", "connect"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("connection-string", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenConnectActionThrows()
    {
        var connectionString = "mongodb://invalid:27017";
        const string expectedError = "Failed to connect to DocumentDB";

        _documentDbService.ConnectAsync(connectionString, true, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--action", "connect",
            "--connection-string", connectionString
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenDisconnectActionFails()
    {
        _documentDbService.DisconnectAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Disconnect failed: Unexpected error"));

        var args = _commandDefinition.Parse([
            "--action", "disconnect"
        ]);

        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Disconnect failed", response.Message);
    }
}