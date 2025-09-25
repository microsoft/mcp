// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Commands.CacheForRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using CacheModel = Azure.Mcp.Tools.Redis.Models.CacheForRedis.Cache;

namespace Azure.Mcp.Tools.Redis.UnitTests.CacheForRedis;

public class CacheListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisService _redisService;
    private readonly ILogger<CacheListCommand> _logger;

    public CacheListCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<CacheListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_redisService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCaches_WhenCachesExist()
    {
        var expectedCaches = new CacheModel[] { new() { Name = "cache1" }, new() { Name = "cache2" } };
        _redisService.ListCachesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedCaches);

        var command = new CacheListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.CacheListCommandResult);

        Assert.NotNull(result);
        Assert.Collection(result.Caches,
            item => Assert.Equal("cache1", item.Name),
            item => Assert.Equal("cache2", item.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoCaches()
    {
        _redisService.ListCachesAsync("sub123").Returns([]);

        var command = new CacheListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.CacheListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Caches);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _redisService.ListCachesAsync("sub123", Arg.Any<string>(), Arg.Any<AuthMethod>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new CacheListCommand(_logger);

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
        var command = new CacheListCommand(_logger);
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
}
