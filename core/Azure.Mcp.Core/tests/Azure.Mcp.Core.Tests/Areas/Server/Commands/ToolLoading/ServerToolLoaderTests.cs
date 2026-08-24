// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Tests.Areas.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public class ServerToolLoaderTests
{
    private static (ServerToolLoader toolLoader, IMcpDiscoveryStrategy mockDiscoveryStrategy) CreateToolLoader(ToolLoaderOptions? options = null)
    {
        var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        var logger = Substitute.For<ILogger<ServerToolLoader>>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(options ?? new ToolLoaderOptions());

        var toolLoader = new ServerToolLoader(mockDiscoveryStrategy, toolLoaderOptions, logger);
        return (toolLoader, mockDiscoveryStrategy);
    }

    [Fact]
    public async Task CallToolHandler_WithoutListToolsFirst_ShouldSucceed()
    {
        // Arrange - use real RegistryDiscoveryStrategy since ServerToolLoader depends on it
        var serviceStartOptions = Microsoft.Extensions.Options.Options.Create(new ServerStartOptions());
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());
        var discoveryLogger = Substitute.For<ILogger<RegistryDiscoveryStrategy>>();
        var discoveryStrategy = RegistryDiscoveryStrategyHelper.CreateStrategy(serviceStartOptions.Value, discoveryLogger);
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolCallRequest("documentation", new Dictionary<string, object?>
        {
            { "intent", "search for information about implementing MCP servers" },
            { "command", "microsoft_docs_search" },
            { "parameters", new Dictionary<string, string>() { { "question", "how to implement mcp server in azure" } } }
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
        var request = McpTestUtilities.CreateToolListRequest();

        mockDiscoveryStrategy.DiscoverServersAsync(TestContext.Current.CancellationToken)
            .Returns([]);

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
        var serviceStartOptions = Microsoft.Extensions.Options.Options.Create(new ServerStartOptions());
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions());
        var discoveryLogger = Substitute.For<ILogger<RegistryDiscoveryStrategy>>();
        var discoveryStrategy = RegistryDiscoveryStrategyHelper.CreateStrategy(serviceStartOptions.Value, discoveryLogger);
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolListRequest();

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
            .Returns([providerA, providerB]);

        var logger = Substitute.For<ILogger<ServerToolLoader>>();
        var toolLoader = new ServerToolLoader(discoveryStrategy, Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions()), logger);
        var request = McpTestUtilities.CreateToolListRequest();

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

        var logger = Substitute.For<ILogger<ServerToolLoader>>();
        var toolLoader = new ServerToolLoader(discoveryStrategy, Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions()), logger);

        var request = McpTestUtilities.CreateToolCallRequest("documentation", new Dictionary<string, object?>
            {
                { "intent", "search docs" },
                { "command", "microsoft_docs_search" },
                { "parameters", new Dictionary<string, string>() { { "question", "how to deploy azure mcp server" } } }
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
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions() { ReadOnly = true });
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolCallRequest("storage");

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
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(new ToolLoaderOptions() { IsHttpMode = true });
        var logger = Substitute.For<ILogger<ServerToolLoader>>();

        var toolLoader = new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger);
        var request = McpTestUtilities.CreateToolCallRequest("storage");

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

        var logger = Substitute.For<ILogger<ServerToolLoader>>();
        var toolLoaderOptions = Microsoft.Extensions.Options.Options.Create(options);

        return (new ServerToolLoader(discoveryStrategy, toolLoaderOptions, logger), discoveryStrategy);
    }

    private static RequestContext<CallToolRequestParams> CreateCallToolRequestWithCommand(
        string serverName, string command, Dictionary<string, object>? extraParams = null)
    {
        var arguments = new Dictionary<string, object?>
        {
            { "intent", $"Execute {command}" },
            { "command", command },
        };

        if (extraParams != null)
        {
            foreach (var kvp in extraParams)
            {
                arguments[kvp.Key] = kvp.Value;
            }
        }

        return McpTestUtilities.CreateToolCallRequest(serverName, arguments);
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

    #region Telemetry tests

    [Fact]
    public async Task ServerToolLoader_HasServerToolParameters_WhenToolDoesNotGetCalled()
    {
        // Arrange
        var clientBuilder = new MockMcpClientBuilder();

        using var activity = new Activity("test-activity");
        activity.Start();

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: false), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow execution when read-only mode is not enabled
        Assert.NotNull(result);
        activity.AssertTagEquals(TagName.ToolParameters, toolParameters =>
        {
            var parametersList = JsonSerializer.Deserialize(toolParameters.ToString()!, ModelsJsonContext.Default.ListString);
            Assert.NotNull(parametersList);
            Assert.Equal(2, parametersList.Count);
            Assert.Contains("intent", parametersList);
            Assert.Contains("command", parametersList);
        });
    }

    [Fact]
    public async Task ServerToolLoader_HasNoToolParameters_WhenToolCallHasNoParameters()
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

        using var activity = new Activity("test-activity");
        activity.Start();

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

        activity.AssertTagDoesNotExist(TagName.ToolParameters);
    }

    [Fact]
    public async Task ServerToolLoader_CollectsToolParameters_WhenToolCallHasParameters()
    {
        // Arrange
        var writeTool = new Tool
        {
            Name = "account_create",
            Description = "Create storage account",
            InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {"subscription": {"type": "string", "description": "The subscription"}}}""").RootElement,
            Annotations = new ToolAnnotations { ReadOnlyHint = false }
        };

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(writeTool, _ => new CallToolResult { Content = [new TextContentBlock { Text = "Created account" }], IsError = false });

        using var activity = new Activity("test-activity");
        activity.Start();

        var (toolLoader, _) = CreateToolLoaderWithMockClient(
            new ToolLoaderOptions(ReadOnly: false), clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "account_create", new()
        {
            { "parameters", new Dictionary<string, string>() { { "subscription", "test-sub" } } }
        });

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Should allow execution when read-only mode is not enabled
        Assert.NotNull(result);
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
