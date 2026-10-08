// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
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

namespace Fabric.Mcp.Tools.Core.Tests;

public class WorkspaceDeleteToolRegistrationTests()
{
    private const string ToolName = "core_delete-workspace";

    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(null, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.Http)]
    public async Task RegisteredTool_AdvertisesDestructiveContractAndReturnsTypedAcknowledgementAfterConsent(
        StructuredOutputMode? mode, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceDeleteTestData.WorkspaceId}", request.RequestUri?.AbsoluteUri);
            Assert.Null(request.Content);
            Assert.Equal(WorkspaceDeleteTestData.Token, request.Headers.Authorization?.Parameter);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var credential = WorkspaceDeleteTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(listed.Tools, static candidate => candidate.Name == ToolName);

        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Contains("AND the items under it", tool.Description);
        Assert.Equal(["workspace-id"], tool.InputSchema.GetProperty("properties").EnumerateObject().Select(static property => property.Name));
        Assert.Equal(["workspace-id"], tool.InputSchema.GetProperty("required").EnumerateArray().Select(static value => value.GetString()));
        Assert.Equal("string", tool.InputSchema.GetProperty("properties").GetProperty("workspace-id").GetProperty("type").GetString());
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var server = CreateElicitationServer();
        var result = await loader.CallToolHandler(CreateRequest(server), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        JsonElement payload;
        if (mode is null)
        {
            Assert.Null(result.StructuredContent);
            using var document = JsonDocument.Parse(text);
            payload = document.RootElement.GetProperty("results").Clone();
        }
        else
        {
            Assert.NotNull(tool.OutputSchema);
            var properties = tool.OutputSchema.Value.GetProperty("properties");
            Assert.Equal(["workspaceId", "deleted"], properties.EnumerateObject().Select(static property => property.Name));
            Assert.Equal("string", properties.GetProperty("workspaceId").GetProperty("type").GetString());
            Assert.Equal("boolean", properties.GetProperty("deleted").GetProperty("type").GetString());
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Compact)
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain(WorkspaceDeleteTestData.WorkspaceId, text);
            }
            else
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
        }

        Assert.Equal(["workspaceId", "deleted"], payload.EnumerateObject().Select(static property => property.Name));
        Assert.Equal(WorkspaceDeleteTestData.WorkspaceId, payload.GetProperty("workspaceId").GetString());
        Assert.True(payload.GetProperty("deleted").GetBoolean());
        Assert.Equal(1, handler.CallCount);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(TransportTypes.StdIo)]
    [InlineData(TransportTypes.Http)]
    public async Task ReadOnlyMode_HidesAndRejectsDeletionBeforeAuthentication(string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var credential = WorkspaceDeleteTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, StructuredOutputMode.Compact, transport, readOnly: true);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == ToolName);
        Assert.Contains(listed.Tools, static tool => tool.Name == "core_search-catalog");
        Assert.Equal(
            ["core_get-capacity", "core_get-workspace", "core_list-capacities", "core_list-items", "core_list-workspaces", "core_search-catalog"],
            listed.Tools.Select(static tool => tool.Name).Order());
        Assert.All(listed.Tools, static tool => Assert.True(tool.Annotations?.ReadOnlyHint));
        var result = await loader.CallToolHandler(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("read-only", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("decline", "accept")]
    [InlineData("cancel", "accept")]
    [InlineData("accept", "reject")]
    [InlineData("accept", null)]
    [InlineData("accept", "")]
    [InlineData("accept", "ACCEPT")]
    public async Task Elicitation_RejectsUnapprovedDecisionsBeforeAuthentication(string action, string? decision)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var credential = WorkspaceDeleteTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(
            CreateRequest(CreateElicitationServer(action, decision)), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("cancelled by user", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task Elicitation_UnsupportedClientCannotDelete()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var credential = WorkspaceDeleteTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task Elicitation_ExistingExplicitDangerousBypassRemainsHostControlled()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        await using var provider = CreateServices(handler, WorkspaceDeleteTestData.CreateCredential(), dangerouslyDisableElicitation: true);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [MemberData(nameof(WorkspaceDeleteTestData.InvalidWorkspaceIds), MemberType = typeof(WorkspaceDeleteTestData))]
    public async Task RegisteredTool_RejectsInvalidWorkspaceBeforeAuthentication(string? workspaceId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var credential = WorkspaceDeleteTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(
            CreateRequest(CreateElicitationServer(), workspaceId), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Admin workspace role")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Admin workspace role")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Admin workspace role")]
    [InlineData(null, HttpStatusCode.NotFound, null, HttpStatusCode.NotFound, "Deletion was not confirmed")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.NotFound, null, HttpStatusCode.NotFound, "Deletion was not confirmed")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.NotFound, null, HttpStatusCode.NotFound, "Deletion was not confirmed")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header", HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "-1", HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", HttpStatusCode.TooManyRequests, "Retry after 2030-01-01")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.InternalServerError, null, HttpStatusCode.InternalServerError, "Unable to confirm")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Accepted, null, HttpStatusCode.BadGateway, "unexpected response")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.NoContent, null, HttpStatusCode.BadGateway, "unexpected response")]
    public async Task RegisteredTool_ReturnsSanitizedFailureWithoutDeletionAcknowledgement(
        StructuredOutputMode? mode, HttpStatusCode upstreamStatus, string? retryAfter, HttpStatusCode expectedStatus, string expectedMessage)
    {
        using var response = new HttpResponseMessage(upstreamStatus) { Content = new StringContent("private-backend-detail") };
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServices(handler, WorkspaceDeleteTestData.CreateCredential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(CreateRequest(CreateElicitationServer()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        Assert.Equal((int)expectedStatus, document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(expectedMessage, document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private-", text);
        Assert.Equal(1, handler.CallCount);
    }

    private static RequestContext<CallToolRequestParams> CreateRequest(
        McpServer? server = null, string? workspaceId = WorkspaceDeleteTestData.WorkspaceId) =>
        McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = ToolName,
            Arguments = workspaceId is null ? [] : new Dictionary<string, JsonElement>
            {
                ["workspace-id"] = JsonSerializer.SerializeToElement(workspaceId, CoreJsonContext.Default.String)
            }
        }, server ?? Substitute.For<McpServer>());

    private static McpServer CreateElicitationServer(string action = "accept", string? decision = "accept")
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new ElicitationCapability { Form = new() } });
        var result = new JsonObject { ["action"] = action };
        if (decision is not null)
        {
            result["content"] = new JsonObject { ["decision"] = decision };
        }
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => new JsonRpcResponse { Id = call.Arg<JsonRpcRequest>().Id, Result = result.DeepClone() });
        return server;
    }

    private static ServiceProvider CreateServices(
        HttpMessageHandler handler,
        TokenCredential credential,
        StructuredOutputMode? mode = null,
        string transport = TransportTypes.StdIo,
        bool readOnly = false,
        bool dangerouslyDisableElicitation = false)
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
            Description = "Fabric Core workspace deletion test server",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            ReadOnly = readOnly,
            StructuredOutputMode = mode,
            Transport = transport,
            DangerouslyDisableElicitation = dangerouslyDisableElicitation
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }
}
