// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Server;

public class ServerParamGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<ServerParamGetCommand> _logger;

    public ServerParamGetCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<ServerParamGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsParameter_WhenSuccessful()
    {
        var expectedValue = "ON";
        _mysqlService.GetServerParameterAsync("sub123", "rg1", "user1", "test-server", "max_connections").Returns(expectedValue);

        var command = new ServerParamGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "max_connections"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.ServerParamGetCommandResult);
        Assert.NotNull(result);
        Assert.Equal("max_connections", result.Parameter);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        _mysqlService.GetServerParameterAsync("sub123", "rg1", "user1", "test-server", "invalid_param")
            .ThrowsAsync(new Exception("Parameter 'invalid_param' not found."));

        var command = new ServerParamGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server",
            "--param", "invalid_param"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Parameter 'invalid_param' not found", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new ServerParamGetCommand(_logger);

        Assert.Equal("param", command.Name);
        Assert.Equal("Retrieves the current value of a single server configuration parameter on an Azure Database for MySQL Flexible Server. Use to inspect a setting (e.g. max_connections, wait_timeout, slow_query_log) before changing it.", command.Description);
        Assert.Equal("Get MySQL Server Parameter", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }
}
