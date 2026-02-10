using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public sealed class LiveServerFixture() : IAsyncLifetime
{
    private readonly SemaphoreSlim _startLock = new(1, 1);
    private Process? _httpServerProcess;
    private McpClient? _mcpClient;
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
            return;
        }
        await _startLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_started)
            {
                return;
            }
            string executablePath = McpTestUtilities.GetAzMcpExecutablePath();
            bool useHttp = string.Equals(Environment.GetEnvironmentVariable("MCP_TEST_TRANSPORT"), "http", StringComparison.OrdinalIgnoreCase);

            if (useHttp)
            {
                var port = GetAvailablePort();
                _serverUrl = $"http://localhost:{port}";
                EnvironmentVariables["ASPNETCORE_URLS"] = _serverUrl;
                await StartHttpServerProcessAsync(executablePath, Arguments);
                var transport = new HttpClientTransport(new()
                {
                    Endpoint = new Uri(_serverUrl),
                    TransportMode = HttpTransportMode.StreamableHttp
                });
                _mcpClient = await McpClient.CreateAsync(transport);
                Output?.WriteLine($"HTTP test client initialized at {_serverUrl}");
            }
            else
            {
                var clientTransport = await CreateStdioTransportAsync(executablePath, Arguments);
                Output?.WriteLine("Attempting to start MCP Client");
                _mcpClient = await McpClient.CreateAsync(clientTransport);
                Output?.WriteLine("MCP client initialized successfully");
            }
            _started = true;
        }
        finally
        {
            _startLock.Release();
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

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async Task StartHttpServerProcessAsync(string executablePath, List<string> processArguments)
    {
        processArguments.AddRange(new[] { "--transport", "http", "--outgoing-auth-strategy", "UseHostingEnvironmentIdentity", "--dangerously-disable-http-incoming-auth" });

        var processStartInfo = new ProcessStartInfo(executablePath, string.Join(" ", processArguments.ToArray()))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var kvp in EnvironmentVariables)
        {
            if (kvp.Value != null)
            {
                processStartInfo.Environment[kvp.Key] = kvp.Value;
            }
        }

        _httpServerProcess = Process.Start(processStartInfo);

        if (_httpServerProcess == null)
        {
            throw new InvalidOperationException("Failed to start HTTP server process.");
        }

        _httpServerProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                try
                {
                    Output?.WriteLine($"[HTTP Server stdout] {e.Data}");
                }
                catch (InvalidOperationException)
                {
                    // Test has completed; ignore output
                }
            }
        };
        _httpServerProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                try
                {
                    Output?.WriteLine($"[HTTP Server stderr] {e.Data}");
                }
                catch (InvalidOperationException)
                {
                    // Test has completed; ignore output
                }
            }
        };

        _httpServerProcess.BeginOutputReadLine();
        _httpServerProcess.BeginErrorReadLine();

        await WaitForServerReadinessAsync();
    }

    private async Task WaitForServerReadinessAsync(int timeoutSeconds = 30, int pollIntervalMs = 500)
    {
        using var httpClient = new HttpClient();
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "tools/list",
                    @params = new { },
                    id = Guid.NewGuid().ToString()
                };

                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _serverUrl)
                {
                    Content = content
                };
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                using var resp = await httpClient!.SendAsync(requestMessage);
                if (resp.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server not yet available, continue polling
            }
            await Task.Delay(pollIntervalMs);
        }

        throw new TimeoutException($"Server at {_serverUrl} did not become ready within {timeoutSeconds} seconds");
    }

    private async Task<IClientTransport> CreateStdioTransportAsync(string executablePath, List<string> stdioArguments)
    {
        string[] args = stdioArguments.ToArray();

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = args,
            // Direct stderr to test output helper as required by task
            StandardErrorLines = line => Output?.WriteLine($"[MCP Server] {line}"),
            EnvironmentVariables = EnvironmentVariables
        };

        if (!string.IsNullOrEmpty(Settings!.TestPackage))
        {
            Environment.CurrentDirectory = Settings!.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", Settings!.TestPackage, .. args];
        }

        return await Task.FromResult(new StdioClientTransport(transportOptions));
    }

    public async ValueTask DisposeAsync()
    {
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

        if (_httpServerProcess != null)
        {
            try
            {
                if (!_httpServerProcess.HasExited)
                {
                    _httpServerProcess.Kill();
                    _httpServerProcess.WaitForExit(2000);
                }
            }
            catch
            {
                // swallow; ensure handle is released
            }

            _httpServerProcess.Dispose();
            _httpServerProcess = null;
        }
    }
}
