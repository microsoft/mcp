using System.Diagnostics;
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
