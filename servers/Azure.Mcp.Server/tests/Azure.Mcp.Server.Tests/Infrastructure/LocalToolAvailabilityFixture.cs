// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ModelContextProtocol.Client;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

public sealed class LocalToolAvailabilityFixture : IAsyncLifetime
{
    private readonly ConcurrentQueue<string> _errors = new();
    private Process? _process;
    private McpClient? _client;

    public McpClient Client => _client ?? throw new InvalidOperationException("The HTTP test server has not started.");

    public async ValueTask InitializeAsync()
    {
        var executable = Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp");
        Assert.True(File.Exists(executable), $"Build Azure.Mcp.Server before running HTTP availability tests: {executable}");
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var endpoint = new Uri($"http://127.0.0.1:{port}/");
        var startInfo = new ProcessStartInfo(executable)
        {
            Arguments = "server start --transport http --mode all --namespace storage --namespace keyvault " +
                "--dangerously-disable-http-incoming-auth --disable-proxy-tools --disable-caching",
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.Environment["ASPNETCORE_URLS"] = endpoint.ToString();
        startInfo.Environment["AZURE_MCP_DANGEROUSLY_DISABLE_HTTPS_REDIRECTION"] = "true";
        startInfo.Environment["AZURE_MCP_COLLECT_TELEMETRY"] = "false";
        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start the HTTP test server.");
        _process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                _errors.Enqueue(args.Data);
            }
        };
        _process.BeginErrorReadLine();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));
        try
        {
            while (true)
            {
                if (_process.HasExited)
                {
                    throw new InvalidOperationException($"HTTP test server exited: {string.Join(Environment.NewLine, _errors)}");
                }
                try
                {
                    using var socket = new TcpClient();
                    await socket.ConnectAsync(IPAddress.Loopback, port, timeout.Token);
                    break;
                }
                catch (SocketException)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
                }
            }

            var transport = new HttpClientTransport(new()
            {
                Endpoint = endpoint,
                TransportMode = HttpTransportMode.StreamableHttp
            });
            _client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_client is not null)
            {
                await _client.DisposeAsync();
                _client = null;
            }
        }
        finally
        {
            if (_process is not null)
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    await _process.WaitForExitAsync();
                }
                _process.Dispose();
                _process = null;
            }
        }
    }
}
