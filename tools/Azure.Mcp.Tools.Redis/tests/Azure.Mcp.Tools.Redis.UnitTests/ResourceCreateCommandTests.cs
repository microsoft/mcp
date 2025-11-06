// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Redis.UnitTests;

public class ResourceCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IRedisService _redisService;
    private readonly ILogger<ResourceCreateCommand> _logger;

    public ResourceCreateCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<ResourceCreateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_redisService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithBasicParameters()
    {
        var expectedResource = new Resource
        {
            Name = "test-redis",
            Type = "AzureManagedRedis",
            ResourceGroupName = "test-rg",
            SubscriptionId = "sub123",
            Location = "eastus",
            Sku = "Balanced_B0",
            Status = "Creating"
        };

        _redisService.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
        .Returns(expectedResource);

        var command = new ResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis",
            "--location", "eastus",
            "--sku", "Balanced_B0"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis", result.Resource.Name);
        Assert.Equal("AzureManagedRedis", result.Resource.Type);
        Assert.Equal("test-rg", result.Resource.ResourceGroupName);
        Assert.Equal("sub123", result.Resource.SubscriptionId);
        Assert.Equal("eastus", result.Resource.Location);
        Assert.Equal("Balanced_B0", result.Resource.Sku);
        Assert.Equal("Creating", result.Resource.Status);
    }

    [Theory]
    [InlineData("--subscription")]
    [InlineData("--resource-group")]
    [InlineData("--resource")]
    [InlineData("--location")]
    public async Task ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing(string missingParameter)
    {
        var command = new ResourceCreateCommand(_logger);
        var argsList = new List<string>();

        if (missingParameter != "--subscription")
        {
            argsList.Add("--subscription");
            argsList.Add("sub123");
        }
        if (missingParameter != "--resource-group")
        {
            argsList.Add("--resource-group");
            argsList.Add("test-rg");
        }
        if (missingParameter != "--resource")
        {
            argsList.Add("--resource");
            argsList.Add("test-redis");
        }
        if (missingParameter != "--location")
        {
            argsList.Add("--location");
            argsList.Add("eastus");
        }

        var args = command.GetCommand().Parse([.. argsList]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesDownstreamException()
    {
        var expectedError = "Resource group 'test-rg' not found. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        
        _redisService.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
        .ThrowsAsync(new Exception("Resource group 'test-rg' not found"));

        var command = new ResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis",
            "--location", "eastus",
            "--sku", "Balanced_B0"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);

        await _redisService.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithModules()
    {
        var expectedResource = new Resource
        {
            Name = "test-redis-with-modules",
            Type = "AzureManagedRedis",
            ResourceGroupName = "test-rg",
            SubscriptionId = "sub123",
            Location = "eastus",
            Sku = "Balanced_B0",
            Status = "Creating"
        };

        _redisService.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-modules",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            Arg.Is<string[]>(modules => 
            modules != null && 
                modules.Length == 2 && 
                modules.Contains("RedisBloom") && 
                modules.Contains("RedisJSON")),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
        .Returns(expectedResource);

        var command = new ResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-with-modules",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--modules", "RedisBloom", "RedisJSON"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-with-modules", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await _redisService.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-modules",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            Arg.Is<string[]>(modules => 
                modules != null && 
                modules.Length == 2 && 
                modules.Contains("RedisBloom") && 
                modules.Contains("RedisJSON")),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithAccessKeyAuthenticationEnabled()
    {
        var expectedResource = new Resource
        {
            Name = "test-redis-with-keys",
            Type = "AzureManagedRedis",
            ResourceGroupName = "test-rg",
            SubscriptionId = "sub123",
            Location = "eastus",
            Sku = "Balanced_B0",
            Status = "Creating"
        };

        _redisService.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-keys",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
        .Returns(expectedResource);

        var command = new ResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-with-keys",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--access-keys-authentication", "true"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-with-keys", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await _redisService.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-keys",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithAllOptionalParameters()
    {
        var expectedResource = new Resource
        {
            Name = "test-redis-full",
            Type = "AzureManagedRedis",
            ResourceGroupName = "test-rg",
            SubscriptionId = "sub123",
            Location = "eastus",
            Sku = "Balanced_B0",
            Status = "Creating"
        };

        _redisService.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-full",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Is<string[]>(modules => 
                modules != null && 
                modules.Length == 1 && 
                modules.Contains("RedisJSON")),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
        .Returns(expectedResource);

        var command = new ResourceCreateCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-full",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--access-keys-authentication", "true",
            "--modules", "RedisJSON"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-full", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await _redisService.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-full",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Is<string[]>(modules => 
                modules != null && 
                modules.Length == 1 && 
                modules.Contains("RedisJSON")),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }
}
