// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using CacheModel = Azure.Mcp.Tools.Redis.Models.Resource;

namespace Azure.Mcp.Tools.Redis.UnitTests;

public class ResourceListCommandTests
{
    private readonly IRedisService _redisService;
    private readonly ILogger<ResourceListCommand> _logger;
    private readonly CommandContext _context;
    private readonly ResourceListCommand _command;
    private readonly Command _commandDefinition;

    public ResourceListCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<ResourceListCommand>>();
        _command = new ResourceListCommand(_redisService, _logger);
        _commandDefinition = _command.GetCommand();
        _context = new CommandContext(new ServiceCollection().BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCaches_WhenCachesExist()
    {
        // Arrange
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" }, new() { Name = "cache2" } };
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item => Assert.Equal("cache1", item.Name),
            item => Assert.Equal("cache2", item.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoCaches()
    {
        // Arrange
        _redisService.ListResourcesAsync("sub123", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>()).Returns([]);

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = DeserializeResult(response.Results);

        Assert.NotNull(result);
        Assert.Empty(result.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        // Arrange
        var argsList = new List<string>();
        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("sub123");
        }

        var args = _commandDefinition.Parse([.. argsList]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
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
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);

        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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

        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);
        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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

        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedCaches);
        var args = _commandDefinition.Parse(["--subscription", "sub123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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

    private static ResourceListCommand.ResourceListCommandResult DeserializeResult(object results)
    {
        var json = JsonSerializer.Serialize(results);
        return JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult)!;
    }
}
