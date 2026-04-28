// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Measures end-to-end MCP client startup time using the official C# MCP SDK.
/// This provides a protocol-correct timing of the full initialize → tools/list
/// handshake as a real client would experience it, plus both token-budget metrics:
/// name+description only (comparable across modes) and full inputSchema
/// (the actual LLM context window cost).
///
/// Mode:
///   --mcp-startup &lt;exe&gt; [serverArgs...]
///       Spawns the server via StdioClientTransport, performs the initialize
///       handshake + tools/list request, stops the clock, then emits a compact
///       JSON line with timing and token metrics.
/// </summary>
internal static class McpClientStartupMeasurement
{

    /// <summary>
    /// Spawns the MCP server at <paramref name="exePath"/> using
    /// <paramref name="serverArgs"/>, connects via the MCP SDK, and times the
    /// full initialize + tools/list round-trip.
    ///
    /// The stopwatch starts just before <see cref="McpClient.CreateAsync"/>
    /// (which spawns the process and performs the initialize handshake) and
    /// stops after <c>ListToolsAsync</c> completes.
    /// </summary>
    internal static async Task RunAsync(string exePath, string[] serverArgs)
    {
        var transportOptions = new StdioClientTransportOptions
        {
            Name = "perf-harness",
            Command = exePath,
            Arguments = serverArgs,
        };

        var sw = Stopwatch.StartNew();
        var transport = new StdioClientTransport(transportOptions);
        await using var client = await McpClient.CreateAsync(transport);
        var tools = (await client.ListToolsAsync()).ToList();
        sw.Stop();

        // --- name+description only (matches --count-tools; comparable across modes) ---
        var nameDescJson = JsonSerializer.Serialize(tools.Select(t => new { name = t.Name, description = t.Description }));
        var (nameDescBytes, nameDescTokens, nameDescApprox) = PerfTokenizer.Measure(nameDescJson);

        // --- full schema (name + description + inputSchema) ---
        // This is the actual LLM context window cost: what a client sends to the model
        // as the tools array. ProtocolTool includes the full JSON Schema per tool.
        var fullSchemaJson = JsonSerializer.Serialize(tools.Select(t => t.ProtocolTool));
        var (fullSchemaBytes, fullSchemaTokens, fullSchemaApprox) = PerfTokenizer.Measure(fullSchemaJson);

        var result = new
        {
            elapsed_ms = sw.ElapsedMilliseconds,
            tool_count = tools.Count,
            name_description = new
            {
                bytes = nameDescBytes,
                exact_tokens_gpt4o_o200k = nameDescTokens,
                approx_tokens_bytes_div_4 = nameDescApprox,
            },
            full_schema = new
            {
                bytes = fullSchemaBytes,
                exact_tokens_gpt4o_o200k = fullSchemaTokens,
                approx_tokens_bytes_div_4 = fullSchemaApprox,
            },
        };

        // Single compact JSON line so PowerShell can parse it with ConvertFrom-Json.
        Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
