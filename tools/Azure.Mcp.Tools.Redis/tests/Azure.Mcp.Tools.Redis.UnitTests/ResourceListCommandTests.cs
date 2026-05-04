// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using CacheModel = Azure.Mcp.Tools.Redis.Models.Resource;

namespace Azure.Mcp.Tools.Redis.UnitTests;

public class ResourceListCommandTests : CommandUnitTestsBase<ResourceListCommand, IRedisService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListResourcesAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCaches_WhenCachesExist()
    {
        // Arrange
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" }, new() { Name = "cache2" } };
        Service.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item => Assert.Equal("cache1", item.Name),
            item => Assert.Equal("cache2", item.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoCaches()
    {
        // Arrange
        Service.ListResourcesAsync("sub123", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        Service.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccessPolicyAssignments_WhenAssignmentsExist()
    {
        // Arrange
        var expectedAssignments = new AccessPolicyAssignment[]
        {
            new() { AccessPolicyName = "policy1", IdentityName = "identity1", ProvisioningState = "Succeeded" },
            new() { AccessPolicyName = "policy2", IdentityName = "identity2", ProvisioningState = "Succeeded" }
        };

        var expectedCaches = new CacheModel[] { new() { Name = "cache1" }, new() { Name = "cache2", AccessPolicyAssignments = expectedAssignments } };
        Service.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item => Assert.Equal("cache1", item.Name),
            item =>
            {
                Assert.Equal("cache2", item.Name);
                Assert.NotNull(item.AccessPolicyAssignments);
                Assert.Collection(item.AccessPolicyAssignments,
                    ap => Assert.Equal("policy1", ap.AccessPolicyName),
                    ap => Assert.Equal("policy2", ap.AccessPolicyName));
            });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccessPolicyAssignments()
    {
        // Arrange
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" } };
        Service.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item =>
            {
                Assert.Equal("cache1", item.Name);
                Assert.Null(item.AccessPolicyAssignments);
            });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDatabases_WhenDatabasesExist()
    {
        // Arrange
        var expectedDatabases = new Database[]
        {
            new()
            {
                Name = "db1",
                ClusterName = "cluster1",
                ResourceGroupName = "rg1",
                SubscriptionId = "sub123",
                Port = 10000,
                ProvisioningState = "Succeeded"
            },
            new()
            {
                Name = "db2",
                ClusterName = "cluster1",
                ResourceGroupName = "rg1",
                SubscriptionId = "sub123",
                Port = 10001,
                ProvisioningState = "Succeeded"
            }
        };

        var expectedCaches = new CacheModel[] { new() { Name = "cache1", Databases = expectedDatabases } };

        Service.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item =>
            {
                Assert.Equal("cache1", item.Name);
                Assert.NotNull(item.Databases);
                Assert.Collection(item.Databases,
                    db => Assert.Equal("db1", db.Name),
                    db => Assert.Equal("db2", db.Name));
            });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDatabases()
    {
        // Arrange
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" } };

        Service.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item =>
            {
                Assert.Equal("cache1", item.Name);
                Assert.Null(item.Databases);
            });
    }

    private static void AssertSuccessResponse(CommandResponse response)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
    }
}
