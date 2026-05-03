// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.ElasticPool;

public class ElasticPoolListCommandTests : CommandUnitTestsBase<ElasticPoolListCommand, ISqlService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.True(CommandDefinition.Description.Length <= 1024, "Description should not exceed 1024 characters");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsElasticPools()
    {
        // Arrange
        var mockElasticPools = new ResourceQueryResults<SqlElasticPool>(
        [
            new(
                Name: "pool1",
                Id: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/server1/elasticPools/pool1",
                Type: "Microsoft.Sql/servers/elasticPools",
                Location: "East US",
                Sku: new ElasticPoolSku("StandardPool", "Standard", 100, null, null),
                State: "Ready",
                CreationDate: DateTimeOffset.UtcNow,
                MaxSizeBytes: 5368709120,
                PerDatabaseSettings: new ElasticPoolPerDatabaseSettings(0, 25),
                ZoneRedundant: false,
                LicenseType: "LicenseIncluded",
                DatabaseDtuMin: 0,
                DatabaseDtuMax: 25,
                Dtu: 100,
                StorageMB: 5120
            )
        ], false);

        Service.GetElasticPoolsAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockElasticPools);

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
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyList_ReturnsEmptyResults()
    {
        // Arrange
        var mockElasticPools = new ResourceQueryResults<SqlElasticPool>([], false);

        Service.GetElasticPoolsAsync(
            Arg.Is("server1"),
            Arg.Is("rg"),
            Arg.Is("sub"),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(mockElasticPools);

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
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetElasticPoolsAsync(
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
    public async Task ExecuteAsync_HandlesRequestFailedException_NotFound()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.NotFound, "Server not found");
        Service.GetElasticPoolsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--resource-group", "rg",
            "--server", "server1");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("SQL server not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesRequestFailedException_Forbidden()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden");
        Service.GetElasticPoolsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(requestException);

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
    [InlineData("--subscription sub --resource-group rg --server server1", true)]
    [InlineData("--resource-group rg --server server1", false)]  // Missing subscription
    [InlineData("--subscription sub --server server1", false)]   // Missing resource group
    [InlineData("--subscription sub --resource-group rg", false)] // Missing server
    [InlineData("", false)]  // Missing all required options
    public async Task ExecuteAsync_ValidatesRequiredParameters(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GetElasticPoolsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<SqlElasticPool>([], false));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Contains("required", response.Message.ToLower());
        }
    }
}
