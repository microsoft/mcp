// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Index;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Index;

public class CurrentOpsCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentDbService _documentDbService;
    private readonly ILogger<CurrentOpsCommand> _logger;
    private readonly CurrentOpsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CurrentOpsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _logger = Substitute.For<ILogger<CurrentOpsCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_documentDbService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOps_WhenOpsExist()
    {
        // Arrange
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Current operations retrieved successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["inprog"] = new List<Dictionary<string, object?>>
                {
                    new Dictionary<string, object?>
                    {
                        ["opid"] = "12345",
                        ["op"] = "query",
                        ["ns"] = "testdb.testcollection",
                        ["secs_running"] = 5
                    },
                    new Dictionary<string, object?>
                    {
                        ["opid"] = "12346",
                        ["op"] = "insert",
                        ["ns"] = "testdb.users",
                        ["secs_running"] = 2
                    }
                },
                ["count"] = 2
            }
        };

        _documentDbService.GetCurrentOpsAsync(
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredOps_WhenFilterProvided()
    {
        // Arrange
        var filter = "{\"op\": \"query\"}";
        var expectedResult = new Dictionary<string, object?>
        {
            ["success"] = true,
            ["statusCode"] = HttpStatusCode.OK,
            ["message"] = "Current operations retrieved successfully",
            ["data"] = new Dictionary<string, object?>
            {
                ["inprog"] = new List<Dictionary<string, object?>>
                {
                    new Dictionary<string, object?>
                    {
                        ["opid"] = "12345",
                        ["op"] = "query",
                        ["ns"] = "testdb.testcollection",
                        ["secs_running"] = 5
                    }
                },
                ["count"] = 1
            }
        };

        _documentDbService.GetCurrentOpsAsync(
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var args = _commandDefinition.Parse([
            "--ops", filter
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceFails()
    {
        // Arrange
        _documentDbService.GetCurrentOpsAsync(
            Arg.Any<BsonDocument>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<string, object?>
            {
                ["success"] = false,
                ["statusCode"] = HttpStatusCode.InternalServerError,
                ["message"] = "Failed to retrieve current operations",
                ["data"] = null
            });

        var args = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to retrieve current operations", response.Message);
    }
}
