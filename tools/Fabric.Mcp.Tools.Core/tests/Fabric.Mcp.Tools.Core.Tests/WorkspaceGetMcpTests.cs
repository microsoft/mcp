// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public class WorkspaceGetMcpTests()
{
    [Theory]
    [InlineData("")]
    [InlineData(""","type":null""")]
    public async Task GetWorkspaceTool_AllowsMissingOrNullType(string typeProperty)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""{"id":"{{WorkspaceTestData.WorkspaceId}}","displayName":"Finance","description":""{{typeProperty}}}""")
        };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateToolServices(handler, WorkspaceTestData.CreateCredential(), StructuredOutputMode.Duplicated);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var schema = Assert.Single(tools.Tools, tool => tool.Name == "core_get-workspace").OutputSchema!.Value;
        Assert.Equal(["displayName", "id"], schema.GetProperty("properties").GetProperty("workspace")
            .GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-workspace",
            new Dictionary<string, object?> { ["workspace-id"] = WorkspaceTestData.WorkspaceId }), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var workspace = result.StructuredContent!.Value.GetProperty("workspace");
        Assert.Equal(WorkspaceTestData.WorkspaceId, workspace.GetProperty("id").GetString());
        Assert.False(workspace.TryGetProperty("type", out _));
        using var envelope = JsonDocument.Parse(Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.True(JsonElement.DeepEquals(envelope.RootElement.GetProperty("results"), result.StructuredContent.Value));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, TransportTypes.StdIo, null)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo, true)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo, false)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http, null)]
    public async Task GetWorkspaceTool_ReturnsMetadataThroughRegisteredHandlers(
        StructuredOutputMode? mode, string transport, bool? preference)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(WorkspaceTestData.FullJson) };
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            var query = preference is { } prefer ? $"?preferWorkspaceSpecificEndpoints={(prefer ? "true" : "false")}" : "";
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/workspaces/{WorkspaceTestData.WorkspaceId}{query}", request.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            return Task.FromResult(response);
        });
        var credential = WorkspaceTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools, candidate => candidate.Name == "core_get-workspace");
        Assert.DoesNotContain(tools.Tools, candidate => candidate.Name == "core_create-item");
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal("workspace-id", Assert.Single(tool.InputSchema.GetProperty("required").EnumerateArray()).GetString());
        Assert.Equal(["prefer-workspace-specific-endpoints", "workspace-id"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).OrderBy(name => name));
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var arguments = new Dictionary<string, object?>
        {
            ["workspace-id"] = $"{{{WorkspaceTestData.WorkspaceId.ToUpperInvariant()}}}"
        };
        if (preference is not null)
        {
            arguments["prefer-workspace-specific-endpoints"] = preference.Value;
        }
        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_get-workspace", arguments), TestContext.Current.CancellationToken);

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
            Assert.True(tool.OutputSchema.Value.GetProperty("properties").TryGetProperty("workspace", out _));
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Compact)
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain("Finance", text);
            }
            else
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
        }

        Assert.Equal("workspace", Assert.Single(payload.EnumerateObject()).Name);
        var workspace = payload.GetProperty("workspace");
        Assert.Equal(WorkspaceTestData.WorkspaceId, workspace.GetProperty("id").GetString());
        Assert.Equal("Finance", workspace.GetProperty("displayName").GetString());
        Assert.Equal("Workspace", workspace.GetProperty("type").GetString());
        Assert.Equal(WorkspaceTestData.RelatedId, workspace.GetProperty("capacityId").GetString());
        Assert.Equal(WorkspaceTestData.RelatedId, workspace.GetProperty("domainId").GetString());
        Assert.Equal(WorkspaceTestData.RelatedId, workspace.GetProperty("workspaceIdentity").GetProperty("applicationId").GetString());
        Assert.Equal("Finance", Assert.Single(workspace.GetProperty("tags").EnumerateArray()).GetProperty("displayName").GetString());
        Assert.Equal(["apiEndpoint", "capacityAssignmentProgress", "capacityId", "capacityRegion", "description", "displayName", "domainId", "id", "oneLakeEndpoints", "tags", "type", "workspaceIdentity"],
            workspace.EnumerateObject().Select(property => property.Name).OrderBy(name => name));
        Assert.DoesNotContain("excluded-backend-property", text);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.NotFound, null, "not found")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.NotFound, null, "not found")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.NotFound, null, "not found")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header-detail", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "-1", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.OK, null, "invalid workspace metadata")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Accepted, null, "invalid workspace metadata")]
    public async Task GetWorkspaceTool_ReturnsSanitizedFailuresThroughRegisteredHandlers(
        StructuredOutputMode? mode, HttpStatusCode status, string? retryAfter, string expected)
    {
        using var response = new HttpResponseMessage(status) { Content = new StringContent("private-backend-detail") };
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateToolServices(handler, WorkspaceTestData.CreateCredential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-workspace",
            new Dictionary<string, object?> { ["workspace-id"] = WorkspaceTestData.WorkspaceId }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        var expectedStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status;
        Assert.Equal((int)expectedStatus, document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(expected, document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private-backend-detail", text);
        Assert.DoesNotContain("private-header-detail", text);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Finance")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData(WorkspaceTestData.WorkspaceId + "/items")]
    public async Task GetWorkspaceTool_RejectsInvalidIdBeforeAuthentication(string? workspaceId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        var credential = WorkspaceTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = new Dictionary<string, object?>();
        if (workspaceId is not null)
        {
            arguments["workspace-id"] = workspaceId;
        }

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_get-workspace", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    private static ServiceProvider CreateToolServices(
        HttpMessageHandler handler, TokenCredential credential, StructuredOutputMode? mode, string transport = TransportTypes.StdIo)
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
            Description = "Offline Fabric Core handler tests",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            ReadOnly = true,
            StructuredOutputMode = mode,
            Transport = transport
        }));
        services.AddCommandContextAccessor();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }
}
