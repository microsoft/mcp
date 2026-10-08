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

public class ItemListMcpTests
{
    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    public async Task ListItemsTool_ExposesSchemaAndMetadataThroughRegisteredPipeline(StructuredOutputMode? mode, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(ItemListTestData.ItemsUrl + "?type=FutureItemType&recursive=false&rootFolderId=" +
                ItemListTestData.FolderId + "&continuationToken=" + ItemListTestData.Token, request.RequestUri?.AbsoluteUri);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            return Task.FromResult(ItemListTestData.Response(ItemListTestData.PageJson));
        });
        var credential = ItemListTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools, candidate => candidate.Name == "core_list-items");
        Assert.DoesNotContain(tools.Tools, candidate => candidate.Name == "core_create-item");
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(["workspace-id"], tool.InputSchema.GetProperty("required").EnumerateArray().Select(value => value.GetString()));
        var properties = tool.InputSchema.GetProperty("properties");
        Assert.Equal(["continuation-token", "recursive", "root-folder-id", "type", "workspace-id"],
            properties.EnumerateObject().Select(property => property.Name).Order());
        Assert.False(properties.GetProperty("type").TryGetProperty("enum", out _));
        Assert.Contains("boolean", properties.GetProperty("recursive").GetRawText());
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_list-items", new Dictionary<string, object?>
        {
            ["workspace-id"] = $"{{{ItemListTestData.WorkspaceId.ToUpperInvariant()}}}",
            ["root-folder-id"] = Guid.Parse(ItemListTestData.FolderId).ToString("N"),
            ["type"] = "FutureItemType",
            ["recursive"] = false,
            ["continuation-token"] = ItemListTestData.Token
        }), TestContext.Current.CancellationToken);

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
            Assert.True(tool.OutputSchema.Value.GetProperty("properties").TryGetProperty("items", out _));
            Assert.DoesNotContain("defaultIdentity", tool.OutputSchema.Value.GetRawText());
            Assert.DoesNotContain("\"definition\"", tool.OutputSchema.Value.GetRawText());
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Compact)
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain("Sales lakehouse", text);
            }
            else
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
        }

        var page = JsonSerializer.Deserialize(payload, CoreJsonContext.Default.ItemListCommandResult);
        Assert.NotNull(page);
        Assert.Equal(Guid.Parse(ItemListTestData.ItemId), Assert.Single(page.Items).Id);
        Assert.Equal(ItemListTestData.Token, page.ContinuationToken);
        Assert.Equal(ItemListTestData.ContinuationUri, page.ContinuationUri);
        var item = Assert.Single(payload.GetProperty("items").EnumerateArray());
        Assert.Equal(["description", "displayName", "folderId", "id", "logicalId", "sensitivityLabel", "tags", "type", "workspaceId"],
            item.EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain("excluded-", payload.GetRawText());
        Assert.DoesNotContain("excluded-", text);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.NotFound, null, "not found")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, "Viewer")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Unauthorized, null, "configured identity")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "2030-01-01")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header-detail", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.OK, null, "invalid item metadata page")]
    public async Task ListItemsTool_ReturnsSanitizedErrors(
        StructuredOutputMode? mode, HttpStatusCode status, string? retryAfter, string expectedMessage)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) =>
        {
            var response = ItemListTestData.Response("private-backend-detail", status);
            if (retryAfter is not null)
            {
                response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
            }
            return Task.FromResult(response);
        });
        await using var provider = CreateToolServices(handler, ItemListTestData.CreateCredential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_list-items", new Dictionary<string, object?>
        {
            ["workspace-id"] = ItemListTestData.WorkspaceId
        }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        Assert.Equal((int)(status == HttpStatusCode.OK ? HttpStatusCode.BadGateway : status), document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(expectedMessage, document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private-", text);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("workspace-id", null)]
    [InlineData("workspace-id", "not-a-uuid")]
    [InlineData("workspace-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("workspace-id", ItemListTestData.WorkspaceId + "/getDefinition")]
    [InlineData("root-folder-id", "../items")]
    [InlineData("type", " ")]
    [InlineData("continuation-token", " ")]
    [InlineData("include", "DefaultIdentity")]
    public async Task ListItemsTool_RejectsInvalidInputBeforeAuthentication(string option, string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not run."));
        var credential = ItemListTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = new Dictionary<string, object?>();
        if (option != "workspace-id")
        {
            arguments["workspace-id"] = ItemListTestData.WorkspaceId;
        }
        if (value is not null)
        {
            arguments[option] = value;
        }

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_list-items", arguments),
            TestContext.Current.CancellationToken);

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
            Description = "Fabric Core offline test server",
            IsTelemetryEnabled = false
        }));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration
        {
            Namespace = ["core"],
            ReadOnly = true,
            StructuredOutputMode = mode,
            Transport = transport
        }));
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }
}
