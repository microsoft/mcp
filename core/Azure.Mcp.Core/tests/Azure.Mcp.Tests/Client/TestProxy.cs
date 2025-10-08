// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Azure.Mcp.Tests.Generated;

namespace Azure.Mcp.Tests.Client;

/// <summary>
/// Lightweight test-proxy process manager used per test class to start/stop the Azure SDK test proxy.
/// This version intentionally avoids dependencies on prior internal abstractions that were missing
/// (e.g. TestEnvironment / ProcessTracker) while still providing stderr/stdout capture for failed tests.
/// </summary>
public sealed class TestProxy(string repositoryRoot, bool debug = false) : IDisposable
{
    private readonly string _repositoryRoot = repositoryRoot;
    private readonly bool _debug = debug;
    private readonly StringBuilder _stderr = new();
    private readonly StringBuilder _stdout = new();
    private Process? _process;
    private CancellationTokenSource? _cts;
    private int? _httpPort;
    private bool _disposed;

    public string? ExecutablePath { get; init; } = ResolveProxyExecutable();
    public string BaseUri => _httpPort is int p ? $"http://127.0.0.1:{p}/" : throw new InvalidOperationException("Proxy not started");

    public TestProxyClient Client { get; private set; } = default!;
    public TestProxyAdminClient AdminClient => Client.GetTestProxyAdminClient();

    /// <summary>
    /// Start the proxy process if not already started.
    /// </summary>
    public void Start()
    {
        if (_process != null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(ExecutablePath) || !File.Exists(ExecutablePath))
        {
            throw new InvalidOperationException("Unable to locate test-proxy executable. Set PROXY_EXE or ensure 'test-proxy' exists on PATH.");
        }

        var storageLocation = Environment.GetEnvironmentVariable("TEST_PROXY_STORAGE") ?? _repositoryRoot;
        var args = $"start -u --storage-location=\"{storageLocation}\"";

        var isDll = ExecutablePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        ProcessStartInfo psi;
        if (isDll)
        {
            // Run through dotnet host
            var host = GetDotNetHost();
            psi = new ProcessStartInfo(host, $"\"{ExecutablePath}\" {args}");
        }
        else
        {
            psi = new ProcessStartInfo(ExecutablePath, args);
        }
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;

        if (_debug)
        {
            psi.EnvironmentVariables["Logging__LogLevel__Azure.Sdk.Tools.TestProxy"] = "Debug";
            psi.EnvironmentVariables["Logging__LogLevel__Default"] = "Information";
        }
        else
        {
            psi.EnvironmentVariables["Logging__LogLevel__Azure.Sdk.Tools.TestProxy"] = "Error";
            psi.EnvironmentVariables["Logging__LogLevel__Default"] = "Error";
        }
        psi.EnvironmentVariables["ASPNETCORE_URLS"] = "http://127.0.0.1:0"; // Let proxy choose free port

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start test proxy process.");
        _cts = new CancellationTokenSource(); //todo: put this in as filler, I feel like I should be pulling a cancellation token from somewhere else
        _ = Task.Run(() => PumpAsync(_process.StandardError, _stderr, _cts.Token));
        _ = Task.Run(() => PumpAsync(_process.StandardOutput, _stdout, _cts.Token));

        // parse port
        _httpPort = WaitForHttpPort(TimeSpan.FromSeconds(15));
        if (_httpPort is null)
        {
            throw new InvalidOperationException($"Failed to detect test-proxy HTTP port. Output: {_stdout}\nErrors: {_stderr}");
        }

        Client = new TestProxyClient(new Uri(BaseUri), new TestProxyClientOptions());
    }

    // NOTE: Previous static singleton implementation removed due to undefined state fields.
    // If needed, reintroduce with explicit private static fields and thread-safe disposal.

    private static async Task PumpAsync(StreamReader reader, StringBuilder sink, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && !reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null) break;
                lock (sink)
                {
                    sink.AppendLine(line);
                }
            }
        }
        catch { /* swallow */ }
    }

    private int? WaitForHttpPort(TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start) < timeout)
        {
            string text;
            lock (_stdout)
            {
                text = _stdout.ToString();
            }
            foreach (var line in text.Split('\n'))
            {
                if (TryParsePort(line.Trim(), out var p))
                {
                    return p;
                }
            }
            if (_process?.HasExited == true) break;
            Thread.Sleep(50);
        }
        return null;
    }

    private static bool TryParsePort(string line, out int port)
    {
        port = 0;
        const string prefix = "Now listening on: http://127.0.0.1:";
        if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        var remainder = line[prefix.Length..].TrimEnd('/','\r');
        return int.TryParse(remainder, out port);
    }

    private static string? ResolveProxyExecutable()
    {
        // 1. Assembly metadata attribute (preferred explicit test assembly provisioning)
        var fromMetadata = GetExecutableFromAssemblyMetadata();
        if (fromMetadata != null) return fromMetadata;

        // 2. Explicit environment variable overrides
        var explicitPath = Environment.GetEnvironmentVariable("PROXY_EXE") ?? Environment.GetEnvironmentVariable("TEST_PROXY_EXE");
        if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath)) return explicitPath;

        // 3. Search PATH for known executable names
        var names = new[] { "test-proxy", "test-proxy.exe", "Azure.Sdk.Tools.TestProxy", "Azure.Sdk.Tools.TestProxy.exe" };
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var name in names)
            {
                var full = Path.Combine(dir, name);
                if (File.Exists(full)) return full;
            }
        }
        return null;
    }

    private static string? GetExecutableFromAssemblyMetadata()
    {
        try
        {
            var assembly = typeof(TestProxy).Assembly;
            var value = assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => string.Equals(a.Key, "TestProxyPath", StringComparison.OrdinalIgnoreCase))?
                .Value;
            if (string.IsNullOrWhiteSpace(value)) return null;

            // If value points to a directory, attempt to locate a dll or exe within.
            if (Directory.Exists(value))
            {
                // Prefer dll
                var dll = Directory.EnumerateFiles(value, "Azure.Sdk.Tools.TestProxy*.dll").FirstOrDefault();
                if (dll != null) return dll;
                var exe = Directory.EnumerateFiles(value, "test-proxy*.exe").FirstOrDefault();
                if (exe != null) return exe;
                return null;
            }

            if (File.Exists(value)) return value;
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string GetDotNetHost()
    {
        var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
        // Try DOTNET_INSTALL_DIR then PATH
        var installDir = Environment.GetEnvironmentVariable("DOTNET_INSTALL_DIR");
        if (!string.IsNullOrWhiteSpace(installDir))
        {
            var candidate = Path.Combine(installDir, exe);
            if (File.Exists(candidate)) return candidate;
        }
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir, exe);
            if (File.Exists(candidate)) return candidate;
        }
        throw new InvalidOperationException("Unable to locate dotnet host.");
    }

    public string? SnapshotStdErr()
    {
        lock (_stderr)
        {
            return _stderr.Length == 0 ? null : _stderr.ToString();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _cts?.Cancel();
            if (_process != null && !_process.HasExited)
            {
                try { _process.Kill(entireProcessTree: true); } catch { }
            }
            _process?.Dispose();
            _cts?.Dispose();
        }
        catch { }
    }
}
