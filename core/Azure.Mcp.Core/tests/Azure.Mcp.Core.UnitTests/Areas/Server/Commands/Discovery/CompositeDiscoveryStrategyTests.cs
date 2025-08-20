// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CompositeDiscoveryStrategyTests
{
    private static IMcpDiscoveryStrategy CreateMockStrategy(params IMcpServerProvider[] providers)
    {
        var strategy = Substitute.For<IMcpDiscoveryStrategy>();
        strategy.DiscoverServersAsync().Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>(providers));
        return strategy;
    }

    private static IMcpServerProvider CreateMockProvider(string id, string? name = null, string? description = null)
    {
        var provider = Substitute.For<IMcpServerProvider>();
        provider.CreateMetadata().Returns(new McpServerMetadata
        {
            Id = id,
            Name = name ?? id,
            Description = description ?? $"Description for {id}"
        });
        return provider;
    }

    private static CompositeDiscoveryStrategy CreateCompositeStrategy(IEnumerable<IMcpDiscoveryStrategy> strategies)
    {
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CompositeDiscoveryStrategy>>();
        return new CompositeDiscoveryStrategy(strategies, logger);
    }

    [Fact]
    public void Constructor_WithValidStrategies_InitializesCorrectly()
    {
        // Arrange
        var strategy1 = Substitute.For<IMcpDiscoveryStrategy>();
        var strategy2 = Substitute.For<IMcpDiscoveryStrategy>();

        // Act
        var composite = CreateCompositeStrategy(new[] { strategy1, strategy2 });

        // Assert
        Assert.NotNull(composite);
        Assert.IsType<CompositeDiscoveryStrategy>(composite);
        Assert.IsAssignableFrom<BaseDiscoveryStrategy>(composite);
    }

    [Fact]
    public void Constructor_WithNullStrategies_ThrowsArgumentNullException()
    {
        // Act & Assert
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CompositeDiscoveryStrategy>>();
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CompositeDiscoveryStrategy(null!, logger));
        Assert.Equal("strategies", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyStrategies_ThrowsArgumentException()
    {
        // Act & Assert
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CompositeDiscoveryStrategy>>();
        var exception = Assert.Throws<ArgumentException>(() =>
            new CompositeDiscoveryStrategy(Array.Empty<IMcpDiscoveryStrategy>(), logger));
        Assert.Equal("strategies", exception.ParamName);
        Assert.Contains("At least one discovery strategy must be provided", exception.Message);
    }

    [Fact]
    public void DiscoverServersAsync_WithEmptyStrategies_ThrowsArgumentException()
    {
        // Act & Assert
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<CompositeDiscoveryStrategy>>();
        var exception = Assert.Throws<ArgumentException>(() =>
            new CompositeDiscoveryStrategy(Array.Empty<IMcpDiscoveryStrategy>(), logger));
        Assert.Equal("strategies", exception.ParamName);
        Assert.Contains("At least one discovery strategy must be provided", exception.Message);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithSingleStrategy_ReturnsProvidersFromThatStrategy()
    {
        // Arrange
        var provider1 = CreateMockProvider("test1");
        var provider2 = CreateMockProvider("test2");
        var strategy = CreateMockStrategy(provider1, provider2);
        var composite = CreateCompositeStrategy(new[] { strategy });

        // Act
        var result = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(provider1, result);
        Assert.Contains(provider2, result);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithMultipleStrategies_AggregatesAllResults()
    {
        // Arrange
        var provider1 = CreateMockProvider("strategy1-provider1");
        var provider2 = CreateMockProvider("strategy1-provider2");
        var provider3 = CreateMockProvider("strategy2-provider1");
        var provider4 = CreateMockProvider("strategy2-provider2");

        var strategy1 = CreateMockStrategy(provider1, provider2);
        var strategy2 = CreateMockStrategy(provider3, provider4);
        var composite = CreateCompositeStrategy(new[] { strategy1, strategy2 });

        // Act
        var result = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(provider1, result);
        Assert.Contains(provider2, result);
        Assert.Contains(provider3, result);
        Assert.Contains(provider4, result);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithStrategiesReturningEmpty_HandlesGracefully()
    {
        // Arrange
        var provider1 = CreateMockProvider("active-provider");
        var activeStrategy = CreateMockStrategy(provider1);
        var emptyStrategy1 = CreateMockStrategy(); // No providers
        var emptyStrategy2 = CreateMockStrategy(); // No providers

        var composite = CreateCompositeStrategy(new[] { activeStrategy, emptyStrategy1, emptyStrategy2 });

        // Act
        var result = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(provider1, result);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithAllEmptyStrategies_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyStrategy1 = CreateMockStrategy();
        var emptyStrategy2 = CreateMockStrategy();
        var composite = CreateCompositeStrategy(new[] { emptyStrategy1, emptyStrategy2 });

        // Act
        var result = await composite.DiscoverServersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DiscoverServersAsync_ExecutesAllStrategiesInParallel()
    {
        // Arrange
        var provider1 = CreateMockProvider("provider1");
        var provider2 = CreateMockProvider("provider2");
        var provider3 = CreateMockProvider("provider3");

        var strategy1 = CreateMockStrategy(provider1);
        var strategy2 = CreateMockStrategy(provider2);
        var strategy3 = CreateMockStrategy(provider3);

        var composite = CreateCompositeStrategy(new[] { strategy1, strategy2, strategy3 });

        // Act
        var result = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(provider1, result);
        Assert.Contains(provider2, result);
        Assert.Contains(provider3, result);

        // Verify all strategies were called
        await strategy1.Received(1).DiscoverServersAsync();
        await strategy2.Received(1).DiscoverServersAsync();
        await strategy3.Received(1).DiscoverServersAsync();
    }

    [Fact]
    public async Task DiscoverServersAsync_PreservesOrderFromStrategies()
    {
        // Arrange
        var provider1 = CreateMockProvider("provider1");
        var provider2 = CreateMockProvider("provider2");
        var provider3 = CreateMockProvider("provider3");
        var provider4 = CreateMockProvider("provider4");

        var strategy1 = CreateMockStrategy(provider1, provider2);
        var strategy2 = CreateMockStrategy(provider3, provider4);

        var composite = CreateCompositeStrategy(new[] { strategy1, strategy2 });

        // Act
        var result = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        Assert.Equal(4, result.Count);
        // Results should maintain the order: strategy1's providers first, then strategy2's providers
        Assert.Equal(provider1, result[0]);
        Assert.Equal(provider2, result[1]);
        Assert.Equal(provider3, result[2]);
        Assert.Equal(provider4, result[3]);
    }

    [Fact]
    public async Task DiscoverServersAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        var provider1 = CreateMockProvider("provider1");
        var provider2 = CreateMockProvider("provider2");
        var strategy = CreateMockStrategy(provider1, provider2);
        var composite = CreateCompositeStrategy(new[] { strategy });

        // Act
        var result1 = (await composite.DiscoverServersAsync()).ToList();
        var result2 = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        Assert.Equal(result1.Count, result2.Count);
        Assert.Equal(2, result1.Count);
        Assert.Equal(2, result2.Count);

        // Should call the underlying strategy each time
        await strategy.Received(2).DiscoverServersAsync();
    }

    [Fact]
    public async Task DiscoverServersAsync_WithDuplicateProviders_IncludesAllProviders()
    {
        // Arrange
        var provider1 = CreateMockProvider("same-provider");
        var provider2 = CreateMockProvider("same-provider"); // Same ID but different instance

        var strategy1 = CreateMockStrategy(provider1);
        var strategy2 = CreateMockStrategy(provider2);

        var composite = CreateCompositeStrategy(new[] { strategy1, strategy2 });

        // Act
        var result = (await composite.DiscoverServersAsync()).ToList();

        // Assert
        // CompositeDiscoveryStrategy doesn't deduplicate - it includes all providers
        Assert.Equal(2, result.Count);
        Assert.Contains(provider1, result);
        Assert.Contains(provider2, result);
    }

    [Fact]
    public async Task DiscoverServersAsync_InheritsFromBaseDiscoveryStrategy()
    {
        // Arrange
        var strategy = CreateMockStrategy();
        var composite = CreateCompositeStrategy(new[] { strategy });

        // Act & Assert
        Assert.IsAssignableFrom<BaseDiscoveryStrategy>(composite);

        // Should implement the base contract
        var result = await composite.DiscoverServersAsync();
        Assert.NotNull(result);
    }

    // Keep original tests for backward compatibility
    [Fact]
    public async Task ShouldAggregateResults()
    {
        var mockStrategy1 = Substitute.For<IMcpDiscoveryStrategy>();
        var mockStrategy2 = Substitute.For<IMcpDiscoveryStrategy>();
        var provider1 = Substitute.For<IMcpServerProvider>();
        var provider2 = Substitute.For<IMcpServerProvider>();
        mockStrategy1.DiscoverServersAsync().Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>(new[] { provider1 }));
        mockStrategy2.DiscoverServersAsync().Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>(new[] { provider2 }));
        var composite = CreateCompositeStrategy(new[] { mockStrategy1, mockStrategy2 });
        var result = await composite.DiscoverServersAsync();
        Assert.Contains(provider1, result);
        Assert.Contains(provider2, result);
    }

    [Fact]
    public async Task ShouldAggregateResults_ReturnsAllProviders()
    {
        var mockStrategy1 = Substitute.For<IMcpDiscoveryStrategy>();
        var mockStrategy2 = Substitute.For<IMcpDiscoveryStrategy>();
        var provider1 = Substitute.For<IMcpServerProvider>();
        var provider2 = Substitute.For<IMcpServerProvider>();
        provider1.CreateMetadata().Returns(new McpServerMetadata { Id = "one", Name = "one", Description = "desc1" });
        provider2.CreateMetadata().Returns(new McpServerMetadata { Id = "two", Name = "two", Description = "desc2" });
        mockStrategy1.DiscoverServersAsync().Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>(new[] { provider1 }));
        mockStrategy2.DiscoverServersAsync().Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>(new[] { provider2 }));
        var composite = CreateCompositeStrategy(new[] { mockStrategy1, mockStrategy2 });
        var result = (await composite.DiscoverServersAsync()).ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.CreateMetadata().Id == "one");
        Assert.Contains(result, p => p.CreateMetadata().Id == "two");
    }

    [Fact]
    public async Task DiscoverServersAsync_WithSingleEmptyStrategy_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyStrategy = CreateMockStrategy(); // No providers
        var composite = CreateCompositeStrategy(new[] { emptyStrategy });

        // Act
        var result = await composite.DiscoverServersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
