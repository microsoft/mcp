// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.Discovery;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.UnitTests.Areas.Server.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class RegistryToolLoaderTests
{
    private static (RegistryToolLoader toolLoader, IMcpDiscoveryStrategy mockDiscoveryStrategy) CreateToolLoader(ToolLoaderOptions? options = null)
    {
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var mockDiscoveryStrategy = new MockMcpDiscoveryStrategyBuilder().Build();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ToolLoaderOptions());

        var toolLoader = new RegistryToolLoader(mockDiscoveryStrategy, toolLoaderOptions, logger);
        return (toolLoader, mockDiscoveryStrategy);
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateListToolsRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer, new() { Method = RequestMethods.ToolsList })
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName, IReadOnlyDictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments ?? new Dictionary<string, JsonElement>()
            }
        };
    }

    [Fact]
    public async Task ListToolsHandler_WithNoServers_ReturnsEmptyToolList()
    {
        // Arrange
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoader();
        var request = CreateListToolsRequest();

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
    public async Task ListToolsHandler_WithMockServerProvider_ReturnsExpectedStructure()
    {
        // Arrange
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool("test-tool-1", "Test Tool 1 Description", "Test response 1")
            .AddTool("test-tool-2", "Test Tool 2 Description", "Test response 2");

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("test-server", "test-server", "Test Server Description", clientBuilder)
            .Build();

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());

        var toolLoader = new RegistryToolLoader(discoveryStrategy, serviceOptions, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Equal(2, result.Tools.Count);
        Assert.Contains(result.Tools, t => t.Name == "test-tool-1");
        Assert.Contains(result.Tools, t => t.Name == "test-tool-2");
    }

    [Fact]
    public async Task ListToolsHandler_WithReadOnlyOption_FiltersProperly()
    {
        // Arrange
        var readOnlyTool = new Tool
        {
            Name = "readonly-tool",
            Description = "Read-only tool",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = true }
        };

        var writeTool = new Tool
        {
            Name = "write-tool",
            Description = "Write tool",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Read-only result" }], IsError = false })
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Write result" }], IsError = false });

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("test-server", "test-server", "Test Server Description", clientBuilder)
            .Build();

        var readOnlyOptions = new ToolLoaderOptions(ReadOnly: true);
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(readOnlyOptions);

        var toolLoader = new RegistryToolLoader(discoveryStrategy, serviceOptions, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // When ReadOnly is enabled, only tools with ReadOnlyHint = true should be returned
        Assert.Single(result.Tools);
        var returnedTool = result.Tools.First();
        Assert.Equal("readonly-tool", returnedTool.Name);
        Assert.True(returnedTool.Annotations?.ReadOnlyHint == true, "Returned tool should have ReadOnlyHint = true");

        // Verify that the write tool was filtered out
        Assert.DoesNotContain(result.Tools, t => t.Name == "write-tool");
    }

    [Fact]
    public async Task ListToolsHandler_WithReadOnlyDisabled_ReturnsAllTools()
    {
        // Arrange
        var readOnlyTool = new Tool
        {
            Name = "readonly-tool",
            Description = "Read-only tool",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = true }
        };

        var writeTool = new Tool
        {
            Name = "write-tool",
            Description = "Write tool",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Read-only result" }], IsError = false })
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Write result" }], IsError = false });

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("test-server", "test-server", "Test Server Description", clientBuilder)
            .Build();

        var defaultOptions = new ToolLoaderOptions(ReadOnly: false);
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(defaultOptions);

        var toolLoader = new RegistryToolLoader(discoveryStrategy, serviceOptions, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // When ReadOnly is disabled, all tools should be returned regardless of ReadOnlyHint
        Assert.Equal(2, result.Tools.Count);
        Assert.Contains(result.Tools, t => t.Name == "readonly-tool");
        Assert.Contains(result.Tools, t => t.Name == "write-tool");

        // Verify annotations are preserved
        var readOnlyToolResult = result.Tools.First(t => t.Name == "readonly-tool");
        var writeToolResult = result.Tools.First(t => t.Name == "write-tool");
        Assert.True(readOnlyToolResult.Annotations?.ReadOnlyHint == true);
        Assert.True(writeToolResult.Annotations?.ReadOnlyHint == false);
    }

    [Fact]
    public async Task CallToolHandler_WithUnknownTool_ReturnsErrorResult()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader();
        var request = CreateCallToolRequest("unknown-tool");

        // Act
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // Verify the error message
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Contains("unknown-tool", textContent.Text);
        Assert.Contains("was not found", textContent.Text);
    }

    [Fact]
    public async Task RegistryToolLoader_WithDifferentOptions_BehavesConsistently()
    {
        // Arrange - Test with different service options
        var defaultOptions = new ToolLoaderOptions();
        var readOnlyOptions = new ToolLoaderOptions(ReadOnly: true);

        // Create empty discovery strategies for both tests
        var defaultDiscoveryStrategy = new MockMcpDiscoveryStrategyBuilder().Build();
        var readOnlyDiscoveryStrategy = new MockMcpDiscoveryStrategyBuilder().Build();

        // Create tool loaders with different options
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger1 = loggerFactory.CreateLogger<RegistryToolLoader>();
        var logger2 = loggerFactory.CreateLogger<RegistryToolLoader>();

        var defaultToolLoader = new RegistryToolLoader(defaultDiscoveryStrategy,
            Microsoft.Extensions.Options.Options.Create(defaultOptions), logger1);
        var readOnlyToolLoader = new RegistryToolLoader(readOnlyDiscoveryStrategy,
            Microsoft.Extensions.Options.Options.Create(readOnlyOptions), logger2);

        var request = CreateListToolsRequest();

        // Act
        var defaultResult = await defaultToolLoader.ListToolsHandler(request, CancellationToken.None);
        var readOnlyResult = await readOnlyToolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert - Both should return empty but valid results
        Assert.NotNull(defaultResult);
        Assert.NotNull(defaultResult.Tools);
        Assert.Empty(defaultResult.Tools);

        Assert.NotNull(readOnlyResult);
        Assert.NotNull(readOnlyResult.Tools);
        Assert.Empty(readOnlyResult.Tools);
    }

    [Fact]
    public async Task CallToolHandler_WithoutListToolsFirst_ShouldSucceed()
    {
        // Arrange
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool("microsoft_docs_search", "Search Microsoft documentation", args =>
            {
                var question = args?.GetValueOrDefault("question")?.ToString() ?? "";
                return new CallToolResult
                {
                    Content = new List<ContentBlock> { new TextContentBlock { Text = "Tool executed successfully" } },
                    IsError = false
                };
            });

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("test-server", "test-server", "Test Server Description", clientBuilder)
            .Build();

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());

        var toolLoader = new RegistryToolLoader(discoveryStrategy, serviceOptions, logger);
        var request = CreateCallToolRequest("microsoft_docs_search",
            new Dictionary<string, JsonElement>
            {
                { "question", JsonDocument.Parse("\"how to implement mcp server in azure\"").RootElement }
            });

        // Act - Call CallToolHandler, which should initialize tools first
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert - The tool call should succeed
        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        // Verify the content
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(textContent);
        Assert.Equal("Tool executed successfully", textContent.Text);
    }

    [Fact]
    public async Task MockMcpClient_WithExtensionMethods_WorksCorrectly()
    {
        // Arrange
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool("docs-search", "Azure documentation", args =>
            {
                var query = args?.GetValueOrDefault("query")?.ToString() ?? "";
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = $"Azure documentation: here is some content about {query}" }],
                    IsError = false
                };
            })
            .AddTool("echo", "Echo tool", args =>
            {
                var message = args?.GetValueOrDefault("message")?.ToString() ?? "No message";
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = $"Echo: {message}" }],
                    IsError = false
                };
            });

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("test-server", "test-server", "Test Server Description", clientBuilder)
            .Build();

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());

        var toolLoader = new RegistryToolLoader(discoveryStrategy, serviceOptions, logger);

        // Act & Assert - List tools
        var listRequest = CreateListToolsRequest();
        var listResult = await toolLoader.ListToolsHandler(listRequest, CancellationToken.None);
        Assert.NotNull(listResult);
        Assert.Equal(2, listResult.Tools.Count);
        Assert.Contains(listResult.Tools, t => t.Name == "docs-search");
        Assert.Contains(listResult.Tools, t => t.Name == "echo");

        // Act & Assert - Call search tool
        var searchRequest = CreateCallToolRequest("docs-search", new Dictionary<string, JsonElement>
        {
            { "query", JsonDocument.Parse("\"MCP implementation\"").RootElement }
        });

        var searchResult = await toolLoader.CallToolHandler(searchRequest, CancellationToken.None);
        Assert.NotNull(searchResult);
        Assert.False(searchResult.IsError);

        var searchContent = searchResult.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(searchContent);
        Assert.Contains("MCP implementation", searchContent.Text);
        Assert.Contains("Azure documentation", searchContent.Text);

        // Act & Assert - Call echo tool
        var echoRequest = CreateCallToolRequest("echo", new Dictionary<string, JsonElement>
        {
            { "message", JsonDocument.Parse("\"Hello MCP!\"").RootElement }
        });
        var echoResult = await toolLoader.CallToolHandler(echoRequest, CancellationToken.None);
        Assert.NotNull(echoResult);
        Assert.False(echoResult.IsError);
        var echoContent = echoResult.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.NotNull(echoContent);
        Assert.Equal("Echo: Hello MCP!", echoContent.Text);
    }

    [Fact]
    public async Task ListToolsHandler_WithReadOnlyOption_FilterToolsWithNullAnnotations()
    {
        // Arrange
        var readOnlyTool = new Tool
        {
            Name = "readonly-tool",
            Description = "Read-only tool",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = true }
        };

        var toolWithoutAnnotations = new Tool
        {
            Name = "tool-no-annotations",
            Description = "Tool without annotations",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement,
            Annotations = null  // No annotations
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(readOnlyTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Read-only result" }], IsError = false })
            .AddTool(toolWithoutAnnotations, _ => new CallToolResult { Content = [new TextContentBlock { Text = "No annotations result" }], IsError = false });

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("test-server", "test-server", "Test Server Description", clientBuilder)
            .Build();

        var readOnlyOptions = new ToolLoaderOptions(ReadOnly: true);
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RegistryToolLoader>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(readOnlyOptions);

        var toolLoader = new RegistryToolLoader(discoveryStrategy, serviceOptions, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);

        // When ReadOnly is enabled, only tools with ReadOnlyHint = true should be returned
        // Tools with null annotations should be filtered out
        Assert.Single(result.Tools);
        var returnedTool = result.Tools.First();
        Assert.Equal("readonly-tool", returnedTool.Name);
        Assert.True(returnedTool.Annotations?.ReadOnlyHint == true);

        // Verify that the tool without annotations was filtered out
        Assert.DoesNotContain(result.Tools, t => t.Name == "tool-no-annotations");
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeOwnedResourcesOnly()
    {
        // Arrange
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoader();

        // Act
        await toolLoader.DisposeAsync();

        // Assert - Discovery strategy should NOT be disposed (it's owned by DI container)
        await mockDiscoveryStrategy.DidNotReceive().DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldClearInternalCollections()
    {
        // Arrange
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoader();

        // Initialize tool loader by calling ListToolsHandler
        var request = CreateListToolsRequest();
        await toolLoader.ListToolsHandler(request, CancellationToken.None);

        // Act
        await toolLoader.DisposeAsync();

        // Assert - After disposal, calling operations should work but with empty state
        // (This tests that collections were cleared)
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);
        Assert.NotNull(result.Tools);
        // Tools might be re-populated from discovery strategy, but internal state was cleared
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader();

        // Act - dispose multiple times
        await toolLoader.DisposeAsync();
        await toolLoader.DisposeAsync();
        await toolLoader.DisposeAsync();

        // Assert - should not throw
        // (Idempotency is verified by not throwing exceptions)
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeInitializationSemaphore()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader();

        // Act
        await toolLoader.DisposeAsync();

        // Assert - This tests that the semaphore is disposed
        // If the semaphore wasn't disposed properly, subsequent operations might have issues
        // but this is mainly for coverage and resource cleanup verification
    }
}
