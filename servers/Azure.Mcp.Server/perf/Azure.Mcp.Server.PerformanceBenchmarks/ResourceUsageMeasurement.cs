// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using ModelContextProtocol.Client;
using static Azure.Mcp.Server.Perf.ProcessResourceSampler;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Measures the Azure MCP Server's process-level resource consumption (CPU, memory, and
/// handles) across the startup, idle, discovery, and steady-state phases, plus an
/// optional sustained "soak" phase used to detect unbounded memory or handle growth.
///
/// The server is launched in HTTP transport mode on a free loopback port; a background
/// <see cref="ProcessResourceSampler"/> reads the process counters throughout the run,
/// tagging every reading with the current phase. Steady-state and soak load are driven by
/// <c>PERF_CONCURRENCY</c> parallel MCP clients issuing repeated <c>tools/list</c>
/// requests (an authentication-free, representative discovery workload).
///
/// Tunable via environment variables (all optional):
///   PERF_SAMPLE_MS           sampling interval in ms (default 100)
///   PERF_IDLE_SECONDS        idle observation window with no client (default 3)
///   PERF_STEADY_SECONDS      steady-state load duration (default 10)
///   PERF_SOAK_SECONDS        sustained soak duration; 0 disables the soak phase (default 0)
///   PERF_CONCURRENCY         number of concurrent load clients (default 1)
///   PERF_LEAK_MB_PER_MIN     working-set slope above which soak is flagged unbounded (default 5)
///   PERF_LEAK_HANDLES_PER_MIN handle slope above which soak is flagged unbounded (default 20)
///
/// Emits a single compact JSON line to stdout for the PowerShell harness to parse.
/// </summary>
internal static class ResourceUsageMeasurement
{
    private const double BytesPerMb = 1024.0 * 1024.0;

    private static readonly TimeSpan s_readinessTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Runs the full resource-usage measurement against the server at
    /// <paramref name="exePath"/> launched with <paramref name="serverArgs"/>.
    /// </summary>
    internal static async Task RunAsync(string exePath, string[] serverArgs)
    {
        var sampleMs = GetEnvInt("PERF_SAMPLE_MS", 100);
        var idleSeconds = GetEnvInt("PERF_IDLE_SECONDS", 3);
        var steadySeconds = GetEnvInt("PERF_STEADY_SECONDS", 10);
        var soakSeconds = GetEnvInt("PERF_SOAK_SECONDS", 0);
        var concurrency = Math.Max(1, GetEnvInt("PERF_CONCURRENCY", 1));

        var port = PerfServerProcess.GetFreeLoopbackPort();
        var endpoint = new Uri($"http://127.0.0.1:{port}");

        using var server = PerfServerProcess.StartHttpServer(exePath, serverArgs, endpoint);
        var sampler = new ProcessResourceSampler(server, TimeSpan.FromMilliseconds(sampleMs));
        sampler.BeginPhase("startup");
        var samplerTask = sampler.RunAsync();

        try
        {
            // Phase: startup — process launch until it accepts its first connection.
            var readinessMs = await PerfServerProcess.WaitForPortAsync(port, s_readinessTimeout, server);

            // Phase: idle — server ready, no client connected.
            sampler.BeginPhase("idle");
            await Task.Delay(TimeSpan.FromSeconds(idleSeconds));

            // Phase: discovery — a single initialize + tools/list handshake.
            sampler.BeginPhase("discovery");
            int toolCount;
            {
                var transport = new HttpClientTransport(new HttpClientTransportOptions
                {
                    Name = "perf-resource-discovery",
                    Endpoint = endpoint,
                    TransportMode = HttpTransportMode.AutoDetect,
                });
                await using var client = await McpClient.CreateAsync(transport);
                toolCount = (await client.ListToolsAsync()).Count;
            }

            // Phase: steady-state — sustained load under PERF_CONCURRENCY clients.
            sampler.BeginPhase("steady_state");
            var steadyRequests = await DriveLoadAsync(endpoint, concurrency, TimeSpan.FromSeconds(steadySeconds));

            // Phase: soak (optional) — longer sustained load for leak detection.
            long soakRequests = 0;
            if (soakSeconds > 0)
            {
                sampler.BeginPhase("soak");
                soakRequests = await DriveLoadAsync(endpoint, concurrency, TimeSpan.FromSeconds(soakSeconds));
            }

            sampler.Stop();
            await samplerTask;

            EmitResult(
                sampler.Samples, readinessMs, toolCount, concurrency, sampleMs,
                steadyRequests, steadySeconds, soakRequests, soakSeconds, serverArgs);
        }
        finally
        {
            sampler.Stop();
            try
            {
                await samplerTask;
            }
            catch
            {
                // Sampler teardown is best-effort.
            }

            PerfServerProcess.TryKillProcessTree(server);
        }
    }

    /// <summary>
    /// Drives <paramref name="concurrency"/> parallel MCP clients, each issuing
    /// <c>tools/list</c> in a tight loop until <paramref name="duration"/> elapses, and
    /// returns the total number of completed requests across all workers.
    /// </summary>
    private static async Task<long> DriveLoadAsync(Uri endpoint, int concurrency, TimeSpan duration)
    {
        var deadline = DateTime.UtcNow + duration;
        var counts = new long[concurrency];
        var workers = new Task[concurrency];

        for (var w = 0; w < concurrency; w++)
        {
            var index = w;
            workers[w] = Task.Run(async () =>
            {
                var transport = new HttpClientTransport(new HttpClientTransportOptions
                {
                    Name = $"perf-load-{index}",
                    Endpoint = endpoint,
                    TransportMode = HttpTransportMode.AutoDetect,
                });
                await using var client = await McpClient.CreateAsync(transport);
                while (DateTime.UtcNow < deadline)
                {
                    _ = await client.ListToolsAsync();
                    counts[index]++;
                }
            });
        }

        await Task.WhenAll(workers);

        long total = 0;
        foreach (var count in counts)
        {
            total += count;
        }

        return total;
    }

    /// <summary>
    /// Aggregates the collected samples into per-phase, peak, growth, and soak statistics
    /// and writes a single compact JSON line to stdout.
    /// </summary>
    private static void EmitResult(
        IReadOnlyList<ResourceSample> all,
        long readinessMs,
        int toolCount,
        int concurrency,
        int sampleMs,
        long steadyRequests,
        int steadySeconds,
        long soakRequests,
        int soakSeconds,
        string[] serverArgs)
    {
        var procCount = Environment.ProcessorCount;

        var phases = new Dictionary<string, object?>();
        foreach (var name in new[] { "startup", "idle", "discovery" })
        {
            var summary = SummarizePhase(all, name, procCount);
            if (summary is not null)
            {
                phases[name] = summary;
            }
        }

        var steadySummary = SummarizePhase(all, "steady_state", procCount);
        if (steadySummary is not null)
        {
            steadySummary["requests"] = steadyRequests;
            steadySummary["throughput_rps"] = steadySeconds > 0
                ? Math.Round((double)steadyRequests / steadySeconds, 1)
                : 0d;
            phases["steady_state"] = steadySummary;
        }

        var peak = new
        {
            working_set_mb = all.Count > 0 ? MaxMb(all, x => x.WorkingSetBytes) : 0d,
            private_mb = all.Count > 0 ? MaxMb(all, x => x.PrivateBytes) : 0d,
            handles = all.Count > 0 ? all.Max(x => x.HandleCount) : 0,
        };

        var growth = ComputeGrowth(all);
        var soak = ComputeSoak(all, soakRequests, soakSeconds);

        var result = new
        {
            server_args = string.Join(' ', serverArgs),
            tool_count = toolCount,
            processor_count = procCount,
            concurrency,
            sample_interval_ms = sampleMs,
            sample_count = all.Count,
            readiness_ms = readinessMs,
            phases,
            peak,
            growth,
            soak,
        };

        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    /// <summary>
    /// Builds the mean/max working-set, private-bytes, handle, and CPU-percent summary for
    /// a single phase, or <c>null</c> when the phase produced no samples.
    /// </summary>
    private static Dictionary<string, object?>? SummarizePhase(
        IReadOnlyList<ResourceSample> all, string phase, int procCount)
    {
        var s = all.Where(x => x.Phase == phase).ToList();
        if (s.Count == 0)
        {
            return null;
        }

        return new Dictionary<string, object?>
        {
            ["samples"] = s.Count,
            ["working_set_mb"] = new { mean = MeanMb(s, x => x.WorkingSetBytes), max = MaxMb(s, x => x.WorkingSetBytes) },
            ["private_mb"] = new { mean = MeanMb(s, x => x.PrivateBytes), max = MaxMb(s, x => x.PrivateBytes) },
            ["handles"] = new { mean = Math.Round(s.Average(x => (double)x.HandleCount), 0), max = s.Max(x => x.HandleCount) },
            ["cpu_percent"] = CpuPercent(s, procCount),
        };
    }

    /// <summary>Computes the mean working-set and handle growth from idle to steady-state.</summary>
    private static object ComputeGrowth(IReadOnlyList<ResourceSample> all)
    {
        var idle = all.Where(x => x.Phase == "idle").ToList();
        var steady = all.Where(x => x.Phase == "steady_state").ToList();

        double? workingSet = null;
        double? handles = null;
        if (idle.Count > 0 && steady.Count > 0)
        {
            workingSet = Math.Round((steady.Average(x => x.WorkingSetBytes) - idle.Average(x => x.WorkingSetBytes)) / BytesPerMb, 2);
            handles = Math.Round(steady.Average(x => (double)x.HandleCount) - idle.Average(x => (double)x.HandleCount), 0);
        }

        return new { idle_to_steady_working_set_mb = workingSet, idle_to_steady_handles = handles };
    }

    /// <summary>
    /// Computes the working-set and handle-count trend (least-squares slope + R²) across the
    /// soak phase and flags unbounded growth when a slope exceeds its threshold with a
    /// well-correlated (R² ≥ 0.5) upward trend. Returns <c>null</c> when no soak ran.
    /// </summary>
    private static object? ComputeSoak(IReadOnlyList<ResourceSample> all, long soakRequests, int soakSeconds)
    {
        var soak = all.Where(x => x.Phase == "soak").ToList();
        if (soak.Count < 2)
        {
            return null;
        }

        // Exclude an initial warmup portion so the leak verdict reflects steady-state growth
        // rather than the JIT/cache/thread-pool ramp that occurs when sustained load begins.
        // Real soak runs should be long enough (minutes) that this window is a small fraction.
        var warmup = soak.Count / 4;
        var analyzed = soak.Skip(warmup).ToList();
        if (analyzed.Count < 2)
        {
            analyzed = soak;
        }

        var leakMbPerMin = GetEnvDouble("PERF_LEAK_MB_PER_MIN", 5.0);
        var leakHandlesPerMin = GetEnvDouble("PERF_LEAK_HANDLES_PER_MIN", 20.0);

        var xs = analyzed.Select(x => x.ElapsedSeconds).ToList();
        var (wsSlopePerSec, wsR2) = LinearTrend(xs, analyzed.Select(x => x.WorkingSetBytes / BytesPerMb).ToList());
        var (handleSlopePerSec, handleR2) = LinearTrend(xs, analyzed.Select(x => (double)x.HandleCount).ToList());

        var wsSlopePerMin = Math.Round(wsSlopePerSec * 60, 3);
        var handleSlopePerMin = Math.Round(handleSlopePerSec * 60, 3);

        var unbounded = (wsSlopePerMin > leakMbPerMin && wsR2 >= 0.5)
            || (handleSlopePerMin > leakHandlesPerMin && handleR2 >= 0.5);

        return new
        {
            duration_s = soakSeconds,
            requests = soakRequests,
            samples = soak.Count,
            analyzed_samples = analyzed.Count,
            working_set_slope_mb_per_min = wsSlopePerMin,
            working_set_r2 = Math.Round(wsR2, 3),
            handle_slope_per_min = handleSlopePerMin,
            handle_r2 = Math.Round(handleR2, 3),
            unbounded_growth = unbounded,
        };
    }

    /// <summary>
    /// Average CPU utilization percentage over a phase, derived from the cumulative CPU
    /// time delta divided by wall-clock time and processor count. Returns <c>null</c> when
    /// fewer than two samples make the rate undefined.
    /// </summary>
    private static double? CpuPercent(IReadOnlyList<ResourceSample> s, int procCount)
    {
        if (s.Count < 2)
        {
            return null;
        }

        var wall = s[^1].ElapsedSeconds - s[0].ElapsedSeconds;
        if (wall <= 0)
        {
            return null;
        }

        var cpu = s[^1].CpuSeconds - s[0].CpuSeconds;
        return Math.Round(cpu / (wall * procCount) * 100.0, 1);
    }

    /// <summary>Least-squares linear regression returning the slope and R² of ys over xs.</summary>
    private static (double Slope, double R2) LinearTrend(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        var n = xs.Count;
        if (n < 2)
        {
            return (0, 0);
        }

        double sx = 0, sy = 0, sxx = 0, sxy = 0;
        for (var i = 0; i < n; i++)
        {
            sx += xs[i];
            sy += ys[i];
            sxx += xs[i] * xs[i];
            sxy += xs[i] * ys[i];
        }

        var denom = n * sxx - sx * sx;
        if (denom == 0)
        {
            return (0, 0);
        }

        var slope = (n * sxy - sx * sy) / denom;
        var intercept = (sy - slope * sx) / n;
        var meanY = sy / n;

        double ssTot = 0, ssRes = 0;
        for (var i = 0; i < n; i++)
        {
            var dy = ys[i] - meanY;
            ssTot += dy * dy;
            var e = ys[i] - (slope * xs[i] + intercept);
            ssRes += e * e;
        }

        var r2 = ssTot <= 0 ? 0 : 1 - ssRes / ssTot;
        return (slope, r2);
    }

    private static double MeanMb(IEnumerable<ResourceSample> s, Func<ResourceSample, long> selector)
        => Math.Round(s.Average(x => (double)selector(x)) / BytesPerMb, 2);

    private static double MaxMb(IEnumerable<ResourceSample> s, Func<ResourceSample, long> selector)
        => Math.Round(s.Max(selector) / BytesPerMb, 2);

    private static int GetEnvInt(string name, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }

    private static double GetEnvDouble(string name, double fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : fallback;
    }
}
