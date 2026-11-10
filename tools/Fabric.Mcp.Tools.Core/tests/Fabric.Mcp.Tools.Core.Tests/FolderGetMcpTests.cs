// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class FolderGetMcpTests()
{
    private const string ToolName = "core_get-folder";

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
                            foreach (var readOnly in new[] { false, true })
                            {
                                data.Add(route, transport, output, selection, readOnly);
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
    public static async Task Get_ExposesOneSnapshotAcrossRoutesFiltersTransportsAndOutputsWithoutConsent(
        string route, string transport, StructuredOutputMode? output, string selection, bool readOnly)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            return Task.FromResult(Response(Snapshot(), retryAfter: "0"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection, readOnly);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<FolderGetCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<FolderGetCommand>());
        Assert.DoesNotContain("jobs_get-folder", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.DoesNotContain("Preview", tool.Description, StringComparison.OrdinalIgnoreCase);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal("folder", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["folder", "retryAfterSeconds", "retryAfterUtc"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var folderSchema = schema.GetProperty("properties").GetProperty("folder");
            Assert.Equal(["displayName", "id", "workspaceId"],
                folderSchema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.Equal(["displayName", "id", "parentFolderId", "workspaceId"],
                folderSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        if (readOnly)
        {
            Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-folders");
            Assert.Contains(catalog.Tools, tool => tool.Name == "core_get-item");
            Assert.DoesNotContain(catalog.Tools, tool => tool.Name == "core_create-folder");
        }
        if (route != "all")
        {
            var learned = await LearnAsync(provider, route);
            AssertInputSchema(Assert.Single(learned, command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = DecliningConsentServer();

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        var folder = payload.GetProperty("folder");
        Assert.Equal(FolderId, folder.GetProperty("id").GetString());
        Assert.Equal(ParentFolderId, folder.GetProperty("parentFolderId").GetString());
        Assert.Equal(WorkspaceId, folder.GetProperty("workspaceId").GetString());
        Assert.Equal("Sales", folder.GetProperty("displayName").GetString());
        Assert.Equal(0, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.Equal(["folder", "retryAfterSeconds"], payload.EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal(1, handler.CallCount);
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
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Get_JobsOnlyCannotDiscoverLearnOrInvokeFolders(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "jobs");
        var catalog = await Loader(provider, route).ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name.StartsWith("core", StringComparison.Ordinal));
        if (route != "all")
        {
            var learned = await LearnAsync(provider, route, "jobs");
            Assert.DoesNotContain(learned, command => command.GetProperty("command").GetString() == ToolName);
        }
        var blocked = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
        if (route == "single")
        {
            Assert.Equal("tool-list", blocked.StructuredContent!.Value.GetProperty("kind").GetString());
            Assert.DoesNotContain(ToolName, blocked.StructuredContent.Value.GetRawText());
        }
        else
        {
            Assert.True(blocked.IsError);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_RejectsMissingInvalidAndRepeatedInputsBeforeAuthentication(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var option in new[] { "workspace-id", "folder-id" })
        {
            var missing = Arguments();
            missing.Remove(option);
            AssertSafeFailure(await CallAsync(provider, route, missing, DecliningConsentServer()), HttpStatusCode.BadRequest);
            foreach (var value in new object?[] { null, "", " ", Guid.Empty.ToString(), "private-invalid", FolderId + "/folders",
                12, true, new[] { FolderId, FolderId } })
            {
                var arguments = Arguments();
                arguments[option] = value;
                AssertSafeFailure(await CallAsync(provider, route, arguments, DecliningConsentServer()), HttpStatusCode.BadRequest);
            }
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_RejectsUnpublishedInputsBeforeAuthentication(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var option in new[] { "display-name", "parent-folder-id", "root-folder-id", "path", "recursive",
            "continuation-token", "continuation-uri", "url", "preview", "sync" })
        {
            var arguments = Arguments();
            arguments[option] = "private-input";
            var result = await CallAsync(provider, route, arguments, DecliningConsentServer());
            Assert.True(result.IsError, Text(result));
            Assert.Null(result.StructuredContent);
            Assert.Contains("Request rejected due to unknown arguments", Text(result));
            Assert.DoesNotContain("private", Text(result));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_ReportsAuthenticationAndAuthorizationFailuresWithoutAConsentGate(string route)
    {
        foreach (var failure in new[] { "credentials", "authentication", "401", "403" })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                Task.FromResult(Response("""{"errorCode":"InsufficientPrivileges","message":"private"}""",
                    failure == "403" ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized)));
            var credential = Credential();
            if (failure is "credentials" or "authentication")
            {
                AuthenticationFailedException exception = failure == "credentials"
                    ? new CredentialUnavailableException("private")
                    : new AuthenticationFailedException("private");
                credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                    .Returns(ValueTask.FromException<AccessToken>(exception));
            }
            await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact, readOnly: true);
            var server = DecliningConsentServer();
            var result = await CallAsync(provider, route, Arguments(), server);
            AssertSafeFailure(result, failure == "403" ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
            if (failure == "403")
            {
                Assert.Contains("Workspace.Read.All", Text(result));
                Assert.Contains("Viewer or higher", Text(result));
            }
            Assert.Equal(failure is "credentials" or "authentication" ? 0 : 1, handler.CallCount);
            Assert.Single(credential.ReceivedCalls());
            await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_RejectsMalformedSnapshotsButKeepsHttpErrorStatusAndRetryGuidance(string route)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
        {
            foreach (var json in new[]
            {
                "null", "{}",
                Snapshot().Replace(FolderId, ParentFolderId, StringComparison.Ordinal),
                Snapshot().Replace(WorkspaceId, ParentFolderId, StringComparison.Ordinal),
                Snapshot()[..^1] + ",\"extra\":\"\\uD800\"}",
                Snapshot()[..^1] + ",\"ID\":\"" + FolderId + "\"}"
            })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
                var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
                AssertSafeFailure(result, status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status);
                Assert.Contains("Wait at least 60 seconds", Text(result));
                Assert.DoesNotContain("creation", Text(result), StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("created", Text(result), StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("mutation", Text(result), StringComparison.OrdinalIgnoreCase);
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_OmitsUnavailableParentsAndFitsWireEnvelopeAtMaximumInputDepth(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            var json = JsonSerializer.Serialize(Folder with { ParentFolderId = null }, CoreJsonContext.Default.FabricFolder);
            json = json[..^1] + ",\"parentFolderId\":null,\"extra\":" + new string('[', 63) + "0" + new string(']', 63) + "}";
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
            await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
            var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
            Assert.False(result.IsError, Text(result));
            var payload = Payload(result, route);
            Assert.Equal(FolderId, payload.GetProperty("folder").GetProperty("id").GetString());
            Assert.False(payload.GetProperty("folder").TryGetProperty("parentFolderId", out _));
            Assert.DoesNotContain("extra", payload.GetRawText());
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
        }
    }

    [Fact]
    public static async Task Get_RegisteredCliRejectsRepeatedAndValuelessOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "core", "get-folder", "--workspace-id", WorkspaceId, "--folder-id", FolderId };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<FolderGetCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--folder-id" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, FolderId]).Errors);
            Assert.NotEmpty(factory.RootCommand.Parse(["core", "get-folder", option]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["jobs", "get-folder", "--workspace-id", WorkspaceId, "--folder-id", FolderId]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["folder-id", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        var properties = schema.GetProperty("properties");
        Assert.Equal(["folder-id", "workspace-id"], properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.All(properties.EnumerateObject(), property =>
        {
            Assert.Equal("string", property.Value.GetProperty("type").GetString());
            Assert.False(property.Value.TryGetProperty("default", out _));
        });
    }

    private static void AssertSafeFailure(CallToolResult result, HttpStatusCode status)
    {
        Assert.True(result.IsError, Text(result));
        Assert.Null(result.StructuredContent);
        using var document = JsonDocument.Parse(Assert.IsType<TextContentBlock>(result.Content.Last()).Text);
        Assert.Equal((int)status, document.RootElement.GetProperty("status").GetInt32());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private", Text(result));
        Assert.DoesNotContain("stackTrace", Text(result));
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
            Description = "Offline folder lookup tests",
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
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server)
    {
        var name = ToolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Read one logical workspace folder", ["command"] = ToolName, ["parameters"] = arguments };
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

    private static McpServer DecliningConsentServer()
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()).Returns(call => new JsonRpcResponse
        {
            Id = call.Arg<JsonRpcRequest>().Id,
            Result = new JsonObject { ["action"] = "decline" }
        });
        return server;
    }

    private static Dictionary<string, object?> Arguments() => new() { ["workspace-id"] = WorkspaceId, ["folder-id"] = FolderId };

    private static string Snapshot() => JsonSerializer.Serialize(Folder, CoreJsonContext.Default.FabricFolder);

    private static string Text(CallToolResult result) => string.Join('\n', result.Content.Select(block => Assert.IsType<TextContentBlock>(block).Text));

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
