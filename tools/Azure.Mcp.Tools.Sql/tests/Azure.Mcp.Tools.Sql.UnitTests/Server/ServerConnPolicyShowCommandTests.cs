// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerConnPolicyShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _sqlService;
    private readonly ILogger<ServerConnPolicyShowCommand> _logger;
    private readonly ServerConnPolicyShowCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServerConnPolicyShowCommandTests()
    {
        _sqlService = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ServerConnPolicyShowCommand>>();

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
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.Contains("connection policy", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsConnectionPolicy()
    {
        // Arrange
        var mockConnectionPolicy = new SqlServerConnectionPolicy(
            Name: "default",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/connectionPolicies/default",
            Type: "Microsoft.Sql/servers/connectionPolicies",
            ConnectionType: "Default");

        _sqlService.GetServerConnectionPolicyAsync(
                Arg.Is("server1"),
                Arg.Is("rg"),
                Arg.Is("sub"),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .Returns(mockConnectionPolicy);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _sqlService.GetServerConnectionPolicyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Connection policy not found"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "missing"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesForbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden");
        _sqlService.GetServerConnectionPolicyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGeneralException()
    {
        // Arrange
        _sqlService.GetServerConnectionPolicyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Unexpected error"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Unexpected error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Theory]
    [InlineData("", false, "Missing required options")]
    [InlineData("--subscription sub", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1", true, null)]
    [InlineData("--resource-group rg --server server1", false, "Missing required options")] // Missing subscription
    [InlineData("--subscription sub --server server1", false, "Missing required options")] // Missing resource-group
    [InlineData("--subscription sub --resource-group rg", false, "Missing required options")] // Missing server
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed, string? expectedError)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockConnectionPolicy = new SqlServerConnectionPolicy(
                Name: "default",
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/connectionPolicies/default",
                Type: "Microsoft.Sql/servers/connectionPolicies",
                ConnectionType: "Default");

            _sqlService.GetServerConnectionPolicyAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(mockConnectionPolicy);
        }

        var args = _commandDefinition.Parse(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            if (expectedError != null)
            {
                Assert.Contains(expectedError, response.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithSubscriptionFromEnvironment_Succeeds()
    {
        // Arrange - Test when subscription comes from environment variable
        Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", "env-sub-id");

        var mockConnectionPolicy = new SqlServerConnectionPolicy(
            Name: "default",
            Id: "/subscriptions/env-sub-id/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/connectionPolicies/default",
            Type: "Microsoft.Sql/servers/connectionPolicies",
            ConnectionType: "Default");

        _sqlService.GetServerConnectionPolicyAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("env-sub-id"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockConnectionPolicy);

        try
        {
            var args = _commandDefinition.Parse([
                "--resource-group", "rg",
                "--server", "server1"
            ]);

            // Act
            var response = await _command.ExecuteAsync(_context, args);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        finally
        {
            // Clean up environment variable
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", null);
        }
    }
}
