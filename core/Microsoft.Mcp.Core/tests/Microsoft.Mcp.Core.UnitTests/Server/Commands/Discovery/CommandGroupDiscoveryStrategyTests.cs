// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.UnitTests.Server.Helpers;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CommandGroupDiscoveryStrategyTests
{
    private readonly IOptions<ServiceStartOptions> _defaultStartOptions;
    private readonly ILogger<CommandGroupDiscoveryStrategy> _logger;
    private readonly MockCommandFactory _commandFactory;
    private readonly CommandGroup _subGroupA = new CommandGroup("A", "A_description");
    private readonly CommandGroup _subGroupB = new CommandGroup("B", "B_description");
    private readonly CommandGroup _subGroupC = new CommandGroup("C", "C_description");
    private readonly CommandGroup _extensionSubGroup = new CommandGroup("extension", "extensions should be ignored.");
    private readonly CommandGroup _serverSubGroup = new CommandGroup("server", "server should be ignored here.");

    public CommandGroupDiscoveryStrategyTests()
    {
        _defaultStartOptions = Options.Create(new ServiceStartOptions());
        _logger = Substitute.For<ILogger<CommandGroupDiscoveryStrategy>>();
        _commandFactory = new MockCommandFactory();
        _commandFactory.RootGroup.SubGroup.Add(_subGroupA);
        _commandFactory.RootGroup.SubGroup.Add(_subGroupB);
        _commandFactory.RootGroup.SubGroup.Add(_subGroupC);
        _commandFactory.RootGroup.SubGroup.Add(_extensionSubGroup);
        _commandFactory.RootGroup.SubGroup.Add(_serverSubGroup);
    }

    [Fact]
    public void Constructor_WithNullCommandFactory_DoesNotThrow()
    {
        // Arrange
        var options = Options.Create(new ServiceStartOptions());
        var logger = Substitute.For<ILogger<CommandGroupDiscoveryStrategy>>();

        // Act & Assert
        // Primary constructor syntax doesn't automatically validate null parameters
        var strategy = new CommandGroupDiscoveryStrategy(null!, options, logger);
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Constructor_WithNullOptions_DoesNotThrow()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var logger = Substitute.For<ILogger<CommandGroupDiscoveryStrategy>>();

        // Act & Assert
        // Primary constructor syntax doesn't automatically validate null parameters
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, null!, logger);
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var options = Options.Create(new ServiceStartOptions());
        var logger = Substitute.For<ILogger<CommandGroupDiscoveryStrategy>>();

        // Act
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, options, logger);

        // Assert
        Assert.NotNull(strategy);
        Assert.Null(strategy.EntryPoint); // Default should be null
    }

    [Fact]
    public void EntryPoint_DefaultsToNull()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act & Assert
        Assert.Null(strategy.EntryPoint);
    }

    [Fact]
    public void EntryPoint_WhenSetToEmpty_RemainsEmpty()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        strategy.EntryPoint = "";

        // Assert
        // The strategy itself just stores the value as-is
        // The defaulting behavior happens in CommandGroupServerProvider
        Assert.Equal("", strategy.EntryPoint);
    }

    [Fact]
    public void EntryPoint_WhenSetToWhitespace_RemainsWhitespace()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        strategy.EntryPoint = "   ";

        // Assert
        // The strategy itself just stores the value as-is
        // The defaulting behavior happens in CommandGroupServerProvider
        Assert.Equal("   ", strategy.EntryPoint);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithDefaultOptions_ReturnsNonEmptyCollection()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, provider => Assert.IsType<CommandGroupServerProvider>(provider));
    }

    [Fact]
    public async Task DiscoverServersAsync_WithReadOnlyFalse_CreatesNonReadOnlyProviders()
    {
        // Arrange
        var options = new ServiceStartOptions { ReadOnly = false };
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, Options.Create(options), _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider =>
            Assert.False(((CommandGroupServerProvider)provider).ReadOnly));
    }

    [Fact]
    public async Task DiscoverServersAsync_WithReadOnlyTrue_CreatesReadOnlyProviders()
    {
        // Arrange
        var options = new ServiceStartOptions { ReadOnly = true };
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, Options.Create(options), _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider =>
            Assert.True(((CommandGroupServerProvider)provider).ReadOnly));
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNullReadOnlyOption_DefaultsToFalse()
    {
        // Arrange
        var options = new ServiceStartOptions { ReadOnly = null };
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, Options.Create(options), _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider =>
            Assert.False(((CommandGroupServerProvider)provider).ReadOnly));
    }

    [Fact]
    public async Task DiscoverServersAsync_ExcludesIgnoredGroups()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        var result = (await strategy.DiscoverServersAsync()).ToList();

        // Assert
        var names = result.Select(p => p.CreateMetadata().Name).ToList();
        var ignoredGroups = new[] { "extension", "server", "tools" };

        foreach (var ignored in ignoredGroups)
        {
            Assert.DoesNotContain(ignored, names, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_EachProviderHasCorrectMetadata()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

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
            Assert.Equal(metadata.Name, metadata.Id); // Should be the same
            Assert.NotNull(metadata.Description);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_ProvidersAreCommandGroupServerProviderType()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider => Assert.IsType<CommandGroupServerProvider>(provider));
    }

    [Fact]
    public async Task DiscoverServersAsync_ProvidersHaveUniqueNames()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        var names = providers.Select(p => p.CreateMetadata().Name).ToList();
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public async Task DiscoverServersAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

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
        var names1 = providers1.Select(p => p.CreateMetadata().Name).OrderBy(n => n).ToList();
        var names2 = providers2.Select(p => p.CreateMetadata().Name).OrderBy(n => n).ToList();
        Assert.Equal(names1, names2);
    }

    [Fact]
    public async Task DiscoverServersAsync_IgnoredGroupsAreCaseInsensitive()
    {
        // Arrange - Test with real factory since we can't easily mock the command groups
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var names = result.Select(p => p.CreateMetadata().Name).ToList();

        // Verify ignored groups are not present (case insensitive)
        Assert.DoesNotContain("extension", names, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("server", names, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("tools", names, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DiscoverServersAsync_ResultCountIsConsistent()
    {
        // Arrange
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, _defaultStartOptions, _logger);

        // Act
        var result1 = await strategy.DiscoverServersAsync();
        var result2 = await strategy.DiscoverServersAsync();

        // Assert
        var count1 = result1.Count();
        var count2 = result2.Count();
        Assert.Equal(count1, count2);
        Assert.True(count1 > 0); // Should have at least some command groups
    }

    // Keep the original tests for backward compatibility
    [Fact]
    public async Task ShouldDiscoverServers()
    {
        var commandFactory = new MockCommandFactory();
        var options =   Options.Create(new ServiceStartOptions());
        var logger = Substitute.For<ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, options, logger);
        var result = await strategy.DiscoverServersAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNamespaceFilter_ReturnsOnlySpecifiedNamespaces()
    {
        // Arrange
        var commandGroup = new CommandGroup("B", "B_Second_Group");
        commandGroup.Commands.Add("test", Substitute.For<IBaseCommand>());

        _commandFactory.RootGroup.SubGroup.Add(commandGroup);

        var options = new ServiceStartOptions
        {
            Namespace = new[] { "A", "B" }
        };
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, Options.Create(options), _logger);

        // Act
        var servers = await strategy.DiscoverServersAsync();
        var serverNames = servers.Select(s => s.CreateMetadata().Name).ToList();

        // Assert
        Assert.NotNull(servers);
        Assert.Equal(3, serverNames.Count);

        // Should not contain other namespaces
        Assert.DoesNotContain(_subGroupC.Name, serverNames);
        Assert.DoesNotContain(_extensionSubGroup.Name, serverNames);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithEmptyNamespaceFilter_ReturnsAllNamespaces()
    {
        // Arrange
        var options = new ServiceStartOptions
        {
            Namespace = Array.Empty<string>()
        };
        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, Options.Create(options), _logger);

        // Act
        var servers = await strategy.DiscoverServersAsync();
        var serverNames = servers.Select(s => s.CreateMetadata().Name).ToList();

        // Assert
        Assert.NotNull(servers);
        Assert.Equal(3, servers.Count());

        // Should contain expected namespaces (but not ignored ones)
        Assert.Contains(_subGroupA.Name, serverNames);
        Assert.Contains(_subGroupB.Name, serverNames);
        Assert.DoesNotContain(_extensionSubGroup.Name, serverNames); // Should be ignored
        Assert.DoesNotContain(_serverSubGroup.Name, serverNames); // Should be ignored
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNullNamespaceFilter_ReturnsAllNamespaces()
    {
        // Arrange
        var options = new ServiceStartOptions
        {
            Namespace = null
        };
        var commandGroup = new CommandGroup("B", "B_Second_Group");
        commandGroup.Commands.Add("test", Substitute.For<IBaseCommand>());

        _commandFactory.RootGroup.SubGroup.Add(commandGroup);

        var strategy = new CommandGroupDiscoveryStrategy(_commandFactory, Options.Create(options), _logger);

        // Act
        var servers = await strategy.DiscoverServersAsync();
        var serverNames = servers.Select(s => s.CreateMetadata().Name).ToList();

        // Assert
        Assert.NotNull(servers);
        Assert.Equal(4, serverNames.Count);

        // Should contain expected namespaces (but not ignored ones)
        Assert.Contains("A", serverNames);
        Assert.Contains("B", serverNames);
        Assert.Contains("C", serverNames);
        Assert.DoesNotContain("extension", serverNames); // Should be ignored
    }

}
