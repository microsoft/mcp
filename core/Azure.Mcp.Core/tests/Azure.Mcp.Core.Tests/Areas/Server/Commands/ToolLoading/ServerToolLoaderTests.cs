// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Tests.Areas.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
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
    private static (ServerToolLoader toolLoader, IMcpDiscoveryStrategy mockDiscoveryStrategy) CreateToolLoaderAndDiscoveryStrategy()
    {
        var mockDiscoveryStrategy = Substitute.For<IMcpDiscoveryStrategy>();
        var toolLoader = CreateToolLoader(mockDiscoveryStrategy);

        return (toolLoader, mockDiscoveryStrategy);
    }

    private static ServerToolLoader CreateToolLoader(
        IMcpDiscoveryStrategy? discoveryStrategy = null,
        ServerRuntimeConfiguration? configuration = null)
    {
        var logger = Substitute.For<ILogger<ServerToolLoader>>();
        var serverConfiguration = Microsoft.Extensions.Options.Options.Create(configuration ?? new ServerRuntimeConfiguration());
        discoveryStrategy ??= RegistryDiscoveryStrategyHelper.CreateStrategy(serverConfiguration.Value);

        return new ServerToolLoader(discoveryStrategy, serverConfiguration, logger);
    }

    [Fact]
    public async Task CallToolHandler_WithoutListToolsFirst_ShouldSucceed()
    {
        // Arrange - use real RegistryDiscoveryStrategy since ServerToolLoader depends on it
        var toolLoader = CreateToolLoader();
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
        var (toolLoader, mockDiscoveryStrategy) = CreateToolLoaderAndDiscoveryStrategy();
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
        var toolLoader = CreateToolLoader();
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

        var toolLoader = CreateToolLoader(discoveryStrategy);
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

        var toolLoader = CreateToolLoader(discoveryStrategy);

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
        var configuration = new ServerRuntimeConfiguration() { ReadOnly = true };

        var toolLoader = CreateToolLoader(discoveryStrategy, configuration);
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
        var configuration = new ServerRuntimeConfiguration() { Transport = TransportTypes.Http };

        var toolLoader = CreateToolLoader(discoveryStrategy, configuration);
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

    private static ServerToolLoader CreateToolLoaderWithMockClient(
        ServerRuntimeConfiguration configuration, MockMcpClientBuilder clientBuilder, string serverName = "test-server")
    {
        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer(serverName, serverName, $"{serverName} description", clientBuilder)
            .Build();

        return CreateToolLoader(discoveryStrategy, configuration);
        ;
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "account_create");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - The non-read-only tool must NOT be executed
        Assert.False(writeToolExecuted, "Non-read-only tool should not be executed in read-only mode");
        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "account_create", "account_list");
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder, "storage");

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { Transport = TransportTypes.Http }, clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "local_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - The local-required tool must NOT be executed in HTTP mode
        Assert.False(localToolExecuted, "Local-required tool should not be executed in HTTP mode");
        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "local_command", "remote_command");
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { Transport = TransportTypes.Http }, clientBuilder, "storage");

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "unknown_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        // Assert - Tool without read-only annotation must NOT be executed in read-only mode
        Assert.False(toolExecuted, "Tool without ReadOnlyHint should not be executed in read-only mode");
        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "unknown_command");
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = false }, clientBuilder, "storage");

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
    public async Task CallToolHandler_WithReadOnlyAndNoSampling_UnknownCommandReturnsFilteredNames()
    {
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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = true }, clientBuilder, "storage");

        var request = CreateCallToolRequestWithCommand("storage", "bad_command");

        // Act
        var result = await toolLoader.CallToolHandler(request, TestContext.Current.CancellationToken);

        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "bad_command", "account_list");
    }

    #endregion

    [Theory]
    [InlineData(ModeTypes.NamespaceProxy, null)]
    [InlineData(ModeTypes.NamespaceProxy, StructuredOutputMode.Duplicated)]
    [InlineData(ModeTypes.NamespaceProxy, StructuredOutputMode.Compact)]
    [InlineData(ModeTypes.ConsolidatedProxy, null)]
    [InlineData(ModeTypes.ConsolidatedProxy, StructuredOutputMode.Duplicated)]
    [InlineData(ModeTypes.ConsolidatedProxy, StructuredOutputMode.Compact)]
    public async Task CallToolHandler_UnknownCommandWithoutUsableSampling_ReturnsNamesOnly(
        string executionMode, StructuredOutputMode? outputMode)
    {
        var executions = 0;
        var clientBuilder = new MockMcpClientBuilder();
        foreach (var name in new[] { "storage_zeta", "storage_alpha" })
        {
            clientBuilder.AddTool(CreateRoutingTool(name), _ =>
            {
                executions++;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Executed" }] };
            });
        }
        await using var loader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration
        {
            Mode = executionMode,
            StructuredOutputMode = outputMode
        }, clientBuilder, "storage");

        (string? Intent, bool SupportsSampling)[] scenarios =
        [
            (null, false), (null, true), ("", true), (" \t ", true), ("list resources", false)
        ];
        foreach (var (intent, supportsSampling) in scenarios)
        {
            var server = BaseToolLoaderTests.CreateSamplingServer(supportsSampling,
                """{"command":"storage_alpha","parameters":{}}""");

            var result = await loader.CallToolHandler(
                BaseToolLoaderTests.CreateCommandRequest(server, intent: intent),
                TestContext.Current.CancellationToken);

            var text = BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "invalid_command",
                "storage_alpha", "storage_zeta");
            Assert.DoesNotContain("Catalog detail.", text);
            await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        }
        Assert.Equal(0, executions);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task CallToolHandler_UnknownCommand_ListsOnlyCommandsAllowedByConfiguration(
        bool readOnly, bool isHttpMode, bool emptyCatalog)
    {
        var executions = 0;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(CreateRoutingTool("storage_blocked", readOnly: false, localRequired: true), _ =>
            {
                executions++;
                return new CallToolResult { Content = [] };
            });
        if (!emptyCatalog)
        {
            clientBuilder.AddTool(CreateRoutingTool("storage_allowed"), _ =>
            {
                executions++;
                return new CallToolResult { Content = [] };
            });
        }
        await using var loader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration
        {
            ReadOnly = readOnly,
            Transport = isHttpMode ? TransportTypes.Http : TransportTypes.StdIo
        }, clientBuilder, "storage");
        var server = BaseToolLoaderTests.CreateSamplingServer(false);

        var result = await loader.CallToolHandler(
            BaseToolLoaderTests.CreateCommandRequest(server, command: "storage_blocked"),
            TestContext.Current.CancellationToken);

        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "storage_blocked",
            emptyCatalog ? [] : ["storage_allowed"]);
        Assert.Equal(0, executions);
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CallToolHandler_UnknownCommandWithEmptyCatalog_DoesNotSample(bool supportsSampling)
    {
        await using var loader = CreateToolLoaderWithMockClient(
            new ServerRuntimeConfiguration(), new MockMcpClientBuilder(), "storage");
        var server = BaseToolLoaderTests.CreateSamplingServer(supportsSampling);

        var result = await loader.CallToolHandler(
            BaseToolLoaderTests.CreateCommandRequest(server), TestContext.Current.CancellationToken);

        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "invalid_command");
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("storage_alpha")]
    [InlineData("STORAGE_ALPHA")]
    public async Task CallToolHandler_SamplingCorrection_ExecutesCanonicalCommandOnceWithSampledParameters(string sampledName)
    {
        var executions = 0;
        IReadOnlyDictionary<string, object?>? executedParameters = null;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(CreateRoutingTool("storage_alpha"), parameters =>
            {
                executions++;
                executedParameters = parameters;
                return new CallToolResult { Content = [new TextContentBlock { Text = "Corrected execution" }], IsError = false };
            });
        await using var loader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration(), clientBuilder, "storage");
        var server = BaseToolLoaderTests.CreateSamplingServer(true,
            $$$"""{"command":"{{{sampledName}}}","parameters":{"subscription":"sampled-subscription","limit":3}}""");

        var result = await loader.CallToolHandler(
            BaseToolLoaderTests.CreateCommandRequest(server,
                parametersJson: """{"subscription":"original-subscription","limit":1}"""),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal("Corrected execution", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(1, executions);
        Assert.NotNull(executedParameters);
        Assert.Equal(2, executedParameters.Count);
        Assert.Equal("sampled-subscription", executedParameters["subscription"]);
        Assert.Equal(3, executedParameters["limit"]);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(rpc => rpc.Method == "sampling/createMessage"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" \t ", false)]
    [InlineData("not JSON", false)]
    [InlineData("null", false)]
    [InlineData("[]", false)]
    [InlineData("{}", false)]
    [InlineData("""{"command":null}""", false)]
    [InlineData("""{"command":17}""", false)]
    [InlineData("""{"command":""}""", false)]
    [InlineData("""{"command":" "}""", false)]
    [InlineData("""{"command":"Unknown","parameters":{}}""", false)]
    [InlineData("""{"command":"nonexistent","parameters":{}}""", false)]
    [InlineData(null, true)]
    public async Task CallToolHandler_UnusableSamplingCorrection_ReturnsErrorWithoutExecutionOrRetry(
        string? samplingText, bool failSampling)
    {
        var executions = 0;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(CreateRoutingTool("storage_alpha"), _ =>
            {
                executions++;
                return new CallToolResult { Content = [] };
            });
        await using var loader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration(), clientBuilder, "storage");
        var server = BaseToolLoaderTests.CreateSamplingServer(true, samplingText, failSampling);

        var result = await loader.CallToolHandler(
            BaseToolLoaderTests.CreateCommandRequest(server), TestContext.Current.CancellationToken);

        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "invalid_command", "storage_alpha");
        Assert.Equal(0, executions);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(rpc => rpc.Method == "sampling/createMessage"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task CallToolHandler_FilteredSamplingCorrection_DoesNotExecuteOrRetry(bool readOnly, bool isHttpMode)
    {
        var executions = 0;
        var clientBuilder = new MockMcpClientBuilder();
        foreach (var tool in new[]
        {
            CreateRoutingTool("storage_alpha"),
            CreateRoutingTool("storage_blocked", readOnly: !readOnly, localRequired: isHttpMode)
        })
        {
            clientBuilder.AddTool(tool, _ =>
            {
                executions++;
                return new CallToolResult { Content = [] };
            });
        }
        await using var loader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration
        {
            ReadOnly = readOnly,
            Transport = isHttpMode ? TransportTypes.Http : TransportTypes.StdIo
        }, clientBuilder, "storage");
        var server = BaseToolLoaderTests.CreateSamplingServer(true,
            """{"command":"STORAGE_BLOCKED","parameters":{}}""");

        var result = await loader.CallToolHandler(
            BaseToolLoaderTests.CreateCommandRequest(server), TestContext.Current.CancellationToken);

        BaseToolLoaderTests.AssertUnknownCommandResult(result, "storage", "invalid_command", "storage_alpha");
        Assert.Equal(0, executions);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(rpc => rpc.Method == "sampling/createMessage"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task CallToolHandler_LearnOrIntentOnly_PreservesCatalogWithoutCorrectionLoop(
        bool explicitLearn, bool supportsSampling)
    {
        var executions = 0;
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(CreateRoutingTool("storage_alpha"), _ =>
            {
                executions++;
                return new CallToolResult { Content = [] };
            });
        await using var loader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration(), clientBuilder, "storage");
        var server = BaseToolLoaderTests.CreateSamplingServer(supportsSampling,
            """{"command":"nonexistent","parameters":{}}""");

        var result = await loader.CallToolHandler(
            BaseToolLoaderTests.CreateCommandRequest(server,
                command: explicitLearn ? "invalid_command" : null, learn: explicitLearn),
            TestContext.Current.CancellationToken);

        Assert.NotEqual(true, result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        Assert.Contains("storage_alpha", text);
        Assert.Contains("Catalog detail.", text);
        Assert.Contains("\"inputSchema\"", text);
        Assert.Equal(0, executions);
        await server.Received(supportsSampling ? 1 : 0).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(rpc => rpc.Method == "sampling/createMessage"), Arg.Any<CancellationToken>());
    }

    private static Tool CreateRoutingTool(string name, bool readOnly = true, bool localRequired = false)
    {
        using var schema = JsonDocument.Parse("""
            {"type":"object","properties":{"subscription":{"type":"string","description":"Catalog detail."},"limit":{"type":"integer"}}}
            """);
        return new Tool
        {
            Name = name,
            Description = "Catalog detail.",
            InputSchema = schema.RootElement.Clone(),
            Annotations = new ToolAnnotations { ReadOnlyHint = readOnly },
            Meta = [new(McpHelper.LocalRequiredHintMetaKey, localRequired)]
        };
    }

    #region Telemetry tests

    [Fact]
    public async Task ServerToolLoader_HasServerToolParameters_WhenToolDoesNotGetCalled()
    {
        // Arrange
        var clientBuilder = new MockMcpClientBuilder();

        using var activity = new Activity("test-activity");
        activity.Start();

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = false }, clientBuilder, "storage");

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = false }, clientBuilder, "storage");

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

        var toolLoader = CreateToolLoaderWithMockClient(new ServerRuntimeConfiguration { ReadOnly = false }, clientBuilder, "storage");

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
