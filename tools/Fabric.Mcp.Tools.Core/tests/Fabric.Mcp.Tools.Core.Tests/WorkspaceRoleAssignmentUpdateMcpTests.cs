// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
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
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class WorkspaceRoleAssignmentUpdateMcpTests()
{
    private const string ToolName = "core_update-workspace-role-assignment";

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
                        foreach (var selection in new[] { "core", "combined", "default" })
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
    public static async Task Update_RegistersOneExplicitMutationAcrossRoutesTransportsOutputsAndFiltersWithStandardConsent(
        string route, string transport, StructuredOutputMode? output, string selection)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal(AssignmentUrl, request.RequestUri?.AbsoluteUri);
            Assert.Equal("role-list-test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal("""{"role":"Viewer"}""", await request.Content!.ReadAsStringAsync(token));
            return Response(AssignmentJson, retryAfter: "0");
        });
        var credential = Credential();
        await using var provider = Services(handler, credential, transport, output, selection);
        var factory = provider.GetRequiredService<ICommandFactory>();
        Assert.Same(provider.GetRequiredService<WorkspaceRoleAssignmentUpdateCommand>(), factory.AllCommands[ToolName].Command);
        Assert.Single(provider.GetServices<WorkspaceRoleAssignmentUpdateCommand>());
        Assert.DoesNotContain("jobs_update-workspace-role-assignment", factory.AllCommands.Keys);
        Assert.DoesNotContain("core_delete-workspace-role-assignment", factory.AllCommands.Keys);
        var catalog = await Loader(provider, "all").ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        AssertInputSchema(tool.InputSchema);
        Assert.Equal(output.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal(["guidance", "retryAfterSeconds", "retryAfterUtc", "roleAssignment", "updateStatus"],
                schema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            var assignment = schema.GetProperty("properties").GetProperty("roleAssignment");
            Assert.Equal(["id", "principal", "role"], assignment.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            var principal = assignment.GetProperty("properties").GetProperty("principal");
            Assert.Equal(["id", "type"], principal.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.False(assignment.GetProperty("properties").GetProperty("role").TryGetProperty("enum", out _));
        }
        if (route != "all")
        {
            AssertInputSchema(Assert.Single(await LearnAsync(provider, route),
                command => command.GetProperty("command").GetString() == ToolName).GetProperty("inputSchema"));
        }
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
        var server = Consent();

        var result = await CallAsync(provider, route, Arguments(), server);

        Assert.False(result.IsError, Text(result));
        var payload = Payload(result, route);
        Assert.Equal("Updated", payload.GetProperty("updateStatus").GetString());
        Assert.Equal(AssignmentId, payload.GetProperty("roleAssignment").GetProperty("id").GetString());
        Assert.Equal(PrincipalId, payload.GetProperty("roleAssignment").GetProperty("principal").GetProperty("id").GetString());
        Assert.Equal("Viewer", payload.GetProperty("roleAssignment").GetProperty("role").GetString());
        Assert.Equal(0, payload.GetProperty("retryAfterSeconds").GetInt64());
        Assert.Contains("Do not resubmit", payload.GetProperty("guidance").GetString());
        if (route == "all" && output == StructuredOutputMode.Duplicated)
        {
            using var document = JsonDocument.Parse(Text(result));
            Assert.Equal(200, document.RootElement.GetProperty("status").GetInt32());
            Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), result.StructuredContent!.Value));
        }
        if (route == "all" && output == StructuredOutputMode.Compact)
        {
            Assert.DoesNotContain(PrincipalId, Text(result));
        }
        if (output is null || route == "single")
        {
            Assert.Null(result.StructuredContent);
        }
        AssertWireSerializable(result);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
        await server.Received(1).SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("all", TransportTypes.StdIo)]
    [InlineData("namespace", TransportTypes.StdIo)]
    [InlineData("single", TransportTypes.StdIo)]
    [InlineData("all", TransportTypes.Http)]
    [InlineData("namespace", TransportTypes.Http)]
    [InlineData("single", TransportTypes.Http)]
    public static async Task Update_ReadOnlyOrJobsOnlyCannotDiscoverLearnOrInvoke(string route, string transport)
    {
        foreach (var readOnly in new[] { false, true })
        {
            using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
            var credential = Credential();
            await using var provider = Services(handler, credential, transport, StructuredOutputMode.Compact, readOnly ? "combined" : "jobs", readOnly);
            var catalog = await Loader(provider, "all").ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
            Assert.DoesNotContain(catalog.Tools, tool => tool.Name == ToolName);
            if (readOnly)
            {
                Assert.Contains(catalog.Tools, tool => tool.Name == "core_get-workspace-role-assignment");
                Assert.Contains(catalog.Tools, tool => tool.Name == "core_list-workspace-role-assignments");
                Assert.Contains(catalog.Tools, tool => tool.Name == "jobs_get-item-job-instance");
                Assert.Contains(catalog.Tools, tool => tool.Name == "jobs_list-item-job-instances");
                Assert.Contains(catalog.Tools, tool => tool.Name == "jobs_get-item-schedule");
                Assert.Contains(catalog.Tools, tool => tool.Name == "jobs_list-item-schedules");
            }
            if (route != "all")
            {
                Assert.DoesNotContain(await LearnAsync(provider, route, readOnly ? "core" : "jobs"),
                    command => command.GetProperty("command").GetString() == ToolName);
            }
            var server = Consent();
            var blocked = await CallAsync(provider, route, Arguments(), server);
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
            await server.DidNotReceive().SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_RequiresEveryExplicitInputAndRejectsUnsupportedOptionsBeforeAuth(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        foreach (var option in new[] { "workspace-id", "workspace-role-assignment-id", "role" })
        {
            foreach (var value in new object?[] { null, "", " ", Array.Empty<string>() })
            {
                var args = Arguments();
                args[option] = value;
                var result = await CallAsync(provider, route, args, Consent());
                Assert.True(result.IsError, Text(result));
                Assert.Null(result.StructuredContent);
            }
            var missing = Arguments();
            missing.Remove(option);
            Assert.True((await CallAsync(provider, route, missing, Consent())).IsError);
            var repeated = Arguments();
            repeated[option.ToUpperInvariant()] = "private";
            Assert.True((await CallAsync(provider, route, repeated, Consent())).IsError);
        }
        foreach (var option in new[] { "principal", "principal-id", "principal-name", "application-id", "expand-groups", "upsert", "sync", "url" })
        {
            var args = Arguments();
            args[option] = "private";
            var result = await CallAsync(provider, route, args, Consent());
            Assert.True(result.IsError, Text(result));
            Assert.Null(result.StructuredContent);
            Assert.DoesNotContain("private", Text(result));
        }
        foreach (var option in new[] { "workspace-id", "workspace-role-assignment-id" })
        {
            var args = Arguments();
            args[option] = "../private";
            AssertSafeFailure(await CallAsync(provider, route, args, Consent()), HttpStatusCode.BadRequest);
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_DeclinedCanceledRejectedAndUnsupportedConsentNeverAuthenticate(string route)
    {
        foreach (var transport in new[] { TransportTypes.StdIo, TransportTypes.Http })
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
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_AuthenticationFailureIsUnsentNotAnUncertainPermissionChange(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromException<AccessToken>(new AuthenticationFailedException("private")));
        await using var provider = Services(handler, credential, TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent());
        AssertSafeFailure(result, HttpStatusCode.Unauthorized);
        Assert.Contains("request was not sent", Text(result));
        Assert.DoesNotContain("may already", Text(result));
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_PreservesConfirmedRejectedAndUncertainOutcomesThroughRouting(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var status in new[] { HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.Conflict, HttpStatusCode.TooManyRequests, HttpStatusCode.ServiceUnavailable, HttpStatusCode.Accepted })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                    Task.FromResult(Response("""{"errorCode":"\uD800","private":"payload"}""", status, "60")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
                var result = await CallAsync(provider, route, Arguments(), Consent());
                AssertSafeFailure(result, status is HttpStatusCode.OK or HttpStatusCode.Accepted ? HttpStatusCode.BadGateway : status);
                Assert.Contains("Wait at least 60 seconds", Text(result));
                Assert.Contains("not permission to replay", Text(result));
                Assert.Equal(status == HttpStatusCode.OK, Text(result).Contains("confirmed role-assignment update (HTTP 200)", StringComparison.Ordinal));
                Assert.Equal(status is HttpStatusCode.ServiceUnavailable or HttpStatusCode.Accepted, Text(result).Contains("may already", StringComparison.Ordinal));
                if (status == HttpStatusCode.Forbidden)
                {
                    Assert.Contains("workspace Admin access", Text(result));
                    using var document = JsonDocument.Parse(Text(result));
                    Assert.Contains("last admin's assignment cannot be changed", document.RootElement.GetProperty("message").GetString());
                }
                AssertWireSerializable(result);
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_ConfirmedUpdateSurvivesCanceledMetadataRead(string route)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new OperationTestStream(Encoding.UTF8.GetBytes(AssignmentJson), (_, token) =>
        {
            cancellation.Cancel();
            token.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        });
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
        response.Content.Headers.ContentType = new("application/json");
        response.Headers.TryAddWithoutValidation("Retry-After", "45");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var result = await CallAsync(provider, route, Arguments(), Consent(), cancellation.Token);
        AssertSafeFailure(result, HttpStatusCode.RequestTimeout);
        Assert.Contains("confirmed role-assignment update (HTTP 200)", Text(result));
        Assert.Contains("Do not resubmit", Text(result));
        Assert.Contains("Wait at least 45 seconds", Text(result));
        Assert.DoesNotContain("may already", Text(result));
        Assert.True(stream.Disposed);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_EnforcesExactRetainedAndWireEnvelopeLimitsWithoutLosing200(string route)
    {
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var leafDetails in new[] { false, true })
            {
                using var handler = new FabricCoreHttpMessageHandler((_, _) =>
                    Task.FromResult(Response(NestedPrincipalAssignment(30, leafDetails), retryAfter: "45")));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
                var result = await CallAsync(provider, route, Arguments("Member"), Consent());
                if (leafDetails)
                {
                    AssertSafeFailure(result, HttpStatusCode.BadGateway);
                    Assert.Contains("confirmed role-assignment update (HTTP 200)", Text(result));
                    Assert.Contains("Do not resubmit", Text(result));
                    Assert.Contains("Wait at least 45 seconds", Text(result));
                }
                else
                {
                    Assert.False(result.IsError, Text(result));
                    Assert.Equal(61, JsonDepth(Payload(result, route)));
                    var principal = Payload(result, route).GetProperty("roleAssignment").GetProperty("principal");
                    for (var index = 1; index < 30; index++)
                    {
                        principal = principal.GetProperty("servicePrincipalProfileDetails").GetProperty("parentPrincipal");
                    }
                    Assert.Equal("User", principal.GetProperty("type").GetString());
                    Assert.False(principal.TryGetProperty("userDetails", out _));
                    if (route == "namespace" && output.HasValue)
                    {
                        Assert.Equal(64, AssertWireSerializable(result));
                    }
                }
                AssertWireSerializable(result);
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_ExactResponseByteLimitSurvivesEveryRoutingAndOutputShape(string route)
    {
        const int limit = 1024 * 1024;
        foreach (var output in new StructuredOutputMode?[] { null, StructuredOutputMode.Compact, StructuredOutputMode.Duplicated })
        {
            foreach (var excess in new[] { 0, 1 })
            {
                using var stream = new PaddedJsonTestStream(Encoding.UTF8.GetBytes(AssignmentJson), limit + excess);
                using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
                response.Content.Headers.ContentType = new("application/json");
                response.Headers.TryAddWithoutValidation("Retry-After", "45");
                using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
                await using var provider = Services(handler, Credential(), TransportTypes.Http, output);
                var result = await CallAsync(provider, route, Arguments(), Consent());
                if (excess == 0)
                {
                    Assert.False(result.IsError, Text(result));
                    Assert.Equal(AssignmentId, Payload(result, route).GetProperty("roleAssignment").GetProperty("id").GetString());
                }
                else
                {
                    AssertSafeFailure(result, HttpStatusCode.BadGateway);
                    Assert.Contains("confirmed role-assignment update (HTTP 200)", Text(result));
                    Assert.Contains("Do not resubmit", Text(result));
                    Assert.Contains("Wait at least 45 seconds", Text(result));
                }
                AssertWireSerializable(result);
                Assert.Equal(limit + excess, stream.BytesRead);
                Assert.True(stream.Disposed);
                Assert.Equal(1, handler.CallCount);
            }
        }
    }

    [Fact]
    public static async Task Update_GetRemainsASeparateExplicitRequestNotHiddenVerification()
    {
        var requests = new List<string>();
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            requests.Add($"{request.Method} {request.RequestUri!.AbsoluteUri}");
            return Task.FromResult(Response(AssignmentJson));
        });
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var updated = await CallAsync(provider, "all", Arguments(), Consent());
        Assert.False(updated.IsError, Text(updated));
        Assert.Equal(["PATCH " + AssignmentUrl], requests);
        var get = await Loader(provider, "all").CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_get-workspace-role-assignment", new Dictionary<string, object?>
            {
                ["workspace-id"] = WorkspaceId,
                ["workspace-role-assignment-id"] = AssignmentId
            }), TestContext.Current.CancellationToken);
        Assert.False(get.IsError, Text(get));
        Assert.True(JsonElement.DeepEquals(Payload(updated, "all").GetProperty("roleAssignment"), Payload(get, "all").GetProperty("roleAssignment")));
        Assert.Equal(["PATCH " + AssignmentUrl, "GET " + AssignmentUrl], requests);
    }

    [Fact]
    public static async Task Update_RegisteredCliRejectsRepeatedAndValuelessOptionsBeforeIo()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        var credential = Credential();
        await using var provider = Services(handler, credential, TransportTypes.StdIo, StructuredOutputMode.Compact);
        var factory = provider.GetRequiredService<ICommandFactory>();
        var args = new[] { "core", "update-workspace-role-assignment", "--workspace-id", WorkspaceId, "--workspace-role-assignment-id", AssignmentId, "--role", "Viewer" };
        var parsed = factory.RootCommand.Parse(args);
        Assert.Empty(parsed.Errors);
        Assert.Same(provider.GetRequiredService<WorkspaceRoleAssignmentUpdateCommand>().GetCommand(), parsed.CommandResult.Command);
        foreach (var option in new[] { "--workspace-id", "--workspace-role-assignment-id", "--role" })
        {
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option, "private"]).Errors);
            Assert.NotEmpty(factory.RootCommand.Parse([.. args, option]).Errors);
        }
        Assert.NotEmpty(factory.RootCommand.Parse(["jobs", "update-workspace-role-assignment"]).Errors);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("namespace")]
    [InlineData("single")]
    public static async Task Update_PreservesSharedScalarBindingWithoutIdentityResolution(string route)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(Response(AssignmentJson)));
        await using var provider = Services(handler, Credential(), TransportTypes.Http, StructuredOutputMode.Compact);
        var args = Arguments().ToDictionary(pair => pair.Key, pair => (object?)new[] { (string)pair.Value! });
        Assert.False((await CallAsync(provider, route, args, Consent())).IsError);
        Assert.Equal(1, handler.CallCount);
    }

    private static void AssertInputSchema(JsonElement schema)
    {
        Assert.Equal(["role", "workspace-id", "workspace-role-assignment-id"], schema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
        var properties = schema.GetProperty("properties");
        Assert.Equal(["role", "workspace-id", "workspace-role-assignment-id"], properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.All(properties.EnumerateObject(), property =>
        {
            Assert.Equal("string", property.Value.GetProperty("type").GetString());
            Assert.False(property.Value.TryGetProperty("default", out _));
        });
        Assert.False(properties.GetProperty("role").TryGetProperty("enum", out _));
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

    private static int AssertWireSerializable(CallToolResult result)
    {
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
        using var document = JsonDocument.Parse(wire.ToJsonString(new JsonSerializerOptions { MaxDepth = 64 }));
        var depth = JsonDepth(document.RootElement);
        Assert.InRange(depth, 1, 64);
        return depth;
    }

    private static ServiceProvider Services(HttpMessageHandler handler, TokenCredential credential, string transport,
        StructuredOutputMode? output, string selection = "core", bool readOnly = false)
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
            Description = "Offline workspace role-assignment update tests",
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

    private static async Task<JsonElement[]> LearnAsync(IServiceProvider provider, string route, string area = "core")
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

    private static Task<CallToolResult> CallAsync(IServiceProvider provider, string route, Dictionary<string, object?> arguments,
        McpServer server, CancellationToken? token = null)
    {
        var name = ToolName;
        if (route != "all")
        {
            arguments = new() { ["intent"] = "Update only the explicitly identified workspace role assignment", ["command"] = ToolName, ["parameters"] = arguments };
            name = route == "single" ? "fabric" : "core";
            if (route == "single")
            {
                arguments["tool"] = "core";
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

    private static Dictionary<string, object?> Arguments(string role = "Viewer") =>
        new() { ["workspace-id"] = WorkspaceId, ["workspace-role-assignment-id"] = AssignmentId, ["role"] = role };

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
