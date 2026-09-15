// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Measures end-to-end MCP client startup time using the official C# MCP SDK.
/// This provides a protocol-correct timing of the full initialize → tools/list
/// handshake as a real client would experience it, plus both token-budget metrics
/// (name+description only, comparable across modes; and full inputSchema, the actual
/// LLM context-window cost) and the JSON serialization time of each payload.
///
/// Modes:
///   --mcp-startup &lt;exe&gt; [serverArgs...]
///       Spawns the server via StdioClientTransport, performs the initialize
///       handshake + tools/list request over stdio, and emits a compact JSON line.
///   --mcp-startup-http &lt;exe&gt; [serverArgs...]
///       Spawns the server with HTTP transport (--transport http
///       --dangerously-disable-http-incoming-auth) on a free loopback port, polls
///       readiness, then times the initialize + tools/list handshake over
///       HttpClientTransport. The emitted JSON additionally includes readiness_ms
///       (process start → server accepting connections).
/// </summary>
internal static class McpClientStartupMeasurement
{
    // Number of repetitions used to average JSON serialization time so the metric is
    // stable despite sub-millisecond per-call durations.
    private const int SerializeSampleCount = 50;

    // How long to wait for the HTTP server to start accepting connections.
    private static readonly TimeSpan s_httpReadinessTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Spawns the MCP server at <paramref name="exePath"/> using
    /// <paramref name="serverArgs"/> over stdio, connects via the MCP SDK, and times
    /// the full initialize + tools/list round-trip.
    ///
    /// The stopwatch starts just before <see cref="McpClient.CreateAsync"/> (which
    /// spawns the process and performs the initialize handshake) and stops after
    /// <c>ListToolsAsync</c> completes.
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

        EmitResult(sw.ElapsedMilliseconds, readinessMs: null, tools);
    }

    /// <summary>
    /// Spawns the MCP server at <paramref name="exePath"/> in HTTP transport mode on a
    /// free loopback port, waits for it to accept connections, then times the full
    /// initialize + tools/list round-trip over <see cref="HttpClientTransport"/>.
    ///
    /// The stopwatch starts before the server process is launched so <c>elapsed_ms</c>
    /// captures the complete cold path (process spawn → ready → initialize → tools/list),
    /// comparable to the stdio measurement. <c>readiness_ms</c> records the sub-interval
    /// from process start to the server accepting its first TCP connection.
    /// </summary>
    internal static async Task RunHttpAsync(string exePath, string[] serverArgs)
    {
        var port = PerfServerProcess.GetFreeLoopbackPort();
        var endpoint = new Uri($"http://127.0.0.1:{port}");

        // Start the stopwatch before launching so elapsed_ms captures the complete cold
        // path (process spawn -> ready -> initialize -> tools/list), comparable to stdio.
        var sw = Stopwatch.StartNew();
        using var server = PerfServerProcess.StartHttpServer(exePath, serverArgs, endpoint);

        try
        {
            var readinessMs = await PerfServerProcess.WaitForPortAsync(port, s_httpReadinessTimeout, server);

            var transportOptions = new HttpClientTransportOptions
            {
                Name = "perf-harness-http",
                Endpoint = endpoint,
                TransportMode = HttpTransportMode.AutoDetect,
            };
            var transport = new HttpClientTransport(transportOptions);
            await using (var client = await McpClient.CreateAsync(transport))
            {
                var tools = (await client.ListToolsAsync()).ToList();
                sw.Stop();
                EmitResult(sw.ElapsedMilliseconds, readinessMs, tools);
            }
        }
        finally
        {
            PerfServerProcess.TryKillProcessTree(server);
        }
    }

    /// <summary>
    /// Serializes the tool metrics and writes a single compact JSON line to stdout so
    /// the PowerShell harness can parse it with ConvertFrom-Json.
    /// </summary>
    private static void EmitResult(long elapsedMs, long? readinessMs, IReadOnlyList<McpClientTool> tools)
    {
        // --- name+description only (matches --count-tools; comparable across modes) ---
        var nameDescPayload = tools.Select(t => new { name = t.Name, description = t.Description }).ToArray();
        var nameDescJson = JsonSerializer.Serialize(nameDescPayload);
        var (nameDescBytes, nameDescTokens, nameDescApprox) = PerfTokenizer.Measure(nameDescJson);
        var nameDescSerializeMs = MeasureSerializeMs(nameDescPayload);

        // --- full schema (name + description + inputSchema) ---
        // This is the actual LLM context-window cost: what a client sends to the model
        // as the tools array. ProtocolTool includes the full JSON Schema per tool.
        var fullSchemaPayload = tools.Select(t => t.ProtocolTool).ToArray();
        var fullSchemaJson = JsonSerializer.Serialize(fullSchemaPayload);
        var (fullSchemaBytes, fullSchemaTokens, fullSchemaApprox) = PerfTokenizer.Measure(fullSchemaJson);
        var fullSchemaSerializeMs = MeasureSerializeMs(fullSchemaPayload);

        var result = new
        {
            elapsed_ms = elapsedMs,
            readiness_ms = readinessMs,
            tool_count = tools.Count,
            name_description = new
            {
                bytes = nameDescBytes,
                exact_tokens_gpt4o_o200k = nameDescTokens,
                approx_tokens_bytes_div_4 = nameDescApprox,
                serialize_ms = nameDescSerializeMs,
            },
            full_schema = new
            {
                bytes = fullSchemaBytes,
                exact_tokens_gpt4o_o200k = fullSchemaTokens,
                approx_tokens_bytes_div_4 = fullSchemaApprox,
                serialize_ms = fullSchemaSerializeMs,
            },
        };

        // Single compact JSON line so PowerShell can parse it with ConvertFrom-Json.
        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    /// <summary>
    /// Averages the wall-clock time to serialize <paramref name="payload"/> to JSON over
    /// <see cref="SerializeSampleCount"/> repetitions, isolating the serialization phase
    /// of tool discovery. Returns milliseconds rounded to three decimals.
    /// </summary>
    private static double MeasureSerializeMs<T>(T payload)
    {
        // Warm up once so JIT and any first-call setup are excluded from the timing.
        _ = JsonSerializer.Serialize(payload);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < SerializeSampleCount; i++)
        {
            _ = JsonSerializer.Serialize(payload);
        }
        sw.Stop();

        return Math.Round(sw.Elapsed.TotalMilliseconds / SerializeSampleCount, 3);
    }
}
