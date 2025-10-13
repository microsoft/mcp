// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
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
        var logger = NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<ConsolidatedToolDiscoveryStrategy>>();
        var strategy = new ConsolidatedToolDiscoveryStrategy(factory, startOptions, logger);
        if (entryPoint != null)
        {
            strategy.EntryPoint = entryPoint;
        }
        return strategy;
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
        Assert.All(providers, provider => Assert.IsType<ConsolidatedToolServerProvider>(provider));
    }
}
