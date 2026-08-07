#pragma warning disable MCP9003 // Obsolete RequestContext constructor - migrating during Phase 1
#pragma warning disable MCP9005 // Deprecated Sampling/Logging APIs - backward compat during Phase 1
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Tests.Areas.Server.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;
using ExtensionsOptions = Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public class ServerToolLoaderTests
{
    private static (ServerToolLoader toolLoader, IMcpDiscoveryStrategy mockDiscoveryStrategy) CreateToolLoader(ToolLoaderOptions? options = null)
    {
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        var logger = loggerFactory.CreateLogger<ServerToolLoader>();
        var toolLoaderOptions = ExtensionsOptions.Options.Create(options ?? new ToolLoaderOptions());

        var toolLoader = new ServerToolLoader(mockDiscoveryStrategy, toolLoaderOptions, logger);
        return (toolLoader, mockDiscoveryStrategy);
    }

    private static RequestContext<ListToolsRequestParams> CreateRequest()
    {
        var mockServer = Substitute.For<McpServer>();
        return new(mockServer, new() { Method = RequestMethods.ToolsList }, new ListToolsRequestParams());
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName, IDictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<McpServer>();
        return new(mockServer, new() { Method = RequestMethods.ToolsCall }, new CallToolRequestParams
        {
            Name = toolName,
            Arguments = arguments ?? new Dictionary<string, JsonElement>()
        });
    }

    [Fact]
    public async Task CallToolHandler_WithoutListToolsFirst_ShouldSucceed()
    {
        // Arrange - use real RegistryDiscoveryStrategy since ServerToolLoader depends on it
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var serviceStartOptions = ExtensionsOptions.Options.Create(new ServerStartOptions());
        var toolLoaderOptions = ExtensionsOptions.Options.Create(new ToolLoaderOptions());
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
        var serviceStartOptions = ExtensionsOptions.Options.Create(new ServerStartOptions());
        var toolLoaderOptions = ExtensionsOptions.Options.Create(new ToolLoaderOptions());
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
    public async Task ListToolsHandler_WithExternalServers_ExposesProxyRouterTools()
    {
        // Arrange
        var providerA = Substitute.For<IMcpServerProvider>();
        providerA.CreateMetadata().Returns(new McpServerMetadata
        {
            Id = "documentation",
            Name = "documentation",
            Description = "Docs server"
        });

        var providerB = Substitute.For<IMcpServerProvider>();
        providerB.CreateMetadata().Returns(new McpServerMetadata
        {
            Id = "arm",
            Name = "arm",
            Description = "ARM server",
            ToolPrefix = "arm_"
        });

        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        discoveryStrategy.DiscoverServersAsync(TestContext.Current.CancellationToken)
            .Returns(Task.FromResult<IEnumerable<IMcpServerProvider>>([providerA, providerB]));

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ServerToolLoader>();
        var toolLoader = new ServerToolLoader(discoveryStrategy, ExtensionsOptions.Options.Create(new ToolLoaderOptions()), logger);
        var request = CreateRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Equal(2, result.Tools.Count);

        var toolNames = result.Tools.Select(t => t.Name).ToList();
        Assert.Contains("documentation", toolNames);
        Assert.Contains("arm", toolNames);

        var documentationTool = result.Tools.Single(t => t.Name == "documentation");
        Assert.Contains("hierarchical MCP command router", documentationTool.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallToolHandler_WithExternalServerCommand_AttemptsProxyRoutingAndReturnsLearnResponse()
    {
        // Arrange
        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        discoveryStrategy.GetOrCreateClientAsync("documentation", Arg.Any<McpClientOptions>(), TestContext.Current.CancellationToken)
            .Returns((McpClient)null!);

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ServerToolLoader>();
        var toolLoader = new ServerToolLoader(discoveryStrategy, ExtensionsOptions.Options.Create(new ToolLoaderOptions()), logger);

        var request = CreateCallToolRequest("documentation",
            new Dictionary<string, JsonElement>
            {
                { "intent", JsonDocument.Parse("\"search docs\"").RootElement },
                { "command", JsonDocument.Parse("\"microsoft_docs_search\"").RootElement },
                { "parameters", JsonDocument.Parse("""
                    {
                        "question": "how to deploy azure mcp server"
                    }
                    """).RootElement }
            });

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(true, result.IsError);
        var text = result.Content.OfType<TextContentBlock>().Single();
        Assert.Contains("available command", text.Text, StringComparison.OrdinalIgnoreCase);

        await discoveryStrategy.Received()
            .GetOrCreateClientAsync("documentation", Arg.Any<McpClientOptions>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetChildToolList_WithReadOnlyOption_ReturnsOnlyReadOnlyTools()
    {
        // Arrange
        var toolsResult = new JsonObject([
            new("tools", new JsonArray([
                new JsonObject([
                    new("name", "storage"),
                    new("inputSchema", new JsonObject { ["type"] = "object" }),
                    new("annotations", new JsonObject([
                        new("readOnlyHint", true)
                    ]))
                ]),
                new JsonObject([
                    new("name", "keyvault"),
                    new("inputSchema", new JsonObject { ["type"] = "object" }),
                    new("annotations", new JsonObject([
                        new("readOnlyHint", false)
                    ]))
                ])
            ]))
        ]);
        var mcpClient = LoopbackMcpClient.Create(req =>
            req.Method == RequestMethods.ToolsList
                ? new JsonRpcResponse { Result = toolsResult }
                : null);
        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        discoveryStrategy.GetOrCreateClientAsync("storage", Arg.Any<McpClientOptions?>(), TestContext.Current.CancellationToken)
            .Returns(mcpClient);
        var toolLoaderOptions = ExtensionsOptions.Options.Create(new ToolLoaderOptions() { ReadOnly = true });
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
        var toolsResult = new JsonObject([
            new("tools", new JsonArray([
                new JsonObject([
                    new("name", "storage"),
                    new("inputSchema", new JsonObject { ["type"] = "object" }),
                    new("meta", new JsonObject([
                        new(McpHelper.LocalRequiredHintMetaKey, true)
                    ]))
                ]),
                new JsonObject([
                    new("name", "keyvault"),
                    new("inputSchema", new JsonObject { ["type"] = "object" }),
                    new("meta", new JsonObject([
                        new(McpHelper.LocalRequiredHintMetaKey, false)
                    ]))
                ])
            ]))
        ]);
        var mcpClient = LoopbackMcpClient.Create(req =>
            req.Method == RequestMethods.ToolsList
                ? new JsonRpcResponse { Result = toolsResult }
                : null);
        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        discoveryStrategy.GetOrCreateClientAsync("storage", Arg.Any<McpClientOptions?>(), TestContext.Current.CancellationToken)
            .Returns(mcpClient);
        var toolLoaderOptions = ExtensionsOptions.Options.Create(new ToolLoaderOptions() { IsHttpMode = true });
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = CreateCallToolRequest("storage");

        // Act
        var tools = await toolLoader.GetChildToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool =>
        {
            Assert.False(McpHelper.HasHint(tool, McpHelper.LocalRequiredHintMetaKey),
                $"Tool '{tool.Name}' should have LocalRequiredHint = false when HTTP mode is enabled");
        });
    }

    #region Execution-Time Mode Enforcement Tests

    private static (ServerToolLoader toolLoader, IMcpDiscoveryStrategy discoveryStrategy) CreateToolLoaderWithMockClient(
        ToolLoaderOptions options, MockMcpClientBuilder clientBuilder, string serverName = "test-server")
    {
        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer(serverName, serverName, $"{serverName} description", clientBuilder)
            .Build();

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ServerToolLoader>();
        var toolLoaderOptions = ExtensionsOptions.Options.Create(options);

        return (new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger), discoveryStrategy);
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequestWithCommand(
        string serverName, string command, Dictionary<string, JsonElement>? extraParams = null)
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            { "intent", JsonDocument.Parse($"\"Execute {command}\"").RootElement },
            { "command", JsonDocument.Parse($"\"{command}\"").RootElement },
        };

        if (extraParams != null)
        {
            foreach (var kvp in extraParams)
            {
                arguments[kvp.Key] = kvp.Value;
            }
        }

        var mockServer = Substitute.For<McpServer>();
        return new(mockServer, new() { Method = RequestMethods.ToolsCall }, new CallToolRequestParams
        {
            Name = serverName,
            Arguments = arguments
        });
    }

    [Fact]
    public async Task CallToolHandler_WithReadOnlyMode_RejectsNonReadOnlyCommand()
    {
        // Arrange
        var readOnlyTool = new Tool
        {
            Name = "account_list",
            Description = "List storage accounts",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = true }
        };

        var writeTool = new Tool
        {
            Name = "account_create",
            Description = "Create storage account",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var writeToolExecuted = false;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Listed accounts" }], IsError = false })
            .AddTool(writeTool, _ =>
            {
                writeToolExecuted = true;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }], IsError = false };
            });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - The non-read-only tool must NOT be executed
        Assert.False(writeToolExecuted, "Non-read-only tool should not be executed in read-only mode");
        Assert.NotNull(result);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        // Should not contain the write tool's success response
        Assert.DoesNotContain("Created account", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithReadOnlyMode_AllowsReadOnlyCommand()
    {
        // Arrange
        var readOnlyTool = new Tool
        {
            Name = "account_list",
            Description = "List storage accounts",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = true }
        };

        var writeTool = new Tool
        {
            Name = "account_create",
            Description = "Create storage account",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Listed accounts" }], IsError = false })
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }], IsError = false });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "account_list");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow the read-only tool call
        Assert.NotNull(result);
        Assert.False(result.IsError);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Equal("Listed accounts", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithIsHttpMode_RejectsLocalRequiredCommand()
    {
        // Arrange
        var localRequiredTool = new Tool
        {
            Name = "local_command",
            Description = "Local-only command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations(),
            Meta = [new(McpHelper.LocalRequiredHintMetaKey, true)]
        };

        var remoteTool = new Tool
        {
            Name = "remote_command",
            Description = "Remote-safe command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations(),
            Meta = [new(McpHelper.LocalRequiredHintMetaKey, false)]
        };

        var localToolExecuted = false;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(localRequiredTool, _ =>
            {
                localToolExecuted = true;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Local result" }], IsError = false };
            })
            .AddTool(remoteTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Remote result" }], IsError = false });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(IsHttpMode: true), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "local_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - The local-required tool must NOT be executed in HTTP mode
        Assert.False(localToolExecuted, "Local-required tool should not be executed in HTTP mode");
        Assert.NotNull(result);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        // Should not contain the local tool's success response
        Assert.DoesNotContain("Local result", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithIsHttpMode_AllowsNonLocalRequiredCommand()
    {
        // Arrange
        var localRequiredTool = new Tool
        {
            Name = "local_command",
            Description = "Local-only command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations(),
            Meta = [new(McpHelper.LocalRequiredHintMetaKey, true)]
        };

        var remoteTool = new Tool
        {
            Name = "remote_command",
            Description = "Remote-safe command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations(),
            Meta = [new(McpHelper.LocalRequiredHintMetaKey, false)]
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(localRequiredTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Local result" }], IsError = false })
            .AddTool(remoteTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Remote result" }], IsError = false });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(IsHttpMode: true), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "remote_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow the non-local-required tool call
        Assert.NotNull(result);
        Assert.False(result.IsError);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Equal("Remote result", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithReadOnlyMode_RejectsCommandWithNullAnnotations()
    {
        // Arrange - tool with null annotations should be rejected in read-only mode
        var toolWithoutAnnotations = new Tool
        {
            Name = "unknown_command",
            Description = "Tool without annotations",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = null
        };

        var toolExecuted = false;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(toolWithoutAnnotations, _ =>
            {
                toolExecuted = true;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Result" }], IsError = false };
            });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "unknown_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Tool without read-only annotation must NOT be executed in read-only mode
        Assert.False(toolExecuted, "Tool without ReadOnlyHint should not be executed in read-only mode");
        Assert.NotNull(result);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        // Should not contain the tool's success response
        Assert.DoesNotContain("Result", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithoutReadOnlyMode_AllowsNonReadOnlyCommand()
    {
        // Arrange
        var writeTool = new Tool
        {
            Name = "account_create",
            Description = "Create storage account",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }], IsError = false });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: false), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow execution when read-only mode is not enabled
        Assert.NotNull(result);
        Assert.False(result.IsError);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Equal("Created account", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithReadOnlyAndSamplingFallback_RejectsNonReadOnlyResolvedCommand()
    {
        // Arrange - Set up a server where the direct command name doesn't match,
        // forcing the code path through sampling. With no sampling support on the mock server,
        // this should fall back to learn mode or reject.
        var readOnlyTool = new Tool
        {
            Name = "account_list",
            Description = "List storage accounts",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = true }
        };

        var writeTool = new Tool
        {
            Name = "account_create",
            Description = "Create storage account",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Listed accounts" }], IsError = false })
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }], IsError = false });

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder, "storage");

        // Use a command name that doesn't exist in the filtered available tools list.
        // "account_create" exists in the backend but is filtered out by ReadOnly.
        // The non-existent command "bad_command" will fail the availableTools check
        // and without sampling support, it should fall back to learn mode.
        var request = CreateCallToolRequestWithCommand("storage", "bad_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should NOT succeed with a write operation.
        // The command doesn't match any filtered tool, so it should trigger learn mode or rejection.
        Assert.NotNull(result);
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        // Should not contain the write tool's response
        Assert.DoesNotContain("Created account", textContent.Text);
    }

    #endregion
}
