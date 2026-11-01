// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:words Retriable

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
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
    private const string ToolName = "core_get-item-job-instance";

    public static TheoryData<string, string, StructuredOutputMode?> Routes
    {
        get
        {
            var data = new TheoryData<string, string, StructuredOutputMode?>();
            foreach (var route in new[] { "all", "namespace", "single" })
            {
                foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
                {
                    foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
                    {
                        data.Add(route, transport, output);
                    }
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Routes))]
    public static async Task Get_IsDiscoverableReadOnlyAndReturnsOneSnapshotAcrossRoutesTransportsAndOutputs(
        string route, string transport, StructuredOutputMode? output)
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
        await using var provider = Services(handler, credential, transport, output);
        var tools = await provider.GetRequiredService<CommandFactoryToolLoader>().ListToolsHandler(
            McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools, tool => tool.Name == ToolName);

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

    private static ServiceProvider Services(HttpMessageHandler handler, TokenCredential credential, string transport, StructuredOutputMode? output)
    {
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddLogging();
        services.AddSingleton<IAreaSetup>(setup);
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
            Namespace = ["core"],
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

    private static async Task<CallToolResult> CallAsync(IServiceProvider provider, string route, Dictionary<string, object?> arguments)
    {
        IToolLoader loader = route switch
        {
            "all" => provider.GetRequiredService<CommandFactoryToolLoader>(),
            "namespace" => provider.GetRequiredService<NamespaceToolLoader>(),
            "single" => provider.GetRequiredService<SingleProxyToolLoader>(),
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };
        if (route == "all")
        {
            return await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(ToolName, arguments), TestContext.Current.CancellationToken);
        }
        var parameters = new Dictionary<string, object?>
        {
            ["intent"] = "Read this known job instance once",
            ["command"] = ToolName,
            ["parameters"] = arguments
        };
        if (route == "single")
        {
            parameters["tool"] = "core";
        }
        return await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(route == "namespace" ? "core" : "fabric", parameters), TestContext.Current.CancellationToken);
    }

    private static Dictionary<string, object?> Arguments() =>
        new() { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId, ["job-instance-id"] = JobInstanceId };

    private static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

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
