// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public class CompositeToolLoaderTests
{
    private static IServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection().AddLogging().BuildServiceProvider();
    }

    private static RequestContext<ListToolsRequestParams> CreateListToolsRequest()
    {
        var mockServer = Substitute.For<McpServer>();
        return new RequestContext<ListToolsRequestParams>(mockServer, new() { Method = RequestMethods.ToolsList })
        {
            Params = new ListToolsRequestParams()
        };
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequest(string toolName, IReadOnlyDictionary<string, JsonElement>? arguments = null)
    {
        var mockServer = Substitute.For<McpServer>();
        return new RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new CallToolRequestParams
            {
                Name = toolName,
                Arguments = arguments ?? new Dictionary<string, JsonElement>()
            }
        };
    }

    private static Tool CreateTestTool(string name, string description = "Test tool")
    {
        return new Tool
        {
            Name = name,
            Description = description,
            InputSchema = JsonDocument.Parse("""
                {
                    "type": "object",
                    "properties": {}
                }
                """).RootElement
        };
    }

    [Fact]
    public void ListToolsHandler_WithEmptyToolLoaderList_ThrowsArgumentException()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();
        var toolLoaders = new List<IToolLoader>();

        var exception = Assert.Throws<ArgumentException>(() =>
            new CompositeToolLoader(toolLoaders, logger));

        Assert.Equal("toolLoaders", exception.ParamName);
        Assert.Contains("At least one tool loader must be provided", exception.Message);
    }

    [Fact]
    public async Task ListToolsHandler_WithSingleToolLoader_ReturnsToolsFromLoader()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader = Substitute.For<IToolLoader>();
        var expectedTools = new List<Tool>
        {
            CreateTestTool("tool1", "First tool"),
            CreateTestTool("tool2", "Second tool")
        };
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = expectedTools });

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var request = CreateListToolsRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Equal(2, result.Tools.Count);
        Assert.Equal("tool1", result.Tools[0].Name);
        Assert.Equal("tool2", result.Tools[1].Name);
        Assert.Equal("First tool", result.Tools[0].Description);
        Assert.Equal("Second tool", result.Tools[1].Description);
    }

    [Fact]
    public async Task ListToolsHandler_WithMultipleToolLoaders_CombinesAllTools()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader1 = Substitute.For<IToolLoader>();
        var mockLoader2 = Substitute.For<IToolLoader>();

        mockLoader1.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { CreateTestTool("tool1") } });

        mockLoader2.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { CreateTestTool("tool2"), CreateTestTool("tool3") } });

        var toolLoaders = new List<IToolLoader> { mockLoader1, mockLoader2 };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var request = CreateListToolsRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Equal(3, result.Tools.Count);
        Assert.Contains(result.Tools, t => t.Name == "tool1");
        Assert.Contains(result.Tools, t => t.Name == "tool2");
        Assert.Contains(result.Tools, t => t.Name == "tool3");
    }

    [Fact]
    public async Task ListToolsHandler_WithToolLoaderReturningNull_ReturnsEmptyResult()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader = Substitute.For<IToolLoader>();
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns((ListToolsResult)null!);

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var request = CreateListToolsRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Tools);
    }

    [Fact]
    public void CallToolHandler_WithEmptyToolLoaderList_ThrowsArgumentException()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();
        var toolLoaders = new List<IToolLoader>();

        var exception = Assert.Throws<ArgumentException>(() =>
            new CompositeToolLoader(toolLoaders, logger));

        Assert.Equal("toolLoaders", exception.ParamName);
        Assert.Contains("At least one tool loader must be provided", exception.Message);
    }

    [Fact]
    public async Task CallToolHandler_WithUnknownTool_ReturnsErrorResult()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        // Setup a loader with a different tool to populate the map
        var mockLoader = Substitute.For<IToolLoader>();
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { CreateTestTool("existing-tool") } });

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);

        // First populate the tool map by calling ListToolsHandler
        var listRequest = CreateListToolsRequest();
        await toolLoader.ListToolsHandler(listRequest, CancellationToken.None);

        // Now try to call an unknown tool
        var callRequest = CreateCallToolRequest("unknown-tool");
        var result = await toolLoader.CallToolHandler(callRequest, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.NotNull(callRequest.Params);
        Assert.Equal($"The tool {callRequest.Params.Name} was not found", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithKnownTool_DelegatesToCorrectLoader()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader = Substitute.For<IToolLoader>();
        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock> { new TextContentBlock { Text = "Tool executed successfully" } },
            IsError = false
        };

        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { CreateTestTool("test-tool") } });

        mockLoader.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);

        // First populate the tool map
        var listRequest = CreateListToolsRequest();
        await toolLoader.ListToolsHandler(listRequest, CancellationToken.None);

        // Now call the known tool
        var callRequest = CreateCallToolRequest("test-tool");
        var result = await toolLoader.CallToolHandler(callRequest, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.Equal(expectedResult.Content, result.Content);

        // Verify the mock loader was called with the correct request
        await mockLoader.Received(1).CallToolHandler(callRequest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithMultipleLoaders_DelegatesToCorrectLoader()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader1 = Substitute.For<IToolLoader>();
        var mockLoader2 = Substitute.For<IToolLoader>();

        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock> { new TextContentBlock { Text = "Tool2 executed" } },
            IsError = false
        };

        mockLoader1.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { CreateTestTool("tool1") } });

        mockLoader2.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { CreateTestTool("tool2") } });

        mockLoader2.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var toolLoaders = new List<IToolLoader> { mockLoader1, mockLoader2 };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);

        // First populate the tool map
        var listRequest = CreateListToolsRequest();
        await toolLoader.ListToolsHandler(listRequest, CancellationToken.None);

        // Call tool2 which should be handled by mockLoader2
        var callRequest = CreateCallToolRequest("tool2");
        var result = await toolLoader.CallToolHandler(callRequest, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.Equal(expectedResult.Content, result.Content);

        // Verify only mockLoader2 was called for CallToolHandler
        await mockLoader1.DidNotReceive().CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>());
        await mockLoader2.Received(1).CallToolHandler(callRequest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsErrorResult()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        var toolLoaders = new List<IToolLoader> { mockToolLoader };

        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var mockServer = Substitute.For<McpServer>();
        var request = new RequestContext<CallToolRequestParams>(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = null
        };

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.Equal("Cannot call tools with null parameters.", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithToolLoaderReturningNull_ReturnsErrorResult()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader = Substitute.For<IToolLoader>();
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns((ListToolsResult)null!);

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var request = CreateCallToolRequest("test-tool");

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.Contains("Failed to initialize tool loaders", textContent.Text);
    }

    [Fact]
    public async Task ListToolsHandler_WithSingleEmptyToolLoader_ReturnsEmptyResult()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();
        var mockToolLoader = Substitute.For<IToolLoader>();
        mockToolLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool>() });

        var toolLoaders = new List<IToolLoader> { mockToolLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var request = CreateListToolsRequest();

        var result = await toolLoader.ListToolsHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Tools);
        Assert.Empty(result.Tools);
    }

    [Fact]
    public async Task CallToolHandler_WithoutListingToolsFirst_LazilyInitializesAndCallsTool()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        // Setup a loader that has the tool we want to call
        var mockLoader = Substitute.For<IToolLoader>();
        var testTool = CreateTestTool("test-tool");
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { testTool } });

        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock> { new TextContentBlock { Text = "Tool executed successfully" } },
            IsError = false
        };
        mockLoader.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);

        // Call tool directly WITHOUT first calling ListToolsHandler
        var callRequest = CreateCallToolRequest("test-tool");
        var result = await toolLoader.CallToolHandler(callRequest, CancellationToken.None);

        // Verify the tool was found and executed successfully
        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.Equal("Tool executed successfully", textContent.Text);

        // Verify that ListToolsHandler was called internally to populate the map
        await mockLoader.Received(1).ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>());

        // Verify that CallToolHandler was called on the loader
        await mockLoader.Received(1).CallToolHandler(callRequest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithoutListingToolsFirst_ReturnsErrorForUnknownTool()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        // Setup a loader that has a different tool
        var mockLoader = Substitute.For<IToolLoader>();
        var testTool = CreateTestTool("different-tool");
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { testTool } });

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);

        // Call tool directly WITHOUT first calling ListToolsHandler
        var callRequest = CreateCallToolRequest("unknown-tool");
        var result = await toolLoader.CallToolHandler(callRequest, CancellationToken.None);

        // Verify the tool was not found
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.Equal("The tool unknown-tool was not found", textContent.Text);

        // Verify that ListToolsHandler was called internally to populate the map
        await mockLoader.Received(1).ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>());

        // Verify that CallToolHandler was NOT called since the tool was not found
        await mockLoader.DidNotReceive().CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_ConcurrentCallsWithoutListingFirst_InitializesOnlyOnce()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        // Setup a loader that has the tool we want to call
        var mockLoader = Substitute.For<IToolLoader>();
        var testTool = CreateTestTool("test-tool");
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { testTool } });

        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock> { new TextContentBlock { Text = "Tool executed successfully" } },
            IsError = false
        };
        mockLoader.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);

        // Make multiple concurrent calls to the same tool
        var tasks = new List<Task<CallToolResult>>();
        const int concurrentCalls = 5;

        for (int i = 0; i < concurrentCalls; i++)
        {
            var callRequest = CreateCallToolRequest("test-tool");
            tasks.Add(toolLoader.CallToolHandler(callRequest, CancellationToken.None).AsTask());
        }

        var results = await Task.WhenAll(tasks);

        // Verify all calls succeeded
        foreach (var result in results)
        {
            Assert.NotNull(result);
            Assert.False(result.IsError);
            Assert.NotNull(result.Content);
            Assert.Single(result.Content);
            var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
            Assert.Equal("Tool executed successfully", textContent.Text);
        }

        // Verify that ListToolsHandler was called exactly once (not once per concurrent call)
        await mockLoader.Received(1).ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>());

        // Verify that CallToolHandler was called for each concurrent request
        await mockLoader.Received(concurrentCalls).CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_ValidToolWithoutPriorListingCall_ExecutesSuccessfully()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader = Substitute.For<IToolLoader>();
        var expectedTool = CreateTestTool("valid-tool", "A valid test tool");
        var expectedResult = new CallToolResult
        {
            Content = new List<ContentBlock> { new TextContentBlock { Text = "Successfully executed valid-tool" } },
            IsError = false
        };

        // Setup the mock loader to return the tool when ListToolsHandler is called
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(new ListToolsResult { Tools = new List<Tool> { expectedTool } });

        // Setup the mock loader to return a successful result when CallToolHandler is called
        mockLoader.CallToolHandler(Arg.Any<RequestContext<CallToolRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var compositeToolLoader = new CompositeToolLoader(toolLoaders, logger);

        // Act - Call the tool directly without first calling ListToolsHandler
        var callRequest = CreateCallToolRequest("valid-tool");
        var result = await compositeToolLoader.CallToolHandler(callRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);

        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.Equal("Successfully executed valid-tool", textContent.Text);

        // Verify that the composite loader internally called ListToolsHandler to initialize the tool map
        await mockLoader.Received(1).ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>());

        // Verify that the composite loader called CallToolHandler on the correct loader
        await mockLoader.Received(1).CallToolHandler(callRequest, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithToolLoaderThrowingException_ReturnsErrorResult()
    {
        var serviceProvider = CreateServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var mockLoader = Substitute.For<IToolLoader>();
        mockLoader.ListToolsHandler(Arg.Any<RequestContext<ListToolsRequestParams>>(), Arg.Any<CancellationToken>())
            .Returns<ListToolsResult>(callInfo => throw new InvalidOperationException("Loader initialization failed"));

        var toolLoaders = new List<IToolLoader> { mockLoader };
        var toolLoader = new CompositeToolLoader(toolLoaders, logger);
        var request = CreateCallToolRequest("test-tool");

        var result = await toolLoader.CallToolHandler(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        var textContent = Assert.IsType<TextContentBlock>(result.Content[0]);
        Assert.Contains("Failed to initialize tool loaders", textContent.Text);
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeAllChildToolLoaders()
    {
        // Arrange
        var mockLoader1 = Substitute.For<IToolLoader>();
        var mockLoader2 = Substitute.For<IToolLoader>();
        var mockLoader3 = Substitute.For<IToolLoader>();

        var toolLoaders = new[] { mockLoader1, mockLoader2, mockLoader3 };
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var compositeLoader = new CompositeToolLoader(toolLoaders, logger);

        // Act
        await compositeLoader.DisposeAsync();

        // Assert - All child loaders should be disposed
        await mockLoader1.Received(1).DisposeAsync();
        await mockLoader2.Received(1).DisposeAsync();
        await mockLoader3.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeInitializationSemaphore()
    {
        // Arrange
        var mockLoader = Substitute.For<IToolLoader>();
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var compositeLoader = new CompositeToolLoader(new[] { mockLoader }, logger);

        // Act
        await compositeLoader.DisposeAsync();

        // Assert - Should dispose without throwing (semaphore disposal is internal)
        await mockLoader.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldHandleChildLoaderDisposalExceptions()
    {
        // Arrange
        var mockLoader1 = Substitute.For<IToolLoader>();
        var mockLoader2 = Substitute.For<IToolLoader>();

        mockLoader1.DisposeAsync().Returns(ValueTask.FromException(new InvalidOperationException("Loader 1 failed")));
        mockLoader2.DisposeAsync().Returns(ValueTask.CompletedTask);

        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var compositeLoader = new CompositeToolLoader(new[] { mockLoader1, mockLoader2 }, logger);

        // Act - Should complete without throwing (best-effort disposal)
        await compositeLoader.DisposeAsync();

        // Both loaders should have been attempted to dispose
        await mockLoader1.Received(1).DisposeAsync();
        await mockLoader2.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_ShouldBeIdempotent()
    {
        // Arrange
        var mockLoader = Substitute.For<IToolLoader>();
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var compositeLoader = new CompositeToolLoader(new[] { mockLoader }, logger);

        // Act - dispose multiple times
        await compositeLoader.DisposeAsync();
        await compositeLoader.DisposeAsync();
        await compositeLoader.DisposeAsync();

        // Assert - child loader should be disposed only once due to idempotent disposal
        await mockLoader.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithSingleChildLoader_ShouldDisposeChild()
    {
        // Arrange
        var mockLoader = Substitute.For<IToolLoader>();
        var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CompositeToolLoader>>();

        var compositeLoader = new CompositeToolLoader(new[] { mockLoader }, logger);

        // Act
        await compositeLoader.DisposeAsync();

        // Assert - should dispose the single child loader
        await mockLoader.Received(1).DisposeAsync();
    }
}
