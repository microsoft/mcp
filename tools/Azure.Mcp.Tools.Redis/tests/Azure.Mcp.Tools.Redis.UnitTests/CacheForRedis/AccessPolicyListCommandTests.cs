// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Commands.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Redis.UnitTests.CacheForRedis;

public class AccessPolicyListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisService _redisService;
    private readonly ILogger<AccessPolicyListCommand> _logger;

    public AccessPolicyListCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<AccessPolicyListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_redisService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAccessPolicyAssignments_WhenAssignmentsExist()
    {
        var expectedAssignments = new AccessPolicyAssignment[]
        {
            new() { AccessPolicyName = "policy1", IdentityName = "identity1", ProvisioningState = "Succeeded" },
            new() { AccessPolicyName = "policy2", IdentityName = "identity2", ProvisioningState = "Succeeded" }
        };

        _redisService.ListAccessPolicyAssignmentsAsync(
            "cache1",
            "rg1",
            "sub123",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedAssignments);

        var command = new AccessPolicyListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--cache", "cache1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.AccessPolicyListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedAssignments.Length, result.AccessPolicyAssignments.Count());
        Assert.Collection(result.AccessPolicyAssignments,
            item => Assert.Equal("policy1", item.AccessPolicyName),
            item => Assert.Equal("policy2", item.AccessPolicyName));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAccessPolicyAssignments()
    {
        _redisService.ListAccessPolicyAssignmentsAsync(
            "cache1",
            "rg1",
            "sub123",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var command = new AccessPolicyListCommand(_logger);
        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--cache", "cache1"]);
        var context = new CommandContext(_serviceProvider);
        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.AccessPolicyListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.AccessPolicyAssignments);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _redisService.ListAccessPolicyAssignmentsAsync(
            "cache1",
            "rg1",
            "sub123",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception("Test error"));

        var command = new AccessPolicyListCommand(_logger);

        var args = command.GetCommand().Parse(["--subscription", "sub123", "--resource-group", "rg1", "--cache", "cache1"]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--cache")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string parameterToKeep)
    {
        var command = new AccessPolicyListCommand(_logger);

        var options = new List<string>();
        if (parameterToKeep == "--subscription")
            options.AddRange(["--subscription", "sub123"]);
        if (parameterToKeep == "--resource-group")
            options.AddRange(["--resource-group", "rg1"]);
        if (parameterToKeep == "--cache")
            options.AddRange(["--cache", "cache1"]);


        var parseResult = command.GetCommand().Parse([.. options]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, parseResult);

        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
