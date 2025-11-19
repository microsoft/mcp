// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class ConsolidatedToolDiscoveryStrategyTests
{
    private static ConsolidatedToolDiscoveryStrategy CreateStrategy(
        CommandFactory? commandFactory = null,
        ServiceStartOptions? options = null,
        string? entryPoint = null)
    {
        var factory = commandFactory ?? CommandFactoryHelpers.CreateCommandFactory();
        var serviceProvider = CommandFactoryHelpers.SetupCommonServices().BuildServiceProvider();
        var startOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        var configurationOptions = Microsoft.Extensions.Options.Options.Create(new AzureMcpServerConfiguration
        {
            Name = "Test Server",
            Version = "Test Version"
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
}
