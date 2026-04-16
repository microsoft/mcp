// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.UnitTests.Areas.Server.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class SingleProxyToolLoaderTests
{
    private static RegistryDiscoveryStrategy CreateStrategy(ServiceStartOptions options, ILogger<RegistryDiscoveryStrategy> logger)
    {
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ServiceStartOptions());
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var registryRoot = RegistryServerHelper.GetRegistryRoot(typeof(Azure.Mcp.Server.Program).Assembly, "Azure.Mcp.Server.Resources.registry.json");
        return new RegistryDiscoveryStrategy(serviceOptions, logger, httpClientFactory, registryRoot!);
    }

    private static (SingleProxyToolLoader toolLoader, IMcpDiscoveryStrategy discoveryStrategy) CreateToolLoader(bool useRealDiscovery = true, ToolLoaderOptions? toolLoaderOptions = null)
    {
        var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<SingleProxyToolLoader>();

        if (useRealDiscovery)
        {
            var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
            var commandGroupLogger = serviceProvider.GetRequiredService<ILogger<CommandGroupDiscoveryStrategy>>();
            var commandGroupDiscoveryStrategy = new CommandGroupDiscoveryStrategy(
                CommandFactoryHelpers.CreateCommandFactory(serviceProvider),
                options,
                commandGroupLogger
            );
            var registryLogger = serviceProvider.GetRequiredService<ILogger<RegistryDiscoveryStrategy>>();
            var registryDiscoveryStrategy = CreateStrategy(options.Value, registryLogger);
            var compositeLogger = serviceProvider.GetRequiredService<ILogger<CompositeDiscoveryStrategy>>();
            var compositeDiscoveryStrategy = new CompositeDiscoveryStrategy([
                commandGroupDiscoveryStrategy,
                registryDiscoveryStrategy
            ], compositeLogger);
            var toolLoader = new SingleProxyToolLoader(compositeDiscoveryStrategy, logger, Microsoft.Extensions.Options.Options.Create(toolLoaderOptions ?? new ToolLoaderOptions()));
            return (toolLoader, compositeDiscoveryStrategy);
        }
        else
        {
            var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
            var toolLoader = new SingleProxyToolLoader(mockDiscoveryStrategy, logger, Microsoft.Extensions.Options.Options.Create(toolLoaderOptions ?? new ToolLoaderOptions()));
            return (toolLoader, mockDiscoveryStrategy);
        }
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateListToolsRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer, new() { Method = RequestMethods.ToolsList })
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequest(
        string toolName = "azure",
        Dictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments ?? []
            }
        };
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsAzureToolWithExpectedSchema()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Tools);

        var azureTool = result.Tools.FirstOrDefault(t => t.Name == "azure");
        Assert.NotNull(azureTool);
        Assert.Contains("real-time, programmatic access to all Azure products", azureTool.Description);

        // Verify the tool has proper structure
        Assert.True(azureTool.InputSchema.ValueKind != JsonValueKind.Undefined);
        Assert.NotNull(azureTool.Annotations);
    }

    [Fact]
    public async Task ListToolsHandler_WithMockedDiscovery_ReturnsSingleAzureTool()
    {
        // Arrange
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoader(useRealDiscovery: false);
        var request = CreateListToolsRequest();

        // Setup mock to return empty servers (SingleProxyToolLoader always returns the azure tool)
        mockDiscoveryStrategy.DiscoverServersAsync(TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(Enumerable.Empty<IMcpServerProvider>()));

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tools);

        var azureTool = result.Tools.First();
        Assert.Equal("azure", azureTool.Name);
        Assert.Contains("real-time, programmatic access to all Azure products", azureTool.Description);
    }

    [Fact]
    public async Task CallToolHandler_WithLearnMode_ReturnsRootToolsList()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, JsonElement>
        {
            ["learn"] = JsonDocument.Parse("true").RootElement,
            ["intent"] = JsonDocument.Parse("\"List available tools\"").RootElement
        };
        var request = CreateCallToolRequest("azure", arguments);

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.IsError);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // Should contain information about available tools
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithToolLearnMode_ThrowsExceptionForUnknownTool()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, JsonElement>
        {
            ["learn"] = JsonDocument.Parse("true").RootElement,
            ["tool"] = JsonDocument.Parse("\"nonexistent\"").RootElement, // Use a tool that doesn't exist
            ["intent"] = JsonDocument.Parse("\"Learn about nonexistent tool\"").RootElement
        };
        var request = CreateCallToolRequest("azure", arguments);

        // Act & Assert
        // The current implementation throws KeyNotFoundException for unknown tools
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CallToolHandler_WithIntentOnly_AutoEnablesLearnMode()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, JsonElement>
        {
            ["intent"] = JsonDocument.Parse("\"Show me available Azure tools\"").RootElement
            // Intent only, should trigger learn mode automatically based on the implementation
        };
        var request = CreateCallToolRequest("azure", arguments);

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.IsError);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // Should return learn mode information since intent was provided without tool/command
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.NotEmpty(textContent.Text);
        // The actual behavior shows available tools list
        Assert.Contains("Here are the available list of tools", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithMissingToolAndCommand_ReturnsGuidanceMessage()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, JsonElement>
        {
            // No learn, tool, or command parameters - should get guidance message
        };
        var request = CreateCallToolRequest("azure", arguments);

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.IsError); // This is guidance, not an error
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Contains("tool\" and \"command\" parameters are required", textContent.Text);
        Assert.Contains("Run again with the \"learn\" argument", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsGuidanceMessage()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = null
        };

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Contains("tool\" and \"command\" parameters are required", textContent.Text);
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
        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();

        var toolLoader = new SingleProxyToolLoader(discoveryStrategy, logger, toolLoaderOptions);
        var request = CreateCallToolRequest("storage");

        // Act
        var tools = await toolLoader.GetToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool => Assert.True(tool.ProtocolTool.Annotations?.ReadOnlyHint, $"Tool '{tool.Name}' should have ReadOnlyHint = true when ReadOnly mode is enabled"));
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
        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();

        var toolLoader = new SingleProxyToolLoader(discoveryStrategy, logger, toolLoaderOptions);
        var request = CreateCallToolRequest("storage");

        // Act
        var tools = await toolLoader.GetToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool =>
        {
            var meta = tool.ProtocolTool.Meta;
            if (meta != null && meta.TryGetPropertyValue("LocalRequiredHint", out var localRequiredHint))
            {
                Assert.False(localRequiredHint?.GetValue<bool>(),
                    $"Tool '{tool.Name}' should have LocalRequiredHint = false when HTTP mode is enabled");
            }
        });
    }

    [Fact]
    public async Task SingleProxyToolLoader_CachesRootToolsJson()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, JsonElement>
        {
            ["learn"] = JsonDocument.Parse("true").RootElement
        };
        var request = CreateCallToolRequest("azure", arguments);

        // Act - Call twice to test caching
        var result1 = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);
        var result2 = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Both calls should succeed and return consistent results
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Null(result1.IsError);
        Assert.Null(result2.IsError);

        // Content should be consistent (testing that caching works)
        var content1 = result1.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text;
        var content2 = result2.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text;
        Assert.NotNull(content1);
        Assert.NotNull(content2);
        Assert.Equal(content1, content2);
    }

    [Fact]
    public void SingleProxyToolLoader_Constructor_ThrowsOnNullArguments()
    {
        // Arrange
        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();
        var discoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(null!, logger, toolLoaderOptions));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(discoveryStrategy, null!, toolLoaderOptions));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(discoveryStrategy, logger, null!));
    }

    #region Execution-Time Mode Enforcement Tests

    private static SingleProxyToolLoader CreateToolLoaderWithMockClient(
        ToolLoaderOptions toolLoaderOptions, MockMcpClientBuilder clientBuilder, string serverName = "storage")
    {
        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer(serverName, serverName, $"{serverName} description", clientBuilder)
            .Build();

        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();
        var options = Microsoft.Extensions.Options.Options.Create(toolLoaderOptions);

        return new SingleProxyToolLoader(discoveryStrategy, logger, options);
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequestWithToolAndCommand(
        string tool, string command)
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["intent"] = JsonDocument.Parse($"\"Execute {command}\"").RootElement,
            ["tool"] = JsonDocument.Parse($"\"{tool}\"").RootElement,
            ["command"] = JsonDocument.Parse($"\"{command}\"").RootElement,
        };

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = "azure",
                Arguments = arguments
            }
        };
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
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Listed accounts" }] })
            .AddTool(writeTool, _ =>
            {
                writeToolExecuted = true;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }] };
            });

        var toolLoader = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(writeToolExecuted, "Non-read-only tool should not be executed in read-only mode");
        Assert.True(result.IsError);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Contains("read-only mode", textContent.Text);
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

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Listed accounts" }] });

        var toolLoader = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_list");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError ?? false);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Equal("Listed accounts", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithHttpMode_RejectsLocalRequiredCommand()
    {
        // Arrange
        var localTool = new Tool
        {
            Name = "local_command",
            Description = "Local-only command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations(),
            Meta = new JsonObject { ["LocalRequiredHint"] = true }
        };

        var remoteTool = new Tool
        {
            Name = "remote_command",
            Description = "Remote-safe command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations()
        };

        var localToolExecuted = false;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(localTool, _ =>
            {
                localToolExecuted = true;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Local result" }] };
            })
            .AddTool(remoteTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Remote result" }] });

        var toolLoader = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(IsHttpMode: true), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "local_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(localToolExecuted, "Local-required tool should not be executed in HTTP mode");
        Assert.True(result.IsError);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Contains("HTTP mode", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithHttpMode_AllowsNonLocalRequiredCommand()
    {
        // Arrange
        var remoteTool = new Tool
        {
            Name = "remote_command",
            Description = "Remote-safe command",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations()
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(remoteTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Remote result" }] });

        var toolLoader = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(IsHttpMode: true), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "remote_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsError ?? false);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Equal("Remote result", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithReadOnlyMode_RejectsCommandWithNullAnnotations()
    {
        // Arrange — tool without annotations should be rejected in read-only mode
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
                return new CallToolResult { Content = [new TextContentBlock { Text = "Result" }] };
            });

        var toolLoader = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: true), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "unknown_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(toolExecuted, "Tool without ReadOnlyHint should not be executed in read-only mode");
        Assert.True(result.IsError);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Contains("read-only mode", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithoutModeRestrictions_AllowsNonReadOnlyCommand()
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
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }] });

        var toolLoader = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert — should execute without restrictions
        Assert.False(result.IsError ?? false);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Equal("Created account", textContent.Text);
    }

    #endregion
}
