// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Microsoft.Mcp.Tests.Client.Helpers;

/// <summary>
/// Profiles a child MCP server process using EventPipe diagnostics.
/// Captures JIT compilation, assembly loading, GC events, and CPU sampling
/// and writes the .nettrace file and a human-readable report to disk.
/// <para>
/// Enable by setting environment variable <c>MCP_TEST_PROFILE=true</c> before running tests.
/// The profiler uses <c>DOTNET_EnableEventPipe</c> to have the CLR automatically write
/// trace data to a file, avoiding the need for a diagnostic port listener that can block
/// or time out under concurrent test execution.
/// </para>
/// <para>
/// Because the trace file is written by the CLR when the process exits, the report must
/// be generated after the server process has terminated.
/// </para>
/// </summary>
public sealed class ServerProfiler : IAsyncDisposable
{
    private static readonly string OutputDirectory = Path.Combine(Path.GetTempPath(), "mcp-profiler");

    private readonly Stopwatch _wallClock = new();
    private bool _disposed;

    private ServerProfiler(string traceFilePath, string reportFilePath)
    {
        TraceFilePath = traceFilePath;
        ReportFilePath = reportFilePath;
        _wallClock.Start();
    }

    /// <summary>
    /// Gets the path to the .nettrace file on disk.
    /// </summary>
    public string TraceFilePath { get; }

    /// <summary>
    /// Gets the path to the human-readable report file on disk.
    /// </summary>
    public string ReportFilePath { get; }

    /// <summary>
    /// Gets a value indicating whether profiling is enabled via the <c>MCP_TEST_PROFILE</c> environment variable.
    /// </summary>
    public static bool IsEnabled =>
        string.Equals(Environment.GetEnvironmentVariable("MCP_TEST_PROFILE"), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new <see cref="ServerProfiler"/> and returns the environment variables
    /// that must be set on the server child process to enable CLR-managed EventPipe tracing.
    /// </summary>
    /// <returns>
    /// A tuple containing the profiler instance and the environment variables to merge
    /// into the server process's environment.
    /// </returns>
    public static (ServerProfiler Profiler, Dictionary<string, string?> EnvironmentVariables) Create()
    {
        Directory.CreateDirectory(OutputDirectory);

        var pid = Environment.ProcessId;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var baseName = $"mcp-server-{pid}-{timestamp}";

        var traceFilePath = Path.Combine(OutputDirectory, $"{baseName}.nettrace");
        var reportFilePath = Path.Combine(OutputDirectory, $"{baseName}-report.txt");

        var profiler = new ServerProfiler(traceFilePath, reportFilePath);

        // CLR runtime events: JIT (0x10) | Loader (0x8) | GC (0x1) | Exception (0x8000) = 0x8019
        // Verbose level (5) is required for MethodJittingStarted and MethodLoadVerbose events.
        // Microsoft-DotNETCore-SampleProfiler at Informational level (4) for CPU sampling (~10ms).
        const string eventPipeConfig =
            "Microsoft-Windows-DotNETRuntime:0x8019:5," +
            "Microsoft-DotNETCore-SampleProfiler:0:4";

        var envVars = new Dictionary<string, string?>
        {
            ["DOTNET_EnableEventPipe"] = "1",
            ["DOTNET_EventPipeOutputPath"] = traceFilePath,
            ["DOTNET_EventPipeConfig"] = eventPipeConfig,
        };

        return (profiler, envVars);
    }

    /// <summary>
    /// Parses the CLR-written .nettrace file and writes a human-readable profiling report to disk.
    /// Must be called after the server process has exited so the trace file is finalized.
    /// The .nettrace file is kept at <see cref="TraceFilePath"/> and the report is written
    /// to <see cref="ReportFilePath"/>.
    /// </summary>
    public async Task WriteReportAsync()
    {
        _wallClock.Stop();

        // The CLR finalizes the .nettrace file during process shutdown, but the OS
        // may still be flushing file buffers after WaitForExit returns. Poll until
        // the file size stabilizes so we don't parse a truncated trace.
        await WaitForStableTraceFileAsync();

        if (!File.Exists(TraceFilePath) || new FileInfo(TraceFilePath).Length == 0)
        {
            await File.WriteAllTextAsync(ReportFilePath, "[ServerProfiler] No trace data captured.\n");
            return;
        }

        try
        {
            var report = ParseTrace(TraceFilePath);
            var text = report.FormatReport(_wallClock.Elapsed);
            await File.WriteAllTextAsync(ReportFilePath, text);
        }
        catch (Exception ex)
        {
            var error = $"[ServerProfiler] Failed to parse trace: {ex.Message}\n" +
                        $"[ServerProfiler] Raw trace saved to: {TraceFilePath}\n";
            await File.WriteAllTextAsync(ReportFilePath, error);
        }
    }

    private async Task WaitForStableTraceFileAsync()
    {
        const int pollIntervalMs = 200;
        const int maxWaitMs = 5000;

        long lastSize = -1;
        int waited = 0;

        while (waited < maxWaitMs)
        {
            if (!File.Exists(TraceFilePath))
            {
                await Task.Delay(pollIntervalMs);
                waited += pollIntervalMs;
                continue;
            }

            long currentSize = new FileInfo(TraceFilePath).Length;
            if (currentSize > 0 && currentSize == lastSize)
            {
                return;
            }

            lastSize = currentSize;
            await Task.Delay(pollIntervalMs);
            waited += pollIntervalMs;
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return ValueTask.CompletedTask;
        }

        _disposed = true;
        return ValueTask.CompletedTask;
    }

    private static ProfileReport ParseTrace(string traceFilePath)
    {
        var jitMethods = new ConcurrentDictionary<string, JitStats>();
        var assemblyLoads = new ConcurrentBag<AssemblyLoadInfo>();
        var gcEvents = new ConcurrentBag<GcInfo>();
        var exceptionEvents = new ConcurrentBag<ExceptionInfo>();

        // Scoped using — the EventPipeEventSource must be disposed before ParseCpuSamples
        // opens the same file for etlx conversion. A `using var` declaration would keep the
        // file memory-mapped until the method returns, blocking TraceLog from reading it.
        using (var source = new EventPipeEventSource(traceFilePath))
        {
            source.Clr.MethodJittingStarted += data =>
            {
                var key = FormatMethodName(data.MethodNamespace, data.MethodName, data.MethodSignature);
                jitMethods.AddOrUpdate(key,
                    _ => new JitStats { StartTimeMSec = data.TimeStampRelativeMSec, Count = 1 },
                    (_, existing) => { existing.Count++; return existing; });
            };

            source.Clr.MethodLoadVerbose += data =>
            {
                var key = FormatMethodName(data.MethodNamespace, data.MethodName, data.MethodSignature);
                if (jitMethods.TryGetValue(key, out var stats))
                {
                    var jitTime = data.TimeStampRelativeMSec - stats.StartTimeMSec;
                    if (jitTime > 0)
                    {
                        stats.TotalJitTimeMSec += jitTime;
                    }
                }
            };

            source.Clr.LoaderAssemblyLoad += data =>
            {
                assemblyLoads.Add(new AssemblyLoadInfo
                {
                    Name = data.FullyQualifiedAssemblyName,
                    TimeMSec = data.TimeStampRelativeMSec
                });
            };

            source.Clr.GCStart += data =>
            {
                gcEvents.Add(new GcInfo
                {
                    Number = data.Count,
                    Generation = data.Depth,
                    Reason = data.Reason.ToString(),
                    TimeMSec = data.TimeStampRelativeMSec
                });
            };

            source.Clr.ExceptionStart += data =>
            {
                exceptionEvents.Add(new ExceptionInfo
                {
                    Type = data.ExceptionType,
                    Message = data.ExceptionMessage,
                    TimeMSec = data.TimeStampRelativeMSec
                });
            };

            // Process() may throw "Read past end of stream" on traces from newer runtimes
            // where the TraceEvent library doesn't fully understand the format. The event
            // callbacks above still collect valid data up to the point of failure, so we
            // catch the exception and use whatever was gathered.
            try
            {
                source.Process();
            }
            catch (Exception) when (jitMethods.Count > 0 || assemblyLoads.Count > 0 || gcEvents.Count > 0)
            {
                // Partial parse is acceptable — use events collected so far.
            }
        }

        var (cpuSamples, totalCpuSamples, cpuSampleError) = ParseCpuSamples(traceFilePath);

        return new ProfileReport
        {
            JitMethods = jitMethods.ToDictionary(kv => kv.Key, kv => kv.Value),
            AssemblyLoads = [.. assemblyLoads.OrderBy(a => a.TimeMSec)],
            GcEvents = [.. gcEvents.OrderBy(g => g.TimeMSec)],
            Exceptions = [.. exceptionEvents.OrderBy(e => e.TimeMSec)],
            CpuSamples = cpuSamples,
            TotalCpuSamples = totalCpuSamples,
            CpuSampleError = cpuSampleError,
        };
    }

    private static (List<CpuSampleEntry> Samples, int TotalSamples, string? Error) ParseCpuSamples(string traceFilePath)
    {
        var exclusiveCounts = new Dictionary<string, int>();
        var inclusiveCounts = new Dictionary<string, int>();
        int totalSamples = 0;
        string? error = null;

        string etlxPath = traceFilePath + ".etlx";
        try
        {
            var options = new TraceLogOptions { ContinueOnError = true };
            TraceLog.CreateFromEventPipeDataFile(traceFilePath, etlxPath, options);
            using var traceLog = TraceLog.OpenOrConvert(etlxPath);

            foreach (var evt in traceLog.Events)
            {
                if (evt.ProviderName != "Microsoft-DotNETCore-SampleProfiler")
                {
                    continue;
                }

                totalSamples++;
                var stack = evt.CallStack();
                if (stack == null)
                {
                    continue;
                }

                bool isTop = true;
                var seen = new HashSet<string>();

                while (stack != null)
                {
                    var fullName = stack.CodeAddress.Method?.FullMethodName;
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        if (isTop)
                        {
                            exclusiveCounts[fullName] = exclusiveCounts.GetValueOrDefault(fullName) + 1;
                            isTop = false;
                        }

                        if (seen.Add(fullName))
                        {
                            inclusiveCounts[fullName] = inclusiveCounts.GetValueOrDefault(fullName) + 1;
                        }
                    }

                    stack = stack.Caller;
                }
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
        finally
        {
            try { File.Delete(etlxPath); } catch { }
        }

        var entries = inclusiveCounts
            .Select(kv => new CpuSampleEntry
            {
                Method = kv.Key,
                InclusiveSamples = kv.Value,
                ExclusiveSamples = exclusiveCounts.GetValueOrDefault(kv.Key),
            })
            .OrderByDescending(e => e.ExclusiveSamples)
            .ThenByDescending(e => e.InclusiveSamples)
            .ToList();

        return (entries, totalSamples, error);
    }

    /// <summary>
    /// Namespace prefixes to exclude from the method report.
    /// These are framework/runtime methods that add noise — the user cares about
    /// application code (Azure.Mcp.*, etc.).
    /// </summary>
    private static readonly string[] ExcludedNamespacePrefixes =
    [
        "System.",
        "Microsoft.Extensions.",
        "Microsoft.AspNetCore.",
        "Microsoft.Win32.",
        "Internal.",
        "Interop.",
        "OpenTelemetry.",
        "Azure.Monitor.",
        "<Module>",
    ];

    private static bool IsExcludedNamespace(string methodKey)
    {
        foreach (var prefix in ExcludedNamespacePrefixes)
        {
            if (methodKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatMethodName(string ns, string name, string signature)
    {
        // Truncate very long signatures for readability
        var sig = signature.Length > 60 ? signature[..57] + "..." : signature;
        return string.IsNullOrEmpty(ns) ? $"{name}({sig})" : $"{ns}.{name}({sig})";
    }

    internal sealed class JitStats
    {
        public double StartTimeMSec { get; set; }
        public double TotalJitTimeMSec { get; set; }
        public int Count { get; set; }
    }

    internal sealed class AssemblyLoadInfo
    {
        public required string Name { get; init; }
        public double TimeMSec { get; init; }
    }

    internal sealed class GcInfo
    {
        public int Number { get; init; }
        public int Generation { get; init; }
        public required string Reason { get; init; }
        public double TimeMSec { get; init; }
    }

    internal sealed class ExceptionInfo
    {
        public required string Type { get; init; }
        public required string Message { get; init; }
        public double TimeMSec { get; init; }
    }

    internal sealed class CpuSampleEntry
    {
        public required string Method { get; init; }
        public int ExclusiveSamples { get; init; }
        public int InclusiveSamples { get; init; }
    }

    internal sealed class ProfileReport
    {
        public required Dictionary<string, JitStats> JitMethods { get; init; }
        public required List<AssemblyLoadInfo> AssemblyLoads { get; init; }
        public required List<GcInfo> GcEvents { get; init; }
        public required List<ExceptionInfo> Exceptions { get; init; }
        public required List<CpuSampleEntry> CpuSamples { get; init; }
        public required int TotalCpuSamples { get; init; }
        public string? CpuSampleError { get; init; }

        public string FormatReport(TimeSpan wallClock)
        {
            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                         SERVER PROFILER REPORT                                   ║");
            sb.AppendLine("╠══════════════════════════════════════════════════════════════════════════════════╝");
            var appMethods = JitMethods.Where(kv => !IsExcludedNamespace(kv.Key)).ToList();
            var systemMethods = JitMethods.Where(kv => IsExcludedNamespace(kv.Key)).ToList();

            sb.AppendLine($"║  Wall clock: {wallClock.TotalMilliseconds:F1}ms                                ");
            sb.AppendLine($"║  CPU samples: {TotalCpuSamples} (~{TotalCpuSamples * 10}ms sampled)            ");
            sb.AppendLine($"║  App method JIT: {appMethods.Sum(kv => kv.Value.TotalJitTimeMSec):F1}ms ({appMethods.Count} methods)");
            sb.AppendLine($"║  System method JIT: {systemMethods.Sum(kv => kv.Value.TotalJitTimeMSec):F1}ms ({systemMethods.Count} methods, hidden)");
            sb.AppendLine($"║  Assemblies loaded: {AssemblyLoads.Count}                                      ");
            sb.AppendLine($"║  GC collections: {GcEvents.Count}                                              ");
            sb.AppendLine($"║  Exceptions thrown: {Exceptions.Count}                                         ");
            sb.AppendLine("╚═════════════════════════════════════════════════════════════════════════════════");

            // CPU samples — exclusive = CPU was in this method; inclusive = method was on the stack
            if (CpuSamples.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("── Top 30 Methods by CPU Samples ──────────────────────────────────────────────");
                sb.AppendLine($"{"Method",-60} {"Exclusive",10} {"Inclusive",10} {"Exc%",7} {"Inc%",7}");
                sb.AppendLine(new string('─', 96));

                foreach (var entry in CpuSamples.Where(e => !IsExcludedNamespace(e.Method)).Take(30))
                {
                    var display = entry.Method.Length > 58 ? entry.Method[..55] + "..." : entry.Method;
                    var excPct = TotalCpuSamples > 0 ? (double)entry.ExclusiveSamples / TotalCpuSamples * 100 : 0;
                    var incPct = TotalCpuSamples > 0 ? (double)entry.InclusiveSamples / TotalCpuSamples * 100 : 0;
                    sb.AppendLine($"{display,-60} {entry.ExclusiveSamples,10} {entry.InclusiveSamples,10} {excPct,6:F1}% {incPct,6:F1}%");
                }
            }
            else if (CpuSampleError != null)
            {
                sb.AppendLine();
                sb.AppendLine($"── CPU Samples: failed to parse ({CpuSampleError}) ──");
            }

            // Top application methods by total JIT time (excluding System.*, etc.)
            sb.AppendLine();
            sb.AppendLine("── Top 30 App Methods by JIT Time (excl. System.*) ────────────────────────────");
            sb.AppendLine($"{"Method",-70} {"JIT (ms)",10} {"Count",6}");
            sb.AppendLine(new string('─', 88));

            foreach (var (method, stats) in appMethods
                .OrderByDescending(kv => kv.Value.TotalJitTimeMSec)
                .Take(30))
            {
                var display = method.Length > 68 ? method[..65] + "..." : method;
                sb.AppendLine($"{display,-70} {stats.TotalJitTimeMSec,10:F2} {stats.Count,6}");
            }

            // Assembly loads timeline
            sb.AppendLine();
            sb.AppendLine("── Assembly Load Timeline ──────────────────────────────────────────────────────");
            sb.AppendLine($"{"Time (ms)",10} {"Assembly",-70}");
            sb.AppendLine(new string('─', 82));

            foreach (var asm in AssemblyLoads.Take(50))
            {
                var name = asm.Name.Length > 68 ? asm.Name[..65] + "..." : asm.Name;
                sb.AppendLine($"{asm.TimeMSec,10:F1} {name,-70}");
            }

            if (AssemblyLoads.Count > 50)
            {
                sb.AppendLine($"  ... and {AssemblyLoads.Count - 50} more assemblies");
            }

            // GC events
            if (GcEvents.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("── GC Events ──────────────────────────────────────────────────────────────────");
                sb.AppendLine($"{"Time (ms)",10} {"Gen",4} {"Reason",-30}");
                sb.AppendLine(new string('─', 46));

                foreach (var gc in GcEvents)
                {
                    sb.AppendLine($"{gc.TimeMSec,10:F1} {gc.Generation,4} {gc.Reason,-30}");
                }
            }

            // Exceptions (first 20)
            if (Exceptions.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("── Exceptions During Startup ──────────────────────────────────────────────────");
                sb.AppendLine($"{"Time (ms)",10} {"Type",-40} {"Message",-30}");
                sb.AppendLine(new string('─', 82));

                foreach (var ex in Exceptions.Take(20))
                {
                    var type = ex.Type.Length > 38 ? ex.Type[..35] + "..." : ex.Type;
                    var msg = ex.Message.Length > 28 ? ex.Message[..25] + "..." : ex.Message;
                    sb.AppendLine($"{ex.TimeMSec,10:F1} {type,-40} {msg,-30}");
                }

                if (Exceptions.Count > 20)
                {
                    sb.AppendLine($"  ... and {Exceptions.Count - 20} more exceptions");
                }
            }

            return sb.ToString();
        }
    }
}
