// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class CommandTestsBase(ITestOutputHelper output, TestProxyFixture fixture) : IAsyncLifetime, IDisposable, IClassFixture<TestProxyFixture>
{
    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";

    protected McpClient Client { get; private set; } = default!;
    protected LiveTestSettings Settings { get; private set; } = default!;
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; } = output;
    protected TestProxy? Proxy { get; private set; } = fixture.Proxy;

    private string[]? _customArguments;
    private TestMode _testMode = TestMode.Live;

    // Recording path support (lightweight) ----------------------------------
    private static readonly RecordingPathResolver _pathResolver = new();

    // TODO: grab asyncncess of the test. Given that it is good practice to separate sync and async tests, this may
    // not be necessary?
    protected virtual bool IsAsync => false;

    // TODO: do I need to worry about service version? Adding a versionQualifier here just in case. Feedback on PR will clean it out possibly.
    protected virtual string? VersionQualifier => null;

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
        _testMode = GetTestMode();

        if (_testMode is TestMode.Live)
        {
            var settingsFixture = new LiveTestSettingsFixture();
            await settingsFixture.InitializeAsync();
            Settings = settingsFixture.Settings;
        }
        else
        {
            Settings = new LiveTestSettings();
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
        Client = await McpClient.CreateAsync(clientTransport);

        Output.WriteLine("MCP client initialized successfully");

        await StartRecordOrPlayback();
    }

    protected Task<JsonElement?> CallToolAsync(string command, Dictionary<string, object?> parameters)
    {
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
        await StopRecordOrPlayback();
        await Client.DisposeAsync().ConfigureAwait(false);
    }

    private async Task StartRecordOrPlayback()
    {
        if (Proxy is null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        var testName = TryGetCurrentTestName();
        var pathToRecording = GetSessionFilePath(testName);
        var assetsPath = _pathResolver.GetAssetsJson(GetType());

        var recordOptions = new Dictionary<string, string>
            {
                { "x-recording-file", pathToRecording },
            };

        if (!string.IsNullOrWhiteSpace(assetsPath))
        {
            recordOptions["x-recording-assets-file"] = assetsPath;
        }
        // todo: replace after regenerating using Azure.Core instead of System.ClientModel
        var bodyContent = BinaryContentHelper.FromObject(recordOptions);

        if (_testMode is TestMode.Playback)
        {
            Output.WriteLine($"[Playback] Session file: {pathToRecording}");
            await Proxy.Client.StartPlaybackAsync(bodyContent).ConfigureAwait(false);
        }
        else if (_testMode is TestMode.Record)
        {
            Output.WriteLine($"[Record] Session file: {pathToRecording}");
            Proxy.Client.StartRecord(bodyContent);
        }

        await Task.CompletedTask;
    }

    private async Task StopRecordOrPlayback()
    {
        if (Proxy is null)
        {
            throw new InvalidOperationException("Test proxy is not initialized.");
        }

        if (_testMode is TestMode.Playback)
        {
            await Proxy.Client.StopPlaybackAsync("placeholder-ignore").ConfigureAwait(false);
        }
        else if (_testMode is TestMode.Record)
        {
            // TODO: feed variables / metadata to proxy stop.
            Proxy.Client.StopRecord("placeholder-ignore", new Dictionary<string, string>());
        }
        await Task.CompletedTask;
    }

    private TestMode GetTestMode()
    {
        var mode = Environment.GetEnvironmentVariable("AZURE_RECORD_MODE");
        if (string.IsNullOrWhiteSpace(mode)) return TestMode.Live;
        return Enum.TryParse<TestMode>(mode, ignoreCase: true, out var parsed) ? parsed : TestMode.Live;
    }

    private static string TryGetCurrentTestName()
    {
        var name = TestContext.Current?.Test?.TestCase.TestCaseDisplayName;
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Test name is not available. Recording requires a valid test name.");
        }
        return name;
    }

    private string GetSessionFilePath(string displayName)
    {
        var sanitized = RecordingPathResolver.Sanitize(displayName);
        var dir = _pathResolver.GetSessionDirectory(GetType(), variantSuffix: null); // TODO: supply variant suffix if needed
        var fileName = RecordingPathResolver.BuildFileName(sanitized, IsAsync, VersionQualifier);
        var fullPath = Path.Combine(dir, fileName).Replace('\\', '/');
        return fullPath;
    }
}
