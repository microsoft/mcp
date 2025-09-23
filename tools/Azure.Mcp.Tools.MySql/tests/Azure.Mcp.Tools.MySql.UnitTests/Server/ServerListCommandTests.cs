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

public class ServerListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<ServerListCommand> _logger;

    public ServerListCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<ServerListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServers_WhenSuccessful()
    {
        var expectedServers = new List<string> { "mysql-server-1", "mysql-server-2", "mysql-server-3" };
        _mysqlService.ListServersAsync("sub123", "rg1", "user1").Returns(expectedServers);

        var command = new ServerListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.ServerListCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedServers, result.Servers);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        _mysqlService.ListServersAsync("sub123", "rg1", "user1")
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var command = new ServerListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new ServerListCommand(_logger);

        Assert.Equal("list", command.Name);
        Assert.Equal("Discovers and lists all Azure Database for MySQL Flexible Server instances within the specified resource group. This command provides an inventory of available MySQL server resources, including their names and current status, enabling efficient server management and resource planning.", command.Description);
        Assert.Equal("List MySQL Servers", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }
}
