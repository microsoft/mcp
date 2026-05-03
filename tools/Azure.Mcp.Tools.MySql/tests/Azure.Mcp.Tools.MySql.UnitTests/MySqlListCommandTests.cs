// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests;

public class MySqlListCommandTests : CommandUnitTestsBase<MySqlListCommand, IMySqlService>
{
    [Fact]
    public async Task ExecuteAsync_ListsServers_WhenNoServerOrDatabaseProvided()
    {
        var expectedServers = new List<string> { "mysql-server-1", "mysql-server-2", "mysql-server-3" };
        Service.ListServersAsync("sub123", "rg1", "user1", Arg.Any<CancellationToken>()).Returns(expectedServers);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.MySqlListCommandResult);
        Assert.Equal(expectedServers, result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDatabases_WhenServerProvided()
    {
        var expectedDatabases = new List<string> { "db1", "db2", "db3" };
        Service.ListDatabasesAsync("sub123", "rg1", "user1", "server1", Arg.Any<CancellationToken>()).Returns(expectedDatabases);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.MySqlListCommandResult);
        Assert.Null(result.Servers);
        Assert.Equal(expectedDatabases, result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ListsTables_WhenServerAndDatabaseProvided()
    {
        var expectedTables = new List<string> { "users", "products", "orders" };
        Service.GetTablesAsync("sub123", "rg1", "user1", "server1", "db1", Arg.Any<CancellationToken>()).Returns(expectedTables);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.MySqlListCommandResult);
        Assert.Null(result.Servers);
        Assert.Null(result.Databases);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoServersExist()
    {
        Service.ListServersAsync("sub123", "rg1", "user1", Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.MySqlListCommandResult);
        Assert.NotNull(result.Servers);
        Assert.Empty(result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoDatabasesExist()
    {
        Service.ListDatabasesAsync("sub123", "rg1", "user1", "server1", Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.MySqlListCommandResult);
        Assert.Null(result.Servers);
        Assert.NotNull(result.Databases);
        Assert.Empty(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoTablesExist()
    {
        Service.GetTablesAsync("sub123", "rg1", "user1", "server1", "db1", Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.MySqlListCommandResult);
        Assert.Null(result.Servers);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Tables);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListServersThrows()
    {
        Service.ListServersAsync("sub123", "rg1", "user1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListDatabasesThrows()
    {
        Service.ListDatabasesAsync("sub123", "rg1", "user1", "server1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenGetTablesThrows()
    {
        Service.GetTablesAsync("sub123", "rg1", "user1", "server1", "db1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Fact]
    public void Name_IsCorrect()
    {
        Assert.Equal("list", Command.Name);
    }

}
