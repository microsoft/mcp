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
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Services.Telemetry;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests;

public sealed class WorkspaceCreateToolRegistrationTests()
{
    private const string ToolName = "core_create-workspace";

    [Theory]
    [InlineData(null)]
    [InlineData(StructuredOutputMode.Compact)]
    [InlineData(StructuredOutputMode.Duplicated)]
    public async Task ListTools_AdvertisesExactOptionsAndCorrectCreationMetadata(StructuredOutputMode? mode)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Discovery must not call Fabric."));
        var credential = WorkspaceCreateTestData.CreateCredential();
        await using var provider = CreateServiceProvider(handler, credential);
        await using var loader = CreateToolLoader(provider, new() { StructuredOutputMode = mode });

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(
            ["core_assign-workspace-to-capacity", "core_create-item", "core_create-workspace", "core_delete-item", "core_delete-workspace", "core_get-capacity", "core_get-item", "core_get-workspace",
             "core_list-capacities", "core_list-items", "core_list-workspaces", "core_search-catalog",
             "core_update-item", "core_update-workspace"],
            tools.Tools.Select(static tool => tool.Name).Order());
        var tool = Assert.Single(tools.Tools, static tool => tool.Name == ToolName);
        Assert.Equal(["capacity-id", "description", "display-name", "domain-id"],
            tool.InputSchema.GetProperty("properties").EnumerateObject().Select(static property => property.Name).Order());
        Assert.Equal("display-name", Assert.Single(tool.InputSchema.GetProperty("required").EnumerateArray()).GetString());
        foreach (var property in tool.InputSchema.GetProperty("properties").EnumerateObject())
        {
            Assert.Equal("string", property.Value.GetProperty("type").GetString());
        }

        Assert.False(tool.Annotations?.ReadOnlyHint);
        Assert.False(tool.Annotations?.IdempotentHint);
        Assert.False(tool.Annotations?.DestructiveHint);
        Assert.False(tool.Annotations?.OpenWorldHint);
        if (mode is null)
        {
            Assert.Null(tool.OutputSchema);
        }
        else
        {
            Assert.NotNull(tool.OutputSchema);
            var schema = tool.OutputSchema.Value;
            Assert.Equal("object", schema.GetProperty("type").GetString());
            Assert.Equal("workspace", Assert.Single(schema.GetProperty("required").EnumerateArray()).GetString());
            var properties = schema.GetProperty("properties");
            Assert.True(properties.TryGetProperty("location", out _));
            var workspaceSchema = properties.GetProperty("workspace");
            Assert.Equal(["displayName", "id"],
                workspaceSchema.GetProperty("required").EnumerateArray().Select(static property => property.GetString()).Order());
            var workspace = workspaceSchema.GetProperty("properties");
            Assert.True(workspace.TryGetProperty("id", out _));
            Assert.True(workspace.TryGetProperty("tags", out _));
            Assert.False(workspace.TryGetProperty("capacityAssignmentProgress", out _));
            Assert.False(workspace.TryGetProperty("workspaceIdentity", out _));
        }

        Assert.Equal(0, handler.CallCount);
        await credential.DidNotReceive().GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(""","type":null""")]
    public async Task CallTool_AllowsMissingOrNullTypeOnCreatedWorkspace(string typeProperty)
    {
        using var response = WorkspaceCreateTestData.CreateResponse(
            $$"""{"id":"{{WorkspaceCreateTestData.WorkspaceId}}","displayName":"New workspace","description":""{{typeProperty}}}""");
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServiceProvider(handler, WorkspaceCreateTestData.CreateCredential());
        await using var loader = CreateToolLoader(provider, new() { StructuredOutputMode = StructuredOutputMode.Duplicated });

        var result = await loader.CallToolHandler(CreateRequest(CreateArguments()), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        var workspace = result.StructuredContent!.Value.GetProperty("workspace");
        Assert.Equal(WorkspaceCreateTestData.WorkspaceId, workspace.GetProperty("id").GetString());
        Assert.False(workspace.TryGetProperty("type", out _));
        using var envelope = JsonDocument.Parse(GetText(result));
        Assert.Equal(201, envelope.RootElement.GetProperty("status").GetInt32());
        Assert.True(JsonElement.DeepEquals(envelope.RootElement.GetProperty("results"), result.StructuredContent.Value));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(StructuredOutputMode.Compact)]
    [InlineData(StructuredOutputMode.Duplicated)]
    public async Task CallTool_UsesRegisteredServiceAndTypedOutput(StructuredOutputMode? mode)
    {
        using var handler = new FabricCoreHttpMessageHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://api.fabric.microsoft.com/v1/workspaces", request.RequestUri?.AbsoluteUri);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            Assert.Equal("New workspace", body.RootElement.GetProperty("displayName").GetString());
            Assert.Equal(WorkspaceCreateTestData.CapacityId, body.RootElement.GetProperty("capacityId").GetString());
            Assert.Equal(WorkspaceCreateTestData.DomainId, body.RootElement.GetProperty("domainId").GetString());
            Assert.False(body.RootElement.TryGetProperty("description", out _));
            return WorkspaceCreateTestData.CreateResponse(WorkspaceCreateTestData.FullMetadata);
        });
        await using var provider = CreateServiceProvider(handler, WorkspaceCreateTestData.CreateCredential());
        await using var loader = CreateToolLoader(provider, new() { StructuredOutputMode = mode });
        var arguments = CreateArguments();
        arguments["capacity-id"] = JsonSerializer.SerializeToElement(WorkspaceCreateTestData.CapacityId, CoreJsonContext.Default.String);
        arguments["domain-id"] = JsonSerializer.SerializeToElement(WorkspaceCreateTestData.DomainId, CoreJsonContext.Default.String);

        var result = await loader.CallToolHandler(CreateRequest(arguments), TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
        if (mode is null)
        {
            Assert.Null(result.StructuredContent);
        }
        else
        {
            Assert.NotNull(result.StructuredContent);
            Assert.Equal(Guid.Parse(WorkspaceCreateTestData.WorkspaceId),
                result.StructuredContent.Value.GetProperty("workspace").GetProperty("id").GetGuid());
            Assert.Equal(WorkspaceCreateTestData.Location, result.StructuredContent.Value.GetProperty("location").GetString());
            Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, result.StructuredContent.Value.GetRawText());
        }

        if (mode != StructuredOutputMode.Compact)
        {
            using var response = JsonDocument.Parse(GetText(result));
            Assert.Equal(201, response.RootElement.GetProperty("status").GetInt32());
            Assert.Equal("New workspace", response.RootElement.GetProperty("results").GetProperty("workspace").GetProperty("displayName").GetString());
        }
    }

    [Theory]
    [InlineData("capacity-id", "not-a-uuid")]
    [InlineData("domain-id", "00000000-0000-0000-0000-000000000000")]
    [InlineData("display-name", "Admin monitoring")]
    public async Task CallTool_RejectsInvalidInputBeforeRegisteredCredentialsOrHttp(string option, string value)
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Invalid input must not call Fabric."));
        var credential = WorkspaceCreateTestData.CreateCredential();
        await using var provider = CreateServiceProvider(handler, credential);
        await using var loader = CreateToolLoader(provider, new());
        var arguments = CreateArguments();
        arguments[option] = JsonSerializer.SerializeToElement(value, CoreJsonContext.Default.String);

        var result = await loader.CallToolHandler(CreateRequest(arguments), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains("--" + option, GetText(result));
        Assert.Equal(0, handler.CallCount);
        await credential.DidNotReceive().GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReadOnlyMode_HidesAndRejectsWorkspaceCreation()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => throw new InvalidOperationException("Read-only mode must not create workspaces."));
        var credential = WorkspaceCreateTestData.CreateCredential();
        await using var provider = CreateServiceProvider(handler, credential);
        await using var loader = CreateToolLoader(provider, new() { ReadOnly = true });

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var result = await loader.CallToolHandler(CreateRequest(CreateArguments()), TestContext.Current.CancellationToken);

        Assert.DoesNotContain(tools.Tools, static tool => tool.Name == ToolName);
        Assert.Equal(
            ["core_get-capacity", "core_get-item", "core_get-workspace", "core_list-capacities", "core_list-items", "core_list-workspaces", "core_search-catalog"],
            tools.Tools.Select(static tool => tool.Name).Order());
        Assert.True(result.IsError);
        Assert.Contains("read-only", GetText(result));
        Assert.Equal(0, handler.CallCount);
        await credential.DidNotReceive().GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HttpMode_AllowsTheTransportAgnosticToolWithSubstitutedCredentials()
    {
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(WorkspaceCreateTestData.CreateResponse()));
        await using var provider = CreateServiceProvider(handler, WorkspaceCreateTestData.CreateCredential());
        await using var loader = CreateToolLoader(provider, new() { Transport = TransportTypes.Http });

        var tools = await loader.ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);
        var result = await loader.CallToolHandler(CreateRequest(CreateArguments()), TestContext.Current.CancellationToken);

        Assert.Contains(tools.Tools, static tool => tool.Name == ToolName);
        Assert.False(result.IsError);
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(429, "120", "120")]
    [InlineData(429, "0", "0")]
    [InlineData(429, " 45 ", "45")]
    [InlineData(429, "2147483647", "2147483647")]
    [InlineData(429, "Wed, 21 Oct 2015 07:28:00 GMT", "Wed, 21 Oct 2015 07:28:00 GMT")]
    [InlineData(503, "30", "30")]
    [InlineData(429, null, null)]
    [InlineData(429, "", null)]
    [InlineData(429, " ", null)]
    [InlineData(429, "+1", null)]
    [InlineData(429, "2147483648", null)]
    [InlineData(429, "-1", null)]
    [InlineData(429, "0.5", null)]
    [InlineData(429, "9999999999999999999999999999", null)]
    [InlineData(429, WorkspaceCreateTestData.SecretMarker, null)]
    [InlineData(429, "30, 60", null)]
    public async Task CallTool_PreservesFailureStatusAndOnlyValidatedRetryAfter(int status, string? retryAfter, string? expectedRetryAfter)
    {
        using var response = new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(WorkspaceCreateTestData.SecretMarker),
            ReasonPhrase = WorkspaceCreateTestData.SecretMarker
        };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }

        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServiceProvider(handler, WorkspaceCreateTestData.CreateCredential());
        await using var loader = CreateToolLoader(provider, new() { StructuredOutputMode = StructuredOutputMode.Compact });

        var result = await loader.CallToolHandler(CreateRequest(CreateArguments()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = GetText(result);
        using var commandResponse = JsonDocument.Parse(text);
        Assert.Equal(status, commandResponse.RootElement.GetProperty("status").GetInt32());
        Assert.False(commandResponse.RootElement.TryGetProperty("results", out _));
        Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, text);
        if (expectedRetryAfter is null)
        {
            Assert.DoesNotContain("Retry-After:", text);
        }
        else
        {
            Assert.Contains("Retry-After: " + expectedRetryAfter, text);
        }

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task CallTool_IgnoresMultipleRetryAfterValuesWithoutRetrying()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.TryAddWithoutValidation("Retry-After", ["10", "20"]);
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServiceProvider(handler, WorkspaceCreateTestData.CreateCredential());
        await using var loader = CreateToolLoader(provider, new());

        var result = await loader.CallToolHandler(CreateRequest(CreateArguments()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.DoesNotContain("Retry-After:", GetText(result));
        Assert.Equal(1, handler.CallCount);
    }

    [Theory]
    [InlineData(200, WorkspaceCreateTestData.MinimalMetadata)]
    [InlineData(202, """{"operationId":"not-a-workspace"}""")]
    [InlineData(204, "")]
    [InlineData(201, "null")]
    [InlineData(201, WorkspaceCreateTestData.SecretMarker)]
    public async Task CallTool_ReportsUncertainCreationRatherThanReturningAnOperationHandle(int status, string body)
    {
        using var response = new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(body) };
        using var handler = new FabricCoreHttpMessageHandler((_, _) => Task.FromResult(response));
        await using var provider = CreateServiceProvider(handler, WorkspaceCreateTestData.CreateCredential());
        await using var loader = CreateToolLoader(provider, new() { StructuredOutputMode = StructuredOutputMode.Duplicated });

        var result = await loader.CallToolHandler(CreateRequest(CreateArguments()), TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Null(result.StructuredContent);
        var text = GetText(result);
        using var commandResponse = JsonDocument.Parse(text);
        Assert.Equal(502, commandResponse.RootElement.GetProperty("status").GetInt32());
        Assert.Contains("may already have been created", text);
        Assert.DoesNotContain(WorkspaceCreateTestData.SecretMarker, text);
        Assert.DoesNotContain("operationId", text);
        Assert.Equal(1, handler.CallCount);
    }

    private static ServiceProvider CreateServiceProvider(FabricCoreHttpMessageHandler handler, TokenCredential credential)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(credential);
        var setup = new FabricCoreSetup();
        services.AddSingleton<IAreaSetup>(setup);
        setup.ConfigureServices(services);
        services.AddHttpClient<IFabricCoreService, FabricCoreService>().ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }

    private static CommandFactoryToolLoader CreateToolLoader(IServiceProvider provider, ServerRuntimeConfiguration configuration)
    {
        var serverConfiguration = Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
        {
            RootCommandGroupName = "fabmcp",
            Name = "Fabric.Mcp.Server",
            ShortName = "fabric",
            DisplayName = "Fabric MCP Server",
            Version = "1.0.0",
            Description = "Offline Fabric tool registration tests",
            IsTelemetryEnabled = false
        });
        var factory = new CommandFactory(provider, provider.GetServices<IAreaSetup>(),
            Substitute.For<ITelemetryService>(), serverConfiguration, NullLogger<CommandFactory>.Instance);
        return new(factory, Microsoft.Extensions.Options.Options.Create(configuration), NullLogger<CommandFactoryToolLoader>.Instance);
    }

    private static Dictionary<string, JsonElement> CreateArguments() => new()
    {
        ["display-name"] = JsonSerializer.SerializeToElement("New workspace", CoreJsonContext.Default.String)
    };

    private static RequestContext<CallToolRequestParams> CreateRequest(Dictionary<string, JsonElement> arguments) =>
        McpTestUtilities.CreateToolCallRequest(new CallToolRequestParams { Name = ToolName, Arguments = arguments }, Substitute.For<McpServer>());

    private static string GetText(CallToolResult result) => Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;
}
