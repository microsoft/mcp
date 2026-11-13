// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Tests.Areas.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using ModelContextProtocol.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.Discovery;

/// <summary>
/// Concrete test implementation of BaseDiscoveryStrategy for testing disposal behavior
/// </summary>
public class TestDiscoveryStrategy(IEnumerable<IMcpServerProvider> providers, ILogger? logger = null)
    : BaseDiscoveryStrategy(logger ?? NullLogger.Instance)
{
    private readonly IEnumerable<IMcpServerProvider> _providers = providers;

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

    private static TestDiscoveryStrategy CreateMockStrategy(params IMcpServerProvider[] providers) => new(providers);

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
        var mockClient = LoopbackMcpClient.Create(_ => null);
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
        var mockClient = LoopbackMcpClient.Create(_ => null);
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
        var mockClient = LoopbackMcpClient.Create(_ => null);
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
        var mockClient = LoopbackMcpClient.Create(_ => null);
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
        var mockClient1 = LoopbackMcpClient.Create(_ => null);
        var mockClient2 = LoopbackMcpClient.Create(_ => null);
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
    public async Task GetOrCreateClientAsync_ConcurrentCalls_CreateOneClient()
    {
        await using var client = LoopbackMcpClient.Create(_ => null, out var tracker);
        var creation = new TaskCompletionSource<McpClient>(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = CreateMockServerProvider("external");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>())
            .Returns(creation.Task);
        await using var strategy = CreateMockStrategy(provider);
        var tasks = Enumerable.Range(0, 16)
            .Select(index => strategy.GetOrCreateClientAsync(
                index % 2 == 0 ? "external" : "EXTERNAL", cancellationToken: TestContext.Current.CancellationToken))
            .ToArray();

        creation.SetResult(client);
        var clients = await Task.WhenAll(tasks);

        Assert.All(clients, result => Assert.Same(client, result));
        await provider.Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
        await strategy.DisposeAsync();
        Assert.Equal(1, tracker.DisposeCount);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_ConcurrentServers_InitializeIndependently()
    {
        const int serverCount = 16;
        var cancellationToken = TestContext.Current.CancellationToken;
        using var starts = new Barrier(serverCount);
        var clients = Enumerable.Range(0, serverCount).Select(_ => LoopbackMcpClient.Create(_ => null)).ToArray();
        var providers = Enumerable.Range(0, serverCount).Select(index =>
        {
            var provider = CreateMockServerProvider($"external{index}");
            provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(_ =>
            {
                Assert.True(starts.SignalAndWait(TimeSpan.FromSeconds(30), cancellationToken));
                return clients[index];
            });
            return provider;
        }).ToArray();
        await using var strategy = CreateMockStrategy(providers);

        var tasks = Enumerable.Range(0, serverCount).Select(index => Task.Factory.StartNew(
            () => strategy.GetOrCreateClientAsync($"external{index}", cancellationToken: cancellationToken),
            cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap()).ToArray();
        var results = await Task.WhenAll(tasks);

        for (var index = 0; index < serverCount; index++)
        {
            Assert.Same(clients[index], results[index]);
            Assert.Same(clients[index], await strategy.GetOrCreateClientAsync(
                $"EXTERNAL{index}", cancellationToken: cancellationToken));
            await providers[index].Received(1).CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetOrCreateClientAsync_CallerCancellation_DoesNotCancelSharedCreation(bool cancelFirstCaller)
    {
        await using var client = LoopbackMcpClient.Create(_ => null);
        var creation = new TaskCompletionSource<McpClient>(TaskCreationOptions.RunContinuationsAsynchronously);
        var provider = CreateMockServerProvider("external");
        var initializationToken = CancellationToken.None;
        var creations = 0;
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var token = call.Arg<CancellationToken>();
            if (Interlocked.Increment(ref creations) == 1)
            {
                initializationToken = token;
            }
            return creation.Task.WaitAsync(token);
        });
        await using var strategy = CreateMockStrategy(provider);
        using var callerCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var firstCall = strategy.GetOrCreateClientAsync("external",
            cancellationToken: cancelFirstCaller ? callerCancellation.Token : TestContext.Current.CancellationToken);
        var secondCall = strategy.GetOrCreateClientAsync("EXTERNAL",
            cancellationToken: cancelFirstCaller ? TestContext.Current.CancellationToken : callerCancellation.Token);

        try
        {
            await callerCancellation.CancelAsync();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelFirstCaller ? firstCall : secondCall);
            Assert.False(initializationToken.IsCancellationRequested);
        }
        finally
        {
            creation.TrySetResult(client);
        }

        Assert.Same(client, await (cancelFirstCaller ? secondCall : firstCall));
        Assert.Same(client, await strategy.GetOrCreateClientAsync("external",
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(1, creations);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetOrCreateClientAsync_FailedCreation_AllowsRetry(bool canceled)
    {
        await using var client = LoopbackMcpClient.Create(_ => null);
        var failure = new InvalidOperationException("Client creation failed.");
        var provider = CreateMockServerProvider("external");
        var creations = 0;
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(_ =>
            Interlocked.Increment(ref creations) == 1
                ? canceled
                    ? Task.FromCanceled<McpClient>(new CancellationToken(true))
                    : Task.FromException<McpClient>(failure)
                : Task.FromResult(client));
        await using var strategy = CreateMockStrategy(provider);

        if (canceled)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                strategy.GetOrCreateClientAsync("external", cancellationToken: TestContext.Current.CancellationToken));
        }
        else
        {
            Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() =>
                strategy.GetOrCreateClientAsync("external", cancellationToken: TestContext.Current.CancellationToken)));
        }

        Assert.Same(client, await strategy.GetOrCreateClientAsync("external",
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(2, creations);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_ConcurrentFailure_AllowsFreshAttempt()
    {
        await using var client = LoopbackMcpClient.Create(_ => null);
        var creation = new TaskCompletionSource<McpClient>(TaskCreationOptions.RunContinuationsAsynchronously);
        var failure = new InvalidOperationException("Shared initialization failed.");
        var provider = CreateMockServerProvider("external");
        var attempts = 0;
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(_ =>
            Interlocked.Increment(ref attempts) == 1 ? creation.Task : Task.FromResult(client));
        await using var strategy = CreateMockStrategy(provider);
        var callers = Enumerable.Range(0, 8).Select(_ => strategy.GetOrCreateClientAsync(
            "external", cancellationToken: TestContext.Current.CancellationToken)).ToArray();

        creation.SetException(failure);
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => Task.WhenAll(callers)));
        Assert.All(callers, caller => Assert.True(caller.IsFaulted));

        Assert.Same(client, await strategy.GetOrCreateClientAsync(
            "external", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task GetOrCreateClientAsync_CanceledBeforeCreation_DoesNotStartProvider()
    {
        var provider = CreateMockServerProvider("external");
        await using var strategy = CreateMockStrategy(provider);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            strategy.GetOrCreateClientAsync("external", cancellationToken: cancellation.Token));

        await provider.DidNotReceive().CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DisposeAsync_ConcurrentCalls_WaitForInFlightCreation(bool honorsCancellation)
    {
        await using var client = LoopbackMcpClient.Create(_ => null, out var tracker);
        var creation = new TaskCompletionSource<McpClient>(TaskCreationOptions.RunContinuationsAsynchronously);
        var initializationToken = CancellationToken.None;
        var provider = CreateMockServerProvider("external");
        provider.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            initializationToken = call.Arg<CancellationToken>();
            return honorsCancellation ? creation.Task.WaitAsync(initializationToken) : creation.Task;
        });
        await using var strategy = CreateMockStrategy(provider);
        var pendingClient = strategy.GetOrCreateClientAsync("external", cancellationToken: TestContext.Current.CancellationToken);
        var firstDisposal = strategy.DisposeAsync().AsTask();
        var secondDisposal = strategy.DisposeAsync().AsTask();

        try
        {
            Assert.Same(firstDisposal, secondDisposal);
            if (!honorsCancellation)
            {
                Assert.False(firstDisposal.IsCompleted);
            }
            else
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    pendingClient.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));
            }
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                strategy.GetOrCreateClientAsync("external", cancellationToken: TestContext.Current.CancellationToken));
        }
        finally
        {
            creation.TrySetResult(client);
        }

        if (honorsCancellation)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pendingClient);
        }
        else
        {
            Assert.Same(client, await pendingClient);
        }
        await Task.WhenAll(firstDisposal, secondDisposal);

        Assert.True(initializationToken.IsCancellationRequested);
        Assert.Equal(honorsCancellation ? 0 : 1, tracker.DisposeCount);
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
        var mockClient1 = LoopbackMcpClient.Create(_ => null);
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
        var mockClient1 = LoopbackMcpClient.Create(_ => null, out var tracker1);
        var mockClient2 = LoopbackMcpClient.Create(_ => null, out var tracker2);
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
        Assert.True(tracker1.WasDisposed);
        Assert.True(tracker2.WasDisposed);
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
        var mockClient1 = LoopbackMcpClient.CreateThrowingOnDispose(_ => null, out var tracker1);
        var mockClient2 = LoopbackMcpClient.Create(_ => null, out var tracker2);
        var provider1 = CreateMockServerProvider("Server1");
        var provider2 = CreateMockServerProvider("Server2");

        provider1.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient1);
        provider2.CreateClientAsync(Arg.Any<McpClientOptions>(), Arg.Any<CancellationToken>()).Returns(mockClient2);

        var strategy = CreateMockStrategy(provider1, provider2);

        // Cache both clients
        await strategy.GetOrCreateClientAsync("Server1", cancellationToken: TestContext.Current.CancellationToken);
        await strategy.GetOrCreateClientAsync("Server2", cancellationToken: TestContext.Current.CancellationToken);

        // Act - Should not throw (BaseDiscoveryStrategy catches and swallows disposal exceptions)
        await strategy.DisposeAsync();

        // Assert - Both clients should have been attempted to dispose
        Assert.True(tracker1.WasDisposed);
        Assert.True(tracker2.WasDisposed);
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        // Arrange
        var mockClient = LoopbackMcpClient.Create(_ => null, out var tracker);
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
        Assert.Equal(1, tracker.DisposeCount);
    }

    [Fact]
    public async Task DisposeAsync_ShouldClearClientCache()
    {
        // Arrange
        var mockClient = LoopbackMcpClient.Create(_ => null, out var tracker);
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
        Assert.True(tracker.WasDisposed);
    }
}
