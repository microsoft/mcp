// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Table;

public class TableListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<TableListCommand> _logger;

    public TableListCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<TableListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTables_WhenSuccessful()
    {
        var expectedTables = new List<string> { "users", "products", "orders" };
        _mysqlService.GetTablesAsync("sub123", "rg1", "user1", "server1", "db1").Returns(expectedTables);

        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MySqlJsonContext.Default.TableListCommandResult);
        Assert.NotNull(result);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        _mysqlService.GetTablesAsync("sub123", "rg1", "user1", "server1", "db1")
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var command = new TableListCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new TableListCommand(_logger);

        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }
}
