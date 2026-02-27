// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Redis.UnitTests;

public class ResourceCreateCommandTests
{
    private readonly IRedisService _redisService;
    private readonly ILogger<ResourceCreateCommand> _logger;
    private readonly CommandContext _context;
    private readonly ResourceCreateCommand _command;
    private readonly Command _commandDefinition;

    public ResourceCreateCommandTests()
    {
        _redisService = Substitute.For<IRedisService>();
        _logger = Substitute.For<ILogger<ResourceCreateCommand>>();
        _command = new ResourceCreateCommand(_redisService, _logger);
        _commandDefinition = _command.GetCommand();
        _context = new CommandContext(new ServiceCollection().BuildServiceProvider());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithBasicParameters()
    {
        // Arrange
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis",
            "--location", "eastus",
            "--sku", "Balanced_B0"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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
        // Arrange
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

        var args = _commandDefinition.Parse([.. argsList]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesDownstreamException()
    {
        // Arrange
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .ThrowsAsync(new Exception("Resource group 'test-rg' not found"));

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis",
            "--location", "eastus",
            "--sku", "Balanced_B0"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithModules()
    {
        // Arrange
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-with-modules",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--modules", "RedisBloom", "RedisJSON"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithAccessKeyAuthenticationEnabled()
    {
        // Arrange
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-with-keys",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--access-keys-authentication", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithAllOptionalParameters()
    {
        // Arrange
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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        var args = _commandDefinition.Parse([
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-full",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--access-keys-authentication", "true",
            "--modules", "RedisJSON"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResult(response.Results!);

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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    private static void AssertSuccessResponse(CommandResponse response)
    {
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);
    }

    private static ResourceCreateCommand.ResourceCreateCommandResult DeserializeResult(object results)
    {
        var json = JsonSerializer.Serialize(results);
        return JsonSerializer.Deserialize(json, RedisJsonContext.Default.ResourceCreateCommandResult)!;
    }

}   
