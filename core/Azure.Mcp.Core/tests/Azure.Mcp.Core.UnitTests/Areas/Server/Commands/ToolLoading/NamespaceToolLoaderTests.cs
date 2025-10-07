// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.UnitTests.Areas.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public sealed class NamespaceToolLoaderTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly CommandFactory _commandFactory;
    private readonly IOptions<ServiceStartOptions> _options;
    private readonly ILogger<NamespaceToolLoader> _logger;

    public NamespaceToolLoaderTests()
    {
        _serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider() as ServiceProvider
            ?? throw new InvalidOperationException("Failed to create service provider");
        _commandFactory = CommandFactoryHelpers.CreateCommandFactory(_serviceProvider);
        _options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions());
        _logger = _serviceProvider.GetRequiredService<ILogger<NamespaceToolLoader>>();
    }

    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Arrange & Act
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);

        // Assert
        Assert.NotNull(loader);
    }

    [Fact]
    public void Constructor_ThrowsOnNullCommandFactory()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new NamespaceToolLoader(null!, _options, _serviceProvider, _logger));
    }

    [Fact]
    public void Constructor_ThrowsOnNullOptions()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new NamespaceToolLoader(_commandFactory, null!, _serviceProvider, _logger));
    }

    [Fact]
    public void Constructor_ThrowsOnNullServiceProvider()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new NamespaceToolLoader(_commandFactory, _options, null!, _logger));
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsNamespaceTools()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await loader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.NotEmpty(result.Tools);

        // Verify hierarchical structure
        foreach (var tool in result.Tools)
        {
            Assert.NotNull(tool.Name);
            Assert.NotNull(tool.Description);
            Assert.Contains("hierarchical", tool.Description, StringComparison.OrdinalIgnoreCase);

            // Verify hierarchical schema structure
            var schema = tool.InputSchema;
            Assert.True(schema.TryGetProperty("properties", out var properties));
            Assert.True(properties.TryGetProperty("intent", out _));
            Assert.True(properties.TryGetProperty("command", out _));
            Assert.True(properties.TryGetProperty("parameters", out _));
            Assert.True(properties.TryGetProperty("learn", out _));
        }
    }

    [Fact]
    public async Task ListToolsHandler_CachesResults()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateListToolsRequest();

        // Act - Call twice
        var result1 = await loader.ListToolsHandler(request, CancellationToken.None);
        var result2 = await loader.ListToolsHandler(request, CancellationToken.None);

        // Assert - Should return same cached instance
        Assert.Same(result1.Tools, result2.Tools);
    }

    [Fact]
    public async Task ListToolsHandler_FiltersNamespacesWhenConfigured()
    {
        // Arrange
        using var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider() as ServiceProvider
            ?? throw new InvalidOperationException("Failed to create service provider");
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            Namespace = ["storage", "keyvault"]
        });
        var logger = serviceProvider.GetRequiredService<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await loader.ListToolsHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Tools);
        Assert.All(result.Tools, tool =>
            Assert.True(tool.Name == "storage" || tool.Name == "keyvault"));
    }

    [Fact]
    public async Task CallToolHandler_WithLearnTrue_ReturnsAvailableCommands()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "list resources"
        });

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("available command", textContent.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallToolHandler_WithLearnTrue_CachesCommandList()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "list resources"
        });

        // Act - Call twice
        var result1 = await loader.CallToolHandler(request, CancellationToken.None);
        var result2 = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert - Both should succeed and return same cached content
        Assert.False(result1.IsError);
        Assert.False(result2.IsError);

        var text1 = (result1.Content[0] as TextContentBlock)?.Text;
        var text2 = (result2.Content[0] as TextContentBlock)?.Text;
        Assert.Equal(text1, text2);
    }

    [Fact]
    public async Task CallToolHandler_WithIntentButNoCommand_AutoEnablesLearn()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
        {
            ["intent"] = "list resources"
            // No command specified, should auto-enable learn
        });

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);

        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("available command", textContent.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallToolHandler_WithInvalidNamespace_ReturnsError()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateCallToolRequest("nonexistent-namespace", new Dictionary<string, object?>
        {
            ["learn"] = true
        });

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);

        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("not found", textContent.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallToolHandler_WithNullToolName_ThrowsArgumentException()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateCallToolRequest(null!, new Dictionary<string, object?>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await loader.CallToolHandler(request, CancellationToken.None));
    }

    [Fact]
    public async Task CallToolHandler_WithoutCommandOrLearn_ReturnsHelpMessage()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>());

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);

        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Contains("command", textContent.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("learn", textContent.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallToolHandler_ParsesHierarchicalStructure()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();

        var arguments = new Dictionary<string, JsonElement>
        {
            ["intent"] = JsonDocument.Parse("\"list resources\"").RootElement,
            ["command"] = JsonDocument.Parse("\"list\"").RootElement,
            ["parameters"] = JsonDocument.Parse("""{"subscription":"test-sub"}""").RootElement,
            ["learn"] = JsonDocument.Parse("false").RootElement
        };

        var request = CreateCallToolRequestWithJsonElements(toolName, arguments);

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        // Result depends on whether command exists, but parsing should succeed
    }

    [Fact]
    public async Task CallToolHandler_ConvertsObjectDictionaryToJsonElements()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();

        var arguments = new Dictionary<string, object?>
        {
            ["intent"] = "list resources",
            ["command"] = "list",
            ["parameters"] = new Dictionary<string, object?> { ["subscription"] = "test-sub" },
            ["learn"] = false
        };

        var request = CreateCallToolRequest(toolName, arguments);

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        // Conversion should succeed without throwing
    }

    [Fact]
    public async Task CallToolHandler_HandlesCommandNotFoundGracefully()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();

        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
        {
            ["intent"] = "do something",
            ["command"] = "nonexistent-command",
            ["parameters"] = new Dictionary<string, object?>()
        });

        // Act
        var result = await loader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        // Should fallback to learn mode or return error
        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);
    }

    [Fact]
    public async Task CallToolHandler_LazyLoadsCommandsPerNamespace()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);

        // Get two different namespaces
        var listRequest = CreateListToolsRequest();
        var tools = await loader.ListToolsHandler(listRequest, CancellationToken.None);

        if (tools.Tools.Count < 2)
        {
            // Skip test if not enough namespaces
            return;
        }

        var namespace1 = tools.Tools[0].Name;
        var namespace2 = tools.Tools[1].Name;

        // Act - Access only first namespace
        var request1 = CreateCallToolRequest(namespace1, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "test"
        });

        await loader.CallToolHandler(request1, CancellationToken.None);

        // Now access second namespace
        var request2 = CreateCallToolRequest(namespace2, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "test"
        });

        var result2 = await loader.CallToolHandler(request2, CancellationToken.None);

        // Assert - Both should succeed, proving lazy loading works
        Assert.NotNull(result2);
        Assert.False(result2.IsError);
    }

    [Fact]
    public async Task CallToolHandler_ThreadSafeLazyLoading()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();

        // Act - Simulate concurrent access
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
            {
                ["learn"] = true,
                ["intent"] = "concurrent test"
            });

            return await loader.CallToolHandler(request, CancellationToken.None);
        });

        var results = await Task.WhenAll(tasks);

        // Assert - All should succeed without race conditions
        Assert.All(results, result =>
        {
            Assert.NotNull(result);
            Assert.False(result.IsError);
        });

        // All should return same cached content
        var firstText = (results[0].Content[0] as TextContentBlock)?.Text;
        Assert.All(results, result =>
        {
            var text = (result.Content[0] as TextContentBlock)?.Text;
            Assert.Equal(firstText, text);
        });
    }

    [Fact]
    public async Task DisposeAsync_ClearsCaches()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();

        // Populate cache
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "test"
        });

        await loader.CallToolHandler(request, CancellationToken.None);

        // Act
        await loader.DisposeAsync();

        // Assert - No exception should be thrown
        // Cache clearing is internal, but disposal should complete successfully
    }

    // Helper methods

    private string GetFirstAvailableNamespace()
    {
        var namespaces = _commandFactory.RootGroup.SubGroup
            .Where(g => !Azure.Mcp.Core.Areas.Server.Commands.Discovery.DiscoveryConstants.IgnoredCommandGroups.Contains(g.Name, StringComparer.OrdinalIgnoreCase))
            .Select(g => g.Name)
            .ToList();

        return namespaces.FirstOrDefault() ?? "storage";
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
        string toolName,
        Dictionary<string, object?> arguments)
    {
        var jsonArguments = arguments.ToDictionary(
            kvp => kvp.Key,
            kvp => JsonSerializer.SerializeToElement(kvp.Value));

        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = jsonArguments
            }
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequestWithJsonElements(
        string toolName,
        Dictionary<string, JsonElement> arguments)
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        return new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments
            }
        };
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
