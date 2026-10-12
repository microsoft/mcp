// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Services;
using Fabric.Mcp.Tools.Core.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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

public sealed class ItemUpdateToolRegistrationTests()
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(StructuredOutputMode.Compact, false)]
    [InlineData(StructuredOutputMode.Duplicated, false)]
    [InlineData(null, true)]
    [InlineData(StructuredOutputMode.Compact, true)]
    [InlineData(StructuredOutputMode.Duplicated, true)]
    public static async Task RegisteredTool_AdvertisesSchemasAndPreservesEmptyDescription(StructuredOutputMode? mode, bool http)
    {
        using var response = ItemUpdateTestData.Response(json: $$"""
            {
              "id": "{{ItemUpdateTestData.ItemId}}",
              "displayName": "Updated item",
              "type": "FutureItemType",
              "workspaceId": "{{ItemUpdateTestData.WorkspaceId}}",
              "description": "",
              "definition": {"payload": "{{ItemUpdateTestData.PrivateDetails}}"},
              "defaultIdentity": {"id": "{{ItemUpdateTestData.PrivateDetails}}"}
            }
            """);
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal("""{"description":""}""", await request.Content!.ReadAsStringAsync(token));
            return response;
        });
        var credential = ItemUpdateTestData.Credential();
        using var provider = CreateProvider(handler, credential);
        var configuration = new ServerRuntimeConfiguration { StructuredOutputMode = mode, DangerouslyDisableElicitation = true };
        if (http)
        {
            configuration.Transport = TransportTypes.Http;
        }
        var loader = CreateLoader(provider, configuration);

        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(catalog.Tools, t => t.Name == "core_update-item");

        Assert.True(tool.Annotations!.DestructiveHint);
        Assert.True(tool.Annotations.IdempotentHint);
        Assert.False(tool.Annotations.ReadOnlyHint);
        Assert.False(tool.Annotations.OpenWorldHint);
        var schema = tool.InputSchema;
        Assert.Equal(["item-id", "workspace-id"], schema.GetProperty("required").EnumerateArray().Select(p => p.GetString()).Order());
        Assert.Equal(
            ["description", "display-name", "item-id", "workspace-id"],
            schema.GetProperty("properties").EnumerateObject().Select(p => p.Name).Order());
        if (mode is not null)
        {
            var itemSchema = tool.OutputSchema!.Value.GetProperty("properties").GetProperty("item");
            Assert.Equal(
                ["description", "displayName", "id", "type", "workspaceId"],
                itemSchema.GetProperty("properties").EnumerateObject().Select(p => p.Name).Order());
        }
        else
        {
            Assert.Null(tool.OutputSchema);
        }

        var result = await loader.CallToolHandler(CreateCall(UpdateArguments()), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, text);
        if (mode is not null)
        {
            var item = result.StructuredContent!.Value.GetProperty("item");
            Assert.Equal("", item.GetProperty("description").GetString());
            Assert.Equal("FutureItemType", item.GetProperty("type").GetString());
            Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, item.GetRawText());
        }
        else
        {
            Assert.Null(result.StructuredContent);
        }
        if (mode != StructuredOutputMode.Compact)
        {
            using var document = JsonDocument.Parse(text);
            Assert.Equal("", document.RootElement.GetProperty("results").GetProperty("item").GetProperty("description").GetString());
        }
        Assert.Equal(1, handler.CallCount);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static async Task RegisteredTool_PreservesOmittedAndNullDescription(bool explicitNull)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal("""{"displayName":"Renamed"}""", await request.Content!.ReadAsStringAsync(token));
            return ItemUpdateTestData.Response();
        });
        using var provider = CreateProvider(handler, ItemUpdateTestData.Credential());
        var loader = CreateLoader(provider, new() { DangerouslyDisableElicitation = true });
        var arguments = UpdateArguments();
        arguments.Remove("description");
        arguments["display-name"] = String("Renamed");
        if (explicitNull)
        {
            using var document = JsonDocument.Parse("null");
            arguments["description"] = document.RootElement.Clone();
        }

        var result = await loader.CallToolHandler(CreateCall(arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("workspace-id", "not-a-uuid")]
    [InlineData("item-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("display-name", " ")]
    [InlineData("definition", "{}")]
    [InlineData("tags", "[]")]
    [InlineData("default-identity", "{}")]
    public static async Task RegisteredTool_RejectsInvalidInputBeforeCredentialsOrHttp(string property, string value)
    {
        var credential = ItemUpdateTestData.Credential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var provider = CreateProvider(handler, credential);
        var arguments = UpdateArguments();
        arguments[property] = String(value);

        var result = await CreateLoader(provider, new() { DangerouslyDisableElicitation = true })
            .CallToolHandler(CreateCall(arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Fact]
    public static async Task RegisteredTool_ReadOnlyModeHidesAndRejectsMutation()
    {
        var credential = ItemUpdateTestData.Credential();
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("No HTTP expected."));
        using var provider = CreateProvider(handler, credential);
        var loader = CreateLoader(provider, new() { ReadOnly = true });

        var catalog = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var result = await loader.CallToolHandler(CreateCall(UpdateArguments()), TestContext.Current.CancellationToken);

        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_update-item");
        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_create-item");
        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_create-workspace");
        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_update-workspace");
        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_delete-item");
        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_delete-workspace");
        Assert.DoesNotContain(catalog.Tools, t => t.Name == "core_assign-workspace-to-capacity");
        Assert.Contains(catalog.Tools, t => t.Name == "core_search-catalog");
        Assert.All(catalog.Tools, t => Assert.True(t.Annotations!.ReadOnlyHint));
        Assert.True(result.IsError);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public static async Task RegisteredTool_PreservesFailureStatusWithoutLeakingResponse(HttpStatusCode status)
    {
        using var response = ItemUpdateTestData.Response(status, ItemUpdateTestData.PrivateDetails);
        response.Headers.TryAddWithoutValidation("Retry-After", "7");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        using var provider = CreateProvider(handler, ItemUpdateTestData.Credential());
        var loader = CreateLoader(provider, new() { StructuredOutputMode = StructuredOutputMode.Compact, DangerouslyDisableElicitation = true });

        var result = await loader.CallToolHandler(CreateCall(UpdateArguments()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        Assert.DoesNotContain(ItemUpdateTestData.PrivateDetails, text);
        using var document = JsonDocument.Parse(text);
        Assert.Equal((int)status, document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains("Wait at least 7 seconds", document.RootElement.GetProperty("message").GetString());
        Assert.Equal(1, handler.CallCount);
    }

    private static ServiceProvider CreateProvider(FabricCoreHttpMessageHandler handler, TokenCredential credential)
    {
        var services = new ServiceCollection();
        var setup = new FabricCoreSetup();
        services.AddLogging();
        services.AddSingleton<IAreaSetup>(setup);
        services.AddSingleton(credential);
        setup.ConfigureServices(services);
        services.AddHttpClient<IFabricCoreService, FabricCoreService>().ConfigurePrimaryHttpMessageHandler(() => handler);
        services.AddSingleton(Substitute.For<ITelemetryService>());
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Fabric MCP Server",
            Version = "test",
            Description = "Offline Fabric Core tests",
            IsTelemetryEnabled = false
        }));
        services.AddCommandContextAccessor();
        services.AddSingleton<ICommandFactory, CommandFactory>();
        return services.BuildServiceProvider();
    }

    private static CommandFactoryToolLoader CreateLoader(ServiceProvider provider, ServerRuntimeConfiguration configuration) =>
        new(provider.GetRequiredService<ICommandContextAccessor>(),
            provider.GetRequiredService<ICommandFactory>(),
            Microsoft.Extensions.Options.Options.Create(configuration),
            NullLogger<CommandFactoryToolLoader>.Instance);

    private static RequestContext<CallToolRequestParams> CreateCall(Dictionary<string, JsonElement> arguments) =>
        McpTestUtilities.CreateToolCallRequest(
            new CallToolRequestParams { Name = "core_update-item", Arguments = arguments }, Substitute.For<McpServer>());

    private static Dictionary<string, JsonElement> UpdateArguments() => new()
    {
        ["workspace-id"] = String(ItemUpdateTestData.WorkspaceId),
        ["item-id"] = String(ItemUpdateTestData.ItemId),
        ["description"] = String("")
    };

    private static JsonElement String(string value) => JsonSerializer.SerializeToElement(value, CoreJsonContext.Default.String);
}
