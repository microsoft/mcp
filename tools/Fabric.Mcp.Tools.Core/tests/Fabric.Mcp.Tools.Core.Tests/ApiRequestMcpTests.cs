// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Services;
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

public sealed class ApiRequestMcpTests()
{
    private const string GetTool = "core_get-api-resource";
    private const string InvokeTool = "core_invoke-api-request";
    private const string JobsPath = "/v1/workspaces/aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb/items/bbbbbbbb-1111-2222-3333-cccccccccccc/jobs/instances?jobType=DefaultJob";
    private const string SchedulePath = "/v1/workspaces/aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb/items/bbbbbbbb-1111-2222-3333-cccccccccccc/JOBS/DefaultJob/schedules";

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Get_DeniedOrUnsupportedConsentSendsNothingInReadOnlyMode(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "core", readOnly: true);
        Assert.True(provider.GetRequiredService<ICommandFactory>().AllCommands[GetTool].Command.Metadata.Secret);
        foreach (var (action, decision) in new[] { ("decline", "accept"), ("cancel", "accept"), ("accept", "reject") })
        {
            var server = Consent(action, decision);
            Assert.True((await CallAsync(provider, route, GetTool, GetArguments("/v1/connections"), server)).IsError);
            await server.Received(1).SendRequestAsync(
                Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
        }
        Assert.True((await CallAsync(provider, route, GetTool, GetArguments("/v1/connections"), Substitute.For<McpServer>())).IsError);
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
    public static async Task CoreOnly_RejectsJobsEndpointsThroughBothGenericToolsBeforeIo(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "core");
        foreach (var path in new[] { JobsPath, SchedulePath })
        {
            foreach (var (tool, arguments) in new[] { (GetTool, GetArguments(path)), (InvokeTool, InvokeArguments(path)) })
            {
                var result = await CallAsync(provider, route, tool, arguments, Consent());
                Assert.True(result.IsError, Text(result));
                Assert.True(Text(result).Contains("jobs namespace", StringComparison.Ordinal), Text(result));
            }
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("combined")]
    [InlineData("default")]
    public static async Task JobsEnabled_AllowsJobsEndpointsThroughGenericTools(string selection)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.JsonResponse("""{"value":[]}""")));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), TransportTypes.Http, StructuredOutputMode.Compact, selection);

        var get = await CallAsync(provider, "all", GetTool, GetArguments(JobsPath), Consent());
        var invoke = await CallAsync(provider, "all", InvokeTool, InvokeArguments(JobsPath), Consent());

        Assert.False(get.IsError, Text(get));
        Assert.False(invoke.IsError, Text(invoke));
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_ValidatesExactRetainedDepthBeforeEveryOutputEnvelope(string route)
    {
        foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
        {
            foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
            {
                foreach (var depth in new[] { FabricApiResponseReader.MaxRetainedJsonDepth, FabricApiResponseReader.MaxRetainedJsonDepth + 1, 64 })
                {
                    var body = new string('[', depth) + new string(']', depth);
                    using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.JsonResponse(body)));
                    await using var provider = Services(handler, FabricOperationTestData.Credential(), transport, output, "core", readOnly: true);

                    var result = await CallAsync(provider, route, GetTool, GetArguments("/v1/connections"), Consent());

                    if (depth == FabricApiResponseReader.MaxRetainedJsonDepth)
                    {
                        Assert.False(result.IsError, Text(result));
                        using var expected = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 64 });
                        Assert.True(JsonElement.DeepEquals(expected.RootElement, Payload(result, route).GetProperty("response").GetProperty("value")));
                    }
                    else
                    {
                        Assert.True(result.IsError, Text(result));
                        Assert.Null(result.StructuredContent);
                        using var document = JsonDocument.Parse(Text(result));
                        Assert.Equal((int)HttpStatusCode.BadGateway, document.RootElement.GetProperty("status").GetInt32());
                        Assert.Contains($"{FabricApiResponseReader.MaxRetainedJsonDepth} nesting levels", Text(result));
                    }
                    var toolResult = new JsonObject
                    {
                        ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = Text(result) }),
                        ["isError"] = result.IsError
                    };
                    if (result.StructuredContent is { } content)
                    {
                        toolResult["structuredContent"] = JsonNode.Parse(content.GetRawText(), documentOptions: new() { MaxDepth = 64 });
                    }
                    var wire = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = 1, ["result"] = toolResult };
                    using var parsedWire = JsonDocument.Parse(wire.ToJsonString(new JsonSerializerOptions { MaxDepth = 64 }));
                    Assert.True(parsedWire.RootElement.TryGetProperty("result", out _));
                    Assert.Equal(1, handler.CallCount);
                }
            }
        }
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, string transport, StructuredOutputMode? output,
        string selection, bool readOnly = false)
    {
        var services = new ServiceCollection();
        foreach (var area in new IAreaSetup[] { new FabricCoreSetup(), new FabricJobsSetup() })
        {
            area.ConfigureServices(services);
            services.AddSingleton(area);
        }
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
            Description = "Offline generic REST tool tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = selection switch
            {
                "core" => ["core"],
                "combined" => ["core", "jobs"],
                "default" => null,
                _ => throw new ArgumentOutOfRangeException(nameof(selection))
            },
            ReadOnly = readOnly,
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

    private static IToolLoader Loader(IServiceProvider provider, string route) => route switch
    {
        "all" => provider.GetRequiredService<CommandFactoryToolLoader>(),
        "namespace" => provider.GetRequiredService<NamespaceToolLoader>(),
        "single" => provider.GetRequiredService<SingleProxyToolLoader>(),
        _ => throw new ArgumentOutOfRangeException(nameof(route))
    };

    private static Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, string tool, Dictionary<string, object?> arguments, McpServer server)
    {
        var name = tool;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Call one Fabric REST endpoint", ["command"] = tool, ["parameters"] = arguments };
            name = route == "single" ? "fabric" : "core";
            if (route == "single")
            {
                arguments["tool"] = "core";
            }
        }
        var request = McpTestUtilities.CreateToolCallRequest(name, arguments);
        return Loader(provider, route).CallToolHandler(
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

    private static Dictionary<string, object?> GetArguments(string path) => new() { ["path"] = path };

    private static Dictionary<string, object?> InvokeArguments(string path) => new() { ["path"] = path, ["method"] = "POST" };

    private static string Text(CallToolResult result) =>
        string.Join('\n', result.Content.Select(block => Assert.IsType<TextContentBlock>(block).Text));

    private static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } content)
        {
            return route == "namespace" ? content.GetProperty("result") : content;
        }
        using var document = JsonDocument.Parse(Text(result), new JsonDocumentOptions { MaxDepth = 64 });
        return document.RootElement.GetProperty("results").Clone();
    }
}
