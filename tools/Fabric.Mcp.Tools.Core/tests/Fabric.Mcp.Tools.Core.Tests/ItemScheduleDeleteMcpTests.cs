// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemScheduleListTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemScheduleDeleteMcpTests()
{
    private const string ToolName = "jobs_delete-item-schedule";

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
                        foreach (var selection in new[] { "jobs", "combined", "default" })
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
    public static async Task Delete_ExposesOneConsentedMutationWithoutBodyAcrossRoutesTransportsOutputsAndSelections(
        string route, string transport, StructuredOutputMode? output, string selection)
    {
        using var content = new UnreadableHttpContent();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"{CollectionUrl()}/{ScheduleId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<ItemScheduleDeleteCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemScheduleDeleteCommand>());
        Assert.DoesNotContain("core_delete-item-schedule", factory.AllCommands.Keys);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.False(factory.AllCommands[ToolName].Command.Metadata.Secret);
        Assert.Contains("does not cancel already-running jobs", tool.Description);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["deletionStatus", "guidance", "itemId", "jobType", "scheduleId", "workspaceId"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        if (route != "all")
        {
            AssertInputSchema(Assert.Single(await LearnAsync(provider, route),
                command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = Consent();

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal("Deleted", payload.GetProperty("deletionStatus").GetString());
        Assert.Equal(WorkspaceId, payload.GetProperty("workspaceId").GetString());
        Assert.Equal(ItemId, payload.GetProperty("itemId").GetString());
        Assert.Equal(JobType, payload.GetProperty("jobType").GetString());
        Assert.Equal(ScheduleId, payload.GetProperty("scheduleId").GetString());
        Assert.False(payload.TryGetProperty("schedule", out _));
        Assert.False(payload.TryGetProperty("jobInstanceId", out _));
        Assert.False(payload.TryGetProperty("operationId", out _));
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(result));
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain(ScheduleId, Text(result));
        }
        if (output is null || route == "single")
        {
            Assert.Null(result.StructuredContent);
        }
        Assert.True(content.IsDisposed);
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
    public static async Task Delete_ReadOnlyAndCoreOnlyCannotDiscoverLearnOrInvoke(string route, string transport)
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
                Assert.Equal(["jobs_get-item-job-instance", "jobs_get-item-schedule", "jobs_list-item-job-instances", "jobs_list-item-schedules"],
                    catalog.Tools.Select(tool => tool.Name).Order());
            }
            if (route != "all")
            {
                Assert.DoesNotContain(await LearnAsync(provider, route, selection == "core" ? "core" : "jobs"),
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
            Assert.Equal(0, handler.CallCount);
            Assert.Empty(credential.ReceivedCalls());
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
    public static async Task Delete_RegisteredCliRejectsRepeatedOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var arguments = new[] { "jobs", "delete-item-schedule", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType, "--schedule-id", ScheduleId };
        var parsed = factory.RootCommand.Parse(arguments);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<ItemScheduleDeleteCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--item-id", "--job-type", "--schedule-id" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. arguments, option, "private"]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["core", "delete-item-schedule"]).Errors);
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
    public static async Task Delete_DeclinedCanceledRejectedAndUnsupportedConsentNeverAuthenticates(string route, string transport)
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

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_HttpFailureAndUncertainSendNeverBecomeAReceiptAcrossOutputs(string route)
    {
        foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
        {
            foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
            {
                foreach (var status in new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.TooManyRequests, HttpStatusCode.ServiceUnavailable })
                {
                    using var handler = new FabricCoreHttpMessageHandler((_, _) => status == HttpStatusCode.ServiceUnavailable
                        ? throw new HttpRequestException("private")
                        : Task.FromResult(Response("""{"errorCode":"\uD800","message":"private"}""", status, "60")));
                    await using var provider = Services(handler, Credential(), transport, output);
                    var result = await CallAsync(provider, route, Arguments(), Consent());
                    Assert.True(result.IsError, Text(result));
                    Assert.Null(result.StructuredContent);
                    using var document = JsonDocument.Parse(Text(result));
                    Assert.Equal((int)(status == HttpStatusCode.NoContent ? HttpStatusCode.BadGateway : status),
                        document.RootElement.GetProperty("status").GetInt32());
                    Assert.False(document.RootElement.TryGetProperty("results", out _));
                    Assert.DoesNotContain("deletionStatus", Text(result));
                    Assert.DoesNotContain("private", Text(result));
                    Assert.DoesNotContain("stackTrace", Text(result));
                    Assert.Contains(status == HttpStatusCode.ServiceUnavailable ? "outcome may be uncertain" : "Wait at least 60 seconds", Text(result));
                    Assert.Equal(1, handler.CallCount);
                }
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Delete_LateCancellationStillReturnsConfirmedReceipt(string route)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnreadableHttpContent() });
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent(), cancellation.Token);
        Assert.False(result.IsError, Text(result));
        Assert.Equal("Deleted", Payload(result, route).GetProperty("deletionStatus").GetString());
        Assert.Equal(1, handler.CallCount);
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
            Description = "Offline schedule deletion tests",
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
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server, CancellationToken? token = null)
    {
        var name = ToolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Delete one explicitly authorized schedule", ["command"] = ToolName, ["parameters"] = arguments };
            name = route == "single" ? "fabric" : "jobs";
            if (route == "single")
            {
                arguments["tool"] = "jobs";
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
