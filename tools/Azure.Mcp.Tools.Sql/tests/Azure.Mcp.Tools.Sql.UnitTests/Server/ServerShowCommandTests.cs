// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<ServerShowCommand> _logger;
    private readonly ServerShowCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServerShowCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ServerShowCommand>>();

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
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("SQL server", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server testserver", true)]
    [InlineData("--subscription sub --resource-group rg", false)] // Missing server
    [InlineData("--subscription sub --server testserver", false)] // Missing resource-group
    [InlineData("--resource-group rg --server testserver", false)] // Missing subscription
    public async Task ExecuteAsync_ValidationScenarios_ReturnsExpectedResults(string args, bool shouldSucceed)
    {
        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult);

        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
        }
        else
        {
            Assert.Equal(400, response.Status);
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidOptions_ReturnsServer()
    {
        // Arrange
        var expectedServer = new SqlServer(
            Name: "test-server",
            FullyQualifiedDomainName: "test-server.database.windows.net",
            Location: "East US",
            ResourceGroup: "test-rg",
            Subscription: "test-sub",
            AdministratorLogin: "sql-admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Enabled",
            Tags: new Dictionary<string, string> { { "environment", "test" } }
        );

        _service.GetServerAsync(
            "test-server",
            "test-rg",
            "test-sub",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        // Act
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server test-server");
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerNotFound_ReturnsNotFound()
    {
        // Arrange
        _service.GetServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(new KeyNotFoundException("SQL server not found")));

        // Act
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server nonexistent-server");
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAzureRequestFails404_ReturnsNotFound()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(404, "Resource not found");
        _service.GetServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(requestFailedException));

        // Act
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server missing-server");
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAzureRequestFails403_ReturnsAuthorizationError()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(403, "Authorization failed");
        _service.GetServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(requestFailedException));

        // Act
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server test-server");
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("authorization failed", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArgumentException_ReturnsBadRequest()
    {
        // Arrange
        _service.GetServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(new ArgumentException("Invalid server name")));

        // Act
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server invalid");
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("invalid parameter", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        _service.GetServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(new InvalidOperationException("Unexpected error")));

        // Act
        var parseResult = _commandDefinition.Parse("--subscription test-sub --resource-group test-rg --server test-server");
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Unexpected error", response.Message);
    }

    [Fact]
    public void Metadata_IndicatesReadOnlyOperation()
    {
        // Act & Assert
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.ReadOnly);
    }
}
