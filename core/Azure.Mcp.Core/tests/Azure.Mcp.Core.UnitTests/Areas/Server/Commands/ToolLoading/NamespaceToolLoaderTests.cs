// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading;

public sealed class NamespaceToolLoaderTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ICommandFactory _commandFactory;
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
        var result = await loader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var result1 = await loader.ListToolsHandler(request, TestContext.Current.CancellationToken);
        var result2 = await loader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.ListToolsHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var result1 = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);
        var result2 = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
            await loader.CallToolHandler(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CallToolHandler_WithoutCommandOrLearn_ReturnsHelpMessage()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>());

        // Act
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
        var tools = await loader.ListToolsHandler(listRequest, TestContext.Current.CancellationToken);

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

        await loader.CallToolHandler(request1, TestContext.Current.CancellationToken);

        // Now access second namespace
        var request2 = CreateCallToolRequest(namespace2, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "test"
        });

        var result2 = await loader.CallToolHandler(request2, TestContext.Current.CancellationToken);

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

            return await loader.CallToolHandler(request, TestContext.Current.CancellationToken);
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

        await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Act
        await loader.DisposeAsync();

        // Assert - No exception should be thrown
        // Cache clearing is internal, but disposal should complete successfully
    }

    [Fact]
    public async Task CallToolHandler_WithInvalidCommand_ReturnsErrorWithGuidance()
    {
        // Arrange - Test error handling and guidance message structure
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var toolName = GetFirstAvailableNamespace();

        // Create request with invalid command that doesn't exist
        var request = CreateCallToolRequest(toolName, new Dictionary<string, object?>
        {
            ["command"] = "nonexistent_invalid_command_xyz",
            ["parameters"] = new Dictionary<string, object?>()
        });

        // Act
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should provide helpful error guidance
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        Assert.NotEmpty(result.Content);

        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);

        // When command doesn't exist or encounters issues, should provide guidance
        // This validates the error handling path preserves informative messages
        Assert.True(textContent.Text.Length > 0);
    }

    // Elicitation Handler Tests (ported from BaseToolLoaderTests)

    [Fact]
    public async Task CallToolHandler_LearnResponse_IncludesOutputSchemaForOptedInCommands()
    {
        // Arrange - Use the "storage" namespace which has commands with ResultTypeInfo
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "list storage accounts"
        });

        // Act
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsError);

        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);

        // The learn response serializes child tools as JSON, which should include outputSchema
        Assert.Contains("outputSchema", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_LearnResponse_OutputSchemaHasCorrectStructure()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "get storage accounts"
        });

        // Act
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);

        // Extract the JSON array of tools from the learn response text
        var toolsJsonStart = textContent.Text.IndexOf('[');
        var toolsJsonEnd = textContent.Text.LastIndexOf(']');
        Assert.True(toolsJsonStart >= 0 && toolsJsonEnd > toolsJsonStart,
            "Learn response should contain a JSON array of tools");

        var toolsJson = textContent.Text.Substring(toolsJsonStart, toolsJsonEnd - toolsJsonStart + 1);
        using var doc = JsonDocument.Parse(toolsJson);
        var tools = doc.RootElement;
        Assert.Equal(JsonValueKind.Array, tools.ValueKind);

        // Find a tool with outputSchema (e.g., storage account_get)
        var toolsWithOutputSchema = tools.EnumerateArray()
            .Where(t => t.TryGetProperty("outputSchema", out _))
            .ToList();

        Assert.True(toolsWithOutputSchema.Count > 0,
            "At least one storage command should have outputSchema in the learn response");

        // Verify the outputSchema structure
        foreach (var tool in toolsWithOutputSchema)
        {
            var outputSchema = tool.GetProperty("outputSchema");
            Assert.Equal("object", outputSchema.GetProperty("type").GetString());
            Assert.True(outputSchema.TryGetProperty("properties", out var props));
            Assert.True(props.EnumerateObject().Any(),
                $"Tool '{tool.GetProperty("name").GetString()}' outputSchema should have at least one property");
        }
    }

    [Fact]
    public async Task CallToolHandler_LearnResponse_NoOutputSchemaForCommandsWithoutResultTypeInfo()
    {
        // Arrange - Use a namespace that should have commands without ResultTypeInfo
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);

        // Find a namespace that has commands without ResultTypeInfo
        var namespaces = _commandFactory.RootGroup.SubGroup
            .Where(g => !DiscoveryConstants.IgnoredCommandGroups.Contains(g.Name, StringComparer.OrdinalIgnoreCase))
            .Select(g => g.Name)
            .ToList();

        // Use a namespace other than storage (which has been opted in)
        var testNamespace = namespaces.FirstOrDefault(n => !string.Equals(n, "storage", StringComparison.OrdinalIgnoreCase))
            ?? namespaces.First();

        var request = CreateCallToolRequest(testNamespace, new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "explore commands"
        });

        // Act
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        var textContent = result.Content[0] as TextContentBlock;
        Assert.NotNull(textContent);

        // Extract JSON tools array
        var toolsJsonStart = textContent.Text.IndexOf('[');
        var toolsJsonEnd = textContent.Text.LastIndexOf(']');
        if (toolsJsonStart >= 0 && toolsJsonEnd > toolsJsonStart)
        {
            var toolsJson = textContent.Text.Substring(toolsJsonStart, toolsJsonEnd - toolsJsonStart + 1);
            using var doc = JsonDocument.Parse(toolsJson);

            // Verify commands in this namespace that haven't opted in don't have outputSchema
            var commands = _commandFactory.GroupCommands([testNamespace]);
            var commandsWithoutResultType = commands
                .Where(kvp => kvp.Value.ResultTypeInfo == null)
                .Select(kvp => kvp.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var tool in doc.RootElement.EnumerateArray())
            {
                var toolName = tool.GetProperty("name").GetString()!;
                if (commandsWithoutResultType.Contains(toolName))
                {
                    Assert.False(tool.TryGetProperty("outputSchema", out _),
                        $"Tool '{toolName}' should NOT have outputSchema since its command does not provide ResultTypeInfo");
                }
            }
        }
    }

    [Fact]
    public async Task CallToolHandler_ChildToolSpec_IncludesOutputSchemaInErrorGuidance()
    {
        // Arrange - Call a storage command with missing required params to trigger the error guidance path
        // which includes the child tool spec JSON
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["command"] = "account_get",
            ["parameters"] = new Dictionary<string, object?>()  // Missing required params
        });

        // Act
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // Whether success or error, if it hits the "missing required" path, the
        // Command Spec embedded in response should contain outputSchema
        var allText = string.Join("\n", result.Content
            .OfType<TextContentBlock>()
            .Select(c => c.Text));

        if (allText.Contains("Command Spec", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Contains("outputSchema", allText);
        }
    }

    [Fact]
    public async Task CallToolHandler_LearnResponse_CachesToolListWithOutputSchema()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);

        // Act - Call learn twice for the same namespace
        var request1 = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "list accounts"
        });
        var result1 = await loader.CallToolHandler(request1, TestContext.Current.CancellationToken);

        var request2 = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "list blobs"
        });
        var result2 = await loader.CallToolHandler(request2, TestContext.Current.CancellationToken);

        // Assert - Both should contain outputSchema (cached tool list is reused)
        var text1 = (result1.Content[0] as TextContentBlock)?.Text;
        var text2 = (result2.Content[0] as TextContentBlock)?.Text;
        Assert.NotNull(text1);
        Assert.NotNull(text2);
        Assert.Contains("outputSchema", text1);
        Assert.Contains("outputSchema", text2);
    }

    [Fact]
    public void CreateClientOptions_WithElicitationCapability_ReturnsOptionsWithElicitationHandler()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = CallCreateClientOptions(loader, mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.NotNull(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public void CreateClientOptions_WithNoElicitationCapability_ReturnsOptionsWithoutElicitationHandler()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        mockServer.ClientCapabilities.Returns(new ClientCapabilities());

        // Act
        var options = CallCreateClientOptions(loader, mockServer);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Handlers);
        Assert.Null(options.Handlers.ElicitationHandler);
    }

    [Fact]
    public async Task CreateClientOptions_ElicitationHandler_DelegatesToServerSendRequestAsync()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
            {
                Form = new(),
            }
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        var elicitationRequest = new ElicitRequestParams
        {
            Message = "Please enter your password:",
            RequestedSchema = new()
            {
                Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>()
                {
                    ["password"] = new ElicitRequestParams.StringSchema
                    {
                        Title = "password",
                        Description = "The user's password.",
                    }
                },
                Required = ["password"],
            }
        };

        var mockResponse = new JsonRpcResponse
        {
            Id = new RequestId(1),
            Result = JsonSerializer.SerializeToNode(new ElicitResult { Action = "accept" })
        };

        mockServer.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(mockResponse));

        // Act
        var options = CallCreateClientOptions(loader, mockServer);
        Assert.NotNull(options.Handlers.ElicitationHandler);

        await options.Handlers.ElicitationHandler(elicitationRequest, TestContext.Current.CancellationToken);

        // Assert - verify SendRequestAsync was called with elicitation method
        await mockServer.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(req => req.Method == "elicitation/create"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateClientOptions_ElicitationHandler_ValidatesRequestAndThrowsOnNull()
    {
        // Arrange
        var loader = new NamespaceToolLoader(_commandFactory, _options, _serviceProvider, _logger);
        var mockServer = Substitute.For<ModelContextProtocol.Server.McpServer>();
        var capabilities = new ClientCapabilities
        {
            Elicitation = new ElicitationCapability()
        };
        mockServer.ClientCapabilities.Returns(capabilities);

        // Act
        var options = CallCreateClientOptions(loader, mockServer);
        Assert.NotNull(options.Handlers.ElicitationHandler);

        // Assert - verify handler validates null request
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await options.Handlers.ElicitationHandler.Invoke(null!, TestContext.Current.CancellationToken));
    }

    // Helper methods

    private string GetFirstAvailableNamespace()
    {
        var namespaces = _commandFactory.RootGroup.SubGroup
            .Where(g => !DiscoveryConstants.IgnoredCommandGroups.Contains(g.Name, StringComparer.OrdinalIgnoreCase))
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

    private static ModelContextProtocol.Client.McpClientOptions CallCreateClientOptions(
        NamespaceToolLoader loader,
        ModelContextProtocol.Server.McpServer server)
    {
        // Use reflection to call the protected CreateClientOptions method
        var method = typeof(BaseToolLoader).GetMethod(
            "CreateClientOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method == null)
        {
            throw new InvalidOperationException("CreateClientOptions method not found on BaseToolLoader");
        }

        var result = method.Invoke(loader, [server]);
        return (ModelContextProtocol.Client.McpClientOptions)result!;
    }
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
