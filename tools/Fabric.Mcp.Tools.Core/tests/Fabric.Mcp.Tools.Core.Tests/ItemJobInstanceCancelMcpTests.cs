// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceCancelTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemJobInstanceCancelMcpTests()
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
    public static async Task Cancel_RegistersOneAtomicToolWithConsentAndTypedReceiptsAcrossRoutes(
        string route, string transport, StructuredOutputMode? output, string selection)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(Location + "/cancel", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            return Task.FromResult(Accepted());
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Equal([ToolName, "jobs_create-item-schedule", "jobs_get-item-job-instance", "jobs_get-item-schedule", "jobs_list-item-job-instances", "jobs_list-item-schedules", "jobs_run-on-demand-item-job", "jobs_update-item-schedule"],
            factory.AllCommands.Keys.Where(key => key.StartsWith("jobs_", StringComparison.Ordinal)).Order());
        Assert.Same(provider.GetRequiredService<ItemJobInstanceCancelCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<ItemJobInstanceCancelCommand>());
        var catalog = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(["item-id", "job-instance-id", "workspace-id"],
            tool.InputSchema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
        Assert.Equal(["item-id", "job-instance-id", "workspace-id"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["cancellationStatus", "guidance", "itemId", "jobInstanceId", "retryAfterIssue", "retryAfterSeconds", "retryAfterUtc", "workspaceId"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        if (route != "all")
        {
            var learn = await LearnAsync(provider, route);
            Assert.Contains(ToolName, learn);
            Assert.Contains("job-instance-id", learn);
            Assert.Contains("jobs_get-item-job-instance", learn);
            Assert.Contains("jobs_run-on-demand-item-job", learn);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var server = Consent();

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var receipt = Payload(result, route);
        Assert.Equal("Requested", receipt.GetProperty("cancellationStatus").GetString());
        Assert.Equal(WorkspaceId, receipt.GetProperty("workspaceId").GetString());
        Assert.Equal(ItemId, receipt.GetProperty("itemId").GetString());
        Assert.Equal(JobInstanceId, receipt.GetProperty("jobInstanceId").GetString());
        Assert.Equal(60, receipt.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(receipt.TryGetProperty("status", out _));
        Assert.False(receipt.TryGetProperty("operationId", out _));
        Assert.False(receipt.TryGetProperty("jobType", out _));
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
    public static async Task Cancel_ReadOnlyHidesAndDeniesMutationButKeepsGet(string route, string transport)
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
        Assert.Equal(["jobs_get-item-job-instance", "jobs_get-item-schedule", "jobs_list-item-job-instances", "jobs_list-item-schedules"], catalog.Tools.Select(tool => tool.Name).Order());
        if (route != "all")
        {
            var learn = await LearnAsync(provider, route);
            Assert.Contains("jobs_get-item-job-instance", learn);
            Assert.Contains("jobs_list-item-job-instances", learn);
            Assert.Contains("jobs_list-item-schedules", learn);
            Assert.DoesNotContain(ToolName, learn);
            Assert.DoesNotContain("jobs_run-on-demand-item-job", learn);
        }
        var server = Consent();
        AssertUnavailable(await CallAsync(provider, route, Arguments(), server), route);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        var get = await CallAsync(provider, route, Arguments(), server, "jobs_get-item-job-instance");
        Assert.False(get.IsError, Text(get));
        Assert.Equal(JobInstanceId, Payload(get, route).GetProperty("jobInstance").GetProperty("id").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Cancel_CoreOnlyCannotDiscoverOrInvokeJobs(string route)
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
    [InlineData("all", "InProgress")]
    [InlineData("namespace", "InProgress")]
    [InlineData("single", "InProgress")]
    [InlineData("all", "Cancelled")]
    [InlineData("namespace", "Cancelled")]
    [InlineData("single", "Cancelled")]
    public static async Task Cancel_HandsKnownIdsToASeparateGetWithoutAnImplicitRead(string route, string state)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal(Location + "/cancel", request.RequestUri?.AbsoluteUri);
                return Task.FromResult(Accepted(retryAfter: "0"));
            }
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(Location, request.RequestUri?.AbsoluteUri);
            return Task.FromResult(Response(Json.Replace("Completed", state, StringComparison.Ordinal)));
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);

        var cancel = await CallAsync(provider, route, Arguments(), Consent());

        Assert.False(cancel.IsError, Text(cancel));
        var receipt = Payload(cancel, route);
        Assert.Equal("Requested", receipt.GetProperty("cancellationStatus").GetString());
        Assert.Equal(0, receipt.GetProperty("retryAfterSeconds").GetInt64());
        Assert.Equal(1, handler.CallCount);

        var get = await CallAsync(provider, route, new()
        {
            ["workspace-id"] = receipt.GetProperty("workspaceId").GetString(),
            ["item-id"] = receipt.GetProperty("itemId").GetString(),
            ["job-instance-id"] = receipt.GetProperty("jobInstanceId").GetString()
        }, Consent(), "jobs_get-item-job-instance");

        Assert.False(get.IsError, Text(get));
        Assert.Equal(state, Payload(get, route).GetProperty("jobInstance").GetProperty("status").GetString());
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Cancel_AlreadyCompletedIsAFailureNotAReceiptOrFinalJobState(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(AlreadyCompleted, HttpStatusCode.Conflict)));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent());
        Assert.True(result.IsError, Text(result));
        Assert.Contains("JobAlreadyCompleted", Text(result));
        Assert.Contains("does not establish its final success or failure status", Text(result));
        Assert.DoesNotContain("cancellationStatus", Text(result));
        Assert.DoesNotContain("private", Text(result));
        Assert.DoesNotContain("stackTrace", Text(result));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Cancel_MalformedUnicodeMetadataKeepsHttpFailureAndRetryGuidance(string route)
    {
        foreach (var json in new[]
        {
            """{"errorCode":"\uD800","message":"private"}""",
            """{"errorCode":"\uDC00","message":"private"}""",
            """{"error\uD800Code":"JobAlreadyCompleted","message":"private"}""",
            """{"error\uDC00Code":"JobAlreadyCompleted","message":"private"}"""
        })
        {
            using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(json));
            using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StreamContent(stream) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response.Headers.TryAddWithoutValidation("Retry-After", "60");
            using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
            var credential = Credential();
            await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);

            var result = await CallAsync(provider, route, Arguments(), Consent());

            Assert.True(result.IsError, Text(result));
            using var error = JsonDocument.Parse(Text(result));
            Assert.Equal(429, error.RootElement.GetProperty("status").GetInt32());
            Assert.Contains("Wait at least 60 seconds", Text(result));
            Assert.Contains("error metadata was unavailable", Text(result));
            Assert.DoesNotContain("JobAlreadyCompleted", Text(result));
            Assert.DoesNotContain("private", Text(result));
            Assert.DoesNotContain("stackTrace", Text(result));
            Assert.True(stream.Disposed);
            Assert.Equal(1, handler.CallCount);
            await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Cancel_MetadataAnomaliesRemainAcceptedWithKnownIds(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = Accepted("private-invalid");
            response.Headers.TryAddWithoutValidation("Location", "https://example.invalid/private");
            return Task.FromResult(response);
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent());
        Assert.False(result.IsError, Text(result));
        var receipt = Payload(result, route);
        Assert.Equal("Requested", receipt.GetProperty("cancellationStatus").GetString());
        Assert.Equal(JobInstanceId, receipt.GetProperty("jobInstanceId").GetString());
        Assert.Equal("MissingOrInvalidRetryAfter", receipt.GetProperty("retryAfterIssue").GetString());
        Assert.DoesNotContain("private", Text(result) + receipt.GetRawText());
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Cancel_RejectsInvalidMissingAndUnsupportedOptionsBeforeAuth(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var (name, value) in new (string, object?)[]
        {
            ("workspace-id", "private-invalid"), ("item-id", Guid.Empty.ToString()), ("job-instance-id", "../private"),
            ("workspace-id", null), ("item-id", null), ("job-instance-id", null), ("job-instance-id", new[] { JobInstanceId, ActivityId }),
            ("job-type", "private"), ("execution-data", "{}"), ("job-parameters", "[]"), ("sync", "private"),
            ("early-poll", "private"), ("max-wait-seconds", 0), ("url", "https://example.invalid/private"), ("operation-id", ActivityId)
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
    [InlineData("all", "decline")]
    [InlineData("namespace", "decline")]
    [InlineData("single", "decline")]
    [InlineData("all", "cancel")]
    [InlineData("namespace", "cancel")]
    [InlineData("single", "cancel")]
    public static async Task Cancel_DeclinedConsentStopsBeforeAuth(string route, string action)
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
    public static async Task Cancel_RegisteredCliRejectsRepeatedOptionsAndRetainsStandardAcceptedStatus()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Accepted()));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, null);
        var factory = provider.GetRequiredService<ICommandFactory>();
        string[] args = ["jobs", "cancel-item-job-instance", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId];
        foreach (var option in new[] { "--workspace-id", "--item-id", "--job-instance-id" })
        {
            foreach (var value in new[] { JobInstanceId, "private" })
            {
                Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, value]).Errors);
            }
        }
        Assert.Contains("cancel-item-job-instance", factory.GetLearnResponse(["jobs", "cancel-item-job-instance", "--learn"]));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Equal((int)HttpStatusCode.Accepted, await parsed.InvokeAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.CallCount);
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
            Description = "Offline job cancellation tests",
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
        return result.StructuredContent?.GetRawText() ?? Text(result);
    }

    private static Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, McpServer server, string toolName = ToolName)
    {
        var name = toolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Request authorized cancellation or read the known job", ["command"] = toolName, ["parameters"] = arguments };
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
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-instance-id"] = JobInstanceId };

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
