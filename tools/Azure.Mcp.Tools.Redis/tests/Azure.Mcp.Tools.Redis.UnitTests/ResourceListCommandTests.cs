// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using CacheModel = Azure.Mcp.Tools.Redis.Models.Resource;

namespace Azure.Mcp.Tools.Redis.UnitTests;

public class ResourceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisService _redisService;
    private readonly ILogger<ResourceListCommand> _logger;

    public ResourceListCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<ResourceListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_redisService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCaches_WhenCachesExist()
    {
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" }, new() { Name = "cache2" } };
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCaches);

        var command = new ResourceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item => Assert.Equal("cache1", item.Name),
            item => Assert.Equal("cache2", item.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoCaches()
    {
        _redisService.ListResourcesAsync("sub123").Returns([]);

        var command = new ResourceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Resources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new ResourceListCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var command = new ResourceListCommand(_logger);
        var argsList = new List<string>();
        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("sub123");
        }

        var args = command.GetCommand().Parse([.. argsList]);

        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccessPolicyAssignments_WhenAssignmentsExist()
    {
        var expectedAssignments = new AccessPolicyAssignment[]
        {
            new() { AccessPolicyName = "policy1", IdentityName = "identity1", ProvisioningState = "Succeeded" },
            new() { AccessPolicyName = "policy2", IdentityName = "identity2", ProvisioningState = "Succeeded" }
        };

        var expectedCaches = new CacheModel[] { new() { Name = "cache1" }, new() { Name = "cache2", AccessPolicyAssignments = expectedAssignments } };
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCaches);

        var command = new ResourceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult);

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
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" } };
        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCaches);

        var command = new ResourceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult);

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

        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCaches);
        var command = new ResourceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult);

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
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" } };

        _redisService.ListResourcesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCaches);
        var command = new ResourceListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Resources,
            item =>
            {
                Assert.Equal("cache1", item.Name);
                Assert.Null(item.Databases);
            });
    }
}
