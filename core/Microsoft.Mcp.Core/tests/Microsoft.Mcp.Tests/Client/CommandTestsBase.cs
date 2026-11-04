// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace Microsoft.Mcp.Tests.Client;

public abstract class CommandTestsBase(ITestOutputHelper output, LiveServerFixture liveServerFixture)
    : ToolTestsBase(output), IClassFixture<LiveServerFixture>
{
    protected LiveServerFixture LiveServerFixture { get; } = liveServerFixture;
    protected McpClient Client => LiveServerFixture.GetMcpClient();

    protected override ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null)
        => McpToolTestClient.InitializeAsync(LiveServerFixture, Settings, TestMode, CustomArguments, Output, proxy);

    protected override Task<JsonElement> InvokeToolAsync(
        string command, Dictionary<string, object?> parameters, CancellationToken cancellationToken)
        => McpToolTestClient.InvokeAsync(Client, command, parameters, cancellationToken);

    protected Task<JsonElement?> CallToolAsync(
        string command, Dictionary<string, object?> parameters, McpClient mcpClient)
        => CallToolAsync(command, parameters, mcpClient, null);

    protected Task<JsonElement?> CallToolAsync(
        string command,
        Dictionary<string, object?> parameters,
        McpClient? mcpClient = null,
        Func<JsonElement, JsonElement?>? resultProcessor = null)
        => CallToolCoreAsync(command, parameters,
            token => McpToolTestClient.InvokeAsync(mcpClient ?? Client, command, parameters, token), resultProcessor);
}
