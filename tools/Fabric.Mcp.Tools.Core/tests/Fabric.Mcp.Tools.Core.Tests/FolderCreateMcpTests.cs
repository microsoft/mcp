// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class FolderCreateMcpTests()
{
    private const string ToolName = "core_create-folder";

    public static TheoryData<string, string, StructuredOutputMode?, string, bool> Routes
    {
        get
        {
            var data = new TheoryData<string, string, StructuredOutputMode?, string, bool>();
            foreach (var route in new[] { "all", "namespace", "single" })
            {
                foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
                {
                    foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
                    {
                        foreach (var selection in new[] { "core", "combined", "default" })
                        {
                            foreach (var child in new[] { false, true })
                            {
                                data.Add(route, transport, output, selection, child);
                            }
                        }
                    }
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public static async Task Create_OneTypedMutationAcrossRoutesTransportsOutputsAndSelectionsWithoutElicitation(
        string route, string transport, StructuredOutputMode? output, string selection, bool child)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal("Sales", body.RootElement.GetProperty("displayName").GetString());
            Assert.Equal(child, body.RootElement.TryGetProperty("parentFolderId", out var parent));
            if (child)
            {
                Assert.Equal(ParentFolderId, parent.GetString());
            }
            return Response(CreatedFolder(child ? ParentFolderId : null), HttpStatusCode.Created, "0");
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<FolderCreateCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<FolderCreateCommand>());
        Assert.DoesNotContain("jobs_create-folder", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["creationStatus", "folder", "guidance", "retryAfterSeconds", "retryAfterUtc"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var folder = schema.GetProperty("properties").GetProperty("folder");
            Assert.Equal(["displayName", "id", "workspaceId"], folder.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        }
        if (route != "all")
        {
            AssertInputSchema(Assert.Single(await LearnAsync(provider, route),
                command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
        var server = Substitute.For<McpServer>();
        var arguments = Arguments();
        if (child)
        {
            arguments["parent-folder-id"] = ParentFolderId;
        }

        var result = await CallAsync(provider, route, arguments, server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal("Created", payload.GetProperty("creationStatus").GetString());
        Assert.Equal(FolderId, payload.GetProperty("folder").GetProperty("id").GetString());
        Assert.Equal(WorkspaceId, payload.GetProperty("folder").GetProperty("workspaceId").GetString());
        Assert.Equal(child, payload.GetProperty("folder").TryGetProperty("parentFolderId", out _));
        Assert.Equal(0, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.Contains("Do not resubmit", payload.GetProperty("guidance").GetString());
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(result));
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain(FolderId, Text(result));
        }
        if (output is null || route == "single")
        {
            Assert.Null(result.StructuredContent);
        }
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Create_ReadOnlyAndJobsOnlyCannotDiscoverLearnOrInvoke(string route, string transport)
    {
        foreach (var readOnly in new[] { false, true })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
            var credential = Credential();
            await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, readOnly ? "combined" : "jobs", readOnly);
            var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
                McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
            Assert.DoesNotContain(catalog.Tools, tool => tool.Name == ToolName);
            if (readOnly)
            {
                Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-folders");
                Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-items");
                Assert.Equal(["jobs_get-item-job-instance", "jobs_get-item-schedule", "jobs_list-item-job-instances", "jobs_list-item-schedules"],
                    catalog.Tools.Where(tool => tool.Name.StartsWith("jobs_", StringComparison.Ordinal)).Select(tool => tool.Name).Order());
            }
            if (route != "all")
            {
                Assert.DoesNotContain(await LearnAsync(provider, route, readOnly ? "core" : "jobs"),
                    command => command.GetProperty("command").GetString() == ToolName);
            }
            var result = await CallAsync(provider, route, Arguments(), Substitute.For<McpServer>());
            if (route == "single")
            {
                Assert.Equal("tool-list", result.StructuredContent!.Value.GetProperty("kind").GetString());
                Assert.DoesNotContain(ToolName, result.StructuredContent.Value.GetRawText());
            }
            else
            {
                Assert.True(result.IsError, Text(result));
            }
            Assert.Empty(credential.ReceivedCalls());
            Assert.Equal(0, handler.CallCount);
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_InvalidMissingRepeatedAndUnsupportedInputsNeverAuthenticate(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var (name, value) in new (string, object?)[]
        {
            ("workspace-id", "private"), ("workspace-id", Guid.Empty.ToString()), ("workspace-id", null),
            ("parent-folder-id", "private"), ("parent-folder-id", ""), ("parent-folder-id", Guid.Empty.ToString()),
            ("display-name", ""), ("display-name", null), ("display-name", "private/path"),
            ("display-name", " private"), ("display-name", new string('a', 256)),
            ("path", "private"), ("recursive", true), ("folder-id", FolderId), ("sync", true),
            ("url", "private"), ("preview", true), ("get-or-create", true), ("continuation-token", "private")
        })
        {
            var arguments = Arguments();
            arguments[name] = value;
            var result = await CallAsync(provider, route, arguments, Substitute.For<McpServer>());
            Assert.True(handler.CallCount == 0, $"Unexpected HTTP for {name}.");
            Assert.True(result.IsError, Text(result));
            Assert.DoesNotContain("private", Text(result));
        }
        foreach (var name in new[] { "workspace-id", "display-name" })
        {
            var arguments = Arguments();
            arguments.Remove(name);
            Assert.True((await CallAsync(provider, route, arguments, Substitute.For<McpServer>())).IsError);
        }
        var repeated = Arguments();
        repeated["DISPLAY-NAME"] = "Second";
        Assert.True((await CallAsync(provider, route, repeated, Substitute.For<McpServer>())).IsError);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_ExplicitNullParentIsOmittedFromPost(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal("displayName", Assert.Single(body.RootElement.EnumerateObject()).Name);
            return Response(CreatedFolder(), HttpStatusCode.Created);
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = Arguments();
        arguments["parent-folder-id"] = null;
        var result = await CallAsync(provider, route, arguments, Substitute.For<McpServer>());
        Assert.False(result.IsError, Text(result));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_FailuresKeepKnownCreationOrUncertaintyAcrossOutputs(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var status in new[] { HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.Conflict, HttpStatusCode.TooManyRequests, HttpStatusCode.ServiceUnavailable })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => status == HttpStatusCode.ServiceUnavailable
                    ? throw new HttpRequestException("private")
                    : Task.FromResult(Response("""{"errorCode":"\uD800","message":"private"}""", status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
                var result = await CallAsync(provider, route, Arguments(), Substitute.For<McpServer>());
                Assert.True(result.IsError, Text(result));
                Assert.Null(result.StructuredContent);
                using var document = JsonDocument.Parse(Text(result));
                Assert.Equal((int)(status is HttpStatusCode.Created or HttpStatusCode.Accepted ? HttpStatusCode.BadGateway : status),
                    document.RootElement.GetProperty("status").GetInt32());
                Assert.False(document.RootElement.TryGetProperty("results", out _));
                Assert.Contains(status == HttpStatusCode.Created ? "confirmed folder creation (HTTP 201)" : "may already have been created", Text(result));
                Assert.DoesNotContain("private", Text(result));
                Assert.DoesNotContain("stackTrace", Text(result));
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_ConfirmedCreationSurvivesCanceledMetadataRead(string route)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new OperationTestStream(System.Text.Encoding.UTF8.GetBytes(CreatedFolder()), (_, token) =>
        {
            cancellation.Cancel();
            token.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        });
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new("application/json");
            return Task.FromResult(response);
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Substitute.For<McpServer>(), cancellation.Token);
        Assert.True(result.IsError, Text(result));
        Assert.Contains("confirmed folder creation (HTTP 201)", Text(result));
        Assert.Contains("Do not resubmit", Text(result));
        Assert.DoesNotContain("may already have been created", Text(result));
        Assert.Null(result.StructuredContent);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_TypedProjectionFitsWireEnvelopeAtMaximumInputDepth(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            var json = CreatedFolder()[..^1] + ",\"extra\":" + new string('[', 63) + "0" + new string(']', 63) + "}";
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, HttpStatusCode.Created)));
            await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
            var result = await CallAsync(provider, route, Arguments(), Substitute.For<McpServer>());
            Assert.False(result.IsError, Text(result));
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
            using var parsed = JsonDocument.Parse(wire.ToJsonString(new JsonSerializerOptions { MaxDepth = 64 }));
            Assert.Equal(FolderId, Payload(result, route).GetProperty("folder").GetProperty("id").GetString());
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Fact]
    public static async Task Create_RegisteredCliRejectsRepeatedAndValuelessOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "core", "create-folder", "--workspace-id", WorkspaceId, "--display-name", "Sales", "--parent-folder-id", ParentFolderId };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<FolderCreateCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--display-name", "--parent-folder-id" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
            Assert.NotEmpty(factory.RootCommand.Parse(["core", "create-folder", "--workspace-id", WorkspaceId, "--display-name", "Sales", option]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["jobs", "create-folder"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["display-name", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        Assert.Equal(["display-name", "parent-folder-id", "workspace-id"], schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.All(schema.GetProperty("properties").EnumerateObject(), property =>
        {
            Assert.Equal("string", property.Value.GetProperty("type").GetString());
            Assert.False(property.Value.TryGetProperty("default", out _));
        });
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, string transport, StructuredOutputMode? output,
        string selection = "core", bool readOnly = false)
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
            Description = "Offline folder creation tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = selection switch
            {
                "core" => ["core"],
                "jobs" => ["jobs"],
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

    private static async Task<JsonElement[]> LearnAsync(IServiceProvider provider, string route, string area = "core")
    {
        var arguments = new Dictionary<string, object?> { ["intent"] = "", ["learn"] = true };
        if (route == "single")
        {
            arguments["tool"] = area;
        }
        var result = await Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "single" ? "fabric" : area, arguments), TestContext.Current.CancellationToken);
        Assert.NotEqual(true, result.IsError);
        if (result.StructuredContent is { } content)
        {
            return content.GetProperty("tools").EnumerateArray().ToArray();
        }
        var text = Text(result);
        var start = text.IndexOf('[');
        Assert.True(start >= 0, text);
        using var document = JsonDocument.Parse(text[start..]);
        return document.RootElement.EnumerateArray().Select(value => value.Clone()).ToArray();
    }

    private static Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server, CancellationToken? token = null)
    {
        var name = ToolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Create one explicitly requested logical workspace folder", ["command"] = ToolName, ["parameters"] = arguments };
            name = route == "single" ? "fabric" : "core";
            if (route == "single")
            {
                arguments["tool"] = "core";
            }
        }
        var request = McpTestUtilities.CreateToolCallRequest(name, arguments);
        return Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(request.Params!, server), token ?? TestContext.Current.CancellationToken).AsTask();
    }

    private static Dictionary<string, object?> Arguments() => new() { ["workspace-id"] = WorkspaceId, ["display-name"] = "Sales" };

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
