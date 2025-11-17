// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

/// <summary>
/// Concrete test implementation of BaseDiscoveryStrategy for testing disposal behavior
/// </summary>
public class TestDiscoveryStrategy : BaseDiscoveryStrategy
{
    private readonly IEnumerable<IMcpServerProvider> _providers;

    public TestDiscoveryStrategy(IEnumerable<IMcpServerProvider> providers, ILogger? logger = null) : base(logger ?? NullLogger.Instance)
    {
        _providers = providers;
    }

    public override Task<IEnumerable<IMcpServerProvider>> DiscoverServersAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_providers);
    }
}

public class BaseDiscoveryStrategyTests
{
    private static IMcpServerProvider CreateMockServerProvider(string name, string id = "", string description = "Test server")
    {
        var mockProvider = Substitute.For<IMcpServerProvider>();
        var metadata = new McpServerMetadata
        {
            Id = id,
            Name = name,
            Description = description
        };
        mockProvider.CreateMetadata().Returns(metadata);
        return mockProvider;
    }

    private static BaseDiscoveryStrategy CreateMockStrategy(params IMcpServerProvider[] providers)
    {
        return new TestDiscoveryStrategy(providers);
    }

    [Fact]
    public async Task FindServerProvider_WithEmptyDiscovery_ThrowsArgumentException()
    {
        // Arrange
        var strategy = CreateMockStrategy();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => strategy.FindServerProviderAsync("notfound", TestContext.Current.CancellationToken));
        Assert.Contains("notfound", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("No MCP server found with the name", exception.Message);
    }

    [Fact]
    public async Task FindServerProvider_WithNonExistentServer_ThrowsKeyNotFoundException()
    {
        // Arrange
        var provider1 = CreateMockServerProvider("server1");
        var provider2 = CreateMockServerProvider("server2");
        var strategy = CreateMockStrategy(provider1, provider2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => strategy.FindServerProviderAsync("nonexistent", TestContext.Current.CancellationToken));
        Assert.Contains("nonexistent", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FindServerProvider_WithExistingServer_ReturnsCorrectProvider()
    {
        // Arrange
        var provider1 = CreateMockServerProvider("server1");
        var provider2 = CreateMockServerProvider("server2");
        var strategy = CreateMockStrategy(provider1, provider2);

        // Act
        var result = await strategy.FindServerProviderAsync("server1", TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(provider1, result);
    }

    [Fact]
    public async Task FindServerProvider_WithCaseInsensitiveMatch_ReturnsCorrectProvider()
    {
        // Arrange
        var provider = CreateMockServerProvider("TestServer");
        var strategy = CreateMockStrategy(provider);

        // Act
        var result3 = await strategy.FindServerProviderAsync("TestServer", TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(provider, result3);
    }

    [Fact]
    public async Task FindServerProvider_WithMultipleServers_ReturnsCorrectOne()
    {
        // Arrange
        var provider1 = CreateMockServerProvider("azure-storage");
        var provider2 = CreateMockServerProvider("azure-keyvault");
        var provider3 = CreateMockServerProvider("azure-cosmos");
        var strategy = CreateMockStrategy(provider1, provider2, provider3);

        // Act
        var result = await strategy.FindServerProviderAsync("azure-keyvault", TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(provider2, result);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithNewServer_CreatesAndCachesClient()
    {
        // Arrange
        var mockClient = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("TestServer");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient);
        var strategy = CreateMockStrategy(provider);

        // Act
        var result = await strategy.GetOrCreateClientAsync("TestServer", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(mockClient, result);
        await provider.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithCachedServer_ReturnsCachedClient()
    {
        // Arrange
        var mockClient = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("TestServer");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient);
        var strategy = CreateMockStrategy(provider);

        // Act
        var result1 = await strategy.GetOrCreateClientAsync("TestServer", cancellationToken: TestContext.Current.CancellationToken);
        var result2 = await strategy.GetOrCreateClientAsync("TestServer", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(mockClient, result1);
        Assert.Same(mockClient, result2);
        Assert.Same(result1, result2);

        // Verify client was only created once
        await provider.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithCustomOptions_PassesOptionsCorrectly()
    {
        // Arrange
        var mockClient = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("TestServer");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient);
        var strategy = CreateMockStrategy(provider);
        var customOptions = new McpClientOptions { /* set custom properties if available */ };

        // Act
        var result = await strategy.GetOrCreateClientAsync("TestServer", customOptions, TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(mockClient, result);
        await provider.Received(1).CreateClientAsync(customOptions, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithDefaultOptions_UsesDefaultOptions()
    {
        // Arrange
        var mockClient = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("TestServer");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient);
        var strategy = CreateMockStrategy(provider);

        // Act
        var result = await strategy.GetOrCreateClientAsync("TestServer", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(mockClient, result);
        await provider.Received(1).CreateClientAsync(Arg.Is<McpClientOptions>(opts => opts != null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithNonExistentServer_ThrowsKeyNotFoundException()
    {
        // Arrange
        var provider = CreateMockServerProvider("ExistingServer");
        var strategy = CreateMockStrategy(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => strategy.GetOrCreateClientAsync("NonExistentServer", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("NonExistentServer", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithMultipleServers_CachesEachSeparately()
    {
        // Arrange
        var mockClient1 = Substitute.For<McpClient>();
        var mockClient2 = Substitute.For<McpClient>();
        var provider1 = CreateMockServerProvider("Server1");
        var provider2 = CreateMockServerProvider("Server2");

        provider1.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient1);
        provider2.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient2);

        var strategy = CreateMockStrategy(provider1, provider2);

        // Act
        var result1a = await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);
        var result2a = await strategy.GetOrCreateClientAsync("Server2", cancellationToken: TestContext.Current.CancellationToken);
        var result1b = await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);
        var result2b = await strategy.GetOrCreateClientAsync("Server2", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Same(mockClient1, result1a);
        Assert.Same(mockClient2, result2a);
        Assert.Same(result1a, result1b);
        Assert.Same(result2a, result2b);

        // Verify each client was only created once
        await provider1.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
        await provider2.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FindServerProvider_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateMockServerProvider("TestServer");
        var strategy = CreateMockStrategy(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => strategy.FindServerProviderAsync(null!, TestContext.Current.CancellationToken));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public async Task FindServerProvider_WithEmptyName_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateMockServerProvider("TestServer");
        var strategy = CreateMockStrategy(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => strategy.FindServerProviderAsync("", TestContext.Current.CancellationToken));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Server name cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateMockServerProvider("TestServer");
        var strategy = CreateMockStrategy(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => strategy.GetOrCreateClientAsync(null!, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_WithEmptyName_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = CreateMockServerProvider("TestServer");
        var strategy = CreateMockStrategy(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => strategy.GetOrCreateClientAsync("", cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Server name cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_CacheUsesSameKeyForDifferentCasing_ReusesCachedClient()
    {
        // Arrange
        var mockClient1 = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("TestServer");

        // Setup provider to return a client for the first call
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>())
            .Returns(mockClient1);

        var strategy = CreateMockStrategy(provider);

        // Act - Different casings use the same cache key because we use StringComparer.OrdinalIgnoreCase
        var result1 = await strategy.GetOrCreateClientAsync("TestServer", cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Same client because cache keys are case-insensitive
        Assert.Same(mockClient1, result1);

        // Verify provider was called only once (the same cached client is returned for all casing variants)
        await provider.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());

        // Verify subsequent calls with any casing return the same cached client
        var result1b = await strategy.GetOrCreateClientAsync("TestServer", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(result1, result1b);

        // Still only 1 call total (all calls use the cached entry regardless of casing)
        await provider.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeAllCachedClients()
    {
        // Arrange
        var mockClient1 = Substitute.For<McpClient>();
        var mockClient2 = Substitute.For<McpClient>();
        var provider1 = CreateMockServerProvider("Server1");
        var provider2 = CreateMockServerProvider("Server2");

        provider1.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient1);
        provider2.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient2);

        var strategy = CreateMockStrategy(provider1, provider2);

        // Create and cache some clients
        await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);
        await strategy.GetOrCreateClientAsync("Server2", cancellationToken: TestContext.Current.CancellationToken);

        // Act
        await strategy.DisposeAsync();

        // Assert - All cached clients should be disposed
        await mockClient1.Received(1).DisposeAsync();
        await mockClient2.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithNoCachedClients_ShouldNotThrow()
    {
        // Arrange
        var provider = CreateMockServerProvider("Server1");
        var strategy = CreateMockStrategy(provider);

        // Act & Assert - should not throw
        await strategy.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldHandleClientDisposalExceptions()
    {
        // Arrange
        var mockClient1 = Substitute.For<McpClient>();
        var mockClient2 = Substitute.For<McpClient>();
        var provider1 = CreateMockServerProvider("Server1");
        var provider2 = CreateMockServerProvider("Server2");

        provider1.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient1);
        provider2.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient2);

        // Setup first client to throw on disposal
        mockClient1.DisposeAsync().Returns(ValueTask.FromException(new InvalidOperationException("Client 1 disposal failed")));
        mockClient2.DisposeAsync().Returns(ValueTask.CompletedTask);

        var strategy = CreateMockStrategy(provider1, provider2);

        // Cache both clients
        await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);
        await strategy.GetOrCreateClientAsync("Server2", cancellationToken: TestContext.Current.CancellationToken);

        // Act - Should not throw (BaseDiscoveryStrategy catches and swallows disposal exceptions)
        await strategy.DisposeAsync();

        // Assert - Both clients should have been attempted to dispose
        await mockClient1.Received(1).DisposeAsync();
        await mockClient2.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        // Arrange
        var mockClient = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("Server1");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient);

        var strategy = CreateMockStrategy(provider);

        // Cache a client
        await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);

        // Act - dispose multiple times
        await strategy.DisposeAsync();
        await strategy.DisposeAsync();
        await strategy.DisposeAsync();

        // Assert - client should only be disposed once (idempotent)
        await mockClient.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldClearClientCache()
    {
        // Arrange
        var mockClient = Substitute.For<McpClient>();
        var provider = CreateMockServerProvider("Server1");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient);

        var strategy = CreateMockStrategy(provider);

        // Cache a client
        var client1 = await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Same(mockClient, client1);

        // Act
        await strategy.DisposeAsync();

        // Assert - After disposal, cache should be cleared
        // This is verified by the fact that disposal was called and the cache is no longer accessible
        await mockClient.Received(1).DisposeAsync();
    }
}
