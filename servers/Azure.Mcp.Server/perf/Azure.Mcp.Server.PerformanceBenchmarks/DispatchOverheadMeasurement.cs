// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Measures per-transport request overhead (issue #3119) by timing many repetitions of a
/// deterministic, Azure-free MCP <c>tools/list</c> request against a live server. It runs over
/// either stdio or the remote HTTP transport; the PowerShell harness runs both and subtracts
/// to isolate transport-specific overhead independently of Azure latency.
///
/// (A <c>ping</c> floor was considered but the stateless HTTP transport does not implement the
/// ping method, so <c>tools/list</c> — supported identically over both transports — is used as
/// the common comparison operation.)
///
/// Modes:
///   --mcp-dispatch-stdio &lt;exe&gt; [serverArgs...]
///   --mcp-dispatch-http  &lt;exe&gt; [serverArgs...]
///
/// Tunable via environment variables:
///   PERF_DISPATCH_ITERATIONS  timed samples per operation (default 200)
///   PERF_DISPATCH_WARMUP      warmup calls (not timed) per operation (default 20)
///
/// Emits a single compact JSON line with the raw per-call millisecond samples so the harness
/// can compute p50/p95/p99 with the shared PowerShell percentile helpers.
/// </summary>
internal static class DispatchOverheadMeasurement
{
    private static readonly TimeSpan s_readinessTimeout = TimeSpan.FromSeconds(60);

    internal static Task RunStdioAsync(string exePath, string[] serverArgs)
        => RunAsync(exePath, serverArgs, useHttp: false);

    internal static Task RunHttpAsync(string exePath, string[] serverArgs)
        => RunAsync(exePath, serverArgs, useHttp: true);

    private static async Task RunAsync(string exePath, string[] serverArgs, bool useHttp)
    {
        var iterations = GetEnvInt("PERF_DISPATCH_ITERATIONS", 200);
        var warmup = GetEnvInt("PERF_DISPATCH_WARMUP", 20);

        Process? server = null;
        long? readinessMs = null;
        IClientTransport transport;

        if (useHttp)
        {
            var port = PerfServerProcess.GetFreeLoopbackPort();
            var endpoint = new Uri($"http://127.0.0.1:{port}");
            server = PerfServerProcess.StartHttpServer(exePath, serverArgs, endpoint);
            readinessMs = await PerfServerProcess.WaitForPortAsync(port, s_readinessTimeout, server);
            transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Name = "perf-dispatch-http",
                Endpoint = endpoint,
                TransportMode = HttpTransportMode.AutoDetect,
            });
        }
        else
        {
            transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "perf-dispatch-stdio",
                Command = exePath,
                Arguments = serverArgs,
            });
        }

        var client = await McpClient.CreateAsync(transport);
        try
        {
            var toolsListMs = await MeasureAsync(iterations, warmup, async () => _ = await client.ListToolsAsync());
            EmitResult(useHttp ? "http" : "stdio", readinessMs, iterations, warmup, toolsListMs);
        }
        finally
        {
            await client.DisposeAsync();
            if (server is not null)
            {
                PerfServerProcess.TryKillProcessTree(server);
            }
        }
    }

    /// <summary>
    /// Runs <paramref name="warmup"/> warmup calls (not timed), then times
    /// <paramref name="iterations"/> calls of <paramref name="action"/>, returning per-call
    /// durations in milliseconds.
    /// </summary>
    private static async Task<double[]> MeasureAsync(int iterations, int warmup, Func<Task> action)
    {
        for (var i = 0; i < warmup; i++)
        {
            await action();
        }

        var samples = new double[iterations];
        for (var i = 0; i < iterations; i++)
        {
            var start = Stopwatch.GetTimestamp();
            await action();
            samples[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        }

        return samples;
    }

    private static void EmitResult(
        string transport, long? readinessMs, int iterations, int warmup, double[] toolsListMs)
    {
        var result = new
        {
            transport,
            iterations,
            warmup,
            readiness_ms = readinessMs,
            tools_list_ms = Array.ConvertAll(toolsListMs, x => Math.Round(x, 4)),
        };

        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    private static int GetEnvInt(string name, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }
}
