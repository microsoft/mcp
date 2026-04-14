// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Caching;

public class CacheServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSingleUserCliCacheService_ShouldRegisterSingleUserCliCacheService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSingleton(Substitute.For<IMemoryCache>());
        services.AddSingleUserCliCacheService(disabled: false);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.IsType<SingleUserCliCacheService>(cacheService);
    }

    [Fact]
    public void AddSingleUserCliCacheService_WithDisabled_ShouldRegisterNoopCacheService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSingleUserCliCacheService(disabled: true);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.IsType<NoopCacheService>(cacheService);
    }

    [Fact]
    public void AddSingleUserCliCacheService_ShouldNotOverrideExistingCacheService()
    {
        // Arrange
        var services = new ServiceCollection();
        var customCacheService = Substitute.For<ICacheService>();
        services.AddSingleton(customCacheService);

        // Act
        services.AddSingleUserCliCacheService(disabled: false);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.Same(customCacheService, cacheService);
    }

    [Fact]
    public void AddSingleUserCliCacheService_WithDisabled_ShouldNotOverrideExistingCacheService()
    {
        // Arrange
        var services = new ServiceCollection();
        var customCacheService = Substitute.For<ICacheService>();
        services.AddSingleton(customCacheService);

        // Act
        services.AddSingleUserCliCacheService(disabled: true);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.Same(customCacheService, cacheService);
    }

    [Fact]
    public void AddHttpServiceCacheService_ShouldRegisterHttpServiceCacheService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpServiceCacheService(disabled: false);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.IsType<HttpServiceCacheService>(cacheService);
    }

    [Fact]
    public void AddHttpServiceCacheService_WithDisabled_ShouldRegisterNoopCacheService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpServiceCacheService(disabled: true);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.IsType<NoopCacheService>(cacheService);
    }

    [Fact]
    public void AddHttpServiceCacheService_ShouldOverrideExistingCacheService()
    {
        // Arrange
        var services = new ServiceCollection();
        var customCacheService = Substitute.For<ICacheService>();
        services.AddSingleton(customCacheService);

        // Act
        services.AddHttpServiceCacheService(disabled: false);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.IsType<HttpServiceCacheService>(cacheService);
    }

    [Fact]
    public void AddHttpServiceCacheService_WithDisabled_ShouldOverrideExistingCacheService()
    {
        // Arrange
        var services = new ServiceCollection();
        var customCacheService = Substitute.For<ICacheService>();
        services.AddSingleton(customCacheService);

        // Act
        services.AddHttpServiceCacheService(disabled: true);
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Assert
        Assert.IsType<NoopCacheService>(cacheService);
    }
}
