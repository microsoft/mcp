// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<ServerGetCommand> _logger;
    private readonly ServerGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServerGetCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ServerGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_sqlService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Azure SQL server", command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithServerName_ReturnsSingleServer()
    {
        // Arrange
        var mockServer = CreateMockServer("server1");

        _sqlService.GetServerAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockServer);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "server1"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
        await _sqlService.Received(1).GetServerAsync("server1", "rg", "sub", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await _sqlService.DidNotReceive().ListServersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutServerName_ReturnsAllServers()
    {
        // Arrange
        var mockServers = new List<SqlServer> { CreateMockServer("server1"), CreateMockServer("server2") };

        _sqlService.ListServersAsync(
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockServers);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
        await _sqlService.Received(1).ListServersAsync("rg", "sub", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await _sqlService.DidNotReceive().GetServerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _sqlService
            .ListServersAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Server not found");
        _sqlService
            .GetServerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg", "--server", "nonexistent"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var authException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden");
        _sqlService
            .ListServersAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(authException);

        var args = _commandDefinition.Parse(["--subscription", "sub", "--resource-group", "rg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub", false)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("--subscription sub --resource-group rg --server server1", true)]
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _sqlService
                .ListServersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
            _sqlService
                .GetServerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(CreateMockServer("server1"));
        }

        var args = _commandDefinition.Parse(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
            Assert.Equal(HttpStatusCode.OK, response.Status);
        else
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    private static SqlServer CreateMockServer(string name) => new(
        Name: name,
        FullyQualifiedDomainName: $"{name}.database.windows.net",
        Location: "East US",
        ResourceGroup: "rg",
        Subscription: "sub",
        AdministratorLogin: "adminuser",
        Version: "12.0",
        State: "Ready",
        PublicNetworkAccess: "Enabled",
        Tags: null
    );
}
