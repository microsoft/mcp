// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Periodically samples an external process's resource counters (working set, private
/// bytes, handle count, and cumulative CPU time) on a background loop, tagging every
/// sample with the caller-supplied phase label so the samples can be aggregated per
/// phase after the run completes.
///
/// The sampler collects into a private list from its own loop; callers must
/// <see cref="Stop"/> the sampler and await the <see cref="RunAsync"/> task before
/// reading <see cref="Samples"/> so there is no concurrent access to the backing list.
/// </summary>
internal sealed class ProcessResourceSampler
{
    private readonly Process _process;
    private readonly TimeSpan _interval;
    private readonly List<ResourceSample> _samples = [];
    private readonly Stopwatch _clock = new();
    private volatile string _phase = "init";
    private volatile bool _running;

    public ProcessResourceSampler(Process process, TimeSpan interval)
    {
        _process = process;
        _interval = interval;
    }

    /// <summary>A single point-in-time reading of the process resource counters.</summary>
    internal readonly record struct ResourceSample(
        double ElapsedSeconds,
        string Phase,
        long WorkingSetBytes,
        long PrivateBytes,
        int HandleCount,
        double CpuSeconds);

    /// <summary>The samples collected so far. Only read after <see cref="Stop"/> + awaiting <see cref="RunAsync"/>.</summary>
    public IReadOnlyList<ResourceSample> Samples => _samples;

    /// <summary>Tags all subsequent samples with <paramref name="phase"/>.</summary>
    public void BeginPhase(string phase) => _phase = phase;

    /// <summary>
    /// Runs the sampling loop until <see cref="Stop"/> is called, taking one reading every
    /// interval plus a final reading on exit.
    /// </summary>
    public async Task RunAsync()
    {
        _running = true;
        _clock.Start();
        while (_running)
        {
            TrySample();
            await Task.Delay(_interval);
        }

        TrySample();
    }

    /// <summary>Signals the sampling loop to stop after its current iteration.</summary>
    public void Stop() => _running = false;

    private void TrySample()
    {
        try
        {
            _process.Refresh();
            if (_process.HasExited)
            {
                return;
            }

            _samples.Add(new ResourceSample(
                _clock.Elapsed.TotalSeconds,
                _phase,
                _process.WorkingSet64,
                _process.PrivateMemorySize64,
                SafeHandleCount(_process),
                _process.TotalProcessorTime.TotalSeconds));
        }
        catch
        {
            // The process can exit between the HasExited check and a counter read; a
            // dropped sample is acceptable for a statistical baseline.
        }
    }

    /// <summary>
    /// Reads the handle count, returning 0 on platforms where it is not supported so the
    /// sampler degrades gracefully instead of throwing.
    /// </summary>
    private static int SafeHandleCount(Process process)
    {
        try
        {
            return process.HandleCount;
        }
        catch
        {
            return 0;
        }
    }
}
