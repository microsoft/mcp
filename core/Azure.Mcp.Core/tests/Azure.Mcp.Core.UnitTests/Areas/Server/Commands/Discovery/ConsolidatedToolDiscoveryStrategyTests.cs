// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class ConsolidatedToolDiscoveryStrategyTests
{
    private static ConsolidatedToolDiscoveryStrategy CreateStrategy(
        ICommandFactory? commandFactory = null,
        ServiceStartOptions? options = null,
        string? entryPoint = null)
    {
        var factory = commandFactory ?? CommandFactoryHelpers.CreateCommandFactory();
        var serviceProvider = CommandFactoryHelpers.SetupCommonServices().BuildServiceProvider();
        var startOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        var configurationOptions = Microsoft.Extensions.Options.Options.Create(new AzureMcpServerConfiguration
        {
            Name = "Test Server",
            Version = "Test Version",
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        });
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<ConsolidatedToolDiscoveryStrategy>>();
        var strategy = new ConsolidatedToolDiscoveryStrategy(factory, serviceProvider, startOptions, configurationOptions, logger);
        if (entryPoint != null)
        {
            strategy.EntryPoint = entryPoint;
        }
        return strategy;
    }

    [Fact]
    public async Task DiscoverServersAsync_ReturnsEmptyList()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var providers = result.ToList();
        Assert.Empty(providers);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithDefaultOptions_ReturnsCommandFactory()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var factory = strategy.CreateConsolidatedCommandFactory();

        // Assert
        Assert.NotNull(factory);
        Assert.True(factory.AllCommands.Count > 10);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_MapsAllRegisteredCommands()
    {
        // Arrange
        var sourceFactory = CommandFactoryHelpers.CreateCommandFactory();
        var strategy = CreateStrategy(commandFactory: sourceFactory);

        // Act - in DEBUG, CreateConsolidatedCommandFactory throws if any commands are unmapped
        // or if metadata mismatches are detected
        var consolidatedFactory = strategy.CreateConsolidatedCommandFactory();

        // Assert - consolidated factory should map all non-ignored source commands
        var ignoredGroups = new HashSet<string>(
            ConsolidatedToolDiscoveryStrategy.IgnoredCommandGroups,
            StringComparer.OrdinalIgnoreCase);

        var expectedCount = sourceFactory.AllCommands.Count(kvp =>
        {
            var area = sourceFactory.GetServiceArea(kvp.Key);
            return area == null || !ignoredGroups.Contains(area);
        });

        Assert.Equal(expectedCount, consolidatedFactory.AllCommands.Count);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithAllAreas_HasSubstantialCommandCount()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var factory = strategy.CreateConsolidatedCommandFactory();

        // Assert - with all tool areas registered, expect a substantial number of commands
        Assert.True(factory.AllCommands.Count > 200,
            $"Expected more than 200 consolidated commands but found {factory.AllCommands.Count}. " +
            "Ensure CommandFactoryHelpers registers all tool areas matching production Program.cs.");
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithNamespaceFilter_FiltersCommands()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = ["storage"] };
        var strategy = CreateStrategy(options: options);

        // Act
        var factory = strategy.CreateConsolidatedCommandFactory();

        // Assert
        Assert.NotNull(factory);
        // Should only have storage-related consolidated commands
        var allCommands = factory.AllCommands;
        Assert.True(allCommands.Count > 0);
        Assert.True(allCommands.Count < 10);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithReadOnlyFilter_FiltersCommands()
    {
        // Arrange
        var options = new ServiceStartOptions { ReadOnly = true };
        var strategy = CreateStrategy(options: options);

        // Act
        var factory = strategy.CreateConsolidatedCommandFactory();

        // Assert
        Assert.NotNull(factory);
        var allCommands = factory.AllCommands;
        Assert.True(allCommands.Count > 0);
        // All commands should be read-only
        Assert.All(allCommands.Values, cmd => Assert.True(cmd.Metadata.ReadOnly));
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_HandlesEmptyNamespaceFilter()
    {
        // Arrange
        var options = new ServiceStartOptions { Namespace = [] };
        var strategy = CreateStrategy(options: options);

        // Act
        var factory = strategy.CreateConsolidatedCommandFactory();

        // Assert
        Assert.NotNull(factory);
        var allCommands = factory.AllCommands;
        Assert.True(allCommands.Count > 0);
    }

    [Fact]
    public void AreMetadataEqual_BothNull_ReturnsTrue()
    {
        Assert.True(ConsolidatedToolDiscoveryStrategy.AreMetadataEqual(null, null));
    }

    [Fact]
    public void AreMetadataEqual_OneNull_ReturnsFalse()
    {
        var metadata = new ToolMetadata { Destructive = false, ReadOnly = true };
        Assert.False(ConsolidatedToolDiscoveryStrategy.AreMetadataEqual(metadata, null));
        Assert.False(ConsolidatedToolDiscoveryStrategy.AreMetadataEqual(null, metadata));
    }

    [Fact]
    public void AreMetadataEqual_MatchingValues_ReturnsTrue()
    {
        var metadata1 = new ToolMetadata
        {
            Destructive = false,
            Idempotent = true,
            OpenWorld = false,
            ReadOnly = true,
            Secret = false,
            LocalRequired = false
        };
        var metadata2 = new ToolMetadata
        {
            Destructive = false,
            Idempotent = true,
            OpenWorld = false,
            ReadOnly = true,
            Secret = false,
            LocalRequired = false
        };
        Assert.True(ConsolidatedToolDiscoveryStrategy.AreMetadataEqual(metadata1, metadata2));
    }

    [Fact]
    public void AreMetadataEqual_DifferentValues_ReturnsFalse()
    {
        var metadata1 = new ToolMetadata { Destructive = false, ReadOnly = true };
        var metadata2 = new ToolMetadata { Destructive = true, ReadOnly = true };
        Assert.False(ConsolidatedToolDiscoveryStrategy.AreMetadataEqual(metadata1, metadata2));
    }
}
