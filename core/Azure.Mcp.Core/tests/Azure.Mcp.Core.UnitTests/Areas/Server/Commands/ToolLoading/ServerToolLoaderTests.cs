// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Areas.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class ServerToolLoaderTests
{
    private static (ServerToolLoader toolLoader, IMcpDiscoveryStrategy mockDiscoveryStrategy) CreateToolLoader(ToolLoaderOptions? options = null)
    {
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        var logger = loggerFactory.CreateLogger<ServerToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ToolLoaderOptions());

        var toolLoader = new ServerToolLoader(mockDiscoveryStrategy, toolLoaderOptions, logger);
        return (toolLoader, mockDiscoveryStrategy);
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer)
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName, IReadOnlyDictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        return new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments ?? new Dictionary<string, JsonElement>()
            }
        };
    }

    [Fact]
    public async Task CallToolHandler_WithoutListToolsFirst_ShouldSucceed()
    {
        // Arrange - use real RegistryDiscoveryStrategy since ServerToolLoader depends on it
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var serviceStartOptions = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());
        var discoveryLogger = loggerFactory.CreateLogger<RegistryDiscoveryStrategy>();
        var discoveryStrategy = new RegistryDiscoveryStrategy(serviceStartOptions, discoveryLogger);
        var logger = loggerFactory.CreateLogger<ServerToolLoader>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = CreateCallToolRequest("documentation",
            new Dictionary<string, JsonElement>
            {
                { "intent", JsonDocument.Parse("\"search for information about implementing MCP servers\"").RootElement },
                { "command", JsonDocument.Parse("\"microsoft_docs_search\"").RootElement },
                { "parameters", JsonDocument.Parse("""
                    {
                        "question": "how to implement mcp server in azure"
                    }
                    """).RootElement }
            });

        // Act - Call CallToolHandler WITHOUT calling ListToolsHandler first
        // This should work without requiring ListToolsHandler to be called first
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert - The tool call should succeed
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task ListToolsHandler_WithNoServers_ReturnsEmptyToolList()
    {
        // Arrange
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoader();
        var request = CreateRequest();

        mockDiscoveryStrategy.DiscoverServersAsync()
            .Returns(Task.FromResult(Enumerable.Empty<IMcpServerProvider>()));

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Empty(result.Tools);
    }

    [Fact]
    public async Task ListToolsHandler_WithRealRegistryDiscovery_ReturnsExpectedStructure()
    {
        // Arrange - use real RegistryDiscoveryStrategy
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var serviceStartOptions = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());
        var discoveryLogger = loggerFactory.CreateLogger<RegistryDiscoveryStrategy>();
        var discoveryStrategy = new RegistryDiscoveryStrategy(serviceStartOptions, discoveryLogger);
        var logger = loggerFactory.CreateLogger<ServerToolLoader>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = CreateRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.True(result.Tools.Count >= 0); // Should return at least an empty list

        // Each tool should have proper structure if any exist
        foreach (var tool in result.Tools)
        {
            Assert.NotNull(tool.Name);
            Assert.NotEmpty(tool.Name);
            Assert.NotNull(tool.Description);
            Assert.True(tool.InputSchema.ValueKind != JsonValueKind.Undefined, "InputSchema should be defined");
        }
    }
}
