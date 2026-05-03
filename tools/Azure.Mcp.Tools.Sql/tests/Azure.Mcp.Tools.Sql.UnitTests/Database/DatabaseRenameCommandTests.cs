// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Database;

public class DatabaseRenameCommandTests : CommandUnitTestsBase<DatabaseRenameCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("rename", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_RenamesDatabase()
    {
        // This test also ensures the fix for the bug where new-database-name was not being bound correctly
        var mockDatabase = new SqlDatabase(
            Name: "newdb",
            Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/newdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        Service.RenameDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("olddb"),
            Arg.Is("newdb"), // Verify new-database-name is correctly bound (not null)
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "newdb");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        // Verify the service was called with the correct new database name (not null)
        await Service.Received(1).RenameDatabaseAsync(
            "server1",
            "olddb",
            "newdb",
            "rg",
            "sub",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", false, "Missing required options")]
    [InlineData("--subscription sub", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database olddb", false, "Missing required options")]
    [InlineData("--subscription sub --resource-group rg --server server1 --database olddb --new-database-name newdb", true, null)]
    [InlineData("--resource-group rg --server server1 --database olddb --new-database-name newdb", false, "Missing required options")] // Missing subscription
    [InlineData("--subscription sub --server server1 --database olddb --new-database-name newdb", false, "Missing required options")] // Missing resource-group
    [InlineData("--subscription sub --resource-group rg --database olddb --new-database-name newdb", false, "Missing required options")] // Missing server
    [InlineData("--subscription sub --resource-group rg --server server1 --new-database-name newdb", false, "Missing required options")] // Missing database
    public async Task ExecuteAsync_ValidatesRequiredParameters(string commandArgs, bool shouldSucceed, string? expectedError)
    {
        // Arrange
        if (shouldSucceed)
        {
            var mockDatabase = new SqlDatabase(
                Name: "newdb",
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/newdb",
                Type: "Microsoft.Sql/servers/databases",
                Location: "East US",
                Sku: null,
                Status: "Online",
                Collation: "SQL_Latin1_General_CP1_CI_AS",
                CreationDate: DateTimeOffset.UtcNow,
                MaxSizeBytes: 2147483648,
                ServiceLevelObjective: "S0",
                Edition: "Standard",
                ElasticPoolName: null,
                EarliestRestoreDate: DateTimeOffset.UtcNow,
                ReadScale: "Disabled",
                ZoneRedundant: false
            );

            Service.RenameDatabaseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(mockDatabase);
        }

        // Act
        var response = await ExecuteCommandAsync(commandArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries));

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
    public async Task ExecuteAsync_HandlesDatabaseNotFound()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "Database not found");
        Service.RenameDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "missing",
            "--new-database-name", "newdb");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictWhenNewNameExists()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "Database name already exists");
        Service.RenameDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "existingdb");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("conflict", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not already exist", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBadRequest()
    {
        // Arrange
        var badRequestException = new RequestFailedException((int)HttpStatusCode.BadRequest, "Invalid rename operation");
        Service.RenameDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(badRequestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "invalid-name!");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid database rename operation", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGeneralException()
    {
        // Arrange
        Service.RenameDatabaseAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "newdb");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Unexpected error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithSubscriptionFromEnvironment_Succeeds()
    {
        // Arrange - Test when subscription comes from environment variable
        TestEnvironment.SetAzureSubscriptionId("env-sub-id");

        var mockDatabase = new SqlDatabase(
            Name: "newdb",
            Id: "/subscriptions/env-sub-id/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/databases/newdb",
            Type: "Microsoft.Sql/servers/databases",
            Location: "East US",
            Sku: null,
            Status: "Online",
            Collation: "SQL_Latin1_General_CP1_CI_AS",
            CreationDate: DateTimeOffset.UtcNow,
            MaxSizeBytes: 2147483648,
            ServiceLevelObjective: "S0",
            Edition: "Standard",
            ElasticPoolName: null,
            EarliestRestoreDate: DateTimeOffset.UtcNow,
            ReadScale: "Disabled",
            ZoneRedundant: false
        );

        Service.RenameDatabaseAsync(
            Arg.Is("server1"),
            Arg.Is("olddb"),
            Arg.Is("newdb"),
            Arg.Is("rg"),
            Arg.Is("env-sub-id"),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDatabase);

        // Act
        var response = await ExecuteCommandAsync(
            "--resource-group", "rg",
            "--server", "server1",
            "--database", "olddb",
            "--new-database-name", "newdb");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }
}
