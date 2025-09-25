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

public class SingleProxyToolLoaderTests
{
    private static (SingleProxyToolLoader toolLoader, IMcpDiscoveryStrategy discoveryStrategy) CreateToolLoader(bool useRealDiscovery = true)
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
            var registryDiscoveryStrategy = new RegistryDiscoveryStrategy(options, registryLogger);
            var compositeLogger = serviceProvider.GetRequiredService<ILogger<CompositeDiscoveryStrategy>>();
            var compositeDiscoveryStrategy = new CompositeDiscoveryStrategy([
                commandGroupDiscoveryStrategy,
                registryDiscoveryStrategy
            ], compositeLogger);
            var toolLoader = new SingleProxyToolLoader(compositeDiscoveryStrategy, logger);
            return (toolLoader, compositeDiscoveryStrategy);
        }
        else
        {
            var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
            var toolLoader = new SingleProxyToolLoader(mockDiscoveryStrategy, logger);
            return (toolLoader, mockDiscoveryStrategy);
        }
    }

    private static ModelContextProtocol.Server.RequestContext<ListToolsRequestParams> CreateListToolsRequest()
    {
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        return new ModelContextProtocol.Server.RequestContext<ListToolsRequestParams>(mockServer)
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static ModelContextProtocol.Server.RequestContext<CallToolRequestParams> CreateCallToolRequest(
        string toolName = "azure",
        Dictionary<string, JsonElement>? arguments = null)
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
    public async Task ListToolsHandler_ReturnsAzureToolWithExpectedSchema()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var request = CreateListToolsRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

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
        mockDiscoveryStrategy.DiscoverServersAsync()
            .Returns(Task.FromResult(Enumerable.Empty<IMcpServerProvider>()));

        // Act
        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

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
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

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
            await toolLoader.CallToolHandler(request, CancellationToken.None));
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
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

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
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

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
        var mockServer = Substitute.For<ModelContextProtocol.Server.IMcpServer>();
        var request = new ModelContextProtocol.Server.RequestContext<CallToolRequestParams>(mockServer)
        {
            Params = null
        };

        // Act
        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Contains("tool\" and \"command\" parameters are required", textContent.Text);
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
        var result1 = await toolLoader.CallToolHandler(request, CancellationToken.None);
        var result2 = await toolLoader.CallToolHandler(request, CancellationToken.None);

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

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(null!, logger));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(discoveryStrategy, null!));
    }
}
