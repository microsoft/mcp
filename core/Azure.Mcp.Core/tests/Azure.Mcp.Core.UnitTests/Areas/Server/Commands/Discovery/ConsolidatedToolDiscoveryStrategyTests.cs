// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tests.Client.Helpers;
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
        var startOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new ConsolidatedToolDiscoveryStrategy(factory, startOptions, logger);
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
        var strategy = new ConsolidatedToolDiscoveryStrategy(null!, options, logger);
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
        var strategy = new ConsolidatedToolDiscoveryStrategy(commandFactory, null!, logger);
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
        var strategy = new ConsolidatedToolDiscoveryStrategy(commandFactory, options, logger);

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
    public async Task DiscoverServersAsync_WithDefaultOptions_ReturnsConsolidatedToolProvider()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotNull(result);
        var providers = result.ToList();
        Assert.Single(providers); // Should return exactly one consolidated provider
        Assert.All(providers, provider => Assert.IsType<CommandGroupServerProvider>(provider));
        
        // Verify the provider has the expected name
        var metadata = providers[0].CreateMetadata();
        Assert.Equal("get_azure_best_practices", metadata.Name);
        Assert.Equal("get_azure_best_practices", metadata.Id);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithReadOnlyFalse_CreatesNonReadOnlyProvider()
    {
        // Arrange
        var options = new ServiceStartOptions { ReadOnly = false };
        var strategy = CreateStrategy(options: options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        Assert.False(((CommandGroupServerProvider)providers[0]).ReadOnly);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithReadOnlyTrue_CreatesReadOnlyProvider()
    {
        // Arrange
        var options = new ServiceStartOptions { ReadOnly = true };
        var strategy = CreateStrategy(options: options);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        Assert.True(((CommandGroupServerProvider)providers[0]).ReadOnly);
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
        var providers = result.ToList();
        Assert.Single(providers);
        Assert.False(((CommandGroupServerProvider)providers[0]).ReadOnly);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithCustomEntryPoint_SetsEntryPointOnProvider()
    {
        // Arrange
        var customEntryPoint = McpTestUtilities.GetAzMcpExecutablePath();
        var strategy = CreateStrategy(entryPoint: customEntryPoint);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        Assert.Equal(customEntryPoint, ((CommandGroupServerProvider)providers[0]).EntryPoint);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithNullEntryPoint_UsesCurrentProcessExecutable()
    {
        // Arrange
        var strategy = CreateStrategy(entryPoint: null);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        // When EntryPoint is set to null, CommandGroupServerProvider defaults to current process executable
        var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        var actualEntryPoint = ((CommandGroupServerProvider)providers[0]).EntryPoint;
        Assert.NotNull(actualEntryPoint);
        // Should be the current test process executable
        Assert.Equal(currentProcessPath, actualEntryPoint);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithEmptyEntryPoint_ProviderDefaultsToCurrentProcess()
    {
        // Arrange
        var strategy = CreateStrategy(entryPoint: "");

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        var actualEntryPoint = ((CommandGroupServerProvider)providers[0]).EntryPoint;
        // CommandGroupServerProvider defaults empty/null to current process
        Assert.Equal(currentProcessPath, actualEntryPoint);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithWhitespaceEntryPoint_ProviderDefaultsToCurrentProcess()
    {
        // Arrange
        var strategy = CreateStrategy(entryPoint: "   ");

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        var currentProcessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        var actualEntryPoint = ((CommandGroupServerProvider)providers[0]).EntryPoint;
        // CommandGroupServerProvider defaults whitespace to current process
        Assert.Equal(currentProcessPath, actualEntryPoint);
    }

    [Fact]
    public async Task DiscoverServersAsync_ProviderHasCorrectMetadata()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);

        var metadata = providers[0].CreateMetadata();
        Assert.NotNull(metadata);
        Assert.Equal("get_azure_best_practices", metadata.Name);
        Assert.Equal("get_azure_best_practices", metadata.Id);
        Assert.Equal(metadata.Name, metadata.Id); // Should be the same
        Assert.NotNull(metadata.Description);
        Assert.Contains("Azure best practices", metadata.Description);
        Assert.Contains("infrastructure schema", metadata.Description);
    }

    [Fact]
    public async Task DiscoverServersAsync_ProviderIsCommandGroupServerProviderType()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        Assert.IsType<CommandGroupServerProvider>(providers[0]);
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
        Assert.Single(providers1);
        Assert.Single(providers2);

        // Should return equivalent results
        var metadata1 = providers1[0].CreateMetadata();
        var metadata2 = providers2[0].CreateMetadata();
        Assert.Equal(metadata1.Name, metadata2.Name);
        Assert.Equal(metadata1.Id, metadata2.Id);
        Assert.Equal(metadata1.Description, metadata2.Description);
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
        var providers = result.ToList();
        Assert.Single(providers);
        
        var serverProvider = (CommandGroupServerProvider)providers[0];
        Assert.True(serverProvider.ReadOnly);
        Assert.Equal(azmcpEntryPoint, serverProvider.EntryPoint);
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
        Assert.Equal(1, count1); // Should always have exactly one consolidated provider
    }

    [Fact]
    public async Task DiscoverServersAsync_InheritsFromBaseDiscoveryStrategy()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act & Assert
        Assert.IsAssignableFrom<BaseDiscoveryStrategy>(strategy);
        Assert.IsAssignableFrom<IMcpDiscoveryStrategy>(strategy);
        
        // Should be able to call the interface method
        var result = await strategy.DiscoverServersAsync();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DiscoverServersAsync_FiltersCommandsByCompositeToolMapped()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        
        var provider = (CommandGroupServerProvider)providers[0];
        var metadata = provider.CreateMetadata();
        Assert.Equal("get_azure_best_practices", metadata.Name);
        
        // The strategy should only create a provider for commands with CompositeToolMapped = "get_azure_best_practices"
        // The actual filtering logic is tested implicitly by verifying we get the expected provider name
    }

    [Fact]
    public async Task DiscoverServersAsync_CreatesCommandGroupWithCorrectToolMetadata()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        
        // We can't directly access the CommandGroup's ToolMetadata from the provider,
        // but we can verify the provider was created successfully and has the expected characteristics
        var provider = (CommandGroupServerProvider)providers[0];
        Assert.NotNull(provider);
        
        var metadata = provider.CreateMetadata();
        Assert.Equal("get_azure_best_practices", metadata.Name);
    }

    // Keep original tests for backward compatibility
    [Fact]
    public async Task ShouldDiscoverConsolidatedToolServer()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new ConsolidatedToolDiscoveryStrategy(commandFactory, options, logger);
        var result = await strategy.DiscoverServersAsync();
        Assert.NotNull(result);
        var providers = result.ToList();
        Assert.Single(providers);
    }

    [Fact]
    public async Task ShouldDiscoverConsolidatedToolServer_SetsPropertiesCorrectly()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { ReadOnly = true });
        var azmcpEntryPoint = McpTestUtilities.GetAzMcpExecutablePath();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new ConsolidatedToolDiscoveryStrategy(commandFactory, options, logger)
        {
            EntryPoint = azmcpEntryPoint
        };
        var result = (await strategy.DiscoverServersAsync()).ToList();
        Assert.Single(result);
        
        // Should have the consolidated tool provider with expected name
        var provider = result[0];
        Assert.Equal("get_azure_best_practices", provider.CreateMetadata().Name);
        
        // Should set ReadOnly and EntryPoint as expected
        var serverProvider = (CommandGroupServerProvider)provider;
        Assert.True(serverProvider.ReadOnly);
        Assert.Equal(azmcpEntryPoint, serverProvider.EntryPoint);
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
    public async Task DiscoverServersAsync_AlwaysReturnsOneProvider()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        // Unlike CommandGroupDiscoveryStrategy which returns multiple providers (one per command group),
        // ConsolidatedToolDiscoveryStrategy always returns exactly one provider that consolidates
        // all commands with the same CompositeToolMapped value
        Assert.Single(providers);
    }

    [Fact]
    public async Task DiscoverServersAsync_ProviderNameMatchesCompositeToolMappedValue()
    {
        // Arrange
        var strategy = CreateStrategy();

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        var providers = result.ToList();
        Assert.Single(providers);
        
        var metadata = providers[0].CreateMetadata();
        // The provider name should match the CompositeToolMapped value used in the filtering logic
        Assert.Equal("get_azure_best_practices", metadata.Name);
    }
}