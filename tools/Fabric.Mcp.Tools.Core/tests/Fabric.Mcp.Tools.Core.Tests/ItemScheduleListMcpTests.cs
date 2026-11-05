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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemScheduleListMcpTests()
{
    private const string ToolName = "jobs_list-item-schedules";

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
    public static async Task List_ExposesOnePageAcrossRoutesFiltersTransportsAndOutputsWithSensitiveReadConsent(
        string route, string transport, StructuredOutputMode? output, string selection, bool readOnly)
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(Response(requests.Count == 1 ? Page(DocumentedToken, true) : """{"value":[]}""", retryAfter: "0"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection, readOnly);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<ItemScheduleListCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemScheduleListCommand>());
        Assert.DoesNotContain("core_list-item-schedules", factory.AllCommands.Keys);
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
            Assert.Equal("schedules", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["continuationToken", "retryAfterSeconds", "retryAfterUtc", "schedules"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var schedule = schema.GetProperty("properties").GetProperty("schedules").GetProperty("items");
            Assert.Equal(["enabled", "id"], schedule.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.Equal(["configuration", "createdDateTime", "enabled", "executionData", "id", "owner"],
                schedule.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.DoesNotContain("\"enum\"", schedule.GetRawText());
            Assert.False(schedule.GetProperty("properties").GetProperty("createdDateTime").TryGetProperty("format", out _));
        }
        if (readOnly)
        {
            Assert.Equal(["jobs_get-item-job-instance", "jobs_get-item-schedule", "jobs_list-item-job-instances", ToolName],
                catalog.Tools.Where(tool => tool.Name.StartsWith("jobs_", StringComparison.Ordinal)).Select(tool => tool.Name).Order());
        }
        if (route != "all")
        {
            var commands = await LearnAsync(provider, route);
            var listing = Assert.Single(commands, command => command.GetProperty("command").GetString() == ToolName);
            AssertInputSchema(listing.GetProperty("inputSchema"));
            Assert.Contains(commands, command => command.GetProperty("command").GetString() == "jobs_get-item-job-instance");
            Assert.Contains(commands, command => command.GetProperty("command").GetString() == "jobs_list-item-job-instances");
            Assert.Equal(!readOnly, commands.Any(command => command.GetProperty("command").GetString() == "jobs_cancel-item-job-instance"));
            Assert.Equal(!readOnly, commands.Any(command => command.GetProperty("command").GetString() == "jobs_run-on-demand-item-job"));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = ConsentServer();

        var first = await CallAsync(provider, route, Arguments(), server);

        Assert.False(first.IsError, Text(first));
        var result = Payload(first, route);
        var entry = Assert.Single(result.GetProperty("schedules").EnumerateArray());
        using var expected = JsonDocument.Parse(ScheduleJson);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, entry));
        Assert.Equal(DocumentedToken, result.GetProperty("continuationToken").GetString());
        Assert.Equal(0, result.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(result.TryGetProperty("continuationUri", out _));
        Assert.Equal([CollectionUrl()], requests);
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
        Assert.Empty(Payload(second, route).GetProperty("schedules").EnumerateArray());
        Assert.False(Payload(second, route).TryGetProperty("continuationToken", out _));
        Assert.Equal([CollectionUrl(), $"{CollectionUrl()}?continuationToken={DocumentedToken}"], requests);
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        await server.Received(2).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task List_AcceptsTerminalNullCombinationsAndTokenOnlyContinuation(string route, string transport)
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
                    using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(page.ToJsonString())));
                    await using var provider = Services(handler, Credential(), transport, StructuredOutputMode.Compact, readOnly: true);
                    var server = ConsentServer();
                    var final = await CallAsync(provider, route, Arguments(), server);
                    Assert.False(final.IsError, Text(final));
                    Assert.Equal(empty ? 0 : 1, Payload(final, route).GetProperty("schedules").GetArrayLength());
                    Assert.False(Payload(final, route).TryGetProperty("continuationToken", out _));
                    Assert.Equal(1, handler.CallCount);

                    page["continuationToken"] = DocumentedToken;
                    var first = await CallAsync(provider, route, Arguments(), server);
                    Assert.False(first.IsError, Text(first));
                    Assert.Equal(DocumentedToken, Payload(first, route).GetProperty("continuationToken").GetString());
                    Assert.Equal(2, handler.CallCount);
                    await server.Received(2).SendRequestAsync(
                        Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
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
    public static async Task List_CoreOnlyCannotDiscoverLearnOrInvokeSchedules(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "core");
        var catalog = await Loader(provider, route).ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name.StartsWith("jobs", StringComparison.Ordinal));
        if (route != "all")
        {
            var commands = await LearnAsync(provider, route, "core");
            Assert.DoesNotContain(commands, command => command.GetProperty("command").GetString() == ToolName);
        }
        var blocked = await CallAsync(provider, route, Arguments(), ConsentServer());
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
    [InlineData("item-id")]
    [InlineData("job-type")]
    public static async Task List_RejectsMissingOrInvalidRequiredInputBeforeAuthentication(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var value in new[] { null, "", " ", "private\r\n", ".", ".." })
        {
            var args = Arguments();
            if (value is null)
            {
                args.Remove(option);
            }
            else
            {
                args[option] = value;
            }
            AssertSafeFailure(await CallAsync(provider, "all", args, ConsentServer()), HttpStatusCode.BadRequest);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("schedule-id")]
    [InlineData("enabled")]
    [InlineData("status")]
    [InlineData("limit")]
    [InlineData("page-size")]
    [InlineData("continuation-uri")]
    [InlineData("url")]
    [InlineData("job-instance-id")]
    public static async Task List_RejectsUnknownOptionsWithoutEchoingTheirValues(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        var args = Arguments();
        args[option] = "private-input";
        var result = await CallAsync(provider, "all", args, ConsentServer());
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
    public static async Task List_SafelyFailsRequiredJsonButPreservesHttpErrorStatusAndRetry(string route)
    {
        foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
        {
            foreach (var json in new[]
            {
                """{"value":[],"continuationToken":"\uD800"}""",
                """{"value":[],"extra\uDC00":"private"}""",
                """{"value":[],"continuationToken":null,"continuationUri":"https://example.invalid/private"}""",
                """{"value":[],"continuationToken":false,"continuationUri":null}""",
                """{"value":[],"continuationToken":null,"continuationUri":0}""",
                """{"value":[],"continuationToken":null,"continuationUri":null,"value":[{}]}"""
            })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(json, status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
                var result = await CallAsync(provider, route, Arguments(), ConsentServer());
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
        var args = new[] { "jobs", "list-item-schedules", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--continuation-token", "first" };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<ItemScheduleListCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--item-id", "--job-type", "--continuation-token" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["core", "list-item-schedules"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task List_DeclinedSensitiveReadConsentSendsNoRequestEvenInReadOnlyMode(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, readOnly: true);
        var server = ConsentServer(accept: false);
        var result = await CallAsync(provider, route, Arguments(), server);
        Assert.True(result.IsError, Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["item-id", "job-type", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        Assert.Equal(["continuation-token", "item-id", "job-type", "workspace-id"],
            schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.All(schema.GetProperty("properties").EnumerateObject(),
            property => Assert.Equal("string", property.Value.GetProperty("type").GetString()));
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
            Description = "Offline schedule listing tests",
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

    private static async Task<JsonElement[]> LearnAsync(IServiceProvider provider, string route, string area = "jobs")
    {
        var args = new Dictionary<string, object?> { ["intent"] = "", ["learn"] = true };
        if (route == "single")
        {
            args["tool"] = area;
        }
        var result = await Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "single" ? "fabric" : area, args), TestContext.Current.CancellationToken);
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
        IServiceProvider provider, string route, Dictionary<string, object?> args, McpServer server)
    {
        var name = ToolName;
        if (route != "all")
        {
            args = new() { ["intent"] = "Read one explicit schedule page", ["command"] = ToolName, ["parameters"] = args };
            name = route == "single" ? "fabric" : "jobs";
            if (route == "single")
            {
                args["tool"] = "jobs";
            }
        }
        var request = McpTestUtilities.CreateToolCallRequest(name, args);
        return Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(request.Params!, server), TestContext.Current.CancellationToken).AsTask();
    }

    private static McpServer ConsentServer(bool accept = true)
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()).Returns(call => new JsonRpcResponse
        {
            Id = call.Arg<JsonRpcRequest>().Id,
            Result = new JsonObject { ["action"] = accept ? "accept" : "decline", ["content"] = new JsonObject { ["decision"] = "accept" } }
        });
        return server;
    }

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-type"] = JobType };

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
