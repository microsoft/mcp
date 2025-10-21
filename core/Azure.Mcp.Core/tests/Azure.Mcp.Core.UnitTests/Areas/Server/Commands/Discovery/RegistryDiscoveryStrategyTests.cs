// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class RegistryDiscoveryStrategyTests
{
    private static RegistryDiscoveryStrategy CreateStrategy(ServiceStartOptions? options = null)
    {
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<RegistryDiscoveryStrategy>>();
        return new RegistryDiscoveryStrategy(serviceOptions, logger);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Act
        var strategy = CreateStrategy();

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<RegistryDiscoveryStrategy>(strategy);
    }

    [Fact]
    public async Task DiscoverServersAsync_ReturnsNonNullResult()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DiscoverServersAsync_ReturnsExpectedProviders()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = (await strategy.DiscoverServersAsync()).ToList();

        // Assert
        Assert.NotEmpty(result);

        // Should contain the 'learn' server from registry.json
        var documentationProvider = result.FirstOrDefault(p => p.CreateMetadata().Name == "documentation");
        Assert.NotNull(documentationProvider);

        var metadata = documentationProvider.CreateMetadata();
        Assert.Equal("documentation", metadata.Id);
        Assert.Equal("documentation", metadata.Name);
        Assert.Contains("documentation", metadata.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DiscoverServersAsync_AllProvidersAreRegistryServerProviderType()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider => Assert.IsType<RegistryServerProvider>(provider));
    }

    [Fact]
    public async Task DiscoverServersAsync_EachProviderHasValidMetadata()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.NotEmpty(providers);

        foreach (var provider in providers)
        {
            var metadata = provider.CreateMetadata();
            Assert.NotNull(metadata);
            Assert.NotEmpty(metadata.Name);
            Assert.NotEmpty(metadata.Id);
            Assert.Equal(metadata.Name, metadata.Id); // Should be the same for registry providers
            Assert.NotNull(metadata.Description);
            Assert.NotEmpty(metadata.Description);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_ProvidersHaveUniqueIds()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        var ids = providers.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public async Task DiscoverServersAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result1 = await strategy.DiscoverServersAsync();
        var result2 = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        var providers1 = result1.ToList();
        var providers2 = result2.ToList();
        Assert.Equal(providers1.Count, providers2.Count);

        // Should return equivalent results
        var ids1 = providers1.Select(p => p.CreateMetadata().Id).OrderBy(i => i).ToList();
        var ids2 = providers2.Select(p => p.CreateMetadata().Id).OrderBy(i => i).ToList();
        Assert.Equal(ids1, ids2);
    }

    [Fact]
    public async Task DiscoverServersAsync_ResultCountIsConsistent()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result1 = await strategy.DiscoverServersAsync();
        var result2 = await strategy.DiscoverServersAsync();

        // Assert
        var count1 = result1.Count();
        var count2 = result2.Count();
        Assert.Equal(count1, count2);
        Assert.True(count1 > 0); // Should have at least one registry server
    }

    [Fact]
    public async Task DiscoverServersAsync_LoadsFromEmbeddedRegistryResource()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        // Should successfully load from the embedded registry.json resource
        Assert.NotEmpty(result);

        // Verify we get expected server(s) from the registry
        var serverIds = result.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Contains("documentation", serverIds); // Known server from registry.json
    }

    [Fact]
    public async Task DiscoverServersAsync_DocumentationServerHasExpectedProperties()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();
        var documentationProvider = result.FirstOrDefault(p => p.CreateMetadata().Name == "documentation");

        // Assert
        Assert.NotNull(documentationProvider);

        var metadata = documentationProvider.CreateMetadata();
        Assert.Equal("documentation", metadata.Id);
        Assert.Equal("documentation", metadata.Name);
        Assert.NotEmpty(metadata.Description);

        // Description should contain key terms related to Microsoft documentation
        var description = metadata.Description.ToLowerInvariant();
        Assert.Contains("microsoft", description);
        Assert.Contains("documentation", description);
    }

    [Fact]
    public async Task DiscoverServersAsync_ServerNamesMatchIds()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);

        // For registry servers, Name should match Id (both are the key from registry.json)
        Assert.All(result, provider =>
        {
            var metadata = provider.CreateMetadata();
            Assert.Equal(metadata.Id, metadata.Name);
        });
    }

    [Fact]
    public async Task DiscoverServersAsync_AllProvidersCanCreateMetadata()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);

        // Every provider should be able to create valid metadata without throwing
        Assert.All(result, provider =>
        {
            var metadata = provider.CreateMetadata();
            Assert.NotNull(metadata);
            Assert.NotNull(metadata.Id);
            Assert.NotNull(metadata.Name);
            Assert.NotNull(metadata.Description);
        });
    }

    [Fact]
    public async Task DiscoverServersAsync_RegistryServerProviderSupportsSSE()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();
        var documentationProvider = result.FirstOrDefault(p => p.CreateMetadata().Name == "documentation");

        // Assert
        Assert.NotNull(documentationProvider);

        // Documentation server should be SSE-based (has URL)
        var registryProvider = (RegistryServerProvider)documentationProvider;
        Assert.NotNull(registryProvider);

        // Should not throw when creating metadata
        var metadata = registryProvider.CreateMetadata();
        Assert.NotNull(metadata);
        Assert.Equal("documentation", metadata.Name);
    }

    [Fact]
    public async Task DiscoverServersAsync_RegistryServersHaveValidDescriptions()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);

        // All registry servers should have meaningful descriptions
        Assert.All(result, provider =>
        {
            var metadata = provider.CreateMetadata();
            Assert.NotEmpty(metadata.Description);
            Assert.True(metadata.Description.Length > 10); // Should be substantial
        });
    }

    [Fact]
    public async Task DiscoverServersAsync_InheritsFromBaseDiscoveryStrategy()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act & Assert
        Assert.IsAssignableFrom<BaseDiscoveryStrategy>(strategy);

        // Should implement the base contract
        var result = await strategy.DiscoverServersAsync();
        Assert.NotNull(result);
    }

    // Keep the original tests for backward compatibility
    [Fact]
    public async Task ShouldDiscoverServers()
    {
        var strategy = CreateStrategy();
        var result = await strategy.DiscoverServersAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ShouldDiscoverServers_ReturnsExpectedProviders()
    {
        var strategy = CreateStrategy();
        var result = (await strategy.DiscoverServersAsync()).ToList();
        Assert.NotEmpty(result);
        // Should contain the 'documentation' server from registry.json
        var documentationProvider = result.FirstOrDefault(p => p.CreateMetadata().Name == "documentation");
        Assert.NotNull(documentationProvider);
        var metadata = documentationProvider.CreateMetadata();
        Assert.Equal("documentation", metadata.Id);
        Assert.Equal("documentation", metadata.Name);
        Assert.Contains("documentation", metadata.Description, StringComparison.OrdinalIgnoreCase);
    }

    // Namespace filtering tests
    [Fact]
    public async Task DiscoverServersAsync_WithNullNamespace_ReturnsAllServers()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = null };
        var strategy = CreateStrategy(options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        // Should contain all servers when namespace is null
        var serverIds = result.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Contains("documentation", serverIds);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithEmptyNamespace_ReturnsAllServers()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = [] };
        var strategy = CreateStrategy(options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        // Should contain all servers when namespace is empty
        var serverIds = result.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Contains("documentation", serverIds);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithMatchingNamespace_ReturnsFilteredServers()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = ["documentation"] };
        var strategy = CreateStrategy(options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.NotEmpty(providers);

        // Should only contain servers that match the namespace filter
        var serverIds = providers.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Contains("documentation", serverIds);

        // All returned servers should match the namespace filter
        Assert.All(serverIds, id => Assert.Contains(id, options.Namespace, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNonMatchingNamespace_ReturnsEmptyResult()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = ["nonexistent"] };
        var strategy = CreateStrategy(options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Empty(providers);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithMultipleNamespaces_ReturnsMatchingServers()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = ["documentation", "another"] };
        var strategy = CreateStrategy(options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.NotEmpty(providers);

        // Should contain servers that match any of the namespaces
        var serverIds = providers.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Contains("documentation", serverIds);

        // All returned servers should match at least one namespace in the filter
        Assert.All(serverIds, id =>
            Assert.Contains(id, options.Namespace!, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DiscoverServersAsync_NamespaceFilteringIsCaseInsensitive()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = ["DOCUMENTATION"] };
        var strategy = CreateStrategy(options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.NotEmpty(providers);

        // Should find "documentation" server even with uppercase namespace filter
        var serverIds = providers.Select(p => p.CreateMetadata().Id).ToList();
        Assert.Contains("documentation", serverIds);
    }
}
