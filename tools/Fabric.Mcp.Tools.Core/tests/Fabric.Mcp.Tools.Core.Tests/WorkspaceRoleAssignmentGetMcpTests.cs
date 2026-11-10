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

public sealed class WorkspaceRoleAssignmentGetMcpTests()
{
    private const string ToolName = "core_get-workspace-role-assignment";

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
    public static async Task Get_ExposesOneTypedSnapshotAcrossRoutesFiltersTransportsAndOutputsWithoutConsent(
        string route, string transport, StructuredOutputMode? output, string selection, bool readOnly)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            return Task.FromResult(Response(AssignmentJson, retryAfter: "0"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection, readOnly);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<WorkspaceRoleAssignmentGetCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<WorkspaceRoleAssignmentGetCommand>());
        Assert.DoesNotContain("jobs_get-workspace-role-assignment", factory.AllCommands.Keys);
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
            Assert.Equal("roleAssignment", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["retryAfterSeconds", "retryAfterUtc", "roleAssignment"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var assignment = schema.GetProperty("properties").GetProperty("roleAssignment");
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
            Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-workspace-role-assignments");
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

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        var row = payload.GetProperty("roleAssignment");
        Assert.Equal(AssignmentId, row.GetProperty("id").GetString());
        Assert.Equal("Viewer", row.GetProperty("role").GetString());
        Assert.Equal(PrincipalId, row.GetProperty("principal").GetProperty("id").GetString());
        Assert.Equal("Analyst", row.GetProperty("principal").GetProperty("displayName").GetString());
        Assert.Equal("analyst@example.invalid", row.GetProperty("principal").GetProperty("userDetails").GetProperty("userPrincipalName").GetString());
        Assert.False(row.TryGetProperty("workspaceId", out _));
        Assert.Equal(0, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(payload.TryGetProperty("continuationToken", out _));
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(result));
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain(PrincipalId, Text(result));
        }
        if (output is null || route == "single")
        {
            Assert.Null(result.StructuredContent);
        }
        Assert.Equal(1, handler.CallCount);
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
    public static async Task Get_JobsOnlyCannotDiscoverLearnOrInvokeRoleAssignments(string route, string transport)
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
    [InlineData("workspace-role-assignment-id")]
    public static async Task Get_RejectsInvalidOrMissingInputsBeforeAuthentication(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var value in new object?[] { null, "", " ", Guid.Empty.ToString(), "private-invalid", AssignmentId + "/private", 12, new[] { WorkspaceId, PrincipalId } })
        {
            var arguments = Arguments();
            arguments[option] = value;
            AssertSafeFailure(await CallAsync(provider, "all", arguments, DecliningConsentServer()), HttpStatusCode.BadRequest);
        }
        var missing = Arguments();
        missing.Remove(option);
        AssertSafeFailure(await CallAsync(provider, "all", missing, DecliningConsentServer()), HttpStatusCode.BadRequest);
        AssertSafeFailure(await CallAsync(provider, "all", [], DecliningConsentServer()), HttpStatusCode.BadRequest);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("role")]
    [InlineData("principal-id")]
    [InlineData("principal-name")]
    [InlineData("expand-groups")]
    [InlineData("continuation-token")]
    [InlineData("continuation-uri")]
    [InlineData("url")]
    [InlineData("sync")]
    public static async Task Get_RejectsUnpublishedInputsBeforeAuthentication(string option)
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
    public static async Task Get_AuthenticationFailureSendsNoRequest(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private-credential")));
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        AssertSafeFailure(await CallAsync(provider, route, Arguments(), DecliningConsentServer()), HttpStatusCode.Unauthorized);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_RejectsMalformedSnapshotsButKeepsHttpErrorStatusAndRetryGuidance(string route)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests, HttpStatusCode.Forbidden, HttpStatusCode.NotFound })
        {
            foreach (var json in new[]
            {
                AssignmentJson.Replace("\"type\":\"User\"", "\"type\":\"\\uD800\"", StringComparison.Ordinal),
                AssignmentJson[..^1] + ",\"extra\\uDC00\":\"private\"}",
                AssignmentJson[..^1] + ",\"id\":\"" + AssignmentId + "\"}",
                AssignmentJson.Replace(AssignmentId, PrincipalId, StringComparison.Ordinal),
                """{"value":[]}""",
                """{"id":"private"}"""
            })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
                var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
                AssertSafeFailure(result, status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status);
                Assert.Contains("Wait at least 60 seconds", Text(result));
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_RetainedPrincipalMetadataFitsTheExactWireEnvelopeDepthBoundary(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(NestedPrincipalAssignment(30, leafDetails: false))));
            await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
            var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());
            Assert.False(result.IsError, Text(result));
            var payload = Payload(result, route);
            Assert.Equal(61, JsonDepth(payload));
            var principal = payload.GetProperty("roleAssignment").GetProperty("principal");
            for (var index = 1; index < 30; index++)
            {
                principal = principal.GetProperty("servicePrincipalProfileDetails").GetProperty("parentPrincipal");
            }
            Assert.Equal("User", principal.GetProperty("type").GetString());
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
            Assert.InRange(JsonDepth(parsedWire.RootElement), 1, 64);
            if (route == "namespace" && output.HasValue)
            {
                Assert.Equal(64, JsonDepth(parsedWire.RootElement));
            }
            Assert.Equal(1, handler.CallCount);

            using var invalidHandler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(NestedPrincipalAssignment(30))));
            await using var invalidProvider = Services(invalidHandler, Credential(), TransportTypes.Http, output);
            AssertSafeFailure(await CallAsync(invalidProvider, route, Arguments(), DecliningConsentServer()), HttpStatusCode.BadGateway);
            Assert.Equal(1, invalidHandler.CallCount);
        }
    }

    [Fact]
    public static async Task Get_UsesAssignmentIdentityFromASeparateExplicitListWithoutTranslatingPrincipalIds()
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(Response(requests.Count == 1 ? Page() : AssignmentJson));
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var list = await Loader(provider, "all").CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_list-workspace-role-assignments",
                new Dictionary<string, object?> { ["workspace-id"] = WorkspaceId }),
            TestContext.Current.CancellationToken);
        Assert.False(list.IsError, Text(list));
        var assignment = list.StructuredContent!.Value.GetProperty("roleAssignments")[0];
        Assert.NotEqual(assignment.GetProperty("id").GetString(), assignment.GetProperty("principal").GetProperty("id").GetString());
        Assert.Equal([CollectionUrl], requests);
        var arguments = Arguments();
        arguments["workspace-role-assignment-id"] = assignment.GetProperty("id").GetString();
        var get = await CallAsync(provider, "all", arguments, DecliningConsentServer());
        Assert.False(get.IsError, Text(get));
        Assert.True(JsonElement.DeepEquals(assignment, Payload(get, "all").GetProperty("roleAssignment")));
        Assert.Equal([CollectionUrl, AssignmentUrl], requests);
    }

    [Fact]
    public static async Task Get_InheritsSharedScalarStringArrayBindingWithoutAddingLookups()
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(Response(AssignmentJson));
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = new Dictionary<string, object?>
        {
            ["workspace-id"] = new[] { WorkspaceId },
            ["workspace-role-assignment-id"] = new[] { AssignmentId }
        };
        Assert.False((await CallAsync(provider, "all", arguments, DecliningConsentServer())).IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Get_RegisteredCliRejectsRepeatedAndValuelessOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "core", "get-workspace-role-assignment", "--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<WorkspaceRoleAssignmentGetCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--workspace-role-assignment-id" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["jobs", "get-workspace-role-assignment"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["workspace-id", "workspace-role-assignment-id"],
            schema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
        var properties = schema.GetProperty("properties");
        Assert.Equal(["workspace-id", "workspace-role-assignment-id"], properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.All(properties.EnumerateObject(), property => Assert.Equal("string", property.Value.GetProperty("type").GetString()));
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
            Description = "Offline workspace role-assignment lookup tests",
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
            arguments = new() { ["intent"] = "Read one workspace role assignment", ["command"] = ToolName, ["parameters"] = arguments };
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

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["workspace-role-assignment-id"] = AssignmentId };

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
