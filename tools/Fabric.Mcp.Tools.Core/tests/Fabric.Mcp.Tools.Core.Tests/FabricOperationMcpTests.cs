// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class FabricOperationMcpTests()
{
    [Theory]
    [InlineData(false, TransportTypes.StdIo)]
    [InlineData(true, TransportTypes.StdIo)]
    [InlineData(true, TransportTypes.Http)]
    public async Task Discovery_AdvertisesReadOnlySingleReadToolsAndAdditiveCreateOptions(bool readOnly, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, StructuredOutputMode.Compact, readOnly: readOnly, transport: transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

        foreach (var name in new[] { "core_get-operation-state", "core_get-operation-result" })
        {
            var tool = Assert.Single(catalog.Tools, tool => tool.Name == name);
            Assert.True(tool.Annotations?.ReadOnlyHint);
            Assert.True(tool.Annotations?.IdempotentHint);
            Assert.False(tool.Annotations?.DestructiveHint);
            Assert.False(tool.Annotations?.OpenWorldHint);
            Assert.Equal("operation-id", Assert.Single(tool.InputSchema.GetProperty("required").EnumerateArray()).GetString());
            var property = Assert.Single(tool.InputSchema.GetProperty("properties").EnumerateObject());
            Assert.Equal("operation-id", property.Name);
            Assert.Equal("string", property.Value.GetProperty("type").GetString());
            Assert.NotNull(tool.OutputSchema);
        }
        Assert.Same(CoreJsonContext.Default.OperationStateResult, provider.GetRequiredService<OperationStateGetCommand>().ResultTypeInfo);
        Assert.Same(CoreJsonContext.Default.OperationResult, provider.GetRequiredService<OperationResultGetCommand>().ResultTypeInfo);
        Assert.Same(CoreJsonContext.Default.ItemCreateCommandResult, provider.GetRequiredService<ItemCreateCommand>().ResultTypeInfo);

        if (!readOnly)
        {
            var create = Assert.Single(catalog.Tools, tool => tool.Name == "core_create-item");
            var options = create.InputSchema.GetProperty("properties");
            Assert.Equal(["description", "display-name", "early-poll", "item-type", "max-wait-seconds", "sync", "workspace", "workspace-id"],
                options.EnumerateObject().Select(property => property.Name).Order());
            Assert.Equal("boolean", options.GetProperty("sync").GetProperty("type").GetString());
            Assert.Equal("boolean", options.GetProperty("early-poll").GetProperty("type").GetString());
            Assert.Equal("number", options.GetProperty("max-wait-seconds").GetProperty("type").GetString());
            var command = provider.GetRequiredService<ItemCreateCommand>();
            var bound = command.BindOptions(command.GetCommand().Parse(
                ["--workspace-id", FabricOperationTestData.WorkspaceId, "--display-name", "Created", "--item-type", "Lakehouse"]));
            Assert.False(bound.Sync);
            Assert.True(bound.EarlyPoll);
            Assert.Equal(FabricOperationWaitOptions.DefaultMaxWaitSeconds, bound.MaxWaitSeconds);
            Assert.NotNull(create.OutputSchema);
            Assert.False(create.Annotations?.ReadOnlyHint);
            Assert.False(create.Annotations?.IdempotentHint);
        }
        else
        {
            Assert.DoesNotContain(catalog.Tools, tool => tool.Name == "core_create-item");
            var result = await loader.CallToolHandler(
                McpTestUtilities.CreateToolCallRequest("core_create-item", CreateArguments()), TestContext.Current.CancellationToken);
            Assert.True(result.IsError);
            Assert.Contains("read-only", Text(result));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, StructuredOutputMode.Compact)]
    [InlineData(false, StructuredOutputMode.Duplicated)]
    [InlineData(true, null)]
    [InlineData(true, StructuredOutputMode.Compact)]
    [InlineData(true, StructuredOutputMode.Duplicated)]
    public async Task ReadTools_ReturnTypedPayloadInEveryOutputMode_WithoutImplicitFollowup(bool readResult, StructuredOutputMode? mode)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/v1/operations/{FabricOperationTestData.OperationId}{(readResult ? "/result" : "")}", request.RequestUri?.AbsolutePath);
            return Task.FromResult(FabricOperationTestData.JsonResponse(readResult ? "null" :
                """{"status":"Failed","percentComplete":50,"error":{"errorCode":"TestFailure","message":"private-backend-detail"}}"""));
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), mode, readOnly: true);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var name = readResult ? "core_get-operation-result" : "core_get-operation-state";
        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == name);
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest(name,
            new Dictionary<string, object?> { ["operation-id"] = FabricOperationTestData.OperationId }), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var payload = Payload(result, mode);
        Assert.Equal(FabricOperationTestData.OperationId, payload.GetProperty("operationId").GetString());
        if (readResult)
        {
            Assert.True(payload.GetProperty("hasBody").GetBoolean());
            Assert.Equal(JsonValueKind.Null, payload.GetProperty("value").ValueKind);
        }
        else
        {
            Assert.Equal("Failed", payload.GetProperty("state").GetProperty("status").GetString());
            Assert.Equal("TestFailure", payload.GetProperty("state").GetProperty("error").GetProperty("errorCode").GetString());
            Assert.DoesNotContain("private-backend-detail", payload.GetRawText());
        }
        if (tool.OutputSchema is { } schema)
        {
            AssertRequiredProperties(schema, payload);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "")]
    [InlineData(true, "name")]
    [InlineData(false, "00000000-0000-0000-0000-000000000000")]
    [InlineData(true, "00000000-0000-0000-0000-000000000000")]
    [InlineData(false, "https://example.invalid")]
    [InlineData(true, "../operations?private")]
    [InlineData(false, 12)]
    [InlineData(true, true)]
    public async Task ReadTools_RejectMissingInvalidOrNonStringIdBeforeAuth(bool readResult, object? id)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = new Dictionary<string, object?>();
        if (id is not null)
        {
            arguments["operation-id"] = id;
        }

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(readResult ? "core_get-operation-result" : "core_get-operation-state", arguments),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.DoesNotContain("example.invalid", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("core_get-operation-state", "sync")]
    [InlineData("core_get-operation-result", "sync")]
    [InlineData("core_get-operation-state", "url")]
    [InlineData("core_get-operation-result", "max-wait-seconds")]
    public async Task ReadTools_RejectWaitingOrArbitraryUrlOptions(string name, string option)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, StructuredOutputMode.Compact);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(name, new Dictionary<string, object?>
            {
                ["operation-id"] = FabricOperationTestData.OperationId,
                [option] = "private-input"
            }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-input", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Forbidden)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.NotFound)]
    [InlineData(false, HttpStatusCode.TooManyRequests)]
    [InlineData(true, HttpStatusCode.TooManyRequests)]
    [InlineData(false, null)]
    [InlineData(true, null)]
    public async Task ReadTools_PreserveHttpStatusAndNetworkFallbackWithoutLeakingDetails(bool readResult, HttpStatusCode? status)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => status is { } code
            ? Task.FromResult(FabricOperationTestData.JsonResponse("private-backend-detail", code, "20"))
            : throw new HttpRequestException("private-network-detail"));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), StructuredOutputMode.Compact);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            McpTestUtilities.CreateToolCallRequest(readResult ? "core_get-operation-result" : "core_get-operation-state",
                new Dictionary<string, object?> { ["operation-id"] = FabricOperationTestData.OperationId }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        using var document = JsonDocument.Parse(Text(result));
        Assert.Equal((int)(status ?? HttpStatusCode.ServiceUnavailable), document.RootElement.GetProperty("status").GetInt32());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private-", Text(result));
        if (status == HttpStatusCode.Forbidden)
        {
            Assert.Contains("same resource permissions and delegated scopes", Text(result));
        }
        if (status == HttpStatusCode.TooManyRequests)
        {
            Assert.Contains("20 seconds", Text(result));
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, "immediate")]
    [InlineData(StructuredOutputMode.Compact, "immediate")]
    [InlineData(StructuredOutputMode.Duplicated, "immediate")]
    [InlineData(null, "accepted")]
    [InlineData(StructuredOutputMode.Compact, "accepted")]
    [InlineData(StructuredOutputMode.Duplicated, "accepted")]
    [InlineData(null, "wait-success")]
    [InlineData(StructuredOutputMode.Compact, "wait-success")]
    [InlineData(StructuredOutputMode.Duplicated, "wait-success")]
    [InlineData(null, "budget")]
    [InlineData(StructuredOutputMode.Compact, "budget")]
    [InlineData(StructuredOutputMode.Duplicated, "budget")]
    [InlineData(null, "failed")]
    [InlineData(StructuredOutputMode.Compact, "failed")]
    [InlineData(StructuredOutputMode.Duplicated, "failed")]
    [InlineData(StructuredOutputMode.Compact, "result-forbidden")]
    [InlineData(null, "result-forbidden")]
    [InlineData(StructuredOutputMode.Duplicated, "result-forbidden")]
    [InlineData(StructuredOutputMode.Compact, "missing-id")]
    [InlineData(null, "missing-id")]
    [InlineData(StructuredOutputMode.Duplicated, "missing-id")]
    public async Task CreateItem_ExposesRealServiceOutcomesAndCompatibleShapes(StructuredOutputMode? mode, string scenario)
    {
        var clock = new OperationTestTimeProvider();
        var posts = 0;
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                posts++;
                return Task.FromResult(scenario == "immediate" ? FabricOperationTestData.ItemResponse() :
                    FabricOperationTestData.Accepted(scenario == "missing-id" ? null : FabricOperationTestData.OperationId));
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("/result", StringComparison.Ordinal))
            {
                if (scenario == "result-forbidden")
                {
                    return Task.FromResult(FabricOperationTestData.JsonResponse("private-backend-detail", HttpStatusCode.Forbidden));
                }
                var response = FabricOperationTestData.ItemResponse();
                response.StatusCode = HttpStatusCode.OK;
                return Task.FromResult(response);
            }
            return Task.FromResult(FabricOperationTestData.JsonResponse(scenario == "failed"
                ? """{"status":"Failed","error":{"errorCode":"CapacityUnavailable","message":"private-backend-detail"}}"""
                : """{"status":"Succeeded"}"""));
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), mode, clock);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == "core_create-item");
        var arguments = CreateArguments();
        var waits = scenario is "wait-success" or "budget" or "failed" or "result-forbidden";
        arguments["sync"] = waits;
        if (scenario == "budget")
        {
            arguments["max-wait-seconds"] = 2;
        }
        var task = loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_create-item", arguments), TestContext.Current.CancellationToken).AsTask();
        if (waits)
        {
            var delay = scenario == "budget" ? 2 : 3;
            await clock.WaitForTimerAsync(delay, TestContext.Current.CancellationToken);
            clock.Advance(delay);
        }
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);
        var failed = scenario is "failed" or "result-forbidden";

        Assert.Equal(failed, result.IsError);
        var payload = Payload(result, failed ? null : mode);
        if (scenario is "immediate" or "wait-success")
        {
            Assert.Equal(FabricOperationTestData.ItemId, payload.GetProperty("item").GetProperty("id").GetString());
            if (scenario == "immediate")
            {
                Assert.Equal("item", Assert.Single(payload.EnumerateObject()).Name);
            }
        }
        else
        {
            Assert.False(payload.TryGetProperty("item", out _));
        }
        if (scenario != "immediate")
        {
            var expected = scenario switch
            {
                "accepted" => "Accepted",
                "missing-id" => "AcceptedWithoutOperationId",
                "wait-success" => "Succeeded",
                "budget" => "Pending",
                "failed" => "Failed",
                _ => "ResultUnavailable"
            };
            Assert.Equal(expected, payload.GetProperty("operation").GetProperty("status").GetString());
            if (scenario != "missing-id")
            {
                Assert.Equal(FabricOperationTestData.OperationId, payload.GetProperty("operation").GetProperty("operationId").GetString());
            }
        }
        if (tool.OutputSchema is { } schema)
        {
            AssertRequiredProperties(schema, payload);
        }
        Assert.Equal(1, posts);
        Assert.Equal(scenario is "wait-success" or "result-forbidden" ? 3 : scenario == "failed" ? 2 : 1, handler.CallCount);
        Assert.DoesNotContain("private-backend-detail", Text(result));
    }

    [Theory]
    [InlineData("max-wait-seconds", 0d)]
    [InlineData("max-wait-seconds", -1d)]
    [InlineData("max-wait-seconds", "NaN")]
    [InlineData("max-wait-seconds", "Infinity")]
    [InlineData("sync", "private-invalid-boolean")]
    [InlineData("early-poll", "private-invalid-boolean")]
    public async Task CreateItem_RejectsInvalidWaitOptionsBeforeMutation(string option, object value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = FabricOperationTestData.Credential();
        await using var provider = Services(handler, credential, StructuredOutputMode.Compact);
        var arguments = CreateArguments();
        arguments[option] = value;

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_create-item", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("private-invalid", Text(result));
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    private static Dictionary<string, object?> CreateArguments() => new()
    {
        ["workspace-id"] = FabricOperationTestData.WorkspaceId,
        ["display-name"] = "Created",
        ["item-type"] = "Lakehouse"
    };

    [Theory]
    [InlineData(null)]
    [InlineData(StructuredOutputMode.Compact)]
    [InlineData(StructuredOutputMode.Duplicated)]
    public async Task ResultTool_EmptyBodyHasNoValueAndMatchesSchema(StructuredOutputMode? mode)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == "core_get-operation-result");

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-operation-result",
            new Dictionary<string, object?> { ["operation-id"] = FabricOperationTestData.OperationId }), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var payload = Payload(result, mode);
        Assert.False(payload.GetProperty("hasBody").GetBoolean());
        Assert.False(payload.TryGetProperty("value", out _));
        if (tool.OutputSchema is { } schema)
        {
            AssertRequiredProperties(schema, payload);
        }
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("application/octet-stream", "")]
    [InlineData("application/json", "private-invalid-json")]
    [InlineData("application/json", "oversize")]
    public async Task ResultTool_ReportsSanitizedExplicitContentErrors(string mediaType, string body)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = FabricOperationTestData.JsonResponse(body == "oversize" ? new string('x', FabricOperationHttp.MaxJsonBytes + 1) : body);
            response.Content.Headers.ContentType = new(mediaType);
            return Task.FromResult(response);
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), StructuredOutputMode.Compact);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_get-operation-result",
                new Dictionary<string, object?> { ["operation-id"] = FabricOperationTestData.OperationId }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        using var json = JsonDocument.Parse(Text(result));
        Assert.Equal(502, json.RootElement.GetProperty("status").GetInt32());
        Assert.False(json.RootElement.TryGetProperty("results", out _));
        Assert.Contains(body == "oversize" ? "1 MiB" : mediaType == "application/octet-stream" ? "Binary" : "invalid JSON", Text(result));
        Assert.DoesNotContain("private-invalid-json", Text(result));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task CreateItem_ExplicitWaitOptionsHonorInitialHintAndBudgetOverride()
    {
        var clock = new OperationTestTimeProvider();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(request.Method == HttpMethod.Post ? 0 : 20, (clock.GetUtcNow() - FabricOperationTestData.Epoch).TotalSeconds);
            return Task.FromResult(request.Method == HttpMethod.Post ? FabricOperationTestData.Accepted() :
                FabricOperationTestData.JsonResponse("""{"status":"Running"}""", retryAfter: "20"));
        });
        await using var provider = Services(handler, FabricOperationTestData.Credential(), StructuredOutputMode.Compact, clock);
        var arguments = CreateArguments();
        arguments["sync"] = true;
        arguments["early-poll"] = false;
        arguments["max-wait-seconds"] = 21;

        var task = provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_create-item", arguments), TestContext.Current.CancellationToken).AsTask();
        await clock.WaitForTimerAsync(20, TestContext.Current.CancellationToken);
        clock.Advance(20);
        await clock.WaitForTimerAsync(1, TestContext.Current.CancellationToken);
        clock.Advance(1);
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal("Pending", Payload(result, StructuredOutputMode.Compact).GetProperty("operation").GetProperty("status").GetString());
        Assert.Equal(2, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(StructuredOutputMode.Compact)]
    [InlineData(StructuredOutputMode.Duplicated)]
    public async Task CreateItem_CallerCancellationRetainsAcceptedReferenceInErrorResponse(StructuredOutputMode? mode)
    {
        var clock = new OperationTestTimeProvider();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(FabricOperationTestData.Accepted()));
        await using var provider = Services(handler, FabricOperationTestData.Credential(), mode, clock);
        var arguments = CreateArguments();
        arguments["sync"] = true;

        var task = provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_create-item", arguments), caller.Token).AsTask();
        await clock.WaitForTimerAsync(3, TestContext.Current.CancellationToken);
        await caller.CancelAsync();
        var result = await task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var operation = Payload(result, null).GetProperty("operation");
        Assert.Equal(FabricOperationTestData.OperationId, operation.GetProperty("operationId").GetString());
        Assert.Equal("CallerCanceled", operation.GetProperty("issue").GetProperty("code").GetString());
        using var json = JsonDocument.Parse(Text(result));
        Assert.Equal(408, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(1, handler.CallCount);
        Assert.Empty(clock.Timers);
    }

    private static string Text(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

    private static JsonElement Payload(CallToolResult result, StructuredOutputMode? mode)
    {
        var text = Text(result);
        if (mode is null)
        {
            Assert.Null(result.StructuredContent);
            using var document = JsonDocument.Parse(text);
            return document.RootElement.GetProperty("results").Clone();
        }
        Assert.NotNull(result.StructuredContent);
        if (mode == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(text);
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent.Value));
        }
        else
        {
            Assert.DoesNotContain(FabricOperationTestData.OperationId, text);
            Assert.DoesNotContain(FabricOperationTestData.ItemId, text);
        }
        return result.StructuredContent.Value;
    }

    private static void AssertRequiredProperties(JsonElement schema, JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return;
        }
        if (schema.TryGetProperty("required", out var required))
        {
            foreach (var name in required.EnumerateArray())
            {
                Assert.True(payload.TryGetProperty(name.GetString()!, out _), $"Missing required schema property {name}.");
            }
        }
        if (schema.TryGetProperty("properties", out var properties))
        {
            foreach (var property in payload.EnumerateObject())
            {
                if (properties.TryGetProperty(property.Name, out var childSchema))
                {
                    AssertRequiredProperties(childSchema, property.Value);
                }
            }
        }
    }

    private static ServiceProvider Services(
        HttpMessageHandler handler, TokenCredential credential, StructuredOutputMode? mode,
        TimeProvider? clock = null, bool readOnly = false, string transport = TransportTypes.StdIo)
    {
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        setup.ConfigureServices(services);
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        services.AddSingleton(credential);
        services.AddSingleton(clock ?? TimeProvider.System);
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
            Description = "Offline Fabric operation tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            ReadOnly = readOnly,
            StructuredOutputMode = mode,
            Transport = transport
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }
}
