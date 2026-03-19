// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.DocumentDb.Commands.Others;
using Azure.Mcp.Tools.DocumentDb.Models;
using Azure.Mcp.Tools.DocumentDb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.UnitTests.Others;

public class CurrentOpsCommandTests
{
    private readonly IDocumentDbService _documentDbService;
    private readonly CurrentOpsCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public CurrentOpsCommandTests()
    {
        _documentDbService = Substitute.For<IDocumentDbService>();
        _command = new(Substitute.For<ILogger<CurrentOpsCommand>>());
        _commandDefinition = _command.GetCommand();
        _context = new(new ServiceCollection().AddSingleton(_documentDbService).BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOps_WhenOpsExist()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.GetCurrentOpsAsync(connectionString, Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Current operations retrieved successfully",
                Data = new Dictionary<string, object?> { ["operations"] = "{\"inprog\":[]}" }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFilteredOps_WhenFilterProvided()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.GetCurrentOpsAsync(connectionString, Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Current operations retrieved successfully",
                Data = new Dictionary<string, object?> { ["operations"] = "{\"inprog\":[{\"op\":\"query\"}]}" }
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString,
            "--ops", "{\"op\":\"query\"}"]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceFails()
    {
        const string connectionString = "mongodb://localhost:27017";

        _documentDbService.GetCurrentOpsAsync(connectionString, Arg.Any<BsonDocument>(), Arg.Any<CancellationToken>())
            .Returns(new DocumentDbResponse
            {
                Success = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Failed to retrieve current operations"
            });

        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse([
            "--connection-string", connectionString]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Failed to retrieve current operations", response.Message);
    }
}