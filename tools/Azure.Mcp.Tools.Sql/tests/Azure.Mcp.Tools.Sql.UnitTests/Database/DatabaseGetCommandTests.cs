// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseGetCommandTests : CommandUnitTestsBase<DatabaseGetCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithDatabaseName_ReturnsSingleDatabase()
    {
        // Arrange
        var mockDatabase = CreateMockDatabase("testdb");

        Service.GetDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("testdb"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "testdb");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
        await Service.Received(1).GetDatabaseAsync("server1", "testdb", "rg", "sub", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceive().ListDatabasesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutDatabaseName_ReturnsAllDatabases()
    {
        // Arrange
        var mockDatabases = new ResourceQueryResults<SqlDatabase>([CreateMockDatabase("db1"), CreateMockDatabase("db2")], false);

        Service.ListDatabasesAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabases);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
        await Service.Received(1).ListDatabasesAsync("server1", "rg", "sub", Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceive().GetDatabaseAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListDatabasesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found");
        Service.GetDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "nonexistent");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var authException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden");
        Service.ListDatabasesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(authException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub", false)]
    [InlineData("--subscription sub --resource-group rg", false)]
    [InlineData("--subscription sub --resource-group rg --server server1", true)]
    [InlineData("--subscription sub --resource-group rg --server server1 --database db1", true)]
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service
                .ListDatabasesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<SqlDatabase>([], false));
            Service
                .GetDatabaseAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(CreateMockDatabase("db1"));
        }

        // Act
        var response = await ExecuteCommandAsync(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Assert
        if (shouldSucceed)
            Assert.Equal(HttpStatusCode.OK, response.Status);
        else
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    private static SqlDatabase CreateMockDatabase(string name) => new(
        Name: name,
        Id: $"/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/{name}",
        Type: "Microsoft.Sql/servers/databases",
        Location: "East US",
        Sku: new DatabaseSku("Basic", "Basic", 5, null, null),
        Status: "Online",
        Collation: "SQL_Latin1_General_CP1_CI_AS",
        CreationDate: DateTimeOffset.UtcNow,
        MaxSizeBytes: 1073741824,
        ServiceLevelObjective: "Basic",
        Edition: "Basic",
        ElasticPoolName: null,
        EarliestRestoreDate: DateTimeOffset.UtcNow,
        ReadScale: "Disabled",
        ZoneRedundant: false
    );
}
