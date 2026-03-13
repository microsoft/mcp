// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
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

    private static RequestContext<ListToolsRequestParams> CreateRequest()
    {
        var mockServer = Substitute.For<McpServer>();
        return new(mockServer, new() { Method = RequestMethods.ToolsList })
        {
            Params = new()
        };
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName, IDictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<McpServer>();
        return new(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new()
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
        var discoveryStrategy = RegistryDiscoveryStrategyHelper.CreateStrategy(serviceStartOptions.Value, discoveryLogger);
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
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

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

        mockDiscoveryStrategy.DiscoverServersAsync(TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(Enumerable.Empty<IMcpServerProvider>()));

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var discoveryStrategy = RegistryDiscoveryStrategyHelper.CreateStrategy(serviceStartOptions.Value, discoveryLogger);
        var logger = loggerFactory.CreateLogger<ServerToolLoader>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = CreateRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task GetChildToolList_WithReadOnlyOption_ReturnsOnlyReadOnlyTools()
    {
        // Arrange
        var mcpClient = Substitute.For<McpClient>();
        mcpClient.SendRequestAsync(Arg.Is<JsonRpcRequest>(r => r.Method == RequestMethods.ToolsList), Arg.Any<CancellationToken>())
            .Returns(new JsonRpcResponse()
            {
                Result = new JsonObject([
                    new("tools", new JsonArray([
                        new JsonObject([
                            new("name", "storage"),
                            new("annotations", new JsonObject([
                                new("readOnlyHint", true)
                            ]))
                        ]),
                        new JsonObject([
                            new("name", "keyvault"),
                            new("annotations", new JsonObject([
                                new("readOnlyHint", false)
                            ]))
                        ])
                    ]))
                ])
            });
        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        discoveryStrategy.GetOrCreateClientAsync("storage", Arg.Any<McpClientOptions?>(), TestContext.Current.CancellationToken)
            .Returns(mcpClient);
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions() { ReadOnly = true });
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = CreateCallToolRequest("storage");

        // Act
        var tools = await toolLoader.GetChildToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool => Assert.True(tool.Annotations?.ReadOnlyHint, $"Tool '{tool.Name}' should have ReadOnlyHint = true when ReadOnly mode is enabled"));
    }

    [Fact]
    public async Task GetChildToolList_WithIsHttpOption_DoesNotReturnLocalRequiredTools()
    {
        // Arrange
        var storageTool = new Tool()
        {
            Name = "storage",
            Meta = new([new("LocalRequiredHint", true)])
        };
        var storageClientTool = new McpClientTool(Substitute.For<McpClient>(), storageTool);
        var keyvaultTool = new Tool()
        {
            Name = "keyvault",
            Meta = new([new("LocalRequiredHint", false)])
        };
        var keyvaultClientTool = new McpClientTool(Substitute.For<McpClient>(), keyvaultTool);
        var mcpClient = Substitute.For<McpClient>();
        mcpClient.SendRequestAsync(Arg.Is<JsonRpcRequest>(r => r.Method == RequestMethods.ToolsList), Arg.Any<CancellationToken>())
            .Returns(new JsonRpcResponse()
            {
                Result = new JsonObject([
                    new("tools", new JsonArray([
                        new JsonObject([
                            new("name", "storage"),
                            new("meta", new JsonObject([
                                new("LocalRequiredHint", true)
                            ]))
                        ]),
                        new JsonObject([
                            new("name", "keyvault"),
                            new("meta", new JsonObject([
                                new("LocalRequiredHint", false)
                            ]))
                        ])
                    ]))
                ])
            });
        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        discoveryStrategy.GetOrCreateClientAsync("storage", Arg.Any<McpClientOptions?>(), TestContext.Current.CancellationToken)
            .Returns(mcpClient);
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions() { IsHttpMode = true });
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = CreateCallToolRequest("storage");

        // Act
        var tools = await toolLoader.GetChildToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool =>
        {
            var meta = tool.Meta;
            if (meta != null && meta.TryGetPropertyValue("LocalRequiredHint", out var localRequiredHint))
            {
                Assert.False(localRequiredHint?.GetValue<bool>(),
                    $"Tool '{tool.Name}' should have LocalRequiredHint = false when HTTP mode is enabled");
            }
        });
    }
}
