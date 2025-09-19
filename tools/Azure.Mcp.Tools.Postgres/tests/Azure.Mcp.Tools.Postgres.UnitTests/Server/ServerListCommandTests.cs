// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.TestUtilities;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Server;

public class ServerListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<ServerListCommand> _logger;

    public ServerListCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<ServerListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServers_WhenServersExist()
    {
        var expectedServers = new List<string> { "server1", "server2" };
        _postgresService.ListServersAsync("sub123", "rg1", "user1").Returns(expectedServers);
        var command = new ServerListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.ServerListCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedServers, result.Servers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoServers()
    {
        _postgresService.ListServersAsync("sub123", "rg1", "user1").Returns([]);

        var command = new ServerListCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.ServerListCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.Servers);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _postgresService.ListServersAsync("sub123", "rg1", "user1")
            .ThrowsAsync(new Exception("Test error"));

        var command = new ServerListCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new ServerListCommand(_logger);
        var args = command.GetCommand().Parse(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ("--user", "user1")
        ));

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }
}
