// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Identity;
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
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public class WorkspaceUpdateToolRegistrationTests()
{
    private const string ClearDescriptionArguments = """
        {"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","description":""}
        """;

    [Theory]
    [InlineData(null, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.StdIo)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.StdIo)]
    [InlineData(null, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Compact, TransportTypes.Http)]
    [InlineData(StructuredOutputMode.Duplicated, TransportTypes.Http)]
    public async Task RegisteredTool_AdvertisesAndReturnsTypedMetadata(StructuredOutputMode? mode, string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Patch, request.Method);
            Assert.Equal($"https://api.fabric.microsoft.com/v1/workspaces/{WorkspaceUpdateTestData.WorkspaceId}", request.RequestUri?.AbsoluteUri);
            Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
            Assert.NotNull(request.Content);
            Assert.Equal("""{"description":""}""", await request.Content.ReadAsStringAsync(token));
            return WorkspaceUpdateTestData.CreateResponse();
        });
        var credential = WorkspaceUpdateTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, mode, transport);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var tool = Assert.Single(listed.Tools, static candidate => candidate.Name == "core_update-workspace");
        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.True(tool.Annotations?.DestructiveHint);
        Assert.True(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        Assert.Equal(
            ["description", "display-name", "workspace-id"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(static property => property.Name).Order());
        Assert.Equal(["workspace-id"], tool.InputSchema.GetProperty("required").EnumerateArray().Select(static value => value.GetString()));
        Assert.Equal(mode.HasValue, tool.OutputSchema.HasValue);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);

        var request = CreateRequest(ClearDescriptionArguments);
        var result = await loader.CallToolHandler(request, TestContext.Current.CancellationToken);

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
            var properties = tool.OutputSchema.Value.GetProperty("properties").GetProperty("workspace").GetProperty("properties");
            Assert.Equal(["description", "displayName", "id", "type"], properties.EnumerateObject().Select(static property => property.Name).Order());
            Assert.NotNull(result.StructuredContent);
            payload = result.StructuredContent.Value;
            if (mode == StructuredOutputMode.Duplicated)
            {
                using var document = JsonDocument.Parse(text);
                Assert.True(JsonElement.DeepEquals(document.RootElement.GetProperty("results"), payload));
            }
            else
            {
                Assert.NotEmpty(text);
                Assert.DoesNotContain("Finance", text);
            }
        }
        var workspace = payload.GetProperty("workspace");
        Assert.Equal(WorkspaceUpdateTestData.WorkspaceId, workspace.GetProperty("id").GetString());
        Assert.Equal("", workspace.GetProperty("description").GetString());
        Assert.Equal("FutureWorkspaceType", workspace.GetProperty("type").GetString());
        Assert.DoesNotContain("excluded-", payload.GetRawText());
        Assert.DoesNotContain("excluded-", text);
        Assert.Equal(1, handler.CallCount);
        await request.Server.Received(1).SendRequestAsync(
            Arg.Is<JsonRpcRequest>(rpc => rpc.Method == "elicitation/create"), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","display-name":"Finance"}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","display-name":"Finance","description":null}""")]
    public async Task RegisteredTool_OmitsUnspecifiedOrNullDescription(string arguments)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, token) =>
        {
            Assert.NotNull(request.Content);
            Assert.Equal("""{"displayName":"Finance"}""", await request.Content.ReadAsStringAsync(token));
            return WorkspaceUpdateTestData.CreateResponse();
        });
        await using var provider = CreateServices(handler, WorkspaceUpdateTestData.CreateCredential());

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            CreateRequest(arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData("""{"description":""}""")]
    [InlineData("""{"workspace-id":"not-a-uuid","description":""}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a"}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","description":null,"display-name":null}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","display-name":""}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","display-name":"Admin monitoring"}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","description":"","capacity-id":"unavailable"}""")]
    [InlineData("""{"workspace-id":"33bae707-5fe7-4352-89bd-061a1318b60a","description":"","endpoint":"https://example.invalid"}""")]
    public async Task RegisteredTool_RejectsInvalidAndUnrelatedInputsBeforeAuthentication(string arguments)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        var credential = WorkspaceUpdateTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            CreateRequest(arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Admin")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Admin")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.Forbidden, null, HttpStatusCode.Forbidden, "Admin")]
    [InlineData(null, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "120 seconds")]
    [InlineData(StructuredOutputMode.Duplicated, HttpStatusCode.TooManyRequests, "120", HttpStatusCode.TooManyRequests, "120 seconds")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.TooManyRequests, "private-header", HttpStatusCode.TooManyRequests, "Wait before retrying")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.OK, null, HttpStatusCode.BadGateway, "invalid workspace update response")]
    [InlineData(StructuredOutputMode.Compact, HttpStatusCode.Accepted, null, HttpStatusCode.BadGateway, "invalid workspace update response")]
    public async Task RegisteredTool_SanitizesFailuresAndDoesNotReturnSuccessShapedContent(
        StructuredOutputMode? mode, HttpStatusCode upstreamStatus, string? retryAfter, HttpStatusCode expectedStatus, string expectedMessage)
    {
        using var response = WorkspaceUpdateTestData.CreateResponse("private-backend-body", upstreamStatus);
        if (retryAfter is not null)
        {
            Assert.True(response.Headers.TryAddWithoutValidation("Retry-After", retryAfter));
        }
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServices(handler, WorkspaceUpdateTestData.CreateCredential(), mode);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            CreateRequest(ClearDescriptionArguments), TestContext.Current.CancellationToken);

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
    [InlineData(TransportTypes.StdIo)]
    [InlineData(TransportTypes.Http)]
    public async Task RegisteredTool_ReadOnlyModeExcludesAndRejectsMutation(string transport)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        var credential = WorkspaceUpdateTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential, transport: transport, readOnly: true);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();

        var listed = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_create-item");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_create-workspace");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_update-item");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_update-workspace");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_delete-item");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_delete-workspace");
        Assert.DoesNotContain(listed.Tools, static tool => tool.Name == "core_assign-workspace-to-capacity");
        Assert.Contains(listed.Tools, static tool => tool.Name == "core_search-catalog");
        var result = await loader.CallToolHandler(CreateRequest(ClearDescriptionArguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData("reject")]
    [InlineData(null)]
    public async Task RegisteredTool_RequiresConsentBeforeAuthentication(string? decision)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        var credential = WorkspaceUpdateTestData.CreateCredential();
        await using var provider = CreateServices(handler, credential);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            CreateRequest(ClearDescriptionArguments, decision), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Empty(credential.ReceivedCalls());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task RegisteredTool_UsesInjectedCredentialWithoutFallbackAndSanitizesAuthFailure()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Unexpected HTTP call."));
        var credential = WorkspaceUpdateTestData.CreateCredential();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Throws(new AuthenticationFailedException("private-token"));
        await using var provider = CreateServices(handler, credential);

        var result = await provider.GetRequiredService<CommandFactoryToolLoader>().CallToolHandler(
            CreateRequest(ClearDescriptionArguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
        using var document = JsonDocument.Parse(text);
        Assert.Equal(401, document.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain("private-", text);
        await credential.Received(1).GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
        Assert.Equal(0, handler.CallCount);
    }

    private static RequestContext<CallToolRequestParams> CreateRequest(string arguments, string? decision = "accept")
    {
        var server = Substitute.For<McpServer>();
        if (decision is not null)
        {
            server.ClientCapabilities.Returns(new ClientCapabilities { Elicitation = new() { Form = new() } });
            server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
                .Returns(new JsonRpcResponse
                {
                    Id = new RequestId(1),
                    Result = new JsonObject
                    {
                        ["action"] = "accept",
                        ["content"] = new JsonObject { ["decision"] = decision }
                    }
                });
        }
        using var document = JsonDocument.Parse(arguments);
        return McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams
        {
            Name = "core_update-workspace",
            Arguments = document.RootElement.EnumerateObject().ToDictionary(static property => property.Name, static property => property.Value.Clone())
        }, server);
    }

    [Theory]
    [InlineData("")]
    [InlineData(""","type":null""")]
    public async Task RegisteredTool_AllowsMissingOrNullTypeOnSuccessfulUpdate(string typeProperty)
    {
        using var response = WorkspaceUpdateTestData.CreateResponse(
            $$"""{"id":"{{WorkspaceUpdateTestData.WorkspaceId}}","displayName":"Finance","description":""{{typeProperty}}}""");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServices(handler, WorkspaceUpdateTestData.CreateCredential(), StructuredOutputMode.Duplicated);
        var loader = provider.GetRequiredService<CommandFactoryToolLoader>();
        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var schema = Assert.Single(tools.Tools, tool => tool.Name == "core_update-workspace").OutputSchema!.Value;
        Assert.Equal(["displayName", "id"], schema.GetProperty("properties").GetProperty("workspace")
            .GetProperty("required").EnumerateArray().Select(property => property.GetString()).Order());

        var result = await loader.CallToolHandler(CreateRequest(ClearDescriptionArguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var workspace = result.StructuredContent!.Value.GetProperty("workspace");
        Assert.Equal(WorkspaceUpdateTestData.WorkspaceId, workspace.GetProperty("id").GetString());
        Assert.False(workspace.TryGetProperty("type", out _));
        using var envelope = JsonDocument.Parse(Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Equal(200, envelope.RootElement.GetProperty("status").GetInt32());
        Assert.True(JsonElement.DeepEquals(envelope.RootElement.GetProperty("results"), result.StructuredContent.Value));
        Assert.Equal(1, handler.CallCount);
    }

    private static ServiceProvider CreateServices(
        HttpMessageHandler handler,
        TokenCredential credential,
        StructuredOutputMode? mode = StructuredOutputMode.Compact,
        string transport = TransportTypes.StdIo,
        bool readOnly = false)
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
            Description = "Offline workspace update test server",
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
