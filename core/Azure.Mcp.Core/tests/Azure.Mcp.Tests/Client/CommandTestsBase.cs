// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;
using static System.Net.WebRequestMethods;

namespace Azure.Mcp.Tests.Client;

public abstract class CommandTestsBase(ITestOutputHelper output) : IAsyncLifetime, IDisposable
{
    private HttpClient? _http;
    private Process? _httpServerProcess;
    private static string? _baseUrl => Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    private static bool UseHttp => string.Equals(Environment.GetEnvironmentVariable("MCP_TEST_TRANSPORT"), "http", StringComparison.OrdinalIgnoreCase);

    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";

    protected McpClient Client { get; private set; } = default!;
    protected LiveTestSettings Settings { get; set; } = default!;
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; } = output;

    public string[]? CustomArguments;
    public TestMode TestMode = TestMode.Live;

    /// <summary>
    /// Sets custom arguments for the MCP server. Call this before InitializeAsync().
    /// </summary>
    /// <param name="arguments">Custom arguments to pass to the server (e.g., ["server", "start", "--mode", "single"])</param>
    public void SetArguments(params string[] arguments)
    {
        CustomArguments = arguments;
    }

    public virtual async ValueTask InitializeAsync()
    {
        await InitializeAsyncInternal(null);
    }

    public static LiveTestSettings PlaybackSettings => new()
    {
        SubscriptionId = "00000000-0000-0000-0000-000000000000",
        TenantId = "00000000-0000-0000-0000-000000000000",
        ResourceBaseName = "Sanitized",
        SubscriptionName = "Sanitized",
        TenantName = "Sanitized",
        ResourceGroupName = "Sanitized",
        TestMode = TestMode.Playback
    };

    protected virtual async ValueTask LoadSettingsAsync()
    {
        Settings = await TryLoadLiveSettingsAsync().ConfigureAwait(false) ?? PlaybackSettings;

        // if the user has set to playback in LiveTestSettings, they're
        // intentionally checking playback mode, load the playback settings
        // and ignore what we got from the .testsettings.json file
        if (Settings.TestMode == TestMode.Playback)
        {
            Settings = PlaybackSettings;
        }

        TestMode = Settings.TestMode;
    }

    private async Task<LiveTestSettings?> TryLoadLiveSettingsAsync()
    {
        try
        {
            var settingsFixture = new LiveTestSettingsFixture();
            await settingsFixture.InitializeAsync().ConfigureAwait(false);
            return settingsFixture.Settings;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a stdio transport for local process-based testing.
    /// </summary>
    /// <param name="proxy">Optional test proxy fixture to configure environment variables for playback or recording.</param>
    private async Task<IClientTransport> CreateStdioTransportAsync(TestProxyFixture? proxy, string executablePath, List<string> arguments)
    {
        string[] args = arguments.ToArray();
        var envVarDictionary = GetEnvironmentVariables(proxy);

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = args,
            // Direct stderr to test output helper as required by task
            StandardErrorLines = line => Output.WriteLine($"[MCP Server] {line}"),
            EnvironmentVariables = envVarDictionary
        };

        if (!string.IsNullOrEmpty(Settings.TestPackage))
        {
            Environment.CurrentDirectory = Settings.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", Settings.TestPackage, .. args];
        }

        return await Task.FromResult(new StdioClientTransport(transportOptions));
    }

    /// <summary>
    /// Creates an HTTP (SSE) transport for remote server testing.
    /// Set MCP_TEST_SERVER_URL environment variable to specify the server URL.
    /// </summary>
    /// <param name="proxy">Optional test proxy fixture to configure environment variables for playback or recording.</param>
    private async Task CreateHttpClientAsync(TestProxyFixture? proxy)
    {
        _http = new HttpClient();

        if (proxy?.Proxy != null)
        {
            _http.DefaultRequestHeaders.TryAddWithoutValidation("x-recording-upstream-base-uri", _baseUrl);
            _http.DefaultRequestHeaders.TryAddWithoutValidation(
                "x-recording-mode", TestMode is TestMode.Playback ? "playback" : TestMode is TestMode.Record ? "record" : "live");
        }

        await Task.CompletedTask;
    }

    protected void SetHttpRecordingId(string recordingId)
    {
        if (_http != null && !string.IsNullOrEmpty(recordingId))
        {
            if (_http.DefaultRequestHeaders.Contains("x-recording-id"))
            {
                _http.DefaultRequestHeaders.Remove("x-recording-id");
            }
            _http.DefaultRequestHeaders.TryAddWithoutValidation("x-recording-id", recordingId);
        }
    }

    private async Task<JsonElement?> CallToolOverHttpAsync(string command, Dictionary<string, object?> parameters)
    {
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new { name = command, arguments = parameters },
            id = Guid.NewGuid().ToString()
        };

        var uri = $"{_baseUrl}/mcp";
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var resp = await _http!.PostAsync(uri, content);
        var body = await resp.Content.ReadAsStringAsync();
        resp.EnsureSuccessStatusCode();

        var root = JsonSerializer.Deserialize<JsonElement>(body);
        if (root.TryGetProperty("result", out var result) &&
            result.TryGetProperty("results", out var results))
        {
            return results;
        }
        return null;
    }

    private async Task StartHttpServerProcessAsync(TestProxyFixture? proxy, string executablePath, List<string> arguments)
    {
        // Configure arguments for HTTP mode
        arguments.AddRange(new[] { "--transport", "http", "--outgoing-auth-strategy", "UseHostingEnvironmentIdentity" });

        var envVarDictionary = GetEnvironmentVariables(proxy);

        var processStartInfo = new ProcessStartInfo(executablePath, string.Join(" ", arguments.ToArray()))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var kvp in envVarDictionary)
        {
            if (kvp.Value != null)
            {
                processStartInfo.Environment[kvp.Key] = kvp.Value;
            }
        }

        _httpServerProcess = Process.Start(processStartInfo);

        await WaitForServerReadinessAsync(_baseUrl);
    }

    private Dictionary<string, string?> GetEnvironmentVariables(TestProxyFixture? proxy)
    {
        Dictionary<string, string?> envVarDictionary = [
            // Propagate playback signaling & sanitized identifiers to server process.

            // TODO: Temporarily commenting these out until we can solve for subscription id tests
            // see https://github.com/microsoft/mcp/issues/1103
            // { "AZURE_TENANT_ID", Settings.TenantId },
            // { "AZURE_SUBSCRIPTION_ID", Settings.SubscriptionId }
        ];

        if (proxy != null && proxy.Proxy != null)
        {
            envVarDictionary.Add("TEST_PROXY_URL", proxy.Proxy.BaseUri);

            if (TestMode is TestMode.Playback)
            {
                envVarDictionary.Add("AZURE_TOKEN_CREDENTIALS", "PlaybackTokenCredential");
            }
        }

        // Add any custom environment variables from settings
        if (Settings?.EnvironmentVariables != null)
        {
            foreach (var kvp in Settings.EnvironmentVariables)
            {
                envVarDictionary[kvp.Key] = kvp.Value;
            }
        }

        return envVarDictionary;
    }

    private async Task WaitForServerReadinessAsync(string? uri, int timeoutSeconds = 30, int pollIntervalMs = 500)
    {
        using var httpClient = new HttpClient();
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var response = await httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    return; // Server is ready
                }
            }
            catch (HttpRequestException)
            {
                // Server not yet available, continue polling
            }
            await Task.Delay(pollIntervalMs);
        }

        throw new TimeoutException($"Server at {uri} did not become ready within {timeoutSeconds} seconds");
    }

    protected virtual async ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null)
    {
        await LoadSettingsAsync();
        string executablePath = McpTestUtilities.GetAzMcpExecutablePath();

        // Use custom arguments if provided, otherwise use standard mode (debug can be enabled via environment variable)
        var debugEnvVar = Environment.GetEnvironmentVariable("AZURE_MCP_TEST_DEBUG");
        var enableDebug = string.Equals(debugEnvVar, "true", StringComparison.OrdinalIgnoreCase) || Settings.DebugOutput;
        List<string> defaultArgs = enableDebug
            ? ["server", "start", "--mode", "all", "--debug"]
            : ["server", "start", "--mode", "all"];
        var arguments = CustomArguments?.ToList() ?? defaultArgs;

        if (UseHttp)
        {
            await CreateHttpClientAsync(proxy);
            await StartHttpServerProcessAsync(proxy, executablePath, arguments);
            Output.WriteLine($"HTTP test client initialized at {_baseUrl}");
            return;
        }

        var clientTransport = await CreateStdioTransportAsync(proxy, executablePath, arguments);
        Output.WriteLine("Attempting to start MCP Client");
        Client = await McpClient.CreateAsync(clientTransport);
        Output.WriteLine("MCP client initialized successfully");
    }

    protected Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters)
    {
        if (UseHttp && _http != null)
            return CallToolOverHttpAsync(command, parameters);
        return CallToolAsync(command, parameters, Client);
    }

    protected async Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters, McpClient mcpClient)
    {
        // Use the same debug logic as MCP server initialization
        var debugEnvVar = Environment.GetEnvironmentVariable("AZURE_MCP_TEST_DEBUG");
        var enableDebug = string.Equals(debugEnvVar, "true", StringComparison.OrdinalIgnoreCase) || Settings.DebugOutput;

        // Output will be streamed, so if we're not in debug mode, hold the debug output for logging in the failure case
        Action<string> writeOutput = enableDebug
            ? s => Output.WriteLine(s)
            : s => FailureOutput.AppendLine(s);

        writeOutput($"request: {JsonSerializer.Serialize(new { command, parameters })}");

        CallToolResult result;
        try
        {
            result = await mcpClient.CallToolAsync(command, parameters);
        }
        catch (ModelContextProtocol.McpException ex)
        {
            // MCP client throws exceptions for error responses, but we want to handle them gracefully
            writeOutput($"MCP exception: {ex.Message}");
            throw; // Re-throw if we can't handle it
        }

        var content = McpTestUtilities.GetFirstText(result.Content);
        if (string.IsNullOrWhiteSpace(content))
        {
            writeOutput($"response: {JsonSerializer.Serialize(result)}");
            throw new Exception("No JSON content found in the response.");
        }

        JsonElement root;
        try
        {
            root = JsonSerializer.Deserialize<JsonElement>(content!);
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new Exception("Invalid JSON response.");
            }

            // Remove the `args` property and log the content
            var trimmed = root.Deserialize<JsonObject>()!;
            trimmed.Remove("args");
            writeOutput($"response: {trimmed.ToJsonString(new JsonSerializerOptions { WriteIndented = true })}");
        }
        catch (Exception ex)
        {
            // If we can't json parse the content as a JsonObject, log the content and throw an exception
            writeOutput($"response: {content}");
            throw new Exception("Failed to deserialize JSON response.", ex);
        }

        return root.TryGetProperty("results", out var property) ? property : null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public virtual async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    // subclasses should override this method to dispose resources
    // overrides should still call base.Dispose(disposing)
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // No unmanaged resources to release, but if we had, we'd release them here.
            // _disposableResource?.Dispose();
            // _disposableResource = null;

            // Handle things normally disposed in DisposeAsyncCore
            if (Client is IDisposable disposable)
            {
                disposable.Dispose();
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
                catch { }
                finally
                {
                    _httpServerProcess.Dispose();
                    _httpServerProcess = null;
                }
            }
        }

        // Failure output may contain request and response details that should be output for failed tests.
        if (TestContext.Current?.TestState?.Result == TestResult.Failed && FailureOutput.Length > 0)
        {
            Output.WriteLine(FailureOutput.ToString());
        }
    }

    // subclasses should override this method to dispose async resources
    // overrides should still call base.DisposeAsyncCore()
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await Client.DisposeAsync().ConfigureAwait(false);
    }
}
