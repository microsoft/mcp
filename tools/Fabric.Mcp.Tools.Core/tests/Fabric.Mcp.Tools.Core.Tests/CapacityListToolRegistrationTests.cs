// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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

namespace Fabric.Mcp.Tools.Core.Tests;

public class CapacityListToolRegistrationTests()
{
    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    public async Task RegisteredTool_ExposesOptionalTokenAndTypedPage(StructuredOutputMode? mode, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(CapacityListTestData.ContinuationUri, request.RequestUri?.AbsoluteUri);
            Assert.Equal("capacity-test-token", request.Headers.Authorization?.Parameter);
            Assert.Null(request.Content);
            return Task.FromResult(CapacityListTestData.CreateResponse(CapacityListTestData.FullPage));
        });
        var credential = CapacityListTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(listed.Tools, static tool => tool.Name == "core_list-capacities");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_create-item");
        Assert.Contains(listed.Tools, static tool => tool.Name == "core_search-catalog");
        Assert.True(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(
            ["continuation-token"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(static property => property.Name));
        Assert.True(!tool.InputSchema.TryGetProperty("required", out var required) || required.GetArrayLength() == 0);
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_list-capacities", new Dictionary<string, object?>
            {
                ["continuation-token"] = CapacityListTestData.ContinuationToken
            }),
            TestContext.Current.CancellationToken);

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
            Assert.Equal(
                ["capacities", "continuationToken", "continuationUri"],
                tool.OutputSchema.Value.GetProperty("properties").EnumerateObject().Select(static property => property.Name));
            Assert.DoesNotContain("\"enum\"", tool.OutputSchema.Value.GetRawText());
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Compact)
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain("Finance Capacity", text);
            }
            else
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
        }

        var capacity = Assert.Single(payload.GetProperty("capacities").EnumerateArray());
        Assert.Equal(CapacityListTestData.CapacityId, capacity.GetProperty("id").GetString());
        Assert.Equal("Finance Capacity", capacity.GetProperty("displayName").GetString());
        Assert.Equal("FutureSku", capacity.GetProperty("sku").GetString());
        Assert.Equal("Future Region", capacity.GetProperty("region").GetString());
        Assert.Equal("FutureState", capacity.GetProperty("state").GetString());
        Assert.Equal(CapacityListTestData.ContinuationToken, payload.GetProperty("continuationToken").GetString());
        Assert.Equal(CapacityListTestData.ContinuationUri, payload.GetProperty("continuationUri").GetString());
        Assert.DoesNotContain("private-", payload.GetRawText());
        Assert.DoesNotContain("private-", text);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Capacity.Read.All")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Capacity.Read.All")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Capacity.Read.All")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "Wait at least 120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "Tue, 01 Jan 2030 00:00:00 GMT", HttpStatusCode.TooManyRequests, "Retry after 2030-01-01 00:00:00 UTC")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, null, HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header-detail", HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.OK, null, HttpStatusCode.BadGateway, "invalid capacity metadata page")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Accepted, null, HttpStatusCode.BadGateway, "invalid capacity metadata page")]
    public async Task RegisteredTool_SanitizesErrorsAndPreservesStatus(
        StructuredOutputMode? mode, HttpStatusCode upstreamStatus, string? retryAfter, HttpStatusCode expectedStatus, string expectedMessage)
    {
        using var response = CapacityListTestData.CreateResponse("private-backend-detail", upstreamStatus);
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServices(handler, CapacityListTestData.CreateCredential(), mode);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_list-capacities"), TestContext.Current.CancellationToken);

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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RegisteredTool_RejectsBlankTokenWithoutAuthentication(string token)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(CapacityListTestData.CreateResponse()));
        var credential = CapacityListTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, StructuredOutputMode.Compact);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var result = await loader.CallToolHandler(
            McpTestUtilities.CreateToolCallRequest("core_list-capacities", new Dictionary<string, object?> { ["continuation-token"] = token }),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Equal(0, handler.CallCount);
        Assert.Empty(credential.ReceivedCalls());
    }

    private static ServiceProvider CreateServices(
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
            Description = "Fabric Core capacity list test server",
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
