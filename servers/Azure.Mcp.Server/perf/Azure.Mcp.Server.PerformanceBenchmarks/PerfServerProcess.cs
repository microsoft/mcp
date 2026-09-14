// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Shared helpers for launching the Azure MCP Server as an external process in HTTP
/// transport mode and probing its readiness. Used by both the startup-timing harness
/// (<see cref="McpClientStartupMeasurement"/>) and the resource-usage harness
/// (<see cref="ResourceUsageMeasurement"/>) so the launch, readiness-poll, and teardown
/// behavior stay identical across measurements.
/// </summary>
internal static class PerfServerProcess
{
    /// <summary>
    /// Reserves and immediately releases a free loopback TCP port so the server can bind it.
    /// </summary>
    internal static int GetFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Starts the server at <paramref name="exePath"/> in HTTP transport mode bound to
    /// <paramref name="endpoint"/>, appending the HTTP transport flags to
    /// <paramref name="serverArgs"/> and draining stdout/stderr so the process pipes never
    /// fill and block it. The caller owns the returned process and must terminate it with
    /// <see cref="TryKillProcessTree"/>.
    /// </summary>
    internal static Process StartHttpServer(string exePath, string[] serverArgs, Uri endpoint)
    {
        var httpArgs = new List<string>(serverArgs)
        {
            "--transport", "http", "--dangerously-disable-http-incoming-auth",
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in httpArgs)
        {
            startInfo.ArgumentList.Add(arg);
        }
        startInfo.Environment["ASPNETCORE_URLS"] = endpoint.ToString();

        var server = new Process { StartInfo = startInfo };
        if (!server.Start())
        {
            throw new InvalidOperationException($"Failed to start HTTP server process '{exePath}'.");
        }

        // Drain the server's stdout/stderr so its pipes never fill and block it.
        _ = server.StandardOutput.ReadToEndAsync();
        _ = server.StandardError.ReadToEndAsync();

        return server;
    }

    /// <summary>
    /// Polls <paramref name="port"/> on the loopback interface until a TCP connection
    /// succeeds, returning the elapsed milliseconds from now. Throws if the server exits
    /// early or the timeout elapses.
    /// </summary>
    internal static async Task<long> WaitForPortAsync(int port, TimeSpan timeout, Process server)
    {
        var sw = Stopwatch.StartNew();
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (server.HasExited)
            {
                throw new InvalidOperationException(
                    $"HTTP server exited early with code {server.ExitCode} before accepting connections.");
            }

            try
            {
                using var probe = new TcpClient();
                var connectTask = probe.ConnectAsync(IPAddress.Loopback, port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(250));
                if (completed == connectTask && probe.Connected)
                {
                    sw.Stop();
                    return sw.ElapsedMilliseconds;
                }
            }
            catch (SocketException)
            {
                // Not listening yet; keep polling.
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"HTTP server did not accept connections on port {port} within {timeout.TotalSeconds:N0}s.");
    }

    /// <summary>
    /// Best-effort termination of the server process and its children.
    /// </summary>
    internal static void TryKillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
        }
        catch
        {
            // Best effort during teardown; nothing actionable if the process is already gone.
        }
    }
}
