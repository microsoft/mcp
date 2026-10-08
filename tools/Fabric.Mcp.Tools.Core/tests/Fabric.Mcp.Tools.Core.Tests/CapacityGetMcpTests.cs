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

public class CapacityGetMcpTests()
{
    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    public async Task GetCapacityTool_ReturnsTypedMetadataThroughRegisteredHandlers(StructuredOutputMode? mode, string transport)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(CapacityGetTestData.FullJson) };
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal($"{FabricEndpoints.FabricApiBaseUrl}/capacities/{CapacityGetTestData.CapacityId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Null(request.Content);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            return Task.FromResult(response);
        });
        var credential = CapacityGetTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(tools.Tools, candidate => candidate.Name == "core_get-capacity");
        Assert.DoesNotContain(tools.Tools, candidate => candidate.Name == "core_create-item");
        Assert.Contains(tools.Tools, candidate => candidate.Name == "core_search-catalog");
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal("capacity-id", Assert.Single(tool.InputSchema.GetProperty("required").EnumerateArray()).GetString());
        var inputProperty = Assert.Single(tool.InputSchema.GetProperty("properties").EnumerateObject());
        Assert.Equal("capacity-id", inputProperty.Name);
        Assert.Equal("string", inputProperty.Value.GetProperty("type").GetString());
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-capacity",
            new Dictionary<string, object?> { ["capacity-id"] = $"{{{CapacityGetTestData.CapacityId.ToUpperInvariant()}}}" }), TestContext.Current.CancellationToken);

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
            var capacitySchema = tool.OutputSchema.Value.GetProperty("properties").GetProperty("capacity");
            Assert.Equal(["displayName", "id", "region", "sku", "state"],
                capacitySchema.GetProperty("required").EnumerateArray().Select(property => property.GetString()).OrderBy(name => name));
            Assert.Equal("string", capacitySchema.GetProperty("properties").GetProperty("state").GetProperty("type").GetString());
            Assert.False(capacitySchema.GetProperty("properties").GetProperty("state").TryGetProperty("enum", out _));
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Compact)
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain("F4 Capacity", text);
            }
            else
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
        }

        Assert.Equal("capacity", Assert.Single(payload.EnumerateObject()).Name);
        var capacity = payload.GetProperty("capacity");
        Assert.Equal(CapacityGetTestData.CapacityId, capacity.GetProperty("id").GetString());
        Assert.Equal("F4 Capacity", capacity.GetProperty("displayName").GetString());
        Assert.Equal("F4", capacity.GetProperty("sku").GetString());
        Assert.Equal("West Central US", capacity.GetProperty("region").GetString());
        Assert.Equal("Active", capacity.GetProperty("state").GetString());
        Assert.Equal(["displayName", "id", "region", "sku", "state"],
            capacity.EnumerateObject().Select(property => property.Name).OrderBy(name => name));
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
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Unauthorized, null, "Authentication failed")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, "Capacity.Read.All")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "120", "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header-detail", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "-1", "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.OK, null, "invalid capacity metadata")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Accepted, null, "invalid capacity metadata")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.ServiceUnavailable, null, "Unable to retrieve")]
    public async Task GetCapacityTool_ReturnsSanitizedFailuresThroughRegisteredHandlers(
        StructuredOutputMode? mode, HttpStatusCode status, string? retryAfter, string expected)
    {
        using var response = new HttpResponseMessage(status) { Content = new StringContent("private-backend-detail") };
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateToolServices(handler, CapacityGetTestData.CreateCredential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-capacity",
            new Dictionary<string, object?> { ["capacity-id"] = CapacityGetTestData.CapacityId }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        Assert.Equal((int)(response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : status), document.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(expected, document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain("private-backend-detail", text);
        Assert.DoesNotContain("private-header-detail", text);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Finance")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData(CapacityGetTestData.CapacityId + "/items")]
    [InlineData(CapacityGetTestData.CapacityId + "?unexpected=true")]
    public async Task GetCapacityTool_RejectsInvalidIdBeforeAuthentication(string? capacityId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        var credential = CapacityGetTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var arguments = new Dictionary<string, object?>();
        if (capacityId is not null)
        {
            arguments["capacity-id"] = capacityId;
        }

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_get-capacity", arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(42)]
    [InlineData(true)]
    public async Task GetCapacityTool_RejectsNonStringIdsBeforeAuthentication(object? capacityId)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        var credential = CapacityGetTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-capacity",
            new Dictionary<string, object?> { ["capacity-id"] = capacityId }), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    [Theory]
    [InlineData("capacity")]
    [InlineData("subscription")]
    [InlineData("tenant")]
    [InlineData("url")]
    [InlineData("retry-policy")]
    public async Task GetCapacityTool_RejectsUnknownArgumentsBeforeAuthentication(string argument)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("HTTP must not be called"));
        var credential = CapacityGetTestData.CreateCredential();
        await using var provider = CreateToolServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(McpTestUtilities.CreateToolCallRequest("core_get-capacity",
            new Dictionary<string, object?> { ["capacity-id"] = CapacityGetTestData.CapacityId, [argument] = "unexpected" }), TestContext.Current.CancellationToken);

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
        services.AddSingleton<ICommandFactory, CommandFactory>();
        services.AddSingleton<CommandFactoryToolLoader>();
        services.AddSingleton<ICommandContextAccessor, CommandContextAccessor>();
        return services.BuildServiceProvider();
    }
}
