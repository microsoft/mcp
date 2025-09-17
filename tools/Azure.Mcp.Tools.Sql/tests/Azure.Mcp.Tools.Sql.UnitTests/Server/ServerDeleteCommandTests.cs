// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<ServerDeleteCommand> _logger;
    private readonly ServerDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServerDeleteCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ServerDeleteCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("delete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Deletes an Azure SQL server", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server testserver --force", true)]
    [InlineData("--subscription sub --resource-group rg --server testserver", true)] // Should show warning without force
    [InlineData("--subscription sub --resource-group rg --force", false)] // Missing server
    [InlineData("--subscription sub --server testserver --force", false)] // Missing resource group
    [InlineData("--resource-group rg --server testserver --force", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed && args.Contains("--force"))
        {
            _service.DeleteServerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Core.Options.RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
        }
        else
        {
            Assert.NotEqual(200, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenForceNotSpecified_ReturnsWarning()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Contains("WARNING", response.Message);
        Assert.Contains("permanently delete", response.Message);
        Assert.Contains("--force", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerDeletedSuccessfully_ReturnsSuccess()
    {
        // Arrange
        _service.DeleteServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --force");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        await _service.Received(1).DeleteServerAsync("testserver", "rg", "sub", Arg.Any<Core.Options.RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerNotFound_Returns404()
    {
        // Arrange
        _service.DeleteServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --force");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        _service.DeleteServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --force");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("error", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerNotFoundFromAzure_Returns404StatusCode()
    {
        // Arrange
        var requestException = new RequestFailedException(404, "Not Found: Server not found");

        _service.DeleteServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(requestException));

        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --force");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnauthorized_Returns403StatusCode()
    {
        // Arrange
        var requestException = new RequestFailedException(403, "Forbidden: Insufficient permissions");

        _service.DeleteServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(requestException));

        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --force");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("authorization failed", response.Message.ToLower());
    }
}
