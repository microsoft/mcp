// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace Microsoft.Mcp.Tests.Client.Helpers;

internal static class McpToolTestClient
{
    public static async ValueTask InitializeAsync(
        LiveServerFixture fixture,
        LiveTestSettings settings,
        TestMode mode,
        string[]? customArguments,
        ITestOutputHelper output,
        TestProxyFixture? proxy)
    {
        bool debug = settings.DebugOutput ||
            string.Equals(Environment.GetEnvironmentVariable("AZURE_MCP_TEST_DEBUG"), "true", StringComparison.OrdinalIgnoreCase);
        List<string> arguments = ["server", "start", "--mode", "all", "--dangerously-disable-elicitation", "--disable-caching", "--disable-proxy-tools"];
        if (debug)
        {
            arguments.Add("--debug");
        }

        Dictionary<string, string?> environment = [];
        foreach (var (key, value) in settings.EnvironmentVariables)
        {
            environment[key] = value;
        }
        if (proxy?.Proxy is { } recordingProxy)
        {
            environment["TEST_PROXY_URL"] = recordingProxy.BaseUri;
            if (mode == TestMode.Playback)
            {
                environment["AZURE_TOKEN_CREDENTIALS"] = "PlaybackTokenCredential";
                environment["TEST_MODE"] = "Playback";
            }
        }

        fixture.EnvironmentVariables = environment;
        fixture.Arguments = customArguments?.ToList() ?? arguments;
        fixture.Output = output;
        fixture.Settings = settings;
        await fixture.EnsureStartedAsync();
    }

    public static async Task<JsonElement> InvokeAsync(
        McpClient client,
        string command,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        var result = await client.CallToolAsync(command, parameters, cancellationToken: cancellationToken);
        var content = McpTestUtilities.GetFirstText(result.Content);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new JsonException("No JSON content found in the response.");
        }
        return JsonSerializer.Deserialize(content, ToolTestJsonContext.Default.JsonElement);
    }
}
