// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using NSubstitute;
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
        var configurationOptions = Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "Test Server",
            Version = "Test Version",
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        });

        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<ConsolidatedToolDiscoveryStrategy>>();
        var providerLogger = Substitute.For<Microsoft.Extensions.Logging.ILogger<ResourceConsolidatedToolDefinitionProvider>>();
        var serverAssembly = typeof(Azure.Mcp.Server.Program).Assembly;

        ResourceConsolidatedToolDefinitionProvider definitionProvider = new(providerLogger, serverAssembly, "consolidated-tools.json");

        var strategy = new ConsolidatedToolDiscoveryStrategy(factory, serviceProvider, definitionProvider, startOptions, configurationOptions, logger);
        if (entryPoint != null)
        {
            strategy.EntryPoint = entryPoint;
        }
        return strategy;
    }

    [Fact]
    public async Task DiscoverServersAsync_ReturnsEmptyList()
    {
        var strategy = CreateStrategy();

        var result = await strategy.DiscoverServersAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithDefaultOptions_ReturnsCommandFactory()
    {
        var strategy = CreateStrategy();

        var factory = strategy.CreateConsolidatedCommandFactory();

        Assert.NotNull(factory);
        Assert.True(factory.AllCommands.Count > 10);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_MapsAllRegisteredCommands()
    {
        var sourceFactory = CommandFactoryHelpers.CreateCommandFactory();
        var strategy = CreateStrategy(commandFactory: sourceFactory);

        var consolidatedFactory = strategy.CreateConsolidatedCommandFactory();

        var ignoredGroups = new HashSet<string>(
            ConsolidatedToolDiscoveryStrategy.IgnoredCommandGroups,
            StringComparer.OrdinalIgnoreCase);

        var expectedCommands = sourceFactory.AllCommands
            .Where(kvp =>
            {
                var area = sourceFactory.GetServiceArea(kvp.Key);
                return area == null || !ignoredGroups.Contains(area);
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var expectedCommandSet = expectedCommands.Values
            .ToHashSet(ReferenceEqualityComparer.Instance);
        var consolidatedCommandSet = consolidatedFactory.AllCommands.Values
            .ToHashSet(ReferenceEqualityComparer.Instance);

        Assert.Equal(
            expectedCommandSet.Count,
            consolidatedCommandSet.Count);

        var missingCommands = expectedCommands
            .Where(kvp => !consolidatedCommandSet.Contains(kvp.Value))
            .Select(kvp => kvp.Key)
            .OrderBy(static name => name)
            .ToList();

        Assert.True(
            missingCommands.Count == 0,
            $"Missing consolidated mappings for commands: {string.Join(", ", missingCommands)}");
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithNamespaceFilter_FiltersCommands()
    {
        var options = new ServiceStartOptions { Namespace = ["storage"] };
        var strategy = CreateStrategy(options: options);

        var factory = strategy.CreateConsolidatedCommandFactory();

        Assert.NotNull(factory);
        Assert.InRange(factory.AllCommands.Count, 1, 9);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithReadOnlyFilter_FiltersCommands()
    {
        var options = new ServiceStartOptions { ReadOnly = true };
        var strategy = CreateStrategy(options: options);

        var factory = strategy.CreateConsolidatedCommandFactory();

        Assert.NotNull(factory);
        var allCommands = factory.AllCommands;
        Assert.NotEmpty(allCommands);
        Assert.All(allCommands.Values, cmd => Assert.True(cmd.Metadata.ReadOnly));
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_WithNullReadOnlyOption_DefaultsToFalse()
    {
        var defaultStrategy = CreateStrategy(options: new ServiceStartOptions { ReadOnly = null });
        var explicitFalseStrategy = CreateStrategy(options: new ServiceStartOptions { ReadOnly = false });

        var defaultFactory = defaultStrategy.CreateConsolidatedCommandFactory();
        var explicitFalseFactory = explicitFalseStrategy.CreateConsolidatedCommandFactory();

        Assert.Equal(explicitFalseFactory.AllCommands.Count, defaultFactory.AllCommands.Count);
        Assert.Equal(
            explicitFalseFactory.AllCommands.Keys.OrderBy(static name => name),
            defaultFactory.AllCommands.Keys.OrderBy(static name => name));
        Assert.Contains(defaultFactory.AllCommands.Values, cmd => !cmd.Metadata.ReadOnly);
    }

    [Fact]
    public void CreateConsolidatedCommandFactory_HandlesEmptyNamespaceFilter()
    {
        var options = new ServiceStartOptions { Namespace = [] };
        var strategy = CreateStrategy(options: options);

        var factory = strategy.CreateConsolidatedCommandFactory();

        Assert.NotNull(factory);
        Assert.NotEmpty(factory.AllCommands);
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
