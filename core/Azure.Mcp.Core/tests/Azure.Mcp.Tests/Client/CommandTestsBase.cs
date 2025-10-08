// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;
using System.Diagnostics;
using System.Reflection;

namespace Azure.Mcp.Tests.Client;

public abstract class CommandTestsBase(ITestOutputHelper output) : IAsyncLifetime, IDisposable
{
    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";

    protected IMcpClient Client { get; private set; } = default!;
    protected LiveTestSettings Settings { get; private set; } = default!;
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; } = output;
    protected TestProxy? Proxy { get; private set; }

    private string[]? _customArguments;
    private TestMode _testMode = TestMode.Live;

    /// <summary>
    /// Sets custom arguments for the MCP server. Call this before InitializeAsync().
    /// </summary>
    /// <param name="arguments">Custom arguments to pass to the server (e.g., ["server", "start", "--mode", "single"])</param>
    public void SetArguments(params string[] arguments)
    {
        _customArguments = arguments;
    }

    public virtual async ValueTask InitializeAsync()
    {
        // Initialize settings
        var settingsFixture = new LiveTestSettingsFixture();
        await settingsFixture.InitializeAsync();
        Settings = settingsFixture.Settings;

        _testMode = GetTestMode();

        // If record/playback requested, start the test proxy BEFORE launching components that may capture env vars
        if (ShouldUseProxy())
        {
            Proxy = new TestProxy(DetermineRepositoryRoot(), debug: Settings.DebugOutput);
            Proxy.Start();
            Output.WriteLine($"Test proxy started at {Proxy.BaseUri} (mode: {_testMode})");
            // Export a conventional environment variable for downstream HTTP clients if they honor it.
            Environment.SetEnvironmentVariable("TEST_PROXY_HTTP_URI", Proxy.BaseUri);
        }

        string executablePath = McpTestUtilities.GetAzMcpExecutablePath();

        // Use custom arguments if provided, otherwise use standard mode (debug can be enabled via environment variable)
        var debugEnvVar = Environment.GetEnvironmentVariable("AZURE_MCP_TEST_DEBUG");
        var enableDebug = string.Equals(debugEnvVar, "true", StringComparison.OrdinalIgnoreCase) || Settings.DebugOutput;
        string[] defaultArgs = enableDebug
            ? ["server", "start", "--mode", "all", "--debug"]
            : ["server", "start", "--mode", "all"];
        var arguments = _customArguments ?? defaultArgs;

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = arguments,
            // Direct stderr to test output helper as required by task
            StandardErrorLines = line => Output.WriteLine($"[MCP Server] {line}")
        };

        if (!string.IsNullOrEmpty(Settings.TestPackage))
        {
            Environment.CurrentDirectory = Settings.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", Settings.TestPackage, .. arguments];
        }

        var clientTransport = new StdioClientTransport(transportOptions);
        Client = await McpClientFactory.CreateAsync(clientTransport);

        Output.WriteLine("MCP client initialized successfully");
    }

    protected Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters)
    {
        return CallToolAsync(command, parameters, Client);
    }

    protected async Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters, IMcpClient mcpClient)
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

            // For validation errors, we'll return a synthetic error response
            if (ex.Message.Contains("An error occurred"))
            {
                // Return null to indicate error response (no results)
                writeOutput("synthetic error response: null (error response)");
                return null;
            }

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

    public async ValueTask DisposeAsync()
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

            // Dispose proxy at end of test class lifetime
            Proxy?.Dispose();
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
        Proxy?.Dispose();
    }

    // Hook into per-test execution boundaries: call start/stop around test body.
    // Consumers should call these manually if they override test invocation pattern. Otherwise utilize xUnit lifecycle via BeforeAfter (future) or direct base usage in test methods.
    protected void BeginTest() => StartRecordPlaybackSessionIfNeeded();
    protected Task BeginTestAsync()
    {
        StartRecordPlaybackSessionIfNeeded();
        return Task.CompletedTask;
    }
    protected Task EndTestAsync() => StopRecordPlaybackSessionAsync();
    protected void EndTest() => EndTestAsync().GetAwaiter().GetResult();

    // Simple placeholders for future record/playback support. Currently no-op to satisfy build.
    private bool _recordPlaybackSessionStarted;
    private void StartRecordPlaybackSessionIfNeeded()
    {
        if (_recordPlaybackSessionStarted) return;
        // In the future: inspect environment variables to decide whether to record or playback.
        // e.g. AZURE_MCP_TEST_RECORD or AZURE_MCP_TEST_PLAYBACK, integrate with a proxy.
        _recordPlaybackSessionStarted = true;
        if (ShouldUseProxy())
        {
            Output.WriteLine($"Begin test session with proxy ({_testMode})");
        }
    }

    private Task StopRecordPlaybackSessionAsync()
    {
        if (!_recordPlaybackSessionStarted) return Task.CompletedTask;
        _recordPlaybackSessionStarted = false;
        // In the future: flush recordings / dispose proxy.
        if (ShouldUseProxy())
        {
            Output.WriteLine("End test session (proxy active)");
        }
        return Task.CompletedTask;
    }

    private (Type? DeclaringType, string? MethodName) GetCurrentMethod()
    {
        // Fallback: unable to access richer xUnit v3 metadata in current reference set.
        // We can attempt to read the current stack and locate the first test method in this assembly.
        try
        {
            var stack = new System.Diagnostics.StackTrace();
            var thisAsm = typeof(CommandTestsBase).Assembly;
            for (int i = 0; i < stack.FrameCount; i++)
            {
                var m = stack.GetFrame(i)?.GetMethod();
                if (m?.DeclaringType?.Assembly == thisAsm && m.GetCustomAttributes(typeof(FactAttribute), inherit: true).Length > 0)
                {
                    return (m.DeclaringType, m.Name);
                }
            }
        }
        catch { }
        return (null, null);        
    }

    private TestMode GetTestMode()
    {
        var mode = Environment.GetEnvironmentVariable("AZURE_MCP_TEST_MODE");
        if (string.IsNullOrWhiteSpace(mode)) return TestMode.Live;
        return Enum.TryParse<TestMode>(mode, ignoreCase: true, out var parsed) ? parsed : TestMode.Live;
    }

    private bool ShouldUseProxy() => _testMode is TestMode.Record or TestMode.Playback ||
        string.Equals(Environment.GetEnvironmentVariable("USE_TEST_PROXY"), "true", StringComparison.OrdinalIgnoreCase);

    private string DetermineRepositoryRoot()
    {
        // Heuristic: walk up from assembly location until we find a .git folder or reach root.
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;
        while (!string.IsNullOrEmpty(path))
        {
            if (Directory.Exists(Path.Combine(path, ".git"))) return path;
            var parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent) || parent == path) break;
            path = parent;
        }
        return Environment.CurrentDirectory;
    }
}
