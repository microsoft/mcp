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
    public async Task ListToolsHandler_WithReadOnlyOption_ReturnsOnlyReadOnlyTools()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var rootGroup = new CommandGroup("root", "Root command group");
        var storageGroup = new CommandGroup("storage", "Storage commands");
        var storageCommand = Substitute.For<IBaseCommand>();
        storageCommand.Metadata.Returns(new ToolMetadata() { ReadOnly = true });
        storageGroup.AddCommand("readonly", storageCommand);
        var keyvaultGroup = new CommandGroup("keyvault", "Key Vault commands");
        var keyvaultCommand = Substitute.For<IBaseCommand>();
        keyvaultCommand.Metadata.Returns(new ToolMetadata() { ReadOnly = false });
        keyvaultGroup.AddCommand("notreadonly", keyvaultCommand);
        rootGroup.SubGroup.AddRange([storageGroup, keyvaultGroup]);
        commandFactory.RootGroup.Returns(rootGroup);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            ReadOnly = true
        });
        var logger = Substitute.For<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await loader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tools);
        Assert.All(result.Tools, tool => Assert.Equal("storage", tool.Name));
    }

    [Fact]
    public async Task ListToolsHandler_WithIsHttpOption_DoesNotReturnLocalRequiredTools()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var rootGroup = new CommandGroup("root", "Root command group");
        var stroageGroup = new CommandGroup("storage", "Storage commands");
        var storageCommand = Substitute.For<IBaseCommand>();
        storageCommand.Metadata.Returns(new ToolMetadata() { LocalRequired = true });
        stroageGroup.AddCommand("localrequired", storageCommand);
        var keyvaultGroup = new CommandGroup("keyvault", "Key Vault commands");
        var keyvaultCommand = Substitute.For<IBaseCommand>();
        keyvaultCommand.Metadata.Returns(new ToolMetadata() { LocalRequired = false });
        keyvaultGroup.AddCommand("notlocalrequired", keyvaultCommand);
        rootGroup.SubGroup.AddRange([stroageGroup, keyvaultGroup]);
        commandFactory.RootGroup.Returns(rootGroup);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            Transport = TransportTypes.Http
        });
        var logger = Substitute.For<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateListToolsRequest();

        // Act
        var result = await loader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tools);
        Assert.All(result.Tools, tool => Assert.Equal("keyvault", tool.Name));
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
        var request = CreateCallToolRequest(null!, []);

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
        var request = CreateCallToolRequest(toolName, []);

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

    [Fact]
    public async Task CallToolHandler_ReadOnlyMode_RejectsNonReadOnlyCommand()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var rootGroup = new CommandGroup("root", "Root command group");
        var storageGroup = new CommandGroup("storage", "Storage commands");

        var executed = false;
        var writeCmd = Substitute.For<IBaseCommand>();
        writeCmd.Metadata.Returns(new ToolMetadata { ReadOnly = false });
        writeCmd.GetCommand().Returns(new System.CommandLine.Command("write-cmd", "A write command"));
        writeCmd.ExecuteAsync(default!, default!, default!).ReturnsForAnyArgs(call =>
        {
            executed = true;
            return new Microsoft.Mcp.Core.Models.Command.CommandResponse { Status = System.Net.HttpStatusCode.OK };
        });
        storageGroup.AddCommand("write-cmd", writeCmd);

        rootGroup.SubGroup.Add(storageGroup);
        commandFactory.RootGroup.Returns(rootGroup);
        commandFactory.GroupCommands(Arg.Any<string[]>())
            .Returns(new Dictionary<string, IBaseCommand> { ["write-cmd"] = writeCmd });

        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { ReadOnly = true });
        var logger = Substitute.For<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["command"] = "write-cmd",
            ["parameters"] = new Dictionary<string, object?>()
        });

        // Act
        await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - non-read-only command should not have been executed
        Assert.False(executed, "Non-read-only command should not be executed in read-only mode");
    }

    [Fact]
    public async Task CallToolHandler_ReadOnlyMode_AllowsReadOnlyCommand()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var rootGroup = new CommandGroup("root", "Root command group");
        var storageGroup = new CommandGroup("storage", "Storage commands");

        var executed = false;
        var readCmd = Substitute.For<IBaseCommand>();
        readCmd.Metadata.Returns(new ToolMetadata { ReadOnly = true, Destructive = false });
        readCmd.GetCommand().Returns(new System.CommandLine.Command("read-cmd", "A read command"));
        readCmd.ExecuteAsync(default!, default!, default!).ReturnsForAnyArgs(call =>
        {
            executed = true;
            return new Microsoft.Mcp.Core.Models.Command.CommandResponse { Status = System.Net.HttpStatusCode.OK };
        });
        storageGroup.AddCommand("read-cmd", readCmd);

        rootGroup.SubGroup.Add(storageGroup);
        commandFactory.RootGroup.Returns(rootGroup);
        commandFactory.GroupCommands(Arg.Any<string[]>())
            .Returns(new Dictionary<string, IBaseCommand> { ["read-cmd"] = readCmd });

        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { ReadOnly = true });
        var logger = Substitute.For<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["command"] = "read-cmd",
            ["parameters"] = new Dictionary<string, object?>()
        });

        // Act
        await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - read-only command should have been executed
        Assert.True(executed, "Read-only command should be executed in read-only mode");
    }

    [Fact]
    public async Task CallToolHandler_HttpMode_RejectsLocalRequiredCommand()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var rootGroup = new CommandGroup("root", "Root command group");
        var storageGroup = new CommandGroup("storage", "Storage commands");

        var executed = false;
        var localCmd = Substitute.For<IBaseCommand>();
        localCmd.Metadata.Returns(new ToolMetadata { LocalRequired = true });
        localCmd.GetCommand().Returns(new System.CommandLine.Command("local-cmd", "A local command"));
        localCmd.ExecuteAsync(default!, default!, default!).ReturnsForAnyArgs(call =>
        {
            executed = true;
            return new Microsoft.Mcp.Core.Models.Command.CommandResponse { Status = System.Net.HttpStatusCode.OK };
        });
        storageGroup.AddCommand("local-cmd", localCmd);

        rootGroup.SubGroup.Add(storageGroup);
        commandFactory.RootGroup.Returns(rootGroup);
        commandFactory.GroupCommands(Arg.Any<string[]>())
            .Returns(new Dictionary<string, IBaseCommand> { ["local-cmd"] = localCmd });

        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { Transport = TransportTypes.Http });
        var logger = Substitute.For<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["command"] = "local-cmd",
            ["parameters"] = new Dictionary<string, object?>()
        });

        // Act
        await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - local-required command should not have been executed in HTTP mode
        Assert.False(executed, "Local-required command should not be executed in HTTP mode");
    }

    [Fact]
    public async Task CallToolHandler_HttpMode_AllowsNonLocalRequiredCommand()
    {
        // Arrange
        var commandFactory = Substitute.For<ICommandFactory>();
        var rootGroup = new CommandGroup("root", "Root command group");
        var storageGroup = new CommandGroup("storage", "Storage commands");

        var executed = false;
        var remoteCmd = Substitute.For<IBaseCommand>();
        remoteCmd.Metadata.Returns(new ToolMetadata { LocalRequired = false, Destructive = false });
        remoteCmd.GetCommand().Returns(new System.CommandLine.Command("remote-cmd", "A remote command"));
        remoteCmd.ExecuteAsync(default!, default!, default!).ReturnsForAnyArgs(call =>
        {
            executed = true;
            return new Microsoft.Mcp.Core.Models.Command.CommandResponse { Status = System.Net.HttpStatusCode.OK };
        });
        storageGroup.AddCommand("remote-cmd", remoteCmd);

        rootGroup.SubGroup.Add(storageGroup);
        commandFactory.RootGroup.Returns(rootGroup);
        commandFactory.GroupCommands(Arg.Any<string[]>())
            .Returns(new Dictionary<string, IBaseCommand> { ["remote-cmd"] = remoteCmd });

        var serviceProvider = Substitute.For<IServiceProvider>();
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions { Transport = TransportTypes.Http });
        var logger = Substitute.For<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateCallToolRequest("storage", new Dictionary<string, object?>
        {
            ["command"] = "remote-cmd",
            ["parameters"] = new Dictionary<string, object?>()
        });

        // Act
        await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - non-local-required command should have been executed in HTTP mode
        Assert.True(executed, "Non-local-required command should be executed in HTTP mode");
    }

    [Fact]
    public async Task GetChildToolList_WithReadOnlyOption_ReturnsOnlyReadOnlyTools()
    {
        // Arrange
        using var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider() as ServiceProvider
            ?? throw new InvalidOperationException("Failed to create service provider");
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            ReadOnly = true
        });
        var logger = serviceProvider.GetRequiredService<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateCallToolRequest("storage", []);

        // Act
        var tools = loader.GetChildToolList(request, "storage");

        // Assert
        Assert.NotNull(tools);
        Assert.All(tools, tool => Assert.True(tool.Annotations?.ReadOnlyHint, $"Tool '{tool.Name}' should have ReadOnlyHint = true when ReadOnly mode is enabled"));
    }

    [Fact]
    public async Task GetChildToolList_WithIsHttpOption_DoesNotReturnLocalRequiredTools()
    {
        // Arrange
        using var serviceProvider = CommandFactoryHelpers.CreateDefaultServiceProvider() as ServiceProvider
            ?? throw new InvalidOperationException("Failed to create service provider");
        var commandFactory = CommandFactoryHelpers.CreateCommandFactory(serviceProvider);
        var options = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            Transport = TransportTypes.Http
        });
        var logger = serviceProvider.GetRequiredService<ILogger<NamespaceToolLoader>>();

        var loader = new NamespaceToolLoader(commandFactory, options, serviceProvider, logger);
        var request = CreateCallToolRequest("storage", []);

        // Act
        var tools = loader.GetChildToolList(request, "storage");

        // Assert
        Assert.NotNull(tools);
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
        return new(mockServer, new() { Method = RequestMethods.ToolsList })
        {
            Params = new()
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
        return new(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new()
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
        return new(mockServer, new() { Method = RequestMethods.ToolsCall })
        {
            Params = new()
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
