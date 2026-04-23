// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests;

public class PostgresListCommandTests : CommandUnitTestsBase<PostgresListCommand, IPostgresService>
{
    [Fact]
    public async Task ExecuteAsync_ListsServers_WhenNoServerOrDatabaseProvided()
    {
        var expectedServers = new List<string> { "postgres-server-1", "postgres-server-2", "postgres-server-3" };
        Service.ListServersAsync("sub123", "rg1", Arg.Any<CancellationToken>()).Returns(expectedServers);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.Equal(expectedServers, result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ListsAllServersInSubscription_WhenNoResourceGroupProvided()
    {
        var expectedServers = new List<string> { "postgres-server-1", "postgres-server-2" };
        Service.ListServersAsync("sub123", null, Arg.Any<CancellationToken>()).Returns(expectedServers);

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.Equal(expectedServers, result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServerProvidedWithoutUser()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--server", "server1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("The --user parameter is required when --server is specified.", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ListsDatabases_WhenServerProvided()
    {
        var expectedDatabases = new List<string> { "db1", "db2", "db3" };
        Service.ListDatabasesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            Arg.Any<CancellationToken>())
            .Returns(expectedDatabases);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.Null(result.Servers);
        Assert.Equal(expectedDatabases, result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ListsTables_WhenServerAndDatabaseProvided()
    {
        var expectedTables = new List<string> { "users", "products", "orders" };
        Service.ListTablesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            "db1",
            Arg.Any<CancellationToken>())
            .Returns(expectedTables);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1",
            "--database", "db1");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.Null(result.Servers);
        Assert.Null(result.Databases);
        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoServersExist()
    {
        Service.ListServersAsync("sub123", "rg1", Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.NotNull(result.Servers);
        Assert.Empty(result.Servers);
        Assert.Null(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoDatabasesExist()
    {
        Service.ListDatabasesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.Null(result.Servers);
        Assert.NotNull(result.Databases);
        Assert.Empty(result.Databases);
        Assert.Null(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoTablesExist()
    {
        Service.ListTablesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            "db1",
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1",
            "--database", "db1");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.PostgresListCommandResult);

        Assert.Null(result.Servers);
        Assert.Null(result.Databases);
        Assert.NotNull(result.Tables);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListServersThrows()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        Service.ListServersAsync("sub123", "rg1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListDatabasesThrows()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        Service.ListDatabasesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenListTablesThrows()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        Service.ListTablesAsync(
            "sub123",
            "rg1",
            AuthTypes.MicrosoftEntra,
            "user1",
            null,
            "server1",
            "db1",
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--server", "server1",
            "--database", "db1");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string missingParameter)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingParameter,("--subscription", "sub123")));

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
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

    [Fact]
    public void Description_IsCorrect()
    {
        Assert.Contains("List PostgreSQL servers", Command.Description);
        Assert.Contains("databases, or tables", Command.Description);
    }
}
