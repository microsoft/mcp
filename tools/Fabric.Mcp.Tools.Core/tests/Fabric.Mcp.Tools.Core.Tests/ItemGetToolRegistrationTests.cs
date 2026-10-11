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

public class ItemGetToolRegistrationTests()
{
    private const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    private const string ItemId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";
    private const string ItemJson = """
        {
            "id": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
            "workspaceId": "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa",
            "displayName": "Sales Lakehouse",
            "description": "Test metadata",
            "type": "Lakehouse",
            "definition": { "payload": "excluded-definition" },
            "defaultIdentity": { "displayName": "excluded-identity" }
        }
        """;

    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(null, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.Http)]
    public async Task GetItemTool_ReturnsMetadataThroughRegisteredPipeline(StructuredOutputMode? mode, string transport)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ItemJson) };
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{WorkspaceId}/items/{ItemId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            return Task.FromResult(response);
        });
        var credential = CreateCredential();
        await using var provider = CreateToolServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools, candidate => candidate.Name == "core_get-item");

        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(["item-id", "workspace-id"], tool.InputSchema.GetProperty("required").EnumerateArray()
            .Select(option => option.GetString()).Order());
        Assert.Equal(["item-id", "workspace-id"], tool.InputSchema.GetProperty("properties").EnumerateObject()
            .Select(property => property.Name).Order());
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        if (tool.OutputSchema is { } schema)
        {
            Assert.Equal("item", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            Assert.Equal("item", Assert.Single(schema.GetProperty("properties").EnumerateObject()).Name);
            var itemSchema = schema.GetProperty("properties").GetProperty("item");
            Assert.Equal(["displayName", "id", "type", "workspaceId"],
                itemSchema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());
            Assert.Equal(["description", "displayName", "id", "type", "workspaceId"],
                itemSchema.GetProperty("properties").EnumerateObject().Select(property => property.Name).Order());
            Assert.False(itemSchema.GetProperty("properties").GetProperty("type").TryGetProperty("enum", out _));
        }
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-item", new Dictionary<string, object?>
        {
            ["workspace-id"] = $"{{{WorkspaceId.ToUpperInvariant()}}}",
            ["item-id"] = Guid.Parse(ItemId).ToString("N").ToUpperInvariant()
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
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Compact)
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain("Sales Lakehouse", text);
            }
            else
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
        }

        Assert.Equal("item", Assert.Single(payload.EnumerateObject()).Name);
        var item = payload.GetProperty("item");
        Assert.Equal(ItemId, item.GetProperty("id").GetString());
        Assert.Equal(WorkspaceId, item.GetProperty("workspaceId").GetString());
        Assert.Equal("Sales Lakehouse", item.GetProperty("displayName").GetString());
        Assert.Equal("Lakehouse", item.GetProperty("type").GetString());
        Assert.Equal("Test metadata", item.GetProperty("description").GetString());
        Assert.Equal(["description", "displayName", "id", "type", "workspaceId"],
            item.EnumerateObject().Select(property => property.Name).Order());
        Assert.DoesNotContain("excluded-", text);
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.SequenceEqual(FabricEndpoints.FabricScopes)),
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.NotFound, null, "The Fabric item was not found")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.NotFound, null, "The Fabric item was not found")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.NotFound, null, "The Fabric item was not found")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, "Access denied")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.ServiceUnavailable, null, "Unable to retrieve")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds before retrying")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header-detail", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "-1", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "60, 120", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "9223372036854775807", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, null, "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.OK, null, "invalid item metadata")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Accepted, null, "invalid item metadata")]
    public async Task GetItemTool_ReturnsSanitizedFailuresThroughRegisteredPipeline(
        StructuredOutputMode? mode, HttpStatusCode statusCode, string? retryAfter, string expectedMessage)
    {
        using var response = new HttpResponseMessage(statusCode) { Content = new StringContent(FabricCoreErrorTestData.PrivateDetails) };
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateToolServices(handler, CreateCredential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-item", new Dictionary<string, object?>
        {
            ["workspace-id"] = WorkspaceId,
            ["item-id"] = ItemId
        }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        var expectedStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : statusCode;
        Assert.Equal((int)expectedStatus, document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(expectedMessage, document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stackTrace", text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("item-id", null)]
    [InlineData("item-id", "")]
    [InlineData("item-id", "not-a-guid")]
    [InlineData("item-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("item-id", ItemId + "/getDefinition")]
    [InlineData("workspace-id", null)]
    [InlineData("workspace-id", "private-input")]
    [InlineData("workspace-id", "00000000-0000-0000-0000-000000000000")]
    public async Task GetItemTool_RejectsInvalidInputBeforeAuthentication(string option, string? value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called."));
        var credential = CreateCredential();
        await using var provider = CreateToolServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = new Dictionary<string, object?> { ["workspace-id"] = WorkspaceId, ["item-id"] = ItemId };
        if (value is null)
        {
            arguments.Remove(option);
        }
        else
        {
            arguments[option] = value;
        }

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_get-item", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private", text, StringComparison.OrdinalIgnoreCase);
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
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        return services.BuildServiceProvider();
    }

    private static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        return credential;
    }
}
