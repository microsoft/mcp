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

public class WorkspaceAssignToCapacityToolRegistrationTests()
{
    private const string ToolName = "core_assign-workspace-to-capacity";
    private const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff512";
    private const string CapacityId = "0f084df7-c13d-451b-af5f-ed0c466403b2";

    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(null, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.Http)]
    public async Task RegisteredAssignment_RequiresConsentAndReturnsPendingAfterOnePost(
        StructuredOutputMode? mode, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceId}/assignToCapacity", request.RequestUri?.AbsoluteUri);
            Assert.Equal("assignment-test-token", request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            using var body = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
            Assert.Single(body.RootElement.EnumerateObject());
            Assert.Equal(CapacityId, body.RootElement.GetProperty("capacityId").GetString());
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });
        var credential = CreateCredential();
        await using var provider = CreateServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, tool => tool.Name == ToolName);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.Equal(["capacity-id", "workspace-id"],
            tool.InputSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()).Order());
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);

        var server = CreateConsentServer();
        var result = await loader.CallToolHandler(CreateRequest(server), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        JsonElement receipt;
        if (mode == StructuredOutputMode.Compact)
        {
            Assert.NotNull(result.StructuredContent);
            receipt = result.StructuredContent.Value;
            Assert.DoesNotContain(WorkspaceId, text);
        }
        else
        {
            using var document = JsonDocument.Parse(text);
            Assert.Equal(202, document.RootElement.GetProperty("status").GetInt32());
            receipt = document.RootElement.GetProperty("results").Clone();
            if (mode == StructuredOutputMode.Duplicated)
            {
                Assert.NotNull(result.StructuredContent);
                Assert.True(JsonElement.DeepEquals(receipt, result.StructuredContent.Value));
            }
            else
            {
                Assert.Null(result.StructuredContent);
            }
        }
        Assert.Equal(WorkspaceId, receipt.GetProperty("workspaceId").GetString());
        Assert.Equal(CapacityId, receipt.GetProperty("capacityId").GetString());
        Assert.True(receipt.GetProperty("accepted").GetBoolean());
        Assert.Equal("Pending", receipt.GetProperty("state").GetString());
        Assert.Equal(4, receipt.EnumerateObject().Count());
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
        await server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(request => request.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CumulativeCoreDiscovery_PreservesAllSchemasAndReadOnlyFiltering(bool readOnly)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Discovery must not send HTTP."));
        var credential = CreateCredential();
        await using var provider = CreateServices(handler, credential, StructuredOutputMode.Compact, readOnly: readOnly);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var factory = provider.GetRequiredService<ICommandFactory>();

        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

        string[] readOnlyNames =
        [
            "core_get-capacity", "core_get-item", "core_get-workspace", "core_list-capacities",
            "core_list-items", "core_list-workspaces", "core_search-catalog"
        ];
        string[] allNames =
        [
            ToolName, "core_create-item", "core_create-workspace", "core_delete-item", "core_delete-workspace",
            "core_get-capacity", "core_get-item", "core_get-workspace", "core_list-capacities", "core_list-items",
            "core_list-workspaces", "core_search-catalog", "core_update-item", "core_update-workspace"
        ];
        Assert.Equal(readOnly ? readOnlyNames : allNames, catalog.Tools.Select(tool => tool.Name).Order());
        foreach (var tool in catalog.Tools)
        {
            var command = factory.AllCommands[tool.Name];
            Assert.Equal(command.Metadata.ReadOnly, tool.Annotations?.ReadOnlyHint);
            Assert.Equal(command.Metadata.Destructive, tool.Annotations?.DestructiveHint);
            Assert.Equal(command.Metadata.Idempotent, tool.Annotations?.IdempotentHint);
            Assert.Equal(command.ResultTypeInfo is not null, tool.OutputSchema.HasValue);
            Assert.Equal(command.GetCommand().Options.Where(option => option.Name != "--learn")
                .Select(option => option.Name.TrimStart('-')).Order(),
                tool.InputSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
        }
        if (readOnly)
        {
            var result = await loader.CallToolHandler(CreateRequest(CreateConsentServer()), TestContext.Current.CancellationToken);
            Assert.True(result.IsError);
            Assert.Contains("read-only", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        }
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
    public async Task AssignmentConsent_RejectionStopsBeforeAuthentication(string action, string? decision)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unapproved assignment must not send HTTP."));
        var credential = CreateCredential();
        await using var provider = CreateServices(handler, credential);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>()
            .CallToolHandler(CreateRequest(CreateConsentServer(action, decision)), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("cancelled by user", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public async Task AssignmentConsent_UnsupportedClientCannotSubmit()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unapproved assignment must not send HTTP."));
        var credential = CreateCredential();
        await using var provider = CreateServices(handler, credential);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>()
            .CallToolHandler(CreateRequest(Substitute.For<McpServer>()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("does not support elicitation", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("", CapacityId)]
    [InlineData("not-a-guid", CapacityId)]
    [InlineData("00000000-0000-0000-0000-000000000000", CapacityId)]
    [InlineData(WorkspaceId, "")]
    [InlineData(WorkspaceId, "not-a-guid")]
    [InlineData(WorkspaceId, "00000000-0000-0000-0000-000000000000")]
    public async Task RegisteredAssignment_RejectsInvalidIdsBeforeAuthentication(string workspaceId, string capacityId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Invalid assignment must not send HTTP."));
        var credential = CreateCredential();
        await using var provider = CreateServices(handler, credential);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>()
            .CallToolHandler(CreateRequest(CreateConsentServer(), workspaceId, capacityId), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    private static RequestContext<CallToolRequestParams> CreateRequest(
        McpServer server, string workspaceId = WorkspaceId, string capacityId = CapacityId) =>
        McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = ToolName,
            Arguments = new Dictionary<string, JsonElement>
            {
                ["workspace-id"] = JsonSerializer.SerializeToElement(workspaceId, CoreJsonContext.Default.String),
                ["capacity-id"] = JsonSerializer.SerializeToElement(capacityId, CoreJsonContext.Default.String)
            }
        }, server);

    private static McpServer CreateConsentServer(string action = "accept", string? decision = "accept")
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

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("assignment-test-token", DateTimeOffset.MaxValue)));
        return credential;
    }

    private static ServiceProvider CreateServices(
        HttpMessageHandler handler, TokenCredential credential, StructuredOutputMode? mode = null,
        string transport = TransportTypes.StdIo, bool readOnly = false)
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
            DisplayName = "Fabric",
            Version = "1.0.0",
            Description = "Offline assignment test server",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            StructuredOutputMode = mode,
            Transport = transport,
            ReadOnly = readOnly
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }
}
