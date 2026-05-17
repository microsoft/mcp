// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseDeleteCommandTests : CommandUnitTestsBase<DatabaseDeleteCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server server1 --database db1", true)]
    [InlineData("--subscription sub --resource-group rg --server server1", false)] // Missing database
    [InlineData("--subscription sub --resource-group rg --database db1", false)] // Missing server
    [InlineData("--subscription sub --server server1 --database db1", false)] // Missing resource group
    [InlineData("--resource-group rg --server server1 --database db1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.DeleteDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeletesDatabaseSuccessfully()
    {
        Service.DeleteDatabaseAsync(
            "server1",
            "db1",
            "rg1",
            "sub1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "db1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_IdempotentWhenDatabaseMissing()
    {
        Service.DeleteDatabaseAsync(
            "server1",
            "missingdb",
            "rg1",
            "sub1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "missingdb");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "db1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404Error()
    {
        var requestFailed = new RequestFailedException((int)HttpStatusCode.NotFound, "Not found");
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestFailed);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "db1");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL server or database not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Error()
    {
        var requestFailed = new RequestFailedException((int)HttpStatusCode.Forbidden, "Access denied");
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestFailed);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "db1");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        const string subscription = "sub1";
        const string resourceGroup = "rg1";
        const string server = "server1";
        const string database = "db1";

        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--server", server,
            "--database", database);

        await Service.Received(1).DeleteDatabaseAsync(
            server,
            database,
            resourceGroup,
            subscription,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicyOptions()
    {
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "db1",
            "--retry-max-retries", "5");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteDatabaseAsync(
            "server1",
            "db1",
            "rg1",
            "sub1",
            Arg.Is<RetryPolicyOptions?>(r => r != null && r.MaxRetries == 5),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("db1")]
    [InlineData("MyDatabase")]
    [InlineData("db-with-hyphens")]
    [InlineData("db_with_underscores")]
    [InlineData("db123")]
    public async Task ExecuteAsync_HandlesVariousDatabaseNames(string dbName)
    {
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", dbName);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteDatabaseAsync(
            "server1",
            dbName,
            "rg1",
            "sub1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesArgumentException()
    {
        var argumentException = new ArgumentException("Invalid database name");
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(argumentException);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "invalidDb");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid database name", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesResultContainsExpectedData()
    {
        Service.DeleteDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--resource-group", "rg1",
            "--server", "server1",
            "--database", "db1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
