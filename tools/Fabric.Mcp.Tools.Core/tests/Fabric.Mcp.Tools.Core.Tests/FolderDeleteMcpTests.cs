// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class FolderDeleteMcpTests()
{
    private const string ToolName = "core_delete-folder";

    public static TheoryData<string, string, StructuredOutputMode?, string> Routes
    {
        get
        {
            var data = new TheoryData<string, string, StructuredOutputMode?, string>();
            foreach (var route in new[] { "all", "namespace", "single" })
            {
                foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
                {
                    foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
                    {
                        foreach (var selection in new[] { "core", "combined", "default" })
                        {
                            data.Add(route, transport, output, selection);
                        }
                    }
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public static async Task Delete_OneTypedReceiptAcrossRoutesTransportsOutputsAndSelectionsWithConsent(
        string route, string transport, StructuredOutputMode? output, string selection)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("folder-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnreadableHttpContent() });
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<FolderDeleteCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<FolderDeleteCommand>());
        Assert.DoesNotContain("jobs_delete-folder", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Contains("empty logical", tool.Description);
        Assert.Contains("FolderNotEmpty", tool.Description);
        Assert.DoesNotContain("Preview", tool.Description, StringComparison.OrdinalIgnoreCase);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["deletionStatus", "folderId", "guidance", "workspaceId"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        if (route != "all")
        {
            AssertInputSchema(Assert.Single(await LearnAsync(provider, route),
                command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
        var server = Consent();

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal(["deletionStatus", "folderId", "guidance", "workspaceId"],
            payload.EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal("Deleted", payload.GetProperty("deletionStatus").GetString());
        Assert.Equal(FolderId, payload.GetProperty("folderId").GetString());
        Assert.Equal(WorkspaceId, payload.GetProperty("workspaceId").GetString());
        Assert.Contains("confirmed deletion of the requested empty folder (HTTP 200)", payload.GetProperty("guidance").GetString());
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
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
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
    public static async Task Delete_ReadOnlyAndJobsOnlyCannotDiscoverLearnOrInvoke(string route, string transport)
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
                Assert.Contains(catalog.Tools, tool => tool.Name == "core_get-folder");
                Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-folders");
                Assert.DoesNotContain(catalog.Tools, tool => tool.Name is "core_create-folder" or "core_update-folder" or "core_move-folder");
                Assert.Equal(["jobs_get-item-job-instance", "jobs_get-item-schedule", "jobs_list-item-job-instances", "jobs_list-item-schedules"],
                    catalog.Tools.Where(tool => tool.Name.StartsWith("jobs_", StringComparison.Ordinal)).Select(tool => tool.Name).Order());
            }
            if (route != "all")
            {
                Assert.DoesNotContain(await LearnAsync(provider, route, readOnly ? "core" : "jobs"),
                    command => command.GetProperty("command").GetString() == ToolName);
            }
            var server = Consent();
            var result = await CallAsync(provider, route, Arguments(), server);
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
            await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_InvalidMissingRepeatedAndUnsupportedInputsNeverAuthenticate(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var (name, value) in new (string, object?)[]
        {
            ("workspace-id", "private"), ("workspace-id", Guid.Empty.ToString()), ("workspace-id", null),
            ("folder-id", "private"), ("folder-id", ""), ("folder-id", " "), ("folder-id", Guid.Empty.ToString()), ("folder-id", null),
            ("folder-id", new[] { FolderId, ParentFolderId }), ("workspace-id", 12), ("folder-id", false),
            ("path", "private"), ("recursive", true), ("force", true), ("hard-delete", true),
            ("parent-folder-id", ParentFolderId), ("sync", true), ("operation-id", FolderId),
            ("url", "private"), ("preview", true), ("display-name", "private"), ("target-folder-id", ParentFolderId), ("item-id", FolderId)
        })
        {
            var arguments = Arguments();
            arguments[name] = value;
            var result = await CallAsync(provider, route, arguments, Consent());
            Assert.True(handler.CallCount == 0, $"Unexpected HTTP for {name}.");
            Assert.True(result.IsError, Text(result));
            Assert.DoesNotContain("private", Text(result));
        }
        foreach (var name in new[] { "workspace-id", "folder-id" })
        {
            var arguments = Arguments();
            arguments.Remove(name);
            Assert.True((await CallAsync(provider, route, arguments, Consent())).IsError);
            var repeated = Arguments();
            repeated[name.ToUpperInvariant()] = ParentFolderId;
            Assert.True((await CallAsync(provider, route, repeated, Consent())).IsError);
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_PreservesSharedStringArgumentBinding(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"{CollectionUrl}/{FolderId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = Arguments();
        arguments["workspace-id"] = new[] { WorkspaceId };
        arguments["folder-id"] = new[] { FolderId };
        var result = await CallAsync(provider, route, arguments, Consent());
        Assert.False(result.IsError, Text(result));
        Assert.Equal(FolderId, Payload(result, route).GetProperty("folderId").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_DeclinedCanceledRejectedAndUnsupportedConsentNeverAuthenticate(string route)
    {
        foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
            var credential = Credential();
            await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact);
            foreach (var (action, decision) in new[] { ("decline", "accept"), ("cancel", "accept"), ("accept", "reject"), ("accept", "") })
            {
                Assert.True((await CallAsync(provider, route, Arguments(), Consent(action, decision))).IsError);
            }
            Assert.True((await CallAsync(provider, route, Arguments(), Substitute.For<McpServer>())).IsError);
            Assert.Equal(0, handler.CallCount);
            Assert.Empty(credential.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_AuthenticationAndAuthorizationFailuresPreserveSendState(string route)
    {
        foreach (var failure in new[] { "credentials", "authentication", "401", "403" })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(
                Response("""{"errorCode":"InsufficientPrivileges","message":"private"}""",
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
            await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
            var result = await CallAsync(provider, route, Arguments(), Consent());
            AssertSafeFailure(result, failure == "403" ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
            Assert.Equal(failure is "credentials" or "authentication", Text(result).Contains("No DELETE request was sent", StringComparison.Ordinal));
            if (failure == "403")
            {
                Assert.Contains("Contributor or higher", Text(result));
                Assert.Contains("Workspace.ReadWrite.All", Text(result));
            }
            Assert.Equal(failure is "credentials" or "authentication" ? 0 : 1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_FailuresKeepKnownRejectionOrUncertaintyAcrossOutputs(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var status in new[] { HttpStatusCode.Accepted, HttpStatusCode.NoContent, HttpStatusCode.TemporaryRedirect, HttpStatusCode.TooManyRequests,
                HttpStatusCode.InternalServerError, HttpStatusCode.ServiceUnavailable })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => status == HttpStatusCode.ServiceUnavailable
                    ? throw new HttpRequestException("private")
                    : Task.FromResult(Response("""{"errorCode":"\uD800","message":"private"}""", status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
                var result = await CallAsync(provider, route, Arguments(), Consent());
                AssertSafeFailure(result, status is HttpStatusCode.Accepted or HttpStatusCode.NoContent ? HttpStatusCode.BadGateway : status);
                Assert.Equal(status != HttpStatusCode.TooManyRequests,
                    Text(result).Contains("may have been deleted", StringComparison.Ordinal));
                if (status != HttpStatusCode.ServiceUnavailable)
                {
                    Assert.Contains("Wait at least 60 seconds", Text(result));
                }
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all", HttpStatusCode.BadRequest, "FolderNotEmpty")]
    [InlineData("namespace", HttpStatusCode.Conflict, "FolderNotEmpty")]
    [InlineData("single", HttpStatusCode.NotFound, "FolderNotFound")]
    public static async Task Delete_DocumentedRejectionsNeverEmptyFoldersOrBecomeSuccess(string route, HttpStatusCode status, string code)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(Response($"{{\"errorCode\":\"{code}\",\"message\":\"private\",\"moreDetails\":[{{\"message\":\"private\"}}]}}", status)));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent());
        AssertSafeFailure(result, status);
        Assert.Contains(code, Text(result));
        Assert.DoesNotContain("may have been deleted", Text(result));
        Assert.Contains("No automatic retry or change to folder contents was attempted", Text(result));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_ConfirmedDeletionSurvivesLateCancellationAndIrrelevantBody(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            using var content = new UnreadableHttpContent();
            using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            {
                cancellation.Cancel();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
            });
            await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
            var result = await CallAsync(provider, route, Arguments(), Consent(), cancellation.Token);
            Assert.False(result.IsError, Text(result));
            Assert.Equal("Deleted", Payload(result, route).GetProperty("deletionStatus").GetString());
            Assert.Contains("Do not resubmit", Payload(result, route).GetProperty("guidance").GetString());
            var toolResult = new JsonObject
            {
                ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = Text(result) }),
                ["isError"] = result.IsError
            };
            if (result.StructuredContent is { } structured)
            {
                toolResult["structuredContent"] = JsonNode.Parse(structured.GetRawText());
            }
            var wire = new JsonObject { ["jsonrpc"] = "2.0", ["id"] = 1, ["result"] = toolResult };
            using var parsed = JsonDocument.Parse(wire.ToJsonString(new JsonSerializerOptions { MaxDepth = 64 }));
            Assert.True(content.IsDisposed);
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_LocalReceiptFailureRetainsConfirmedDeletion(string route)
    {
        var stream = Substitute.For<Stream>();
        stream.When(value => value.Close()).Do(_ => throw new IOException("private"));
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) }));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent());
        AssertSafeFailure(result, HttpStatusCode.BadGateway);
        Assert.Contains("confirmed folder deletion (HTTP 200)", Text(result));
        Assert.Contains("Do not resubmit", Text(result));
        Assert.DoesNotContain("may have been deleted", Text(result));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public static async Task Delete_RegisteredCliRejectsRepeatedAndValuelessOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "core", "delete-folder", "--workspace-id", WorkspaceId, "--folder-id", FolderId };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<FolderDeleteCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--folder-id" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["jobs", "delete-folder"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["folder-id", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        Assert.Equal(["folder-id", "workspace-id"], schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.All(schema.GetProperty("properties").EnumerateObject(), property =>
        {
            Assert.Equal("string", property.Value.GetProperty("type").GetString());
            Assert.False(property.Value.TryGetProperty("default", out _));
        });
    }

    private static void AssertSafeFailure(CallToolResult result, HttpStatusCode status)
    {
        Assert.True(result.IsError, Text(result));
        Assert.Null(result.StructuredContent);
        Assert.DoesNotContain("private", Text(result));
        Assert.DoesNotContain("stackTrace", Text(result));
        using var document = JsonDocument.Parse(Text(result));
        Assert.Equal((int)status, document.RootElement.GetProperty("status").GetInt32());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
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
            Description = "Offline folder deletion tests",
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
            arguments = new() { ["intent"] = "Delete one explicitly identified empty logical folder", ["command"] = ToolName, ["parameters"] = arguments };
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

    private static McpServer Consent(string action = "accept", string decision = "accept")
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => new JsonRpcResponse
            {
                Id = call.Arg<JsonRpcRequest>().Id,
                Result = new JsonObject { ["action"] = action, ["content"] = new JsonObject { ["decision"] = decision } }
            });
        return server;
    }

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["folder-id"] = FolderId };

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
