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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemScheduleCreateMcpTests()
{
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
                            foreach (var enabled in new[] { false, true })
                            {
                                data.Add(route, transport, output, selection, enabled);
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
    public static async Task Create_RegistersSchemaAndPreservesOnePostAcrossRoutesTransportsAndOutputs(
        string route, string transport, StructuredOutputMode? output, string selection, bool enabled)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(CollectionUrl(), request.RequestUri?.AbsoluteUri);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal(enabled, body.RootElement.GetProperty("enabled").GetBoolean());
            using var config = JsonDocument.Parse(Configuration);
            using var execution = JsonDocument.Parse(ExecutionData);
            Assert.True(JsonElement.DeepEquals(config.RootElement, body.RootElement.GetProperty("configuration")));
            Assert.True(JsonElement.DeepEquals(execution.RootElement, body.RootElement.GetProperty("executionData")));
            return Response(ScheduleJson, HttpStatusCode.Created);
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<ItemScheduleCreateCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemScheduleCreateCommand>());
        Assert.DoesNotContain("core_create-item-schedule", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.True(factory.AllCommands[ToolName].Command.Metadata.Secret);
        Assert.Contains("Before requesting consent", tool.Description);
        Assert.Contains("past start time may trigger a job immediately", tool.Description);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["creationStatus", "guidance", "itemId", "jobType", "retryAfterSeconds", "retryAfterUtc", "schedule", "workspaceId"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        if (route != "all")
        {
            var commands = await LearnAsync(provider, route);
            AssertInputSchema(Assert.Single(commands, command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = Consent();
        var arguments = Arguments(enabled);
        arguments["execution-data"] = ExecutionData;

        var result = await CallAsync(provider, route, arguments, server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal("Created", payload.GetProperty("creationStatus").GetString());
        Assert.Equal(WorkspaceId, payload.GetProperty("workspaceId").GetString());
        Assert.Equal(ItemId, payload.GetProperty("itemId").GetString());
        Assert.Equal(JobType, payload.GetProperty("jobType").GetString());
        Assert.Equal(ScheduleId, payload.GetProperty("schedule").GetProperty("id").GetString());
        Assert.False(payload.TryGetProperty("jobInstanceId", out _));
        Assert.False(payload.TryGetProperty("operationId", out _));
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(result));
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain("owner@example.invalid", Text(result));
        }
        if (output is null || route == "single")
        {
            Assert.Null(result.StructuredContent);
        }
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Create_ReadOnlyAndCoreOnlyCannotDiscoverLearnOrInvoke(string route, string transport)
    {
        foreach (var selection in new[] { "read-only", "core" })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
            var credential = Credential();
            await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact,
                selection == "core" ? "core" : "jobs", readOnly: selection == "read-only");
            var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
                McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
            Assert.DoesNotContain(catalog.Tools, tool => tool.Name == ToolName);
            if (selection == "read-only")
            {
                Assert.Equal(["jobs_get-item-job-instance", "jobs_list-item-job-instances", "jobs_list-item-schedules"],
                    catalog.Tools.Select(tool => tool.Name).Order());
            }
            if (route != "all")
            {
                var commands = await LearnAsync(provider, route, selection == "core" ? "core" : "jobs");
                Assert.DoesNotContain(commands, command => command.GetProperty("command").GetString() == ToolName);
            }
            var server = Consent();
            var result = await CallAsync(provider, route, Arguments(false), server);
            if (route == "single")
            {
                Assert.Equal("tool-list", result.StructuredContent!.Value.GetProperty("kind").GetString());
                Assert.DoesNotContain(ToolName, result.StructuredContent.Value.GetRawText());
            }
            else
            {
                Assert.True(result.IsError, Text(result));
            }
            Assert.Equal(0, handler.CallCount);
            Assert.Empty(credential.ReceivedCalls());
            await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_InvalidOrMissingArgumentsNeverAuthenticate(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var (name, value) in new (string, object?)[]
        {
            ("workspace-id", "private-invalid"), ("item-id", Guid.Empty.ToString()), ("job-type", ".."),
            ("configuration", "{}"), ("configuration", """{"private":"\uD800"}"""),
            ("execution-data", "[]"), ("execution-data", """{"x":1,"x":2}"""),
            ("enabled", "private-invalid"), ("enabled", ""), ("enabled", 1), ("enabled", null),
            ("schedule-id", "private"), ("sync", "private"), ("job-parameters", "private")
        })
        {
            var arguments = Arguments(false);
            arguments[name] = value;
            var result = await CallAsync(provider, route, arguments, Consent());
            Assert.True(result.IsError, Text(result));
            Assert.DoesNotContain("private", Text(result));
        }
        foreach (var name in new[] { "workspace-id", "item-id", "job-type", "enabled", "configuration" })
        {
            var arguments = Arguments(false);
            arguments.Remove(name);
            var result = await CallAsync(provider, route, arguments, Consent());
            Assert.True(result.IsError, Text(result));
        }
        var repeated = Arguments(false);
        repeated["ENABLED"] = true;
        Assert.True((await CallAsync(provider, route, repeated, Consent())).IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_DeclinedCanceledRejectedAndUnsupportedConsentNeverAuthenticate(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var (action, decision) in new[] { ("decline", "accept"), ("cancel", "accept"), ("accept", "reject"), ("accept", "") })
        {
            Assert.True((await CallAsync(provider, route, Arguments(false), Consent(action, decision))).IsError);
        }
        Assert.True((await CallAsync(provider, route, Arguments(false), Substitute.For<McpServer>())).IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_ReportsConfirmedCreationWhenItsBodyIsUnusableAndPreservesSafeHttpFailures(string route)
    {
        foreach (var status in new[] { HttpStatusCode.Created, HttpStatusCode.TooManyRequests })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                Task.FromResult(Response("""{"errorCode":"\uD800","message":"private"}""", status, "60")));
            await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
            var result = await CallAsync(provider, route, Arguments(false), Consent());
            Assert.True(result.IsError, Text(result));
            Assert.Contains(status == HttpStatusCode.Created ? "confirmed schedule creation (HTTP 201)" : "Fabric throttled", Text(result));
            Assert.Contains("Wait at least 60 seconds", Text(result));
            Assert.DoesNotContain("private", Text(result));
            Assert.DoesNotContain("stackTrace", Text(result));
            Assert.Equal(1, handler.CallCount);
        }
    }

    [Fact]
    public static async Task Create_RegisteredCliRequiresExplicitEnabledAndRejectsEveryRepeatedOption()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(ScheduleJson, HttpStatusCode.Created)));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, null);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var required = new[] { "jobs", "create-item-schedule", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--configuration", Configuration };
        foreach (var flag in new string[][] { [], ["--enabled"], ["--enabled", ""], ["--enabled", "yes"], ["--enabled", "true", "false"] })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. required, .. flag]).Errors);
        }
        foreach (var option in new[] { "--workspace-id", "--item-id", "--job-type", "--configuration", "--enabled", "--execution-data" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. required, "--enabled", "false", "--execution-data", "{}", option, "private"]).Errors);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        foreach (var flag in new[] { "false", "true" })
        {
            var parsed = factory.RootCommand.Parse([.. required, "--enabled", flag]);
            Assert.Empty(parsed.Errors);
            Assert.Equal((int)HttpStatusCode.Created, await parsed.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken));
        }
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Create_DeepRetainedResponsePreservesConfirmedCreationGuidance(string route)
    {
        foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
        {
            foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
            {
                foreach (var field in new[] { "configuration", "owner", "executionData" })
                {
                    foreach (var depth in new[] { 60, 61, 64 })
                    {
                        var body = DeepSchedule(depth, field);
                        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                            Task.FromResult(Response(body, HttpStatusCode.Created, "60")));
                        var credential = Credential();
                        await using var provider = Services(handler, credential, transport, output);
                        var result = await CallAsync(provider, route, Arguments(false), Consent());
                        if (depth == 60)
                        {
                            Assert.False(result.IsError, Text(result));
                            using var expected = JsonDocument.Parse(body);
                            Assert.True(JsonElement.DeepEquals(expected.RootElement, Payload(result, route).GetProperty("schedule")));
                        }
                        else
                        {
                            Assert.True(result.IsError, Text(result));
                            Assert.Contains("confirmed schedule creation (HTTP 201)", Text(result));
                            Assert.Contains("Do not resubmit", Text(result));
                            Assert.Contains("Wait at least 60 seconds", Text(result));
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
                    }
                }
            }
        }
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["configuration", "enabled", "item-id", "job-type", "workspace-id"],
            schema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
        var properties = schema.GetProperty("properties");
        Assert.Equal(["configuration", "enabled", "execution-data", "item-id", "job-type", "workspace-id"],
            properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal("boolean", properties.GetProperty("enabled").GetProperty("type").GetString());
        Assert.False(properties.GetProperty("enabled").TryGetProperty("default", out _));
        Assert.Equal("string", properties.GetProperty("configuration").GetProperty("type").GetString());
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
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
            Description = "Offline schedule creation tests",
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
            Transport = transport,
            StructuredOutputMode = output,
            ReadOnly = readOnly
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddCommandContextAccessor();
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

    private static Task<CallToolResult> CallAsync(IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server)
    {
        var name = ToolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Create one authorized schedule with explicit enabled and timing intent", ["command"] = ToolName, ["parameters"] = arguments };
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

    private static Dictionary<string, object?> Arguments(bool enabled) =>
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-type"] = JobType, ["enabled"] = enabled, ["configuration"] = Configuration };

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
