// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace Microsoft.Mcp.Tests.Client;

public sealed class LiveServerFixture() : IAsyncLifetime
{
    private readonly SemaphoreSlim _startLock = new(1, 1);
    private Process? _httpServerProcess;
    private McpClient? _mcpClient;
    private ServerProfiler? _profiler;
    private string? _serverUrl;
    private bool _started;

    public Dictionary<string, string?> EnvironmentVariables { get; set; } = new();
    public List<string> Arguments { get; set; } = new();
    public ITestOutputHelper? Output { get; set; }
    public LiveTestSettings? Settings { get; set; }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async Task EnsureStartedAsync()
    {
        if (_started)
        {
            LogProfilerPaths();
            return;
        }
        await _startLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_started)
            {
                LogProfilerPaths();
                return;
            }

            // When enableProfiling is set in .testsettings.json (or MCP_TEST_PROFILE=true env var),
            // configure the server process to write an EventPipe trace file automatically.
            // The CLR writes trace data to disk on its own via DOTNET_EnableEventPipe, avoiding
            // the need for a diagnostic port listener that can block under concurrent tests.
            if (Settings?.EnableProfiling == true || ServerProfiler.IsEnabled)
            {
                var (profiler, profilerEnvVars) = ServerProfiler.Create();
                _profiler = profiler;
                foreach (var kv in profilerEnvVars)
                {
                    EnvironmentVariables[kv.Key] = kv.Value;
                }
            }

            string executablePath = McpTestUtilities.GetAzMcpExecutablePath();

            var (client, serverUrl) = await McpTestUtilities.CreateMcpClientAsync(
                executablePath,
                Arguments,
                EnvironmentVariables,
                process => _httpServerProcess = process,
                Output,
                Settings?.TestPackage,
                Settings?.SettingsDirectory);

            _mcpClient = client;
            _serverUrl = serverUrl;
            _started = true;

            LogProfilerPaths();
        }
        finally
        {
            _startLock.Release();
        }
    }

    private void LogProfilerPaths()
    {
        if (_profiler != null && Output != null)
        {
            Output.WriteLine($"[ServerProfiler] Report will be at: {_profiler.ReportFilePath}");
            Output.WriteLine($"[ServerProfiler] Trace will be at: {_profiler.TraceFilePath}");
        }
    }

    public McpClient GetMcpClient()
    {
        if (_mcpClient == null)
        {
            throw new InvalidOperationException("MCP Client has not been initialized.");
        }
        return _mcpClient;
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose the MCP client first — this closes stdio and causes the server to exit.
        if (_mcpClient != null)
        {
            try
            {
                await _mcpClient.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // swallow; continue disposing process
            }
            finally
            {
                _mcpClient = null;
            }
        }

        // Wait for the server process to exit so the CLR finalizes the .nettrace file.
        // The MCP client disposal above closes stdio, which should cause the server to
        // shut down gracefully. We give it time before resorting to a hard kill, because
        // DOTNET_EnableEventPipe needs a clean shutdown to flush the trace buffers.
        if (_httpServerProcess != null)
        {
            try
            {
                if (!_httpServerProcess.HasExited)
                {
                    if (!_httpServerProcess.WaitForExit(5000))
                    {
                        _httpServerProcess.Kill();
                        _httpServerProcess.WaitForExit(2000);
                    }
                }
            }
            catch
            {
                // swallow; ensure handle is released
            }

            _httpServerProcess.Dispose();
            _httpServerProcess = null;
        }

        // Generate the profiler report AFTER the process has exited, because
        // DOTNET_EnableEventPipe causes the CLR to finalize the .nettrace file on exit.
        if (_profiler != null)
        {
            await _profiler.WriteReportAsync();
            Console.Error.WriteLine($"[ServerProfiler] Trace: {_profiler.TraceFilePath}");
            Console.Error.WriteLine($"[ServerProfiler] Report: {_profiler.ReportFilePath}");
            await _profiler.DisposeAsync();
            _profiler = null;
        }
    }
}
