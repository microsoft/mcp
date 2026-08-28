// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Tests.Areas.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public class SingleProxyToolLoaderTests
{
    private static Microsoft.Extensions.Options.IOptions<McpServerConfiguration> CreateServerConfigurationOptions()
    {
        return Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            Name = "Azure.Mcp.Server",
            ShortName = "azure",
            DisplayName = "Azure MCP Server",
            Version = "1.0.0",
            RootCommandGroupName = "azmcp",
            Description = "This server/tool provides real-time, programmatic access to all Azure products, services, and resources."
        });
    }

    private static RegistryDiscoveryStrategy CreateStrategy(ServerRuntimeConfiguration configuration, ILogger<RegistryDiscoveryStrategy> logger)
    {
        var serverConfiguration = Microsoft.Extensions.Options.Options.Create(configuration ?? new ServerRuntimeConfiguration());
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var registryRoot = RegistryServerHelper.GetRegistryRoot(typeof(Mcp.Server.Program).Assembly, "Azure.Mcp.Server.Resources.registry.json");
        return new RegistryDiscoveryStrategy(serverConfiguration, logger, httpClientFactory, registryRoot!);
    }

    private static (SingleProxyToolLoader toolLoader, IMcpDiscoveryStrategy discoveryStrategy) CreateToolLoader(
        bool useRealDiscovery = true,
        ServerRuntimeConfiguration? configuration = null)
    {
        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();
        configuration ??= new ServerRuntimeConfiguration();
        configuration.Mode = ModeTypes.SingleToolProxy;
        var runtimeConfiguration = Microsoft.Extensions.Options.Options.Create(configuration);
        var serverConfiguration = CreateServerConfigurationOptions();
        var commandFactoryToolLoader = CreateCommandFactoryToolLoader(runtimeConfiguration, includeCommands: useRealDiscovery);

        if (useRealDiscovery)
        {
            var registryLogger = Substitute.For<ILogger<RegistryDiscoveryStrategy>>();
            var registryDiscoveryStrategy = CreateStrategy(runtimeConfiguration.Value, registryLogger);
            var toolLoader = new SingleProxyToolLoader(commandFactoryToolLoader, registryDiscoveryStrategy, logger, runtimeConfiguration, serverConfiguration);
            return (toolLoader, registryDiscoveryStrategy);
        }
        else
        {
            var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
            var toolLoader = new SingleProxyToolLoader(commandFactoryToolLoader, mockDiscoveryStrategy, logger, runtimeConfiguration, serverConfiguration);
            return (toolLoader, mockDiscoveryStrategy);
        }
    }

    private static CommandFactoryToolLoader CreateCommandFactoryToolLoader(
        Microsoft.Extensions.Options.IOptions<ServerRuntimeConfiguration> configuration,
        bool includeCommands = false)
    {
        ICommandFactory commandFactory;
        if (includeCommands)
        {
            commandFactory = CommandFactoryHelpers.CreateCommandFactory();
        }
        else
        {
            commandFactory = Substitute.For<ICommandFactory>();
            commandFactory.RootGroup.Returns(new CommandGroup("root", "Root"));
            commandFactory.AllCommands.Returns(new Dictionary<string, IBaseCommand>());
            commandFactory.GroupCommands(Arg.Any<string[]>()).Returns(new Dictionary<string, IBaseCommand>());
        }

        return new CommandFactoryToolLoader(
            commandFactory,
            configuration,
            Substitute.For<ILogger<CommandFactoryToolLoader>>());
    }

    [Fact]
    public async Task ListToolsHandler_ReturnsAzureToolWithExpectedSchema()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var request = McpTestUtilities.CreateToolListRequest();

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Tools);

        var azureTool = result.Tools.FirstOrDefault(t => t.Name == "azure");
        Assert.NotNull(azureTool);
        Assert.NotNull(azureTool.Description);
        Assert.NotEmpty(azureTool.Description!);
        // Verify the tool has proper structure
        Assert.True(azureTool.InputSchema.ValueKind != JsonValueKind.Undefined);
        Assert.NotNull(azureTool.Annotations);
    }

    [Fact]
    public async Task ListToolsHandler_WithMockedDiscovery_ReturnsSingleAzureTool()
    {
        // Arrange
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoader(useRealDiscovery: false);
        var request = McpTestUtilities.CreateToolListRequest();

        // Setup mock to return empty servers (SingleProxyToolLoader always returns the azure tool)
        mockDiscoveryStrategy.DiscoverServersAsync(TestContext.Current.CancellationToken)
            .Returns([]);

        // Act
        var result = await toolLoader.ListToolsHandler(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tools);

        var azureTool = result.Tools.First();
        Assert.Equal("azure", azureTool.Name);
    }

    [Fact]
    public async Task CallToolHandler_WithLearnMode_ReturnsRootToolsList()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["intent"] = "List available tools"
        };
        var request = McpTestUtilities.CreateToolCallRequest("azure", arguments);

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
        var arguments = new Dictionary<string, object?>
        {
            ["learn"] = true,
            ["tool"] = "nonexistent", // Use a tool that doesn't exist
            ["intent"] = "Learn about nonexistent tool"
        };
        var request = McpTestUtilities.CreateToolCallRequest("azure", arguments);

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
        var arguments = new Dictionary<string, object?>
        {
            ["intent"] = "Show me available Azure tools"
            // Intent only, should trigger learn mode automatically based on the implementation
        };
        var request = McpTestUtilities.CreateToolCallRequest("azure", arguments);

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
        Assert.Contains("Here are the available tools", textContent.Text);
    }

    [Fact]
    public async Task CallToolHandler_WithMissingToolAndCommand_ReturnsGuidanceMessage()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);

        // No learn, tool, or command parameters - should get guidance message
        var request = McpTestUtilities.CreateToolCallRequest("azure", new Dictionary<string, object?>());

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
    public async Task CallToolHandler_InternalCommand_ExecutesWithoutCreatingMcpClient()
    {
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Mode = ModeTypes.SingleToolProxy
        });
        var command = Substitute.For<IBaseCommand>();
        command.Id.Returns(Guid.NewGuid().ToString());
        command.Title.Returns("List storage accounts");
        command.Metadata.Returns(new ToolMetadata { ReadOnly = true, Destructive = false });
        command.GetCommand().Returns(new Command("account_list", "List storage accounts"));
        command.ExecuteAsync(Arg.Any<CommandContext>(), Arg.Any<ParseResult>(), Arg.Any<CancellationToken>())
            .Returns(new CommandResponse { Status = HttpStatusCode.OK, Message = "In-process result" });

        var storageGroup = new CommandGroup("storage", "Storage commands");
        storageGroup.AddCommand("account_list", command);
        var rootGroup = new CommandGroup("root", "Root commands");
        rootGroup.AddSubGroup(storageGroup);

        var commands = new Dictionary<string, IBaseCommand> { ["account_list"] = command };
        var commandFactory = Substitute.For<ICommandFactory>();
        commandFactory.RootGroup.Returns(rootGroup);
        commandFactory.AllCommands.Returns(commands);
        commandFactory.GroupCommands(Arg.Any<string[]>()).Returns(commands);
        commandFactory.GetServiceArea("account_list").Returns("storage");

        var commandFactoryToolLoader = new CommandFactoryToolLoader(
            commandFactory,
            configuration,
            Substitute.For<ILogger<CommandFactoryToolLoader>>());
        var registryDiscovery = Substitute.For<IMcpDiscoveryStrategy>();
        var toolLoader = new SingleProxyToolLoader(
            commandFactoryToolLoader,
            registryDiscovery,
            Substitute.For<ILogger<SingleProxyToolLoader>>(),
            configuration,
            CreateServerConfigurationOptions());
        var request = McpTestUtilities.CreateToolCallRequest("azure", new Dictionary<string, object?>
        {
            ["intent"] = "List storage accounts",
            ["tool"] = "storage",
            ["command"] = "account_list",
            ["parameters"] = new Dictionary<string, object?>()
        });

        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsError, string.Join(Environment.NewLine, result.Content.OfType<TextContentBlock>().Select(content => content.Text)));
        Assert.Contains("In-process result", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        await command.Received(1).ExecuteAsync(
            Arg.Any<CommandContext>(),
            Arg.Any<ParseResult>(),
            TestContext.Current.CancellationToken);
        await registryDiscovery.DidNotReceive().GetOrCreateClientAsync(
            Arg.Any<string>(),
            Arg.Any<McpClientOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallToolHandler_WithNullParams_ReturnsGuidanceMessage()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var request = McpTestUtilities.CreateToolCallRequest((CallToolRequestParams)null!, Substitute.For<McpServer>());

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
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration() { ReadOnly = true });
        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();

        var toolLoader = new SingleProxyToolLoader(CreateCommandFactoryToolLoader(configuration), discoveryStrategy, logger, configuration, CreateServerConfigurationOptions());
        var request = McpTestUtilities.CreateToolCallRequest("storage");

        // Act
        var tools = await toolLoader.GetMcpClientToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool => Assert.True(tool.ProtocolTool.Annotations?.ReadOnlyHint, $"Tool '{tool.Name}' should have ReadOnlyHint = true when ReadOnly mode is enabled"));
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
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration() { Transport = TransportTypes.Http });
        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();

        var toolLoader = new SingleProxyToolLoader(CreateCommandFactoryToolLoader(configuration), discoveryStrategy, logger, configuration, CreateServerConfigurationOptions());
        var request = McpTestUtilities.CreateToolCallRequest("storage");

        // Act
        var tools = await toolLoader.GetMcpClientToolListAsync(request, "storage", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(tools);
        Assert.All(tools, tool =>
        {
            Assert.False(McpHelper.HasHint(tool.ProtocolTool, McpHelper.LocalRequiredHintMetaKey),
                $"Tool '{tool.Name}' should have LocalRequiredHint = false when HTTP mode is enabled");
        });
    }

    [Fact]
    public async Task SingleProxyToolLoader_CachesRootToolsJson()
    {
        // Arrange
        var (toolLoader, _) = CreateToolLoader(useRealDiscovery: true);
        var arguments = new Dictionary<string, object?>
        {
            ["learn"] = true
        };
        var request = McpTestUtilities.CreateToolCallRequest("azure", arguments);

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
        var configuration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());
        var serverConfigurationOptions = CreateServerConfigurationOptions();
        var commandFactoryToolLoader = CreateCommandFactoryToolLoader(configuration);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(null!, discoveryStrategy, logger, configuration, serverConfigurationOptions));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(commandFactoryToolLoader, null!, logger, configuration, serverConfigurationOptions));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(commandFactoryToolLoader, discoveryStrategy, null!, configuration, serverConfigurationOptions));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(commandFactoryToolLoader, discoveryStrategy, logger, null!, serverConfigurationOptions));
        Assert.Throws<ArgumentNullException>(() => new SingleProxyToolLoader(commandFactoryToolLoader, discoveryStrategy, logger, configuration, null!));
    }

    #region Execution-Time Mode Enforcement Tests

    private static SingleProxyToolLoader CreateToolLoaderWithMockClient(
        ServerRuntimeConfiguration configuration, MockMcpClientBuilder clientBuilder, string serverName = "storage")
    {
        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer(serverName, serverName, $"{serverName} description", clientBuilder)
            .Build();

        var logger = Substitute.For<ILogger<SingleProxyToolLoader>>();
        var runtimeConfiguration = Microsoft.Extensions.Options.Options.Create(configuration);

        return new SingleProxyToolLoader(
            CreateCommandFactoryToolLoader(runtimeConfiguration),
            discoveryStrategy,
            logger,
            runtimeConfiguration,
            CreateServerConfigurationOptions());
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequestWithToolAndCommand(
        string tool, string command)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["intent"] = $"Execute {command}",
            ["tool"] = tool,
            ["command"] = command,
        };

        return McpTestUtilities.CreateToolCallRequest("azure", arguments);
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder);

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder);

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
            Meta = [new(McpHelper.LocalRequiredHintMetaKey, true)]
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { Transport = TransportTypes.Http }, clientBuilder);

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { Transport = TransportTypes.Http }, clientBuilder);

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder);

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration(), clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert — should execute without restrictions
        Assert.False(result.IsError ?? false);
        var textContent = result.Content.OfType<TextContentBlock>().First();
        Assert.Equal("Created account", textContent.Text);
    }

    #endregion

    #region Telemetry tests

    [Fact]
    public async Task SingleToolLoader_HasSingleToolParameters_WhenToolDoesNotGetCalled()
    {
        // Arrange
        var clientBuilder = new MockMcpClientBuilder();

        using var activity = new Activity("test-activity");
        activity.Start();

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_list");
        request.Params.Arguments?.Add("learn", JsonDocument.Parse("true").RootElement);

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);
        activity.AssertTagEquals(TagName.ToolParameters, toolParameters =>
        {
            var parametersList = JsonSerializer.Deserialize(toolParameters.ToString()!, ModelsJsonContext.Default.ListString);
            Assert.NotNull(parametersList);
            Assert.Equal(4, parametersList.Count);
            Assert.Contains("intent", parametersList);
            Assert.Contains("command", parametersList);
            Assert.Contains("tool", parametersList);
            Assert.Contains("learn", parametersList);
        });
    }

    [Fact]
    public async Task SingleToolLoader_HasNoToolParameters_WhenToolCallHasNoParameters()
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

        using var activity = new Activity("test-activity");
        activity.Start();

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_list");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        activity.AssertTagDoesNotExist(TagName.ToolParameters);
    }

    [Fact]
    public async Task SingleToolLoader_CollectsToolParameters_WhenToolCallHasParameters()
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

        using var activity = new Activity("test-activity");
        activity.Start();

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder);

        var request = CreateCallToolRequestWithToolAndCommand("storage", "account_list");
        request.Params.Arguments?.Add("parameters", JsonDocument.Parse("""{"subscription": "test-sub"}""").RootElement);

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);
        activity.AssertTagEquals(TagName.ToolParameters, toolParameters =>
        {
            var parametersList = JsonSerializer.Deserialize(toolParameters.ToString()!, ModelsJsonContext.Default.ListString);
            Assert.NotNull(parametersList);
            Assert.Single(parametersList);
            Assert.Contains("subscription", parametersList);
        });
    }

    #endregion
}
