// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Models;
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
using NSubstitute;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class ItemJobInstanceGetMcpTests()
{
    private const string ToolName = "jobs_get-item-job-instance";
    private const string OldToolName = "core_get-item-job-instance";

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

    public static TheoryData<string, string, string> NamespaceCases
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (var route in new[] { "all", "namespace", "single" })
            {
                foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
                {
                    foreach (var selection in new[] { "core", "jobs", "combined", "default" })
                    {
                        data.Add(route, transport, selection);
                    }
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public static async Task Get_IsDiscoverableReadOnlyAndReturnsOneSnapshotAcrossRoutesTransportsAndOutputs(
        string route, string transport, StructuredOutputMode? output, string selection)
    {
        var json = JsonNode.Parse(Json)!.AsObject();
        json["status"] = "Failed";
        json["failureReason"] = new JsonObject
        {
            ["errorCode"] = "JobFailed",
            ["requestId"] = ActivityId,
            ["isRetriable"] = false,
            ["message"] = FabricCoreErrorTestData.PrivateDetails
        };
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/v1/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances/{JobInstanceId}", request.RequestUri?.AbsolutePath);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            return Task.FromResult(Response(json.ToJsonString(), retryAfter: "45"));
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var tools = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools, tool => tool.Name == ToolName);
        Assert.DoesNotContain(tools.Tools, tool => tool.Name == OldToolName);

        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(["item-id", "job-instance-id", "workspace-id"],
            tool.InputSchema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
        Assert.Equal(["item-id", "job-instance-id", "workspace-id"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal("jobInstance", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal(["jobInstance", "retryAfterSeconds", "retryAfterUtc"], schema.GetProperty("properties").EnumerateObject()
                .Select(property => property.Name).Order());
            var instance = schema.GetProperty("properties").GetProperty("jobInstance");
            Assert.Equal(["id", "invokeType", "itemId", "jobType", "status"],
                instance.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            var properties = instance.GetProperty("properties");
            Assert.Equal(["endTimeUtc", "failureReason", "id", "invokeType", "itemId", "jobType", "rootActivityId", "startTimeUtc", "status"],
                properties.EnumerateObject().Select(property => property.Name).Order());
            Assert.False(properties.GetProperty("status").TryGetProperty("enum", out _));
            Assert.False(properties.GetProperty("invokeType").TryGetProperty("enum", out _));
            Assert.DoesNotContain("message", properties.GetProperty("failureReason").GetRawText());
            Assert.False(properties.GetProperty("startTimeUtc").TryGetProperty("format", out _));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var result = await CallAsync(provider, route, Arguments());

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal(JobInstanceId, payload.GetProperty("jobInstance").GetProperty("id").GetString());
        Assert.Equal("Failed", payload.GetProperty("jobInstance").GetProperty("status").GetString());
        Assert.Equal("JobFailed", payload.GetProperty("jobInstance").GetProperty("failureReason").GetProperty("errorCode").GetString());
        Assert.False(payload.GetProperty("jobInstance").GetProperty("failureReason").GetProperty("isRetriable").GetBoolean());
        Assert.Equal(45, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.False(payload.TryGetProperty("retryAfterUtc", out _));
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
    }

    [Fact]
    public static async Task JobsSetup_RegistersOneIndependentAreaAndKeepsTheCommandIdentity()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var setup = Assert.Single(provider.GetServices<IAreaSetup>(), area => area.Name == "jobs");
        Assert.IsType<FabricJobsSetup>(setup);
        Assert.Equal("Microsoft Fabric Jobs", setup.Title);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Single(factory.RootGroup.SubGroup, group => group.Name == "jobs");
        var registration = Assert.Single(factory.AllCommands, pair => pair.Key == ToolName);
        Assert.Equal(ToolName, registration.Key);
        Assert.Same(provider.GetRequiredService<ItemJobInstanceGetCommand>(), registration.Value.Command);
        Assert.Single(provider.GetServices<ItemJobInstanceGetCommand>());
        Assert.Equal("6a7f2abe-b9db-4c7f-8ae4-c9764b3cc591", registration.Value.Command.Id);
        Assert.DoesNotContain(OldToolName, factory.AllCommands.Keys);
        Assert.IsType<FabricCoreService>(provider.GetRequiredService<IFabricCoreService>());
        var parsed = factory.RootCommand.Parse(
            ["jobs", "get-item-job-instance", "--workspace-id", WorkspaceId, "--item-id", ItemId, "--job-instance-id", JobInstanceId]);
        Assert.Empty(parsed.Errors);
        Assert.Same(registration.Value.Command.GetCommand(), parsed.CommandResult.Command);
        Assert.NotEmpty(factory.RootCommand.Parse(["core", "get-item-job-instance"]).Errors);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [MemberData(nameof(NamespaceCases))]
    public static async Task Filters_RestrictDiscoveryLearnAndInvocationWithoutAFormerCoreAlias(
        string route, string transport, string selection)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, selection);
        var expectedAreas = selection switch
        {
            "core" => new[] { "core" },
            "jobs" => ["jobs"],
            _ => ["core", "jobs"]
        };
        var loader = Loader(provider, route);
        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        if (route == "all")
        {
            Assert.Equal(expectedAreas, catalog.Tools.Select(tool => tool.Name.Split('_')[0]).Distinct().Order());
            Assert.Equal(selection != "core", catalog.Tools.Any(tool => tool.Name == ToolName));
            Assert.Equal(selection != "jobs", catalog.Tools.Any(tool => tool.Name == "core_get-item"));
        }
        else if (route == "namespace")
        {
            Assert.Equal(expectedAreas, catalog.Tools.Select(tool => tool.Name).Order());
        }
        else
        {
            Assert.Equal("fabric", Assert.Single(catalog.Tools).Name);
            var root = await LearnAsync(provider, route);
            Assert.Equal(expectedAreas, root.Select(tool => tool.GetProperty("tool").GetString()).Order());
        }
        Assert.DoesNotContain(catalog.Tools, tool => tool.Name == OldToolName);

        if (route != "all")
        {
            foreach (var area in expectedAreas)
            {
                var commands = await LearnAsync(provider, route, area);
                Assert.All(commands, command => Assert.StartsWith(area + "_", command.GetProperty("command").GetString()));
                Assert.DoesNotContain(commands, command => command.GetProperty("command").GetString() == OldToolName);
                if (area == "jobs")
                {
                    Assert.Equal([ToolName, "jobs_list-item-job-instances", "jobs_list-item-schedules"],
                        commands.Select(command => command.GetProperty("command").GetString()).Order());
                    var command = Assert.Single(commands, command => command.GetProperty("command").GetString() == ToolName);
                    Assert.Equal(ToolName, command.GetProperty("command").GetString());
                    Assert.Equal(["item-id", "job-instance-id", "workspace-id"],
                        command.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
                }
                else
                {
                    Assert.Contains(commands, command => command.GetProperty("command").GetString() == "core_get-item");
                    Assert.DoesNotContain(commands, command => command.GetProperty("command").GetString() == ToolName);
                }
            }
        }

        var oldAlias = await CallAsync(provider, route, Arguments(), OldToolName, "core");
        AssertUnavailable(oldAlias, route, OldToolName);
        if (route != "all")
        {
            var wrongArea = await CallAsync(provider, route, Arguments(), ToolName, "core");
            AssertUnavailable(wrongArea, route, ToolName);
        }
        if (selection == "core")
        {
            var hiddenJob = await CallAsync(provider, route, Arguments());
            AssertUnavailable(hiddenJob, route, ToolName);
        }
        if (selection == "jobs")
        {
            var hiddenCore = await CallAsync(provider, route,
                new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId }, "core_get-item", "core");
            AssertUnavailable(hiddenCore, route, "core_get-item");
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
    public static async Task CombinedWorkflow_HandsCoreItemIdsToJobsWithoutDiscoveringOrStartingAJob(string route, string transport)
    {
        var paths = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("job-test-token", request.Headers.Authorization?.Parameter);
            var path = request.RequestUri!.AbsolutePath;
            paths.Add(path);
            if (path == $"/v1/workspaces/{WorkspaceId}/items/{ItemId}")
            {
                return Task.FromResult(Response(JsonSerializer.Serialize(new FabricItemMetadata
                {
                    Id = Guid.Parse(ItemId),
                    WorkspaceId = Guid.Parse(WorkspaceId),
                    DisplayName = "Existing item",
                    Type = "Notebook"
                }, CoreJsonContext.Default.FabricItemMetadata)));
            }
            Assert.Equal($"/v1/workspaces/{WorkspaceId}/items/{ItemId}/jobs/instances/{JobInstanceId}", path);
            return Task.FromResult(Response());
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, "combined");

        var itemResult = await CallAsync(provider, route,
            new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId }, "core_get-item", "core");
        Assert.False(itemResult.IsError, Text(itemResult));
        Assert.Equal(1, handler.CallCount);
        var item = Payload(itemResult, route).GetProperty("item");
        var jobResult = await CallAsync(provider, route, new()
        {
            ["workspace-id"] = item.GetProperty("workspaceId").GetString(),
            ["item-id"] = item.GetProperty("id").GetString(),
            ["job-instance-id"] = JobInstanceId
        });

        Assert.False(jobResult.IsError, Text(jobResult));
        Assert.Equal(JobInstanceId, Payload(jobResult, route).GetProperty("jobInstance").GetProperty("id").GetString());
        Assert.Equal(2, handler.CallCount);
        Assert.Equal(2, paths.Distinct().Count());
        await credential.Received(2).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("workspace-id")]
    [InlineData("item-id")]
    [InlineData("job-instance-id")]
    public static async Task Get_RejectsMissingInvalidAndNonStringIdsBeforeAuthentication(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var value in new object?[] { null, "", " ", Guid.Empty.ToString(), "private-invalid", JobInstanceId + "/result", 12, true })
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

            var result = await CallAsync(provider, "all", arguments);

            Assert.True(result.IsError);
            Assert.Null(result.StructuredContent);
            using var document = JsonDocument.Parse(Text(result));
            Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
            Assert.DoesNotContain("private", Text(result));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("operation-id")]
    [InlineData("sync")]
    [InlineData("max-wait-seconds")]
    [InlineData("early-poll")]
    [InlineData("url")]
    [InlineData("job-type")]
    public static async Task Get_RejectsUnpublishedInputsWithoutHttp(string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        var arguments = Arguments();
        arguments[option] = "private-input";

        var result = await CallAsync(provider, "all", arguments);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.DoesNotContain("private-input", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, null, "invalid job-instance response")]
    [InlineData(HttpStatusCode.Accepted, null, "invalid job-instance response")]
    [InlineData(HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, null, "Access denied")]
    [InlineData(HttpStatusCode.NotFound, null, "not found")]
    [InlineData(HttpStatusCode.TooManyRequests, "60", "Wait at least 60 seconds")]
    [InlineData(HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(HttpStatusCode.TooManyRequests, "private-invalid", "Wait before retrying")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "45", "Wait at least 45 seconds")]
    public static async Task Get_ReturnsSanitizedErrorsWithStatusAndRetryContext(HttpStatusCode status, string? retryAfter, string guidance)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
            Task.FromResult(Response(FabricCoreErrorTestData.PrivateDetails, status, retryAfter)));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);

        var result = await CallAsync(provider, "all", Arguments());

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Text(result);
        using var document = JsonDocument.Parse(text);
        Assert.Equal(status is HttpStatusCode.OK or HttpStatusCode.Accepted ? 502 : (int)status, document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(guidance, document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private", text);
        Assert.DoesNotContain("stackTrace", text);
        Assert.Equal(1, handler.CallCount);
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, string transport, StructuredOutputMode? output, string selection = "jobs")
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
            Description = "Offline job-instance tests",
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
            ReadOnly = true,
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

    private static async Task<JsonElement[]> LearnAsync(IServiceProvider provider, string route, string? area = null)
    {
        var arguments = new Dictionary<string, object?> { ["intent"] = "", ["learn"] = true };
        if (route == "single" && area is not null)
        {
            arguments["tool"] = area;
        }
        var result = await Loader(provider, route).CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "single" ? "fabric" : area!, arguments), TestContext.Current.CancellationToken);
        Assert.NotEqual(true, result.IsError);
        Assert.NotNull(result.StructuredContent);
        return result.StructuredContent.Value.GetProperty("tools").EnumerateArray().ToArray();
    }

    private static async Task<CallToolResult> CallAsync(
        IServiceProvider provider, string route, Dictionary<string, object?> arguments, string toolName = ToolName, string area = "jobs")
    {
        var loader = Loader(provider, route);
        if (route == "all")
        {
            return await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(toolName, arguments), TestContext.Current.CancellationToken);
        }
        var parameters = new Dictionary<string, object?>
        {
            ["intent"] = "Read this known job instance once",
            ["command"] = toolName,
            ["parameters"] = arguments
        };
        if (route == "single")
        {
            parameters["tool"] = area;
        }
        return await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "namespace" ? area : "fabric", parameters), TestContext.Current.CancellationToken);
    }

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-instance-id"] = JobInstanceId };

    private static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

    private static void AssertUnavailable(CallToolResult result, string route, string rejectedCommand)
    {
        if (route == "single")
        {
            // The existing single router returns discovery guidance, not a command result, for unavailable commands.
            Assert.NotNull(result.StructuredContent);
            Assert.Equal("tool-list", result.StructuredContent.Value.GetProperty("kind").GetString());
            Assert.DoesNotContain(rejectedCommand, result.StructuredContent.Value.GetRawText());
            Assert.False(result.StructuredContent.Value.TryGetProperty("result", out _));
        }
        else
        {
            Assert.True(result.IsError);
        }
    }

    private static JsonElement Payload(CallToolResult result, string route)
    {
        if (result.StructuredContent is { } structured)
        {
            return route == "namespace" ? structured.GetProperty("result") : structured;
        }
        using var document = JsonDocument.Parse(Text(result));
        return document.RootElement.GetProperty("results").Clone();
    }
}
