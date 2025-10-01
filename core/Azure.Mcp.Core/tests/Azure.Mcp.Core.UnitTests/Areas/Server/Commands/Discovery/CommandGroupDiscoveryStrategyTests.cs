// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CommandGroupDiscoveryStrategyTests
{
    private static CommandGroupDiscoveryStrategy CreateStrategy(
        CommandFactory? commandFactory = null,
        ServiceStartOptions? options = null,
        string? entryPoint = null)
    {
        var factory = commandFactory ?? CommandFactoryHelpers.CreateCommandFactory();
        var startOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new CommandGroupDiscoveryStrategy(factory, startOptions, logger);
        if (entryPoint != null)
        {
            strategy.EntryPoint = entryPoint;
        }
        return strategy;
    }

    [Fact]
    public void Constructor_WithNullCommandFactory_DoesNotThrow()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();

        // Act & Assert
        // Primary constructor syntax doesn't automatically validate null parameters
        var strategy = new CommandGroupDiscoveryStrategy(null!, options, logger);
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Constructor_WithNullOptions_DoesNotThrow()
    {
        // Arrange
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();

        // Act & Assert
        // Primary constructor syntax doesn't automatically validate null parameters
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, null!, logger);
        Assert.NotNull(strategy);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();

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
        var strategy = CreateStrategy();

        // Act & Assert
        Assert.Null(strategy.EntryPoint);
    }

    [Fact]
    public void EntryPoint_CanBeSetAndRetrieved()
    {
        // Arrange
        var strategy = CreateStrategy();
        var azmcpPath = McpTestUtilities.GetAzMcpExecutablePath();

        // Act
        strategy.EntryPoint = azmcpPath;

        // Assert
        Assert.Equal(azmcpPath, strategy.EntryPoint);
    }

    [Fact]
    public void EntryPoint_WhenSetToEmpty_RemainsEmpty()
    {
        // Arrange
        var strategy = CreateStrategy();

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
        var strategy = CreateStrategy();

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
        var strategy = CreateStrategy();

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
        var strategy = CreateStrategy(options: options);

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
        var strategy = CreateStrategy(options: options);

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
        var strategy = CreateStrategy(options: options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider =>
            Assert.False(((CommandGroupServerProvider)provider).ReadOnly));
    }

    [Fact]
    public async Task DiscoverServersAsync_WithCustomEntryPoint_SetsEntryPointOnAllProviders()
    {
        // Arrange
        var customEntryPoint = McpTestUtilities.GetAzMcpExecutablePath();
        var strategy = CreateStrategy(entryPoint: customEntryPoint);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider =>
            Assert.Equal(customEntryPoint, ((CommandGroupServerProvider)provider).EntryPoint));
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNullEntryPoint_UsesCurrentProcessExecutable()
    {
        // Arrange
        var strategy = CreateStrategy(entryPoint: null);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        // When EntryPoint is set to null, CommandGroupServerProvider defaults to current process executable
        var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        Assert.All(result, provider =>
        {
            var actualEntryPoint = ((CommandGroupServerProvider)provider).EntryPoint;
            Assert.NotNull(actualEntryPoint);
            // Should be the current test process executable
            Assert.Equal(currentProcessPath, actualEntryPoint);
        });
    }

    [Fact]
    public async Task DiscoverServersAsync_WithEmptyEntryPoint_ProvidersDefaultToCurrentProcess()
    {
        // Arrange
        var strategy = CreateStrategy(entryPoint: "");

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        Assert.All(result, provider =>
        {
            var actualEntryPoint = ((CommandGroupServerProvider)provider).EntryPoint;
            // CommandGroupServerProvider defaults empty/null to current process
            Assert.Equal(currentProcessPath, actualEntryPoint);
        });
    }

    [Fact]
    public async Task DiscoverServersAsync_WithWhitespaceEntryPoint_ProvidersDefaultToCurrentProcess()
    {
        // Arrange
        var strategy = CreateStrategy(entryPoint: "   ");

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        Assert.All(result, provider =>
        {
            var actualEntryPoint = ((CommandGroupServerProvider)provider).EntryPoint;
            // CommandGroupServerProvider defaults whitespace to current process
            Assert.Equal(currentProcessPath, actualEntryPoint);
        });
    }

    [Fact]
    public async Task DiscoverServersAsync_ExcludesIgnoredGroups()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var names = result.Select(p => p.CreateMetadata().Name).ToList();
        var ignoredGroups = DiscoveryConstants.IgnoredCommandGroups;

        foreach (var ignored in ignoredGroups)
        {
            Assert.DoesNotContain(ignored, names, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_EachProviderHasCorrectMetadata()
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
            Assert.Equal(metadata.Name, metadata.Id); // Should be the same
            Assert.NotNull(metadata.Description);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_ProvidersAreCommandGroupServerProviderType()
    {
        // Arrange
        var strategy = CreateStrategy();

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
        var strategy = CreateStrategy();

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
        var names1 = providers1.Select(p => p.CreateMetadata().Name).OrderBy(n => n).ToList();
        var names2 = providers2.Select(p => p.CreateMetadata().Name).OrderBy(n => n).ToList();
        Assert.Equal(names1, names2);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithRealCommandFactory_IncludesKnownGroups()
    {
        // Arrange
        var strategy = CreateStrategy(); // Uses real command factory

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        var names = providers.Select(p => p.CreateMetadata().Name).ToList();

        // Should include at least some known groups (based on actual implementation)
        Assert.Contains("storage", names, StringComparer.OrdinalIgnoreCase);

        // Should not include ignored groups
        var ignoredGroups = DiscoveryConstants.IgnoredCommandGroups;
        foreach (var ignored in ignoredGroups)
        {
            Assert.DoesNotContain(ignored, names, StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_RespectsServiceStartOptionsValues()
    {
        // Arrange
        var options = new ServiceStartOptions
        {
            ReadOnly = true,
        };
        var azmcpEntryPoint = McpTestUtilities.GetAzMcpExecutablePath();
        var strategy = CreateStrategy(options: options, entryPoint: azmcpEntryPoint);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        foreach (var provider in result)
        {
            var serverProvider = (CommandGroupServerProvider)provider;
            Assert.True(serverProvider.ReadOnly);
            Assert.Equal(azmcpEntryPoint, serverProvider.EntryPoint);
        }
    }

    [Fact]
    public async Task DiscoverServersAsync_IgnoredGroupsAreCaseInsensitive()
    {
        // Arrange - Test with real factory since we can't easily mock the command groups
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var names = result.Select(p => p.CreateMetadata().Name).ToList();

        // Verify ignored groups are not present (case insensitive)
        foreach (var ignored in DiscoveryConstants.IgnoredCommandGroups)
        {
            Assert.DoesNotContain(ignored, names, StringComparer.OrdinalIgnoreCase);
        }
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
        Assert.True(count1 > 0); // Should have at least some command groups
    }

    // Keep the original tests for backward compatibility
    [Fact]
    public async Task ShouldDiscoverServers()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, options, logger);
        var result = await strategy.DiscoverServersAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ShouldDiscoverServers_ExcludesIgnoredGroupsAndSetsProperties()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { ReadOnly = true });
        var azmcpEntryPoint = McpTestUtilities.GetAzMcpExecutablePath();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, options, logger)
        {
            EntryPoint = azmcpEntryPoint
        };
        var result = (await strategy.DiscoverServersAsync()).ToList();
        Assert.NotEmpty(result);
        // Should not include ignored groups
        var ignored = DiscoveryConstants.IgnoredCommandGroups;
        Assert.DoesNotContain(result, p => ignored.Contains(p.CreateMetadata().Name, StringComparer.OrdinalIgnoreCase));
        // Should include at least one known group (e.g. storage)
        Assert.Contains(result, p => p.CreateMetadata().Name == "storage");
        // Should set ReadOnly and EntryPoint as expected
        foreach (var provider in result)
        {
            Assert.True(((CommandGroupServerProvider)provider).ReadOnly);
            Assert.Equal(azmcpEntryPoint, ((CommandGroupServerProvider)provider).EntryPoint);
        }
    }

    [Fact]
    public void GetAzmcpExecutablePath_ReturnsCorrectPathForCurrentOS()
    {
        // Arrange & Act
        var azmcpPath = McpTestUtilities.GetAzMcpExecutablePath();

        // Assert
        Assert.NotNull(azmcpPath);
        Assert.NotEmpty(azmcpPath);

        // Should end with the correct executable name for the current OS
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            Assert.EndsWith("azmcp.exe", azmcpPath);
        }
        else
        {
            Assert.EndsWith("azmcp", azmcpPath);
            Assert.False(azmcpPath.EndsWith("azmcp.exe"));
        }

        // Should be in the same directory as the test assembly
        var testAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(testAssemblyPath);
        var expectedDirectory = Path.GetDirectoryName(azmcpPath);
        Assert.Equal(testDirectory, expectedDirectory);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNamespaceFilter_ReturnsOnlySpecifiedNamespaces()
    {
        // Arrange
        var options = new ServiceStartOptions
        {
            Namespace = ["storage", "keyvault"]
        };
        var strategy = CreateStrategy(options: options);

        // Act
        var servers = await strategy.DiscoverServersAsync();
        var serverNames = servers.Select(s => s.CreateMetadata().Name).ToList();

        // Assert
        Assert.NotNull(servers);
        Assert.Equal(2, serverNames.Count);
        Assert.Contains("storage", serverNames);
        Assert.Contains("keyvault", serverNames);

        // Should not contain other namespaces
        Assert.DoesNotContain("cosmos", serverNames);
        Assert.DoesNotContain("monitor", serverNames);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithEmptyNamespaceFilter_ReturnsAllNamespaces()
    {
        // Arrange
        var options = new ServiceStartOptions
        {
            Namespace = []
        };
        var strategy = CreateStrategy(options: options);

        // Act
        var servers = await strategy.DiscoverServersAsync();
        var serverNames = servers.Select(s => s.CreateMetadata().Name).ToList();

        // Assert
        Assert.NotNull(servers);
        Assert.True(serverNames.Count > 2); // Should have more than just storage and keyvault

        // Should contain expected namespaces (but not ignored ones)
        Assert.Contains("storage", serverNames);
        Assert.Contains("keyvault", serverNames);
        Assert.DoesNotContain("server", serverNames); // Should be ignored
        Assert.DoesNotContain("extension", serverNames); // Should be ignored
        Assert.DoesNotContain("subscription", serverNames); // Should be ignored
        Assert.DoesNotContain("group", serverNames); // Should be ignored
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNullNamespaceFilter_ReturnsAllNamespaces()
    {
        // Arrange
        var options = new ServiceStartOptions
        {
            Namespace = null
        };
        var strategy = CreateStrategy(options: options);

        // Act
        var servers = await strategy.DiscoverServersAsync();
        var serverNames = servers.Select(s => s.CreateMetadata().Name).ToList();

        // Assert
        Assert.NotNull(servers);
        Assert.True(serverNames.Count > 2); // Should have more than just storage and keyvault

        // Should contain expected namespaces (but not ignored ones)
        Assert.Contains("storage", serverNames);
        Assert.Contains("keyvault", serverNames);
        Assert.DoesNotContain("server", serverNames); // Should be ignored
        Assert.DoesNotContain("extension", serverNames); // Should be ignored
        Assert.DoesNotContain("subscription", serverNames); // Should be ignored
        Assert.DoesNotContain("group", serverNames); // Should be ignored
    }

}
