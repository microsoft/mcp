// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.Discovery;

public class CommandGroupDiscoveryStrategyTests
{
    private static string GetAzmcpExecutablePath()
    {
        // Get the directory where the test binary is located
        var testAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var testDirectory = Path.GetDirectoryName(testAssemblyPath);

        // On Windows, use .exe extension; on other OS, no extension
        var executableName = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows) ? "azmcp.exe" : "azmcp";

        return Path.Combine(testDirectory!, executableName);
    }

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
    public void EntryPoint_CanBeSetAndRetrieved()
    {
        // Arrange
        var strategy = CreateStrategy();
        var azmcpPath = GetAzmcpExecutablePath();

        // Act
        strategy.EntryPoint = azmcpPath;

        // Assert
        Assert.Equal(azmcpPath, strategy.EntryPoint);
    }

    [Fact]
    public async Task DiscoverServersAsync_WithCustomEntryPoint_SetsEntryPointOnAllProviders()
    {
        // Arrange
        var customEntryPoint = GetAzmcpExecutablePath();
        var strategy = CreateStrategy(entryPoint: customEntryPoint);

        // Act
        var result = await strategy.DiscoverServersAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, provider =>
            Assert.Equal(customEntryPoint, ((CommandGroupServerProvider)provider).EntryPoint));
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
        var ignoredGroups = new[] { "extension", "server", "tools" };
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
        var azmcpEntryPoint = GetAzmcpExecutablePath();
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
        Assert.DoesNotContain("extension", names, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("server", names, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("tools", names, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShouldDiscoverServers_ExcludesIgnoredGroupsAndSetsProperties()
    {
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { ReadOnly = true });
        var azmcpEntryPoint = GetAzmcpExecutablePath();
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<CommandGroupDiscoveryStrategy>>();
        var strategy = new CommandGroupDiscoveryStrategy(commandFactory, options, logger)
        {
            EntryPoint = azmcpEntryPoint
        };
        var result = (await strategy.DiscoverServersAsync()).ToList();
        Assert.NotEmpty(result);
        // Should not include ignored groups
        var ignored = new[] { "extension", "server", "tools" };
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
        var azmcpPath = GetAzmcpExecutablePath();

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
}
