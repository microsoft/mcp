// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Helpers;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class CommandTestsBase(ITestOutputHelper output) : IAsyncLifetime, IDisposable
{
    protected const string TenantNameReason = "Service principals cannot use TenantName for lookup";

    protected McpClient Client { get; private set; } = default!;
    public LiveTestSettings Settings { get; set; } = default!;
    protected StringBuilder FailureOutput { get; } = new();
    protected ITestOutputHelper Output { get; } = output;

    public string[]? CustomArguments;
    public TestMode TestingMode = TestMode.Live;


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

    protected virtual async ValueTask InitializeAsyncInternal(TestProxyFixture? proxy = null)
    {
        TestingMode = TestEnvironment.GetEnvironmentTestMode();

        if (TestingMode is TestMode.Live || TestingMode is TestMode.Record)
        {
            var settingsFixture = new LiveTestSettingsFixture();
            await settingsFixture.InitializeAsync();
            Settings = settingsFixture.Settings;
        }
        else
        {
            // Playback mode: use sanitized placeholder values and disable any interactive auth.
            Settings = new LiveTestSettings
            {
                // Subscription / Tenant placeholders used by playback matching & sanitizers.
                SubscriptionId = "00000000-0000-0000-0000-000000000000",
                TenantId = "00000000-0000-0000-0000-000000000000",
                // ResourceBaseName used for vault name input to tools (maps to sanitized recording host).
                ResourceBaseName = "Sanitized",
                SubscriptionName = "Sanitized",
                TenantName = "Sanitized"
            };

            // Signal playback mode to child process & any credential logic.
            Environment.SetEnvironmentVariable("AZURE_MCP_PLAYBACK_MODE", "true");
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", Settings.TenantId);
            Environment.SetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", Settings.SubscriptionId);
            // TODO: Consider setting AZURE_TOKEN_CREDENTIALS=playback if additional branching is introduced.
        }

        string executablePath = McpTestUtilities.GetAzMcpExecutablePath();

        // Use custom arguments if provided, otherwise use standard mode (debug can be enabled via environment variable)
        var debugEnvVar = Environment.GetEnvironmentVariable("AZURE_MCP_TEST_DEBUG");
        var enableDebug = string.Equals(debugEnvVar, "true", StringComparison.OrdinalIgnoreCase) || Settings.DebugOutput;
        string[] defaultArgs = enableDebug
            ? ["server", "start", "--mode", "all", "--debug"]
            : ["server", "start", "--mode", "all"];
        var arguments = CustomArguments ?? defaultArgs;

        var dictionaryEvents = new Dictionary<string, string?> {
            // Propagate playback signaling & sanitized identifiers to server process.
            { "AZURE_MCP_PLAYBACK_MODE", TestingMode is TestMode.Playback ? "true" : null },
            { "AZURE_TENANT_ID", Settings.TenantId },
            { "AZURE_SUBSCRIPTION_ID", Settings.SubscriptionId }
        };

        if (proxy != null && proxy.Proxy != null)
        {
            dictionaryEvents.Add("TEST_PROXY_URL", proxy.Proxy.BaseUri);
        }

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = arguments,
            // Direct stderr to test output helper as required by task
            StandardErrorLines = line => Output.WriteLine($"[MCP Server] {line}"),
            EnvironmentVariables = dictionaryEvents
        };

        if (!string.IsNullOrEmpty(Settings.TestPackage))
        {
            Environment.CurrentDirectory = Settings.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", Settings.TestPackage, .. arguments];
        }

        var clientTransport = new StdioClientTransport(transportOptions);
        Output.WriteLine("Attemptig to start MCP Client");
        Client = await McpClient.CreateAsync(clientTransport);
        Output.WriteLine("MCP client initialized successfully");
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
