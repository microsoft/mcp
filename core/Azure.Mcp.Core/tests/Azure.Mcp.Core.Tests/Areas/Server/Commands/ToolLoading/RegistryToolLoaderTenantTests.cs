// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Tests.Areas.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Areas.Server.Commands.Discovery;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Per-call tenant support for tools proxied from tenant-scoped registry servers.
/// </summary>
public class RegistryToolLoaderTenantTests
{
    private const string TenantId = "11111111-1111-1111-1111-111111111111";

    private static RegistryToolLoader CreateToolLoader(IMcpDiscoveryStrategy discoveryStrategy)
    {
        var logger = Substitute.For<ILogger<RegistryToolLoader>>();
        var serverConfiguration = Microsoft.Extensions.Options.Options.Create(new ServerRuntimeConfiguration());
        return new RegistryToolLoader(discoveryStrategy, serverConfiguration, logger);
    }

    private static Tool EmptySchemaTool(string name) => new()
    {
        Name = name,
        Description = $"{name} description",
        InputSchema = JsonDocument.Parse("""{"type": "object", "properties": {}}""").RootElement
    };

    [Fact]
    public async Task ListToolsHandler_WithTenantScopedServer_InjectsTenantProperty()
    {
        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(EmptySchemaTool("execute_query"), _ => new CallToolResult());

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("arm", "arm", "ARM", clientBuilder, toolPrefix: "arm_", supportsTenantScope: true)
            .Build();

        var result = await CreateToolLoader(discoveryStrategy)
            .ListToolsHandler(McpTestUtilities.CreateToolListRequest(), TestContext.Current.CancellationToken);

        var tool = Assert.Single(result.Tools);
        Assert.Equal("arm_execute_query", tool.Name);
        Assert.True(tool.InputSchema.GetProperty("properties").TryGetProperty("tenant", out _));
    }

    [Fact]
    public async Task CallToolHandler_ConsumesTenantArgumentAndPublishesItAsAmbientTenant()
    {
        IReadOnlyDictionary<string, object?>? forwarded = null;
        string? ambientTenant = null;

        var clientBuilder = new MockMcpClientBuilder()
            .AddTool(EmptySchemaTool("execute_query"), args =>
            {
                forwarded = args;
                ambientTenant = RegistryTenantContext.CurrentTenantId;
                return new CallToolResult { Content = [new TextContentBlock { Text = "ok" }], IsError = false };
            });

        var discoveryStrategy = new MockMcpDiscoveryStrategyBuilder()
            .AddServer("arm", "arm", "ARM", clientBuilder, toolPrefix: "arm_", supportsTenantScope: true)
            .Build();

        var request = McpTestUtilities.CreateToolCallRequest("arm_execute_query", new Dictionary<string, object?>
        {
            ["query"] = "Resources | count",
            ["tenant"] = TenantId
        });

        var result = await CreateToolLoader(discoveryStrategy)
            .CallToolHandler(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(TenantId, ambientTenant);
        Assert.True(forwarded!.ContainsKey("query"));
        Assert.False(forwarded.ContainsKey("tenant"), "'tenant' is consumed locally and must not reach the upstream server.");
        Assert.Null(RegistryTenantContext.CurrentTenantId);
    }
}
