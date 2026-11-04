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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobRunTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemJobRunMcpTests()
{
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
    public static async Task Run_RegistersExactlyOneNewToolAndPreservesBodyAcrossRoutesAndOutputs(
        string route, string transport, StructuredOutputMode? output, string selection)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"/v1/workspaces/{WorkspaceId}/items/{ItemId}/jobs/{JobType}/instances", request.RequestUri?.AbsolutePath);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
            Assert.Equal("private-input", body.RootElement.GetProperty("executionData").GetProperty("opaque").GetString());
            using var parameters = JsonDocument.Parse(Parameters);
            Assert.True(JsonElement.DeepEquals(parameters.RootElement, body.RootElement.GetProperty("parameters")));
            return Accepted();
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Equal(["jobs_cancel-item-job-instance", "jobs_get-item-job-instance", "jobs_list-item-job-instances", "jobs_list-item-schedules", ToolName], factory.AllCommands.Keys.Where(key => key.StartsWith("jobs_", StringComparison.Ordinal)).Order());
        Assert.Same(provider.GetRequiredService<ItemJobRunCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemJobRunCommand>());
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(["item-id", "job-type", "workspace-id"],
            tool.InputSchema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
        Assert.Equal(["execution-data", "item-id", "job-parameters", "job-type", "workspace-id"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.False(tool.InputSchema.GetProperty("properties").TryGetProperty("parameters", out _));
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["guidance", "itemId", "jobInstanceId", "jobType", "retryAfterIssue", "retryAfterSeconds", "retryAfterUtc", "submissionStatus", "trackingIssue", "workspaceId"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = Consent();
        var arguments = Arguments();
        arguments["execution-data"] = """{"opaque":"private-input"}""";
        arguments["job-parameters"] = Parameters;

        var result = await CallAsync(provider, route, arguments, server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal("Accepted", payload.GetProperty("submissionStatus").GetString());
        Assert.Equal(WorkspaceId, payload.GetProperty("workspaceId").GetString());
        Assert.Equal(ItemId, payload.GetProperty("itemId").GetString());
        Assert.Equal(JobType, payload.GetProperty("jobType").GetString());
        Assert.Equal(JobInstanceId, payload.GetProperty("jobInstanceId").GetString());
        Assert.Equal(60, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(payload.TryGetProperty("status", out _));
        Assert.False(payload.TryGetProperty("operationId", out _));
        Assert.DoesNotContain("private", payload.GetRawText());
        Assert.DoesNotContain("private", Text(result));
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(result));
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain(JobInstanceId, Text(result));
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
    public static async Task Run_ReadOnlyHidesAndDeniesSubmissionButRetainsGet(string route, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            return Task.FromResult(Response());
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, readOnly: true);
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.Equal(["jobs_get-item-job-instance", "jobs_list-item-job-instances", "jobs_list-item-schedules"], catalog.Tools.Select(tool => tool.Name).Order());
        if (route != "all")
        {
            var learn = await LearnAsync(provider, route);
            Assert.Contains("jobs_get-item-job-instance", learn);
            Assert.Contains("jobs_list-item-job-instances", learn);
            Assert.Contains("jobs_list-item-schedules", learn);
            Assert.DoesNotContain(ToolName, learn);
        }
        var server = Consent();
        var blocked = await CallAsync(provider, route, Arguments(), server);
        AssertUnavailable(blocked, route);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());

        var get = await CallAsync(provider, route,
            new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-instance-id"] = JobInstanceId },
            server, "jobs_get-item-job-instance");
        Assert.False(get.IsError, Text(get));
        Assert.Equal(JobInstanceId, Payload(get, route).GetProperty("jobInstance").GetProperty("id").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Run_CoreOnlyCannotDiscoverOrInvokeJobs(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact, "core");
        var catalog = await Loader(provider, route).ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name.StartsWith("jobs", StringComparison.Ordinal));
        AssertUnavailable(await CallAsync(provider, route, Arguments(), Consent()), route);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Run_HandsItsTrustedIdToOneSeparateGetWithoutAnImplicitRead(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return Task.FromResult(Accepted(retryAfter: "0"));
            }
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(new Uri(Location).AbsolutePath, request.RequestUri?.AbsolutePath);
            return Task.FromResult(Response());
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);

        var run = await CallAsync(provider, route, Arguments(), Consent());
        var receipt = Payload(run, route);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(0, receipt.GetProperty("retryAfterSeconds").GetInt64());
        var get = await CallAsync(provider, route, new()
        {
            ["workspace-id"] = receipt.GetProperty("workspaceId").GetString(),
            ["item-id"] = receipt.GetProperty("itemId").GetString(),
            ["job-instance-id"] = receipt.GetProperty("jobInstanceId").GetString()
        }, Consent(), "jobs_get-item-job-instance");

        Assert.False(get.IsError, Text(get));
        Assert.Equal(JobInstanceId, Payload(get, route).GetProperty("jobInstance").GetProperty("id").GetString());
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Run_AcceptanceWithoutTrackingIsNotARetryableFailure(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Accepted("https://example.invalid/private", "private")));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent());
        Assert.False(result.IsError, Text(result));
        var receipt = Payload(result, route);
        Assert.Equal("Accepted", receipt.GetProperty("submissionStatus").GetString());
        Assert.False(receipt.TryGetProperty("jobInstanceId", out _));
        Assert.Equal("InvalidLocation", receipt.GetProperty("trackingIssue").GetString());
        Assert.Equal("MissingOrInvalidRetryAfter", receipt.GetProperty("retryAfterIssue").GetString());
        Assert.Contains("do not resubmit", receipt.GetProperty("guidance").GetString());
        Assert.DoesNotContain("private", Text(result) + receipt.GetRawText());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Run_RejectsBadContextJsonAndUnpublishedOptionsBeforeAuth(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var (name, value) in new (string, object?)[]
        {
            ("workspace-id", "private-invalid"), ("item-id", Guid.Empty.ToString()), ("job-type", ".."),
            ("execution-data", """{"private":1,"private":2}"""), ("execution-data", "[]"),
            ("execution-data", """{"value":"\uD800"}"""),
            ("job-parameters", """[{"name":"x","type":"Text","value":1},{"name":"X","type":"Text","value":2}]"""),
            ("job-parameters", """{"parameters":[]}"""), ("sync", "private-invalid"), ("parameters", "private-invalid")
        })
        {
            var arguments = Arguments();
            arguments[name] = value;
            var result = await CallAsync(provider, route, arguments, Consent());
            Assert.True(result.IsError, Text(result));
            Assert.DoesNotContain("private", Text(result));
            Assert.DoesNotContain("stackTrace", Text(result));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Run_RetainsSharedMcpNullAsOmittedOptionSemantics(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Null(request.Content);
            return Task.FromResult(Accepted());
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = Arguments();
        arguments["execution-data"] = null;
        arguments["job-parameters"] = null;
        var result = await CallAsync(provider, route, arguments, Consent());
        Assert.False(result.IsError, Text(result));
        Assert.Equal("Accepted", Payload(result, route).GetProperty("submissionStatus").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all", "decline")]
    [InlineData("namespace", "decline")]
    [InlineData("single", "decline")]
    [InlineData("all", "cancel")]
    [InlineData("namespace", "cancel")]
    [InlineData("single", "cancel")]
    public static async Task Run_DeclinedConsentStopsBeforeAuth(string route, string action)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent(action));
        Assert.True(result.IsError, Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public static async Task Run_RegisteredCliRejectsRepeatedOptions()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, null);
        var factory = provider.GetRequiredService<ICommandFactory>();
        foreach (var option in new[] { "--workspace-id", "--item-id", "--job-type", "--execution-data", "--job-parameters" })
        {
            var args = new List<string>
            {
                "jobs", "run-on-demand-item-job", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-type", JobType,
                "--execution-data", "{}", "--job-parameters", "[]", option, "private"
            };
            Assert.NotEmpty(factory.RootCommand.Parse(args.ToArray()).Errors);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
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
            Description = "Offline job submission tests",
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

    private static async Task<string> LearnAsync(IServiceProvider provider, string route)
    {
        var arguments = new Dictionary<string, object?> { ["intent"] = "", ["learn"] = true };
        if (route == "single")
        {
            arguments["tool"] = "jobs";
        }
        var result = await Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "single" ? "fabric" : "jobs", arguments), TestContext.Current.CancellationToken);
        Assert.NotEqual(true, result.IsError);
        return result.StructuredContent!.Value.GetRawText();
    }

    private static Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server, string toolName = ToolName)
    {
        var name = toolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Submit one authorized job or read its receipt", ["command"] = toolName, ["parameters"] = arguments };
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

    private static McpServer Consent(string action = "accept")
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => new JsonRpcResponse
            {
                Id = call.Arg<JsonRpcRequest>().Id,
                Result = new JsonObject { ["action"] = action, ["content"] = new JsonObject { ["decision"] = "accept" } }
            });
        return server;
    }

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-type"] = JobType };

    private static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

    private static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } content)
        {
            return route == "namespace" ? content.GetProperty("result") : content;
        }
        using var document = JsonDocument.Parse(Text(result));
        return document.RootElement.GetProperty("results").Clone();
    }

    private static void AssertUnavailable(CallToolResult result, string route)
    {
        if (route == "single")
        {
            Assert.Equal("tool-list", result.StructuredContent!.Value.GetProperty("kind").GetString());
            Assert.DoesNotContain(ToolName, result.StructuredContent.Value.GetRawText());
        }
        else
        {
            Assert.True(result.IsError, Text(result));
        }
    }
}
