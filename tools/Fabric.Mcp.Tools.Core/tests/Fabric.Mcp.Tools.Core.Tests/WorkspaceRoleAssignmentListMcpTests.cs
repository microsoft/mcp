// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Commands;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class WorkspaceRoleAssignmentListMcpTests()
{
    private const string ToolName = "core_list-workspace-role-assignments";

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
    public static async Task List_ExposesOneTypedPageAcrossRoutesFiltersTransportsAndOutputsWithoutConsent(
        string route, string transport, StructuredOutputMode? output, string selection, bool readOnly)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(Response(requests.Count == 1
                ? Page(DocumentedToken, $"{CollectionUrl}?continuationToken={DocumentedToken}")
                : """{"value":[],"continuationToken":null,"continuationUri":null}""", retryAfter: "0"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection, readOnly);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<WorkspaceRoleAssignmentListCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<WorkspaceRoleAssignmentListCommand>());
        Assert.DoesNotContain("jobs_list-workspace-role-assignments", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal("roleAssignments", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["continuationToken", "retryAfterSeconds", "retryAfterUtc", "roleAssignments"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var assignment = schema.GetProperty("properties").GetProperty("roleAssignments").GetProperty("items");
            Assert.Equal(["id", "principal", "role"], assignment.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.Equal(["id", "principal", "role"], assignment.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var principal = assignment.GetProperty("properties").GetProperty("principal");
            Assert.Equal(["id", "type"], principal.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.Equal(["displayName", "groupDetails", "id", "servicePrincipalDetails", "servicePrincipalProfileDetails", "type", "userDetails"],
                principal.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.False(assignment.GetProperty("properties").GetProperty("role").TryGetProperty("enum", out _));
            Assert.False(principal.GetProperty("properties").GetProperty("type").TryGetProperty("enum", out _));
        }
        if (readOnly)
        {
            Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-workspaces");
            Assert.DoesNotContain(catalog.Tools, tool => tool.Name == "core_delete-workspace");
        }
        if (route != "all")
        {
            var learned = await LearnAsync(provider, route);
            AssertInputSchema(Assert.Single(learned, command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = DecliningConsentServer();

        var first = await CallAsync(provider, route, Arguments(), server);

        Assert.False(first.IsError, Text(first));
        var payload = Payload(first, route);
        var row = Assert.Single(payload.GetProperty("roleAssignments").EnumerateArray());
        Assert.Equal(AssignmentId, row.GetProperty("id").GetString());
        Assert.Equal("Viewer", row.GetProperty("role").GetString());
        Assert.Equal(PrincipalId, row.GetProperty("principal").GetProperty("id").GetString());
        Assert.Equal("Analyst", row.GetProperty("principal").GetProperty("displayName").GetString());
        Assert.Equal("analyst@example.invalid", row.GetProperty("principal").GetProperty("userDetails").GetProperty("userPrincipalName").GetString());
        Assert.False(row.TryGetProperty("workspaceId", out _));
        Assert.Equal(DocumentedToken, payload.GetProperty("continuationToken").GetString());
        Assert.Equal(0, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(payload.TryGetProperty("continuationUri", out _));
        Assert.Equal([CollectionUrl], requests);
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(first));
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), first.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain(DocumentedToken, Text(first));
        }
        if (output is null || route == "single")
        {
            Assert.Null(first.StructuredContent);
        }
        var next = Arguments();
        next["continuation-token"] = payload.GetProperty("continuationToken").GetString();
        var second = await CallAsync(provider, route, next, server);
        Assert.False(second.IsError, Text(second));
        Assert.Empty(Payload(second, route).GetProperty("roleAssignments").EnumerateArray());
        Assert.False(Payload(second, route).TryGetProperty("continuationToken", out _));
        Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={DocumentedToken}"], requests);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task List_JobsOnlyCannotDiscoverLearnOrInvokeRoleAssignments(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "jobs");
        var catalog = await Loader(provider, route).ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name.StartsWith("core", StringComparison.Ordinal));
        if (route != "all")
        {
            Assert.DoesNotContain(await LearnAsync(provider, route, "jobs"),
                command => command.GetProperty("command").GetString() == ToolName);
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
    [InlineData("workspace-id")]
    [InlineData("continuation-token")]
    public static async Task List_RejectsInvalidInputsBeforeAuthentication(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        var values = option == "workspace-id"
            ? new object?[] { null, "", " ", Guid.Empty.ToString(), "private-invalid", WorkspaceId + "/roleAssignments", 12, new[] { WorkspaceId, PrincipalId } }
            : ["", " ", "private\r\ninput", "private\0", new string('x', 8193)];
        foreach (var value in values)
        {
            var arguments = Arguments();
            arguments[option] = value;
            AssertSafeFailure(await CallAsync(provider, "all", arguments, DecliningConsentServer()), HttpStatusCode.BadRequest);
        }
        AssertSafeFailure(await CallAsync(provider, "all", [], DecliningConsentServer()), HttpStatusCode.BadRequest);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("role")]
    [InlineData("roles")]
    [InlineData("principal-id")]
    [InlineData("expand-groups")]
    [InlineData("page-size")]
    [InlineData("continuation-uri")]
    [InlineData("url")]
    [InlineData("sync")]
    public static async Task List_RejectsUnpublishedInputsBeforeAuthentication(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = Arguments();
        arguments[option] = "private-input";
        var result = await CallAsync(provider, "all", arguments, DecliningConsentServer());
        Assert.True(result.IsError, Text(result));
        Assert.Null(result.StructuredContent);
        Assert.Contains("Request rejected due to unknown arguments", Text(result));
        Assert.DoesNotContain("private", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task List_NullTokenOmitsQueryAndAuthenticationFailureSendsNoRequest(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(Response("""{"value":[]}"""));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = Arguments();
        arguments["continuation-token"] = null;
        Assert.False((await CallAsync(provider, route, arguments, DecliningConsentServer())).IsError);
        Assert.Equal(1, handler.CallCount);
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private-credential")));
        AssertSafeFailure(await CallAsync(provider, route, Arguments(), DecliningConsentServer()), HttpStatusCode.Unauthorized);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task List_RejectsMalformedPagesButKeepsHttpErrorStatusAndRetryGuidance(string route)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests, HttpStatusCode.Forbidden })
        {
            foreach (var json in new[]
            {
                """{"value":[],"continuationToken":"\uD800"}""",
                """{"value":[],"extra\uDC00":"private"}""",
                """{"value":[],"continuationToken":null,"continuationUri":"https://example.invalid/private"}""",
                """{"value":[],"continuationToken":false,"continuationUri":null}""",
                """{"value":[],"continuationToken":null,"continuationUri":0}""",
                """{"value":[],"value":[{}]}""",
                """{"value":[{"id":"private"}]}"""
            })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
                var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
                AssertSafeFailure(result, status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status);
                Assert.Contains(status == HttpStatusCode.OK ? "No partial page" : "Wait at least 60 seconds", Text(result));
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task List_RetainedPrincipalMetadataFitsTheActualWireEnvelopeAtTheDepthBoundary(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var json in new[]
            {
                NestedPrincipalPage(29),
                Page()[..^1] + ",\"extra\":" + new string('[', 63) + "0" + new string(']', 63) + "}"
            })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json)));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
                var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
                Assert.False(result.IsError, Text(result));
                Assert.Equal(AssignmentId, Payload(result, route).GetProperty("roleAssignments")[0].GetProperty("id").GetString());
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
            using var invalidHandler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(NestedPrincipalPage(30))));
            await using var invalidProvider = Services(invalidHandler, Credential(), TransportTypes.Http, output);
            AssertSafeFailure(await CallAsync(invalidProvider, route, Arguments(), DecliningConsentServer()), HttpStatusCode.BadGateway);
        }
    }

    [Fact]
    public static async Task List_RegisteredCliRejectsRepeatedAndValuelessOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "core", "list-workspace-role-assignments", "--workspace-id", WorkspaceId, "--continuation-token", "first" };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<WorkspaceRoleAssignmentListCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--continuation-token" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
            Assert.NotEmpty(factory.RootCommand.Parse(["core", "list-workspace-role-assignments", "--workspace-id", WorkspaceId, option]).Errors);
        }
        Assert.Empty(factory.RootCommand.Parse(["core", "list-workspace-role-assignments", "--workspace-id", WorkspaceId]).Errors);
        Assert.NotEmpty(factory.RootCommand.Parse(["jobs", "list-workspace-role-assignments"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal("workspace-id", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
        var properties = schema.GetProperty("properties");
        Assert.Equal(["continuation-token", "workspace-id"], properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal("string", properties.GetProperty("workspace-id").GetProperty("type").GetString());
        Assert.Equal("string", properties.GetProperty("continuation-token").GetProperty("type").GetString());
    }

    private static void AssertSafeFailure(CallToolResult result, HttpStatusCode status)
    {
        Assert.True(result.IsError, Text(result));
        Assert.Null(result.StructuredContent);
        using var document = JsonDocument.Parse(Text(result));
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
            Description = "Offline workspace role-assignment listing tests",
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
            arguments = new() { ["intent"] = "Read one workspace role-assignment page", ["command"] = ToolName, ["parameters"] = arguments };
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

    private static Dictionary<string, object?> Arguments() => new() { ["workspace-id"] = WorkspaceId };

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
