// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol.Client;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Measures the Azure MCP Server's behavior under concurrent load (issue #3122). It launches
/// the server over HTTP and drives a <b>sweep of concurrency levels</b>; at each level a pool of
/// parallel MCP clients issues <c>tools/list</c> for a fixed window, and the harness records the
/// throughput, latency distribution (p50/p95/p99), error/timeout rate, and a state-isolation
/// signal (responses whose tool count does not match the expected count — a sign of a
/// thread-safety or request-state-leakage failure). Per-level <b>scaling efficiency</b> is the
/// throughput relative to perfect linear scaling from the first level, so the emitted "saturation
/// curve" shows where throughput plateaus or collapses.
///
/// Mode:
///   --mcp-concurrency &lt;exe&gt; [serverArgs...]
///
/// Tunable via environment variables:
///   PERF_CONC_LEVELS            comma-separated concurrency levels (default "1,2,4,8,16")
///   PERF_CONC_DURATION_SECONDS  timed window per level (default 5)
///   PERF_CONC_WARMUP_SECONDS    warmup window per level, not counted (default 1)
///   PERF_CONC_TIMEOUT_MS        per-request timeout; a timeout counts as an error (default 10000)
///
/// Emits a single compact JSON line for the PowerShell harness to parse.
/// </summary>
internal static class ConcurrencyScalingMeasurement
{
    private static readonly TimeSpan s_readinessTimeout = TimeSpan.FromSeconds(60);

    internal static async Task RunAsync(string exePath, string[] serverArgs)
    {
        var levels = GetEnvIntArray("PERF_CONC_LEVELS", [1, 2, 4, 8, 16]);
        var durationSeconds = GetEnvInt("PERF_CONC_DURATION_SECONDS", 5);
        var warmupSeconds = GetEnvInt("PERF_CONC_WARMUP_SECONDS", 1);
        var timeoutMs = GetEnvInt("PERF_CONC_TIMEOUT_MS", 10000);

        var port = PerfServerProcess.GetFreeLoopbackPort();
        var endpoint = new Uri($"http://127.0.0.1:{port}");
        using var server = PerfServerProcess.StartHttpServer(exePath, serverArgs, endpoint);

        try
        {
            var readinessMs = await PerfServerProcess.WaitForPortAsync(port, s_readinessTimeout, server);

            // Establish the expected tool count once; concurrent responses that differ from it
            // indicate cross-request state leakage or a thread-safety failure.
            int expectedToolCount;
            {
                await using var probe = await McpClient.CreateAsync(NewHttpTransport(endpoint, "perf-conc-probe"));
                expectedToolCount = (await probe.ListToolsAsync()).Count;
            }

            var levelResults = new List<object>();
            double? baselineThroughput = null;

            foreach (var level in levels)
            {
                var r = await RunLevelAsync(endpoint, level, durationSeconds, warmupSeconds, timeoutMs, expectedToolCount);
                baselineThroughput ??= r.ThroughputRps;

                var scalingEfficiency = baselineThroughput.Value > 0 && level > 0
                    ? Math.Round(r.ThroughputRps / baselineThroughput.Value / level, 3)
                    : 0d;

                levelResults.Add(new
                {
                    concurrency = level,
                    requests = r.Requests,
                    errors = r.Errors,
                    error_rate_pct = r.ErrorRatePct,
                    tool_count_mismatches = r.ToolCountMismatches,
                    throughput_rps = r.ThroughputRps,
                    scaling_efficiency = scalingEfficiency,
                    latency_ms = new { p50 = r.P50, p95 = r.P95, p99 = r.P99 },
                });
            }

            var payload = new
            {
                server_args = string.Join(' ', serverArgs),
                readiness_ms = readinessMs,
                expected_tool_count = expectedToolCount,
                duration_seconds = durationSeconds,
                levels = levelResults,
            };

            Console.WriteLine(JsonSerializer.Serialize(payload));
        }
        finally
        {
            PerfServerProcess.TryKillProcessTree(server);
        }
    }

    /// <summary>
    /// Runs a single concurrency level: <paramref name="concurrency"/> parallel clients issue
    /// <c>tools/list</c> until the window closes, after a warmup window that is not counted.
    /// Returns throughput, error rate, tool-count mismatches, and latency percentiles for the
    /// timed window.
    /// </summary>
    private static async Task<LevelResult> RunLevelAsync(
        Uri endpoint, int concurrency, int durationSeconds, int warmupSeconds, int timeoutMs, int expectedToolCount)
    {
        var latencies = new ConcurrentBag<double>();
        long requests = 0;
        long errors = 0;
        long mismatches = 0;

        var warmupDeadline = DateTime.UtcNow + TimeSpan.FromSeconds(warmupSeconds);
        var endDeadline = warmupDeadline + TimeSpan.FromSeconds(durationSeconds);

        var workers = new Task[concurrency];
        for (var w = 0; w < concurrency; w++)
        {
            var index = w;
            workers[w] = Task.Run(async () =>
            {
                await using var client = await McpClient.CreateAsync(NewHttpTransport(endpoint, $"perf-conc-{concurrency}-{index}"));
                while (DateTime.UtcNow < endDeadline)
                {
                    var timed = DateTime.UtcNow >= warmupDeadline;
                    using var cts = new CancellationTokenSource(timeoutMs);
                    var start = Stopwatch.GetTimestamp();
                    try
                    {
                        var tools = await client.ListToolsAsync(cancellationToken: cts.Token);
                        if (timed)
                        {
                            latencies.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
                            Interlocked.Increment(ref requests);
                            if (tools.Count != expectedToolCount)
                            {
                                Interlocked.Increment(ref mismatches);
                            }
                        }
                    }
                    catch
                    {
                        if (timed)
                        {
                            Interlocked.Increment(ref requests);
                            Interlocked.Increment(ref errors);
                        }
                    }
                }
            });
        }

        await Task.WhenAll(workers);

        var sorted = latencies.ToArray();
        Array.Sort(sorted);
        var throughput = durationSeconds > 0 ? Math.Round((double)requests / durationSeconds, 1) : 0d;
        var errorRate = requests > 0 ? Math.Round((double)errors / requests * 100, 3) : 0d;

        return new LevelResult(
            requests, errors, mismatches, errorRate, throughput,
            Percentile(sorted, 50), Percentile(sorted, 95), Percentile(sorted, 99));
    }

    private readonly record struct LevelResult(
        long Requests, long Errors, long ToolCountMismatches, double ErrorRatePct,
        double ThroughputRps, double P50, double P95, double P99);

    /// <summary>Linear-interpolated percentile of a pre-sorted array (0 when empty).</summary>
    private static double Percentile(double[] sorted, double p)
    {
        if (sorted.Length == 0)
        {
            return 0;
        }
        if (sorted.Length == 1)
        {
            return Math.Round(sorted[0], 3);
        }

        var rank = p / 100.0 * (sorted.Length - 1);
        var low = (int)Math.Floor(rank);
        var high = (int)Math.Ceiling(rank);
        var frac = rank - low;
        return Math.Round(sorted[low] + (sorted[high] - sorted[low]) * frac, 3);
    }

    private static IClientTransport NewHttpTransport(Uri endpoint, string name)
        => new HttpClientTransport(new HttpClientTransportOptions
        {
            Name = name,
            Endpoint = endpoint,
            TransportMode = HttpTransportMode.AutoDetect,
        });

    private static int GetEnvInt(string name, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }

    private static int[] GetEnvIntArray(string name, int[] fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        var values = new List<int>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
            {
                values.Add(value);
            }
        }

        return values.Count > 0 ? [.. values] : fallback;
    }
}
