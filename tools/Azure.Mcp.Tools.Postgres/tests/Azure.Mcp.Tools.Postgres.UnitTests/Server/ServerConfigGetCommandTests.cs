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
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Server;

public class ServerConfigGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPostgresService _postgresService;
    private readonly ILogger<ServerConfigGetCommand> _logger;

    public ServerConfigGetCommandTests()
    {
        _postgresService = Substitute.For<IPostgresService>();
        _logger = Substitute.For<ILogger<ServerConfigGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_postgresService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConfig_WhenConfigExists()
    {
        var expectedConfig = "config123";
        _postgresService.GetServerConfigAsync("sub123", "rg1", "user1", "server123").Returns(expectedConfig);

        var command = new ServerConfigGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PostgresJsonContext.Default.ServerConfigGetCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedConfig, result.Configuration);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenConfigDoesNotExist()
    {
        _postgresService.GetServerConfigAsync("sub123", "rg1", "user1", "server123").Returns("");

        var command = new ServerConfigGetCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--user", "user1", "--server", "server123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--user")]
    [InlineData("--server")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new ServerConfigGetCommand(_logger);
        var args = command.GetCommand().Parse(ArgBuilder.BuildArgs(missingParameter,
            ("--subscription", "sub123"),
            ("--resource-group", "rg1"),
            ("--user", "user1"),
            ("--server", "server123")
        ));

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }
}
