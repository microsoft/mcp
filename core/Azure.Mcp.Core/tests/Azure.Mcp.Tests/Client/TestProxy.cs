// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Azure.Mcp.Tests.Generated;

namespace Azure.Mcp.Tests.Client;

/// <summary>
/// Lightweight test-proxy process manager used per test class to start/stop the Azure SDK test proxy.
/// This version intentionally avoids dependencies on prior internal abstractions that were missing
/// (e.g. TestEnvironment / ProcessTracker) while still providing stderr/stdout capture for failed tests.
/// </summary>
public sealed class TestProxy(bool debug = false) : IDisposable
{
    private readonly bool _debug = debug;
    public StringBuilder stderr = new();
    public readonly StringBuilder stdout = new();
    private Process? _process;
    private CancellationTokenSource? _cts;
    private int? _httpPort;
    private bool _disposed;

    public string BaseUri => _httpPort is int p ? $"http://127.0.0.1:{p}/" : throw new InvalidOperationException("Proxy not started");

    public TestProxyClient Client { get; private set; } = default!;
    public TestProxyAdminClient AdminClient { get; private set; } = default!;

    private static string? _cachedProxyDir;

    private static async Task<string> GetClient()
    {
        var proxyDir = GetProxyDirectory();
        var version = "1.0.0-dev.20251022.1";
        var (assetName, isArchive) = GetAssetNameForPlatform();
        var targetDir = Path.Combine(proxyDir, "Azure.Sdk.Tools.TestProxy", version);

        Directory.CreateDirectory(targetDir);

        var url = $"https://github.com/Azure/azure-sdk-tools/releases/download/Azure.Sdk.Tools.TestProxy_{version}/{assetName}";
        var downloadPath = Path.Combine(targetDir, assetName);

        if (!File.Exists(downloadPath))
        {
            using var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(downloadPath, bytes);
        }

        if (isArchive)
        {
            var extractDir = Path.Combine(targetDir, "extracted");
            if (!Directory.Exists(extractDir))
            {
                if (assetName.EndsWith(".tar.gz"))
                {
                    TarFile.ExtractToDirectory(downloadPath, extractDir, true);
                }
                else
                {
                    ZipFile.ExtractToDirectory(downloadPath, extractDir);
                }
            }
            return FindExecutableInDirectory(extractDir);
        }

        return downloadPath;
    }

    private static (string assetName, bool isArchive) GetAssetNameForPlatform()
    {
        var arch = RuntimeInformation.ProcessArchitecture;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return (arch == Architecture.Arm64 ? "test-proxy-standalone-win-arm64.zip" : "test-proxy-standalone-win-x64.zip", true);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return (arch == Architecture.Arm64 ? "test-proxy-standalone-osx-arm64.zip" : "test-proxy-standalone-osx-x64.zip", true);
        }
        return (arch == Architecture.Arm64 ? "test-proxy-standalone-linux-arm64.tar.gz" : "test-proxy-standalone-linux-x64.tar.gz", true);
    }

    private static string FindExecutableInDirectory(string dir)
    {
        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Azure.Sdk.Tools.TestProxy.exe" : "Azure.Sdk.Tools.TestProxy";
        foreach (var file in Directory.EnumerateFiles(dir, exeName, SearchOption.AllDirectories))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                EnsureExecutable(file);
            }
            return file;
        }
        throw new FileNotFoundException($"Could not find {exeName} in {dir}");
    }

    private static void EnsureExecutable(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        var mode = File.GetUnixFileMode(path);
        if (!mode.HasFlag(UnixFileMode.UserExecute))
        {
            File.SetUnixFileMode(path, mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }
    }

    private static string GetProxyDirectory()
    {
        if (_cachedProxyDir != null) return _cachedProxyDir;

        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            var gitPath = Path.Combine(current, ".git");
            if (File.Exists(gitPath) || Directory.Exists(gitPath))
            {
                _cachedProxyDir = Path.Combine(current, ".proxy");
                Directory.CreateDirectory(_cachedProxyDir);
                return _cachedProxyDir;
            }
            current = Directory.GetParent(current)?.FullName;
        }
        throw new InvalidOperationException("Could not find repository root (.git)");
    }

    private static string? GetExecutableFromAssetsDirectory()
    {
        var proxyDir = GetProxyDirectory();
        var toolDir = Path.Combine(proxyDir, "Azure.Sdk.Tools.TestProxy");

        if (!Directory.Exists(toolDir)) return null;

        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "test-proxy.exe" : "test-proxy";
        foreach (var file in Directory.EnumerateFiles(toolDir, exeName, SearchOption.AllDirectories))
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                EnsureExecutable(file);
            }
            return file;
        }

        return null;
    }

    public void Start(string repositoryRoot)
    {
        if (_process != null)
        {
            return;
        }

        var proxyExe = GetExecutableFromAssetsDirectory() ?? GetClient().GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(proxyExe) || !File.Exists(proxyExe))
        {
            throw new InvalidOperationException("Unable to locate test-proxy executable.");
        }

        var storageLocation = Environment.GetEnvironmentVariable("TEST_PROXY_STORAGE") ?? repositoryRoot;
        var args = $"start --http-proxy --storage-location=\"{storageLocation}\"";

        ProcessStartInfo psi = new(proxyExe, args);
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.EnvironmentVariables["ASPNETCORE_URLS"] = "http://127.0.0.1:0"; // Let proxy choose free port

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start test proxy process.");
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => PumpAsync(_process.StandardError, stderr, _cts.Token));
        _ = Task.Run(() => PumpAsync(_process.StandardOutput, stdout, _cts.Token));

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PROXY_MANUAL_START")))
        {
            _httpPort = 5000;
        }
        else
        {
            _httpPort = WaitForHttpPort(TimeSpan.FromSeconds(15));
        }

        if (_httpPort is null)
        {
            throw new InvalidOperationException($"Failed to detect test-proxy HTTP port. Output: {stdout}\nErrors: {stderr}");
        }

        Client = new TestProxyClient(new Uri(BaseUri), new TestProxyClientOptions());
        AdminClient = Client.GetTestProxyAdminClient();
    }

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
            lock (stdout)
            {
                text = stdout.ToString();
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

    /// <summary>
    /// Snapshots the current stderr output from the testproxy. This is a destructive read; the internal buffer is cleared after the call.
    ///
    /// This means that if multiple tests fail in sequence, each test will only see the stderr output generated since the last call to SnapshotStdErr(), which means
    /// we won't be seeing errors from previous test failures. This is intentional to ensure that each test only gets the relevant stderr output.
    /// </summary>
    /// <returns></returns>
    public string? SnapshotStdErr()
    {
        lock (stderr)
        {
            var toOutput = stderr.Length == 0 ? null : stderr.ToString();

            stderr = new();

            return toOutput;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        _cts?.Cancel();
        _cts?.Dispose();

        if (_process != null && !_process.HasExited)
        {
            _process.Kill();
        }
        _process?.Dispose();
    }
}
