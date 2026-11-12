// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ApiResourceGetMcpTests()
{
    private const string ToolName = "core_get-api-resource";

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Get_DeniedCanceledRejectedAndUnsupportedConsentNeverAuthenticates(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact);
        foreach (var (api, path) in new[]
        {
            ("fabric", "/v1/workspaces/aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb/items/bbbbbbbb-1111-2222-3333-cccccccccccc/jobs/Refresh/schedules/cccccccc-2222-3333-4444-dddddddddddd"),
            ("powerbi", "/v1.0/myorg/gateways")
        })
        {
            foreach (var (action, decision) in new[] { ("decline", "accept"), ("cancel", "accept"), ("accept", "reject"), ("accept", "") })
            {
                var server = Consent(action, decision);
                var result = await CallAsync(provider, route, server, api, path);
                Assert.True(result.IsError, Text(result));
                await server.Received(1).SendRequestAsync(
                    Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
            }
            Assert.True((await CallAsync(provider, route, Substitute.For<McpServer>(), api, path)).IsError);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Get_ValidatesBodyDepthBeforeEveryOutputEnvelope(string route, string transport)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var depth in new[] { 59, 60, 64, 65 })
            {
                foreach (var objectRoot in new[] { false, true })
                {
                    var arrays = objectRoot ? depth - 1 : depth;
                    var body = new string('[', arrays) + "\"private\"" + new string(']', arrays);
                    if (objectRoot)
                    {
                        body = "{\"nested\":" + body + "}";
                    }
                    using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                        Task.FromResult(FabricOperationTestData.JsonResponse(body)));
                    var credential = FabricOperationTestData.Credential();
                    await using var provider = Services(handler, credential, transport, output);
                    var server = Consent();

                    var result = await CallAsync(provider, route, server);

                    if (depth == 59)
                    {
                        Assert.False(result.IsError, Text(result));
                        using var expected = JsonDocument.Parse(body);
                        var payload = Payload(result, route);
                        Assert.True(JsonElement.DeepEquals(expected.RootElement, payload.GetProperty("response").GetProperty("value")));
                    }
                    else
                    {
                        Assert.True(result.IsError, Text(result));
                        Assert.Null(result.StructuredContent);
                        using var error = JsonDocument.Parse(Text(result));
                        Assert.Equal((int)HttpStatusCode.BadGateway, error.RootElement.GetProperty("status").GetInt32());
                        Assert.False(error.RootElement.TryGetProperty("results", out _));
                        Assert.Contains("59 nesting levels", Text(result));
                        Assert.DoesNotContain("private", Text(result));
                        Assert.DoesNotContain("CurrentDepth", Text(result));
                        Assert.DoesNotContain("stackTrace", Text(result));
                    }
                    var toolResult = new JsonObject
                    {
                        ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = Text(result) }),
                        ["isError"] = result.IsError
                    };
                    if (result.StructuredContent is { } content)
                    {
                        toolResult["structuredContent"] = JsonNode.Parse(content.GetRawText());
                    }
                    var wire = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = 1, ["result"] = toolResult };
                    using var parsedWire = JsonDocument.Parse(wire.ToJsonString(new JsonSerializerOptions { MaxDepth = 64 }));
                    Assert.True(parsedWire.RootElement.TryGetProperty("result", out _));
                    Assert.Equal(1, handler.CallCount);
                    await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
                    await server.Received(1).SendRequestAsync(
                        Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
                }
            }
        }
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, string transport, StructuredOutputMode? output)
    {
        var services = new ServiceCollection();
        var area = new FabricCoreSetup();
        area.ConfigureServices(services);
        services.AddSingleton<IAreaSetup>(area);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddLogging();
        services.AddSingleton(Substitute.For<ITelemetryService>());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Microsoft Fabric MCP Server",
            Version = "1.0.0",
            Description = "Offline API resource tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            ReadOnly = true,
            Transport = transport,
            StructuredOutputMode = output
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<ICommandContextAccessor, CommandContextAccessor>();
        services.AddSingleton<CommandFactoryToolLoader>();
        services.AddSingleton<NamespaceToolLoader>();
        services.AddSingleton<SingleProxyToolLoader>();
        return services.BuildServiceProvider();
    }

    private static Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, McpServer server, string api = "fabric", string path = "/v1/connections")
    {
        Dictionary<string, object?> arguments = new() { ["api"] = api, ["path"] = path };
        var name = ToolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Read one API resource", ["command"] = ToolName, ["parameters"] = arguments };
            name = route == "single" ? "fabric" : "core";
            if (route == "single")
            {
                arguments["tool"] = "core";
            }
        }
        IToolLoader loader = route switch
        {
            "all" => provider.GetRequiredService<CommandFactoryToolLoader>(),
            "namespace" => provider.GetRequiredService<NamespaceToolLoader>(),
            "single" => provider.GetRequiredService<SingleProxyToolLoader>(),
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };
        var request = McpTestUtilities.CreateToolCallRequest(name, arguments);
        return loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(request.Params!, server), TestContext.Current.CancellationToken).AsTask();
    }

    private static McpServer Consent(string action = "accept", string decision = "accept")
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()).Returns(call => new JsonRpcResponse
        {
            Id = call.Arg<JsonRpcRequest>().Id,
            Result = new JsonObject { ["action"] = action, ["content"] = new JsonObject { ["decision"] = decision } }
        });
        return server;
    }

    private static string Text(CallToolResult result) =>
        string.Join('\n', result.Content.Select(block => Assert.IsType<TextContentBlock>(block).Text));

    private static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } content)
        {
            return route == "namespace" ? content.GetProperty("result") : content;
        }
        using var document = JsonDocument.Parse(Text(result));
        return document.RootElement.GetProperty("results").Clone();
    }
}
