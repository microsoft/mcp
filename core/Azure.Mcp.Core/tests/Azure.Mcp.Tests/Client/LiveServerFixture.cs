using System.Diagnostics;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using Xunit;
using Xunit.Sdk;

namespace Azure.Mcp.Tests.Client;

public sealed class LiveServerFixture() : IAsyncLifetime
{
    private readonly SemaphoreSlim _startLock = new(1, 1);
    private Process? _httpServerProcess;
    private McpClient? _mcpClient;
    private string? _baseUrl;
    private bool _started;

    public Dictionary<string, string?> environmentVariables = new();
    public List<string> arguments = new();
    public ITestOutputHelper? output = null;
    public LiveTestSettings? settings = null;

    public async ValueTask InitializeAsync()
    {
    }

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
            _baseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5000";

            if (useHttp)
            {
                await StartHttpServerProcessAsync(executablePath, arguments);
                var transport = new HttpClientTransport(new()
                {
                    Endpoint = new Uri(_baseUrl),
                    TransportMode = HttpTransportMode.StreamableHttp
                });
                _mcpClient = await McpClient.CreateAsync(transport);
                output?.WriteLine($"HTTP test client initialized at {_baseUrl}");
                await WaitForServerReadinessAsync();
            }
            else
            {
                var clientTransport = await CreateStdioTransportAsync(executablePath, arguments);
                output?.WriteLine("Attempting to start MCP Client");
                _mcpClient = await McpClient.CreateAsync(clientTransport);
                output?.WriteLine("MCP client initialized successfully");
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

    private async Task StartHttpServerProcessAsync(string executablePath, List<string> arguments)
    {
        arguments.AddRange(new[] { "--transport", "http", "--outgoing-auth-strategy", "UseHostingEnvironmentIdentity", "--dangerously-disable-http-incoming-auth" });

        var processStartInfo = new ProcessStartInfo(executablePath, string.Join(" ", arguments.ToArray()))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var kvp in environmentVariables)
        {
            if (kvp.Value != null)
            {
                processStartInfo.Environment[kvp.Key] = kvp.Value;
            }
        }

        _httpServerProcess = Process.Start(processStartInfo);
    }

    private async Task WaitForServerReadinessAsync(int timeoutSeconds = 60, int pollIntervalMs = 500)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            throw new ArgumentException("Base URL is not set (ASPNETCORE_URLS).");
        }

        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                await _mcpClient!.ListToolsAsync(cancellationToken: CancellationToken.None);
                return;
            }
            catch
            {
                // swallow and retry
            }

            await Task.Delay(pollIntervalMs);
        }

        throw new TimeoutException($"MCP server at {_baseUrl} did not become ready within {timeoutSeconds} seconds.");
    }

    private async Task<IClientTransport> CreateStdioTransportAsync(string executablePath, List<string> arguments)
    {
        string[] args = arguments.ToArray();

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = args,
            // Direct stderr to test output helper as required by task
            StandardErrorLines = line => output?.WriteLine($"[MCP Server] {line}"),
            EnvironmentVariables = environmentVariables
        };

        if (!string.IsNullOrEmpty(settings!.TestPackage))
        {
            Environment.CurrentDirectory = settings!.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", settings!.TestPackage, .. args];
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
            finally
            {
                _httpServerProcess.Dispose();
                _httpServerProcess = null;
            }
        }
    }
}
