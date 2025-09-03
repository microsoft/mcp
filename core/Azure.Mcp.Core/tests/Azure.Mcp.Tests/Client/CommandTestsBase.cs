// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class CommandTestsBase(ITestOutputHelper output) : IAsyncLifetime, IDisposable
{
    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";

    // Shared client instance to avoid multiple server processes
    private static IMcpClient? _sharedClient;
    private static LiveTestSettings? _sharedSettings;
    private static readonly object _lock = new();
    private static readonly List<string> _serverErrorLog = new();

    protected IMcpClient Client => _sharedClient ?? throw new InvalidOperationException("Client not initialized");
    protected LiveTestSettings Settings => _sharedSettings ?? throw new InvalidOperationException("Settings not initialized");
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; } = output;

    private string[]? _customArguments;

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
        bool shouldInitialize = false;

        lock (_lock)
        {
            // If client is already initialized, just return
            if (_sharedClient != null && _sharedSettings != null)
            {
                // Output any previously captured server errors to this test's output
                if (_serverErrorLog.Count > 0)
                {
                    Output.WriteLine($"=== MCP Server Error Log ({_serverErrorLog.Count} entries) ===");
                    foreach (var error in _serverErrorLog)
                    {
                        Output.WriteLine($"[MCP Server] {error}");
                    }
                }
                return;
            }

            shouldInitialize = true;
        }

        if (!shouldInitialize)
        {
            return;
        }

        // Initialize settings
        var settingsFixture = new LiveTestSettingsFixture();
        await settingsFixture.InitializeAsync();

        lock (_lock)
        {
            _sharedSettings = settingsFixture.Settings;
        }

        string testAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string executablePath = OperatingSystem.IsWindows() ?
            Path.Combine(testAssemblyPath, "azmcp.exe") :
            Path.Combine(testAssemblyPath, "azmcp");

        // Use custom arguments if provided, otherwise default to debug mode
        var arguments = _customArguments ?? ["server", "start", "--mode", "all", "--debug"];

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = arguments,
            // Redirect stderr to shared log for later output during test failure
            StandardErrorLines = line =>
            {
                lock (_lock)
                {
                    _serverErrorLog.Add(line);
                }
            }
        };

        if (!string.IsNullOrEmpty(Settings.TestPackage))
        {
            Environment.CurrentDirectory = Settings.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", Settings.TestPackage, .. arguments];
        }

        var clientTransport = new StdioClientTransport(transportOptions);

        try
        {
            // Add timeout to prevent hanging in CI environments
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var clientTask = McpClientFactory.CreateAsync(clientTransport);
            var client = await clientTask.WaitAsync(cts.Token);

            lock (_lock)
            {
                _sharedClient = client;
            }

            Output.WriteLine("MCP client initialized successfully");
        }
        catch (TimeoutException)
        {
            var recentErrors = _serverErrorLog.TakeLast(5).ToList();
            var errorSummary = recentErrors.Count > 0 ?
                $"Recent server errors: {string.Join("; ", recentErrors)}" :
                "No server error output captured";

            Output.WriteLine($"MCP client initialization timed out after 2 minutes. {errorSummary}");
            throw new TimeoutException($"MCP client initialization timed out after 2 minutes. {errorSummary}");
        }
        catch (OperationCanceledException)
        {
            var recentErrors = _serverErrorLog.TakeLast(5).ToList();
            var errorSummary = recentErrors.Count > 0 ?
                $"Recent server errors: {string.Join("; ", recentErrors)}" :
                "No server error output captured";

            Output.WriteLine($"MCP client initialization was cancelled or timed out. {errorSummary}");
            throw new OperationCanceledException($"MCP client initialization was cancelled or timed out. {errorSummary}");
        }
    }

    public virtual ValueTask DisposeAsync()
    {
        // Don't dispose the shared client - it will be disposed when the last test completes
        return ValueTask.CompletedTask;
    }

    protected async Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters)
    {
        // Output will be streamed, so if we're not in debug mode, hold the debug output for logging in the failure case
        Action<string> writeOutput = Settings.DebugOutput
            ? s => Output.WriteLine(s)
            : s => FailureOutput.AppendLine(s);

        writeOutput($"request: {JsonSerializer.Serialize(new { command, parameters })}");

        CallToolResult result;
        try
        {
            result = await Client.CallToolAsync(command, parameters);
        }
        catch (ModelContextProtocol.McpException ex)
        {
            // MCP client throws exceptions for error responses, but we want to handle them gracefully
            // Check if the exception contains error response information that we can parse
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
        // Output stderr logs for debugging purposes
        try
        {
            lock (_lock)
            {
                if (_serverErrorLog.Count > 0)
                {
                    Output.WriteLine("=== MCP Server stderr logs ===");
                    foreach (var line in _serverErrorLog)
                    {
                        Output.WriteLine($"[MCP Server] {line}");
                    }
                    Output.WriteLine("=== End MCP Server stderr logs ===");
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Output helper may not be available in some test execution contexts
        }

        // Failure output may contain request and response details that should be output for failed tests.
        try
        {
            if (TestContext.Current?.TestState?.Result == TestResult.Failed && FailureOutput.Length > 0)
            {
                Output.WriteLine(FailureOutput.ToString());
            }
        }
        catch (InvalidOperationException)
        {
            // TestContext.Current may not be available in some test execution contexts
            // In this case, we skip the failure output logging
        }
    }

    // Static cleanup method - call this in a cleanup fixture if needed
    public static async ValueTask CleanupSharedClientAsync()
    {
        IMcpClient? clientToDispose = null;

        lock (_lock)
        {
            if (_sharedClient != null)
            {
                clientToDispose = _sharedClient;
                _sharedClient = null;
                _sharedSettings = null;
                _serverErrorLog.Clear();
            }
        }

        if (clientToDispose != null)
        {
            await clientToDispose.DisposeAsync();
        }
    }
}
