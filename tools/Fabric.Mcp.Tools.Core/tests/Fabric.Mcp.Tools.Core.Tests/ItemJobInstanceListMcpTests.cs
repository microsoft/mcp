// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceListTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemJobInstanceListMcpTests()
{
    private const string ToolName = "jobs_list-item-job-instances";

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
                        foreach (var selection in new[] { "jobs", "combined", "default" })
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
    public static async Task List_ExposesTypedPagesAcrossModesFiltersTransportsAndOutputsWithoutConsent(
        string route, string transport, StructuredOutputMode? output, string selection, bool readOnly)
    {
        var requests = new List<string>();
        var page = JsonNode.Parse(Page(DocumentedToken, includeUri: true))!.AsObject();
        page["value"]![0]!["status"] = "Failed";
        page["value"]![0]!["failureReason"] = new JsonObject
        {
            ["errorCode"] = "JobFailed",
            ["isRetriable"] = false,
            ["message"] = FabricCoreErrorTestData.PrivateDetails
        };
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(Response(requests.Count == 1 ? page.ToJsonString() : """{"value":[]}""", retryAfter: "0"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection, readOnly);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<ItemJobInstanceListCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemJobInstanceListCommand>());
        Assert.DoesNotContain("core_list-item-job-instances", factory.AllCommands.Keys);
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
            Assert.Equal("jobInstances", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["continuationToken", "jobInstances", "retryAfterSeconds", "retryAfterUtc"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var instance = schema.GetProperty("properties").GetProperty("jobInstances").GetProperty("items");
            Assert.Equal(["id", "invokeType", "itemId", "jobType", "status"],
                instance.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            var properties = instance.GetProperty("properties");
            Assert.False(properties.GetProperty("status").TryGetProperty("enum", out _));
            Assert.False(properties.GetProperty("invokeType").TryGetProperty("enum", out _));
            Assert.False(properties.GetProperty("startTimeUtc").TryGetProperty("format", out _));
            Assert.DoesNotContain("message", properties.GetProperty("failureReason").GetRawText());
        }
        if (readOnly)
        {
            Assert.Equal(["jobs_get-item-job-instance", ToolName],
                catalog.Tools.Where(tool => tool.Name.StartsWith("jobs_", StringComparison.Ordinal)).Select(tool => tool.Name).Order());
        }
        if (route != "all")
        {
            var commands = await LearnAsync(provider, route);
            var listing = Assert.Single(commands, command => command.GetProperty("command").GetString() == ToolName);
            AssertInputSchema(listing.GetProperty("inputSchema"));
            Assert.Contains(commands, command => command.GetProperty("command").GetString() == "jobs_get-item-job-instance");
            Assert.Equal(!readOnly, commands.Any(command => command.GetProperty("command").GetString() == "jobs_cancel-item-job-instance"));
            Assert.Equal(!readOnly, commands.Any(command => command.GetProperty("command").GetString() == "jobs_run-on-demand-item-job"));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = DecliningConsentServer();

        var first = await CallAsync(provider, route, Arguments(), server);

        Assert.False(first.IsError, Text(first));
        var result = Payload(first, route);
        var job = Assert.Single(result.GetProperty("jobInstances").EnumerateArray());
        Assert.Equal(JobInstanceId, job.GetProperty("id").GetString());
        Assert.Equal("Failed", job.GetProperty("status").GetString());
        Assert.Equal("JobFailed", job.GetProperty("failureReason").GetProperty("errorCode").GetString());
        Assert.False(job.GetProperty("failureReason").GetProperty("isRetriable").GetBoolean());
        Assert.Equal(DocumentedToken, result.GetProperty("continuationToken").GetString());
        Assert.Equal(0, result.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(result.TryGetProperty("continuationUri", out _));
        Assert.DoesNotContain("private", result.GetRawText());
        Assert.DoesNotContain("private", Text(first));
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
        next["continuation-token"] = result.GetProperty("continuationToken").GetString();
        var second = await CallAsync(provider, route, next, server);

        Assert.False(second.IsError, Text(second));
        Assert.Empty(Payload(second, route).GetProperty("jobInstances").EnumerateArray());
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
    public static async Task List_AcceptsEveryAbsentOrNullTerminalCombinationWithoutConsent(string route, string transport)
    {
        foreach (var nullToken in new[] { false, true })
        {
            foreach (var nullUri in new[] { false, true })
            {
                foreach (var empty in new[] { false, true })
                {
                    var page = JsonNode.Parse(Page())!.AsObject();
                    if (nullToken)
                    {
                        page["continuationToken"] = null;
                    }
                    if (nullUri)
                    {
                        page["continuationUri"] = null;
                    }
                    if (empty)
                    {
                        page["value"] = new JsonArray();
                    }
                    using var handler = new FabricCoreHttpMessageHandler((request, _) =>
                    {
                        Assert.Equal(HttpMethod.Get, request.Method);
                        Assert.Equal(CollectionUrl, request.RequestUri?.AbsoluteUri);
                        return Task.FromResult(Response(page.ToJsonString()));
                    });
                    var credential = Credential();
                    await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, readOnly: true);
                    var server = DecliningConsentServer();

                    var result = await CallAsync(provider, route, Arguments(), server);

                    Assert.False(result.IsError, Text(result));
                    var payload = Payload(result, route);
                    var instances = payload.GetProperty("jobInstances").EnumerateArray().ToArray();
                    Assert.Equal(empty ? 0 : 1, instances.Length);
                    if (!empty)
                    {
                        Assert.Equal(JobInstanceId, instances[0].GetProperty("id").GetString());
                    }
                    Assert.False(payload.TryGetProperty("continuationToken", out _));
                    Assert.False(payload.TryGetProperty("continuationUri", out _));
                    Assert.Equal(1, handler.CallCount);
                    await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
                    await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
                }
            }
        }
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task List_UsesTokenWithAbsentOrNullUriForExplicitContinuation(string route, string transport)
    {
        foreach (var nullUri in new[] { false, true })
        {
            foreach (var empty in new[] { false, true })
            {
                var page = JsonNode.Parse(Page(DocumentedToken))!.AsObject();
                if (nullUri)
                {
                    page["continuationUri"] = null;
                }
                if (empty)
                {
                    page["value"] = new JsonArray();
                }
                var requests = new List<string>();
                using var handler = new FabricCoreHttpMessageHandler((request, _) =>
                {
                    Assert.Equal(HttpMethod.Get, request.Method);
                    requests.Add(request.RequestUri!.AbsoluteUri);
                    return Task.FromResult(Response(requests.Count == 1
                        ? page.ToJsonString()
                        : """{"value":[],"continuationToken":null,"continuationUri":null}"""));
                });
                await using var provider = Services(handler, Credential(), transport, StructuredOutputMode.Compact, readOnly: true);
                var server = DecliningConsentServer();

                var first = await CallAsync(provider, route, Arguments(), server);

                Assert.False(first.IsError, Text(first));
                var payload = Payload(first, route);
                Assert.Equal(empty ? 0 : 1, payload.GetProperty("jobInstances").GetArrayLength());
                Assert.Equal(DocumentedToken, payload.GetProperty("continuationToken").GetString());
                Assert.False(payload.TryGetProperty("continuationUri", out _));
                Assert.Equal([CollectionUrl], requests);
                var next = Arguments();
                next["continuation-token"] = payload.GetProperty("continuationToken").GetString();

                var second = await CallAsync(provider, route, next, server);

                Assert.False(second.IsError, Text(second));
                Assert.Empty(Payload(second, route).GetProperty("jobInstances").EnumerateArray());
                Assert.False(Payload(second, route).TryGetProperty("continuationToken", out _));
                Assert.Equal([CollectionUrl, $"{CollectionUrl}?continuationToken={DocumentedToken}"], requests);
                Assert.Equal(2, handler.CallCount);
                await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
            }
        }
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task List_CoreOnlyCannotDiscoverLearnOrInvokeJobs(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "core");
        var catalog = await Loader(provider, route).ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name.StartsWith("jobs", StringComparison.Ordinal));
        if (route == "single")
        {
            var root = await LearnAsync(provider, route, area: null);
            Assert.DoesNotContain(root, tool => tool.GetProperty("tool").GetString() == "jobs");
        }
        if (route != "all")
        {
            var learned = await LearnAsync(provider, route, "core");
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
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task List_AllowsAnExplicitGetHandoffButDoesNotAuthorizeCancellation(string route, string transport)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            var url = request.RequestUri!.AbsoluteUri;
            requests.Add(url);
            return Task.FromResult(Response(url == CollectionUrl ? Page() : Json));
        });
        await using var provider = Services(handler, Credential(), transport, StructuredOutputMode.Compact);
        var server = DecliningConsentServer();

        var page = await CallAsync(provider, route, Arguments(), server);
        Assert.False(page.IsError, Text(page));
        Assert.Equal([CollectionUrl], requests);
        var chosen = Payload(page, route).GetProperty("jobInstances").EnumerateArray()
            .Single(instance => instance.GetProperty("id").GetString() == JobInstanceId);
        var getArguments = Arguments();
        getArguments["item-id"] = chosen.GetProperty("itemId").GetString();
        getArguments["job-instance-id"] = chosen.GetProperty("id").GetString();
        var snapshot = await CallAsync(provider, route, getArguments, server, "jobs_get-item-job-instance");

        Assert.False(snapshot.IsError, Text(snapshot));
        Assert.Equal(JobInstanceId, Payload(snapshot, route).GetProperty("jobInstance").GetProperty("id").GetString());
        Assert.Equal([CollectionUrl, CollectionUrl + "/" + JobInstanceId], requests);
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());

        var cancellation = await CallAsync(provider, route, getArguments, server, "jobs_cancel-item-job-instance");

        Assert.True(cancellation.IsError, Text(cancellation));
        Assert.Equal(2, handler.CallCount);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("workspace-id")]
    [InlineData("item-id")]
    public static async Task List_RejectsMissingInvalidAndNonStringIdsBeforeAuthentication(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var value in new object?[] { null, "", " ", Guid.Empty.ToString(), "private-invalid", ItemId + "/jobs", 12, true })
        {
            var arguments = Arguments();
            if (value is null)
            {
                arguments.Remove(option);
            }
            else
            {
                arguments[option] = value;
            }
            AssertSafeFailure(await CallAsync(provider, "all", arguments, DecliningConsentServer()), HttpStatusCode.BadRequest);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("job-type")]
    [InlineData("status")]
    [InlineData("limit")]
    [InlineData("page-size")]
    [InlineData("continuation-uri")]
    [InlineData("url")]
    [InlineData("job-instance-id")]
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
    public static async Task List_RejectsMalformedRequiredUnicodeAndKeepsHttpErrorRetryGuidance(string route)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
        {
            foreach (var json in new[]
            {
                """{"value":[],"continuationToken":"\uD800"}""",
                """{"value":[],"extra\uDC00":"private"}""",
                """{"value":[],"continuationUri":"https://example.invalid/private"}""",
                """{"value":[],"continuationToken":null,"continuationUri":"https://example.invalid/private"}""",
                """{"value":[],"continuationToken":false,"continuationUri":null}""",
                """{"value":[],"continuationToken":null,"continuationUri":0}""",
                """{"value":[],"continuationToken":null,"continuationUri":null,"value":[{}]}""",
                """{"value":[],"value":[{}]}"""
            })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                    Task.FromResult(Response(json, status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);

                var result = await CallAsync(provider, route, Arguments(), DecliningConsentServer());

                AssertSafeFailure(result, status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status);
                Assert.Contains(status == HttpStatusCode.OK ? "No partial page" : "Wait at least 60 seconds", Text(result));
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Fact]
    public static async Task List_RegisteredCliRejectsRepeatedOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "jobs", "list-item-job-instances", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--continuation-token", "first" };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<ItemJobInstanceListCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--item-id", "--continuation-token" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["core", "list-item-job-instances"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["item-id", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        Assert.Equal(["continuation-token", "item-id", "workspace-id"],
            schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal("string", schema.GetProperty("properties").GetProperty("continuation-token").GetProperty("type").GetString());
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
        string selection = "jobs", bool readOnly = false)
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
            Description = "Offline job-instance listing tests",
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

    private static async Task<JsonElement[]> LearnAsync(IServiceProvider provider, string route, string? area = "jobs")
    {
        var arguments = new Dictionary<string, object?> { ["intent"] = "", ["learn"] = true };
        if (route == "single" && area is not null)
        {
            arguments["tool"] = area;
        }
        var result = await Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "single" ? "fabric" : area!, arguments), TestContext.Current.CancellationToken);
        Assert.NotEqual(true, result.IsError);
        if (result.StructuredContent is { } content)
        {
            return content.GetProperty("tools").EnumerateArray().ToArray();
        }
        // Legacy learn output prefixes the JSON array with routing instructions.
        var text = Text(result);
        var start = text.IndexOf('[');
        Assert.True(start >= 0, text);
        using var document = JsonDocument.Parse(text[start..]);
        return document.RootElement.EnumerateArray().Select(value => value.Clone()).ToArray();
    }

    private static Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server, string toolName = ToolName)
    {
        var name = toolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Read one page of jobs or explicitly read a selected job", ["command"] = toolName, ["parameters"] = arguments };
            name = route == "single" ? "fabric" : "jobs";
            if (route == "single")
            {
                arguments["tool"] = "jobs";
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
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId };

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
