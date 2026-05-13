// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Redis.UnitTests;

public class ResourceCreateCommandTests : CommandUnitTestsBase<ResourceCreateCommand, IRedisService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group test-rg --resource test-redis --location eastus", true)]
    [InlineData("--resource-group test-rg --resource test-redis --location eastus", false)]
    [InlineData("--subscription sub123 --resource test-redis --location eastus", false)]
    [InlineData("--subscription sub123 --resource-group test-rg --location eastus", false)]
    [InlineData("--subscription sub123 --resource-group test-rg --resource test-redis", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateResourceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<bool?>(),
                Arg.Any<string[]?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new Resource { Name = "test-redis" });
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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

        Service.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            false,
            false,
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis",
            "--location", "eastus",
            "--sku", "Balanced_B0");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis", result.Resource.Name);
        Assert.Equal("AzureManagedRedis", result.Resource.Type);
        Assert.Equal("test-rg", result.Resource.ResourceGroupName);
        Assert.Equal("sub123", result.Resource.SubscriptionId);
        Assert.Equal("eastus", result.Resource.Location);
        Assert.Equal("Balanced_B0", result.Resource.Sku);
        Assert.Equal("Creating", result.Resource.Status);

        await Service.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            false,
            false,
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var expectedError = "Resource group 'test-rg' not found. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .ThrowsAsync(new Exception("Resource group 'test-rg' not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis",
            "--location", "eastus",
            "--sku", "Balanced_B0");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);

        await Service.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
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

        Service.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-modules",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
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

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-with-modules",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--modules", "RedisBloom", "RedisJSON");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-with-modules", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await Service.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-modules",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
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

        Service.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-keys",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Any<bool?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-with-keys",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--access-keys-authentication", "true");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-with-keys", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await Service.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-with-keys",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Any<bool?>(),
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesResource_WithPublicNetworkAccessEnabled()
    {
        // Arrange
        var expectedResource = new Resource
        {
            Name = "test-redis-public",
            Type = "AzureManagedRedis",
            ResourceGroupName = "test-rg",
            SubscriptionId = "sub123",
            Location = "eastus",
            Sku = "Balanced_B0",
            Status = "Creating"
        };

        Service.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-public",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
            true,
            Arg.Any<string[]?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-public",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--public-network-access", "true");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-public", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await Service.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-public",
            "eastus",
            "Balanced_B0",
            Arg.Any<bool?>(),
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

        Service.CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-full",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Any<bool?>(),
            Arg.Is<string[]>(modules =>
                modules != null &&
                modules.Length == 1 &&
                modules.Contains("RedisJSON")),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
        .Returns(expectedResource);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "test-rg",
            "--resource", "test-redis-full",
            "--location", "eastus",
            "--sku", "Balanced_B0",
            "--access-keys-authentication", "true",
            "--modules", "RedisJSON");

        // Assert
        AssertSuccessResponse(response);

        var result = DeserializeResponse(response, RedisJsonContext.Default.ResourceCreateCommandResult);

        Assert.NotNull(result);
        Assert.Equal("test-redis-full", result.Resource.Name);
        Assert.Equal("Creating", result.Resource.Status);

        await Service.Received(1).CreateResourceAsync(
            "sub123",
            "test-rg",
            "test-redis-full",
            "eastus",
            "Balanced_B0",
            true,
            Arg.Any<bool?>(),
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
}
