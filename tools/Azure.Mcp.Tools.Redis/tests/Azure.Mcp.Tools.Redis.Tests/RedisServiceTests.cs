// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Services;
using Azure.ResourceManager.Redis;
using Azure.ResourceManager.Redis.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Redis.Tests;

public class RedisServiceTests
{
    private static RedisService CreateService(ISubscriptionService subscriptionService)
    {
        var tenantService = Substitute.For<ITenantService>();
        var logger = Substitute.For<ILogger<RedisService>>();
        return new RedisService(subscriptionService, tenantService, logger);
    }

    [Fact]
    public async Task ListResourcesAsync_ThrowsKeyNotFoundException_WhenSubscriptionNotFound()
    {
        // Arrange - GetSubscription returning null should surface as a typed
        // not-found exception (404) rather than a plain Exception (500). See #458.
        var subscriptionService = Substitute.For<ISubscriptionService>();
        subscriptionService.GetSubscription("sub123", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns((SubscriptionResource)null!);

        var service = CreateService(subscriptionService);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ListResourcesAsync("sub123", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateResourceAsync_ThrowsKeyNotFoundException_WhenSubscriptionNotFound()
    {
        // Arrange
        var subscriptionService = Substitute.For<ISubscriptionService>();
        subscriptionService.GetSubscription("sub123", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns((SubscriptionResource)null!);

        var service = CreateService(subscriptionService);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateResourceAsync("sub123", "rg1", "cache1", "eastus", "Balanced_B0", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void MapAcrResource_WithNullRedisConfiguration_DoesNotThrow_AndYieldsNullConfigFields()
    {
        // Arrange - Basic/Standard tier caches can be created without explicit
        // configuration, in which case RedisConfiguration is null (see issue #457).
        var data = ArmRedisModelFactory.RedisData(
            id: new ResourceIdentifier("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg1/providers/Microsoft.Cache/Redis/cache1"),
            name: "cache1",
            location: AzureLocation.EastUS,
            sku: new RedisSku(RedisSkuName.Basic, RedisSkuFamily.BasicOrStandard, 0),
            redisConfiguration: null,
            zonalAllocationPolicy: null);

        // Act - mapping must not throw when RedisConfiguration is null.
        var resource = RedisService.MapAcrResource(data, []);

        // Assert
        Assert.Equal("cache1", resource.Name);
        Assert.Equal("AzureCacheForRedis", resource.Type);
        Assert.Equal("rg1", resource.ResourceGroupName);
        Assert.Equal("11111111-1111-1111-1111-111111111111", resource.SubscriptionId);

        // All configuration-backed fields must be null when RedisConfiguration is null.
        Assert.Null(resource.AuthNotRequired);
        Assert.Null(resource.IsRdbBackupEnabled);
        Assert.Null(resource.IsAofBackupEnabled);
        Assert.Null(resource.RdbBackupFrequency);
        Assert.Null(resource.RdbBackupMaxSnapshotCount);
        Assert.Null(resource.MaxFragmentationMemoryReserved);
        Assert.Null(resource.MaxMemoryPolicy);
        Assert.Null(resource.MaxMemoryReserved);
        Assert.Null(resource.MaxMemoryDelta);
        Assert.Null(resource.MaxClients);
        Assert.Null(resource.NotifyKeyspaceEvents);
        Assert.Null(resource.PreferredDataArchiveAuthMethod);
        Assert.Null(resource.PreferredDataPersistenceAuthMethod);
        Assert.Null(resource.ZonalConfiguration);
        Assert.Null(resource.StorageSubscriptionId);
        Assert.Null(resource.IsEntraIDAuthEnabled);
    }

    [Fact]
    public void MapAcrResource_WithRedisConfiguration_MapsConfigFields()
    {
        // Arrange
        var configuration = ArmRedisModelFactory.RedisCommonConfiguration(
            maxClients: "100",
            maxMemoryPolicy: "test-policy",
            isAadEnabled: "True");

        var data = ArmRedisModelFactory.RedisData(
            id: new ResourceIdentifier("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg1/providers/Microsoft.Cache/Redis/cache1"),
            name: "cache1",
            location: AzureLocation.EastUS,
            sku: new RedisSku(RedisSkuName.Standard, RedisSkuFamily.BasicOrStandard, 1),
            redisConfiguration: configuration,
            zonalAllocationPolicy: null);

        // Act
        var resource = RedisService.MapAcrResource(data, []);

        // Assert
        Assert.Equal(100, resource.MaxClients);
        Assert.Equal("test-policy", resource.MaxMemoryPolicy);
        Assert.True(resource.IsEntraIDAuthEnabled);
    }
}
