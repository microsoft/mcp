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

public sealed class ItemScheduleGetMcpTests()
{
    private const string ToolName = "jobs_get-item-schedule";

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
    public static async Task Get_ExposesOneSensitiveSnapshotAcrossRoutesFiltersTransportsAndOutputs(
        string route, string transport, StructuredOutputMode? output, string selection, bool readOnly)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{CollectionUrl()}/{ScheduleId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            return Task.FromResult(Response(ScheduleJson, retryAfter: "0"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection, readOnly);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<ItemScheduleGetCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemScheduleGetCommand>());
        Assert.DoesNotContain("core_get-item-schedule", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.True(factory.AllCommands[ToolName].Command.Metadata.Secret);
        Assert.Contains("disclosure consent", tool.Description);
        Assert.Contains("not job execution status", tool.Description);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["itemId", "jobType", "retryAfterSeconds", "retryAfterUtc", "schedule", "workspaceId"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var schedule = schema.GetProperty("properties").GetProperty("schedule");
            Assert.Equal(["enabled", "id"], schedule.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.Equal(["configuration", "createdDateTime", "enabled", "executionData", "id", "owner"],
                schedule.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.DoesNotContain("\"enum\"", schedule.GetRawText());
            Assert.False(schedule.GetProperty("properties").GetProperty("createdDateTime").TryGetProperty("format", out _));
        }
        if (readOnly)
        {
            Assert.Equal(["jobs_get-item-job-instance", ToolName, "jobs_list-item-job-instances", "jobs_list-item-schedules"],
                catalog.Tools.Where(tool => tool.Name.StartsWith("jobs_", StringComparison.Ordinal)).Select(tool => tool.Name).Order());
        }
        if (route != "all")
        {
            var commands = await LearnAsync(provider, route);
            AssertInputSchema(Assert.Single(commands, command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
            foreach (var read in new[] { "jobs_get-item-job-instance", "jobs_list-item-job-instances", "jobs_list-item-schedules" })
            {
                Assert.Contains(commands, command => command.GetProperty("command").GetString() == read);
            }
            foreach (var mutation in new[] { "jobs_cancel-item-job-instance", "jobs_run-on-demand-item-job", "jobs_create-item-schedule", "jobs_update-item-schedule" })
            {
                Assert.Equal(!readOnly, commands.Any(command => command.GetProperty("command").GetString() == mutation));
            }
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = Consent();

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal(WorkspaceId, payload.GetProperty("workspaceId").GetString());
        Assert.Equal(ItemId, payload.GetProperty("itemId").GetString());
        Assert.Equal(JobType, payload.GetProperty("jobType").GetString());
        using var expected = JsonDocument.Parse(ScheduleJson);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, payload.GetProperty("schedule")));
        Assert.Equal(0, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(payload.TryGetProperty("jobInstanceId", out _));
        Assert.False(payload.TryGetProperty("operationId", out _));
        Assert.False(payload.TryGetProperty("creationStatus", out _));
        Assert.False(payload.TryGetProperty("updateStatus", out _));
        Assert.False(payload.TryGetProperty("continuationToken", out _));
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
    public static async Task Get_CoreOnlyCannotDiscoverLearnOrInvoke(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "core");
        var catalog = await Loader(provider, route).ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name.StartsWith("jobs", StringComparison.Ordinal));
        if (route != "all")
        {
            Assert.DoesNotContain(await LearnAsync(provider, route, "core"), command => command.GetProperty("command").GetString() == ToolName);
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
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_InvalidMissingRepeatedAndUnsupportedInputsNeverAuthenticate(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact, readOnly: true);
        foreach (var (name, value) in new (string, object?)[]
        {
            ("workspace-id", "private-invalid"), ("item-id", Guid.Empty.ToString()), ("job-type", ".."),
            ("schedule-id", "private-invalid"), ("schedule-id", Guid.Empty.ToString()), ("schedule-id", null),
            ("job-type", ""), ("job-type", "private\r\n"), ("job-type", new string('x', 1025)),
            ("enabled", true), ("configuration", "{}"), ("execution-data", "private"),
            ("continuation-token", "private"), ("job-instance-id", "private"), ("url", "private"), ("sync", true)
        })
        {
            var arguments = Arguments();
            arguments[name] = value;
            var result = await CallAsync(provider, route, arguments, Consent());
            Assert.True(result.IsError, Text(result));
            Assert.DoesNotContain("private", Text(result));
        }
        foreach (var name in new[] { "workspace-id", "item-id", "job-type", "schedule-id" })
        {
            var arguments = Arguments();
            arguments.Remove(name);
            Assert.True((await CallAsync(provider, route, arguments, Consent())).IsError);
        }
        var repeated = Arguments();
        repeated["SCHEDULE-ID"] = JobInstanceId;
        Assert.True((await CallAsync(provider, route, repeated, Consent())).IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public static async Task Get_RegisteredCliRejectsRepeatedOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var arguments = new[] { "jobs", "get-item-schedule", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--schedule-id", ScheduleId };
        var parsed = factory.RootCommand.Parse(arguments);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<ItemScheduleGetCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--item-id", "--job-type", "--schedule-id" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. arguments, option, "private"]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["core", "get-item-schedule"]).Errors);
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
    public static async Task Get_DeniedCanceledRejectedAndUnsupportedConsentSendsNothingInReadOnlyMode(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, readOnly: true);
        foreach (var (action, decision) in new[] { ("decline", "accept"), ("cancel", "accept"), ("accept", "reject"), ("accept", "") })
        {
            var server = Consent(action, decision);
            Assert.True((await CallAsync(provider, route, Arguments(), server)).IsError);
            await server.Received(1).SendRequestAsync(
                Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
        }
        Assert.True((await CallAsync(provider, route, Arguments(), Substitute.For<McpServer>())).IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Get_UnusableSnapshotsAndHttpFailuresKeepSafeStatusAndRetryAcrossOutputs(string route)
    {
        foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
        {
            foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
            {
                foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.TooManyRequests })
                {
                    foreach (var body in new[] { """{"errorCode":"\uD800","message":"private"}""", ScheduleJson.Replace(ScheduleId, JobInstanceId, StringComparison.Ordinal) })
                    {
                        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(body, status, "60")));
                        await using var provider = Services(handler, Credential(), transport, output, readOnly: true);
                        var result = await CallAsync(provider, route, Arguments(), Consent());
                        AssertSafeFailure(result, status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status);
                        Assert.Contains("Wait at least 60 seconds", Text(result));
                        Assert.DoesNotContain(JobInstanceId, Text(result));
                        Assert.DoesNotContain("confirmed", Text(result));
                        Assert.Equal(1, handler.CallCount);
                    }
                }
            }
        }
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
                foreach (var field in new[] { "configuration", "owner", "executionData" })
                {
                    foreach (var depth in new[] { 60, 61, 64 })
                    {
                        var body = ItemScheduleCreateTestData.DeepSchedule(depth, field);
                        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(body, retryAfter: "60")));
                        await using var provider = Services(handler, Credential(), transport, output, readOnly: true);
                        var result = await CallAsync(provider, route, Arguments(), Consent());
                        if (depth == 60)
                        {
                            Assert.False(result.IsError, Text(result));
                            using var expected = JsonDocument.Parse(body);
                            Assert.True(JsonElement.DeepEquals(expected.RootElement, Payload(result, route).GetProperty("schedule")));
                        }
                        else
                        {
                            AssertSafeFailure(result, HttpStatusCode.BadGateway);
                            Assert.Contains("60 retained levels", Text(result));
                            Assert.Contains("Wait at least 60 seconds", Text(result));
                            Assert.DoesNotContain("CurrentDepth", Text(result));
                            Assert.DoesNotContain("confirmed", Text(result));
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
                    }
                }
            }
        }
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["item-id", "job-type", "schedule-id", "workspace-id"],
            schema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        Assert.Equal(["item-id", "job-type", "schedule-id", "workspace-id"],
            schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
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
            Description = "Offline schedule lookup tests",
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
            arguments = new() { ["intent"] = "Read one explicit schedule snapshot", ["command"] = ToolName, ["parameters"] = arguments };
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
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>()).Returns(call => new JsonRpcResponse
        {
            Id = call.Arg<JsonRpcRequest>().Id,
            Result = new JsonObject { ["action"] = action, ["content"] = new JsonObject { ["decision"] = decision } }
        });
        return server;
    }

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-type"] = JobType, ["schedule-id"] = ScheduleId };

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
