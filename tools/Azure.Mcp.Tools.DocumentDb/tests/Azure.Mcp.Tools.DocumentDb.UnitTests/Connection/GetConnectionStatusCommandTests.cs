// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Connection;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Connection;

public class GetConnectionStatusCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<GetConnectionStatusCommand> _logger;
    private readonly GetConnectionStatusCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public GetConnectionStatusCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<GetConnectionStatusCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConnectedStatus_WhenConnected()
    {
        // Arrange
        _documentDbService.GetConnectionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Connection status retrieved successfully",
                ["data"] = new Dictionary<string, object?>
                {
                    ["isConnected"] = true,
                    ["connectionString"] = "mongodb://localhost:27017",
                    ["details"] = new Dictionary<string, object?>
                    {
                        ["status"] = "Connected and verified"
                    }
                }
            });

        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotConnectedStatus_WhenNotConnected()
    {
        // Arrange
        _documentDbService.GetConnectionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = true,
                ["statusCode"] = HttpStatusCode.OK,
                ["message"] = "Not connected",
                ["data"] = new Dictionary<string, object?>
                {
                    ["isConnected"] = false,
                    ["connectionString"] = null,
                    ["details"] = null
                }
            });

        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenConnectionCheckFails()
    {
        // Arrange
        _documentDbService.GetConnectionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = "Failed to check connection status: Connection timeout",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to check connection status", response.Message);
    }
}
