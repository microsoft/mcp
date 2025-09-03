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

    protected IMcpClient Client { get; private set; } = default!;
    protected LiveTestSettings Settings { get; private set; } = default!;
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

    // public virtual async ValueTask InitializeAsync()
    // {
    //     var settingsFixture = new LiveTestSettingsFixture();
    //     await settingsFixture.InitializeAsync();
    //     Settings = settingsFixture.Settings;

    //     string testAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    //     string executablePath = OperatingSystem.IsWindows() ? Path.Combine(testAssemblyPath, "azmcp.exe") : Path.Combine(testAssemblyPath, "azmcp");

    //     // Use custom arguments if provided, otherwise default to ["server", "start", "--debug"]
    //     var arguments = _customArguments ?? ["server", "start", "--mode", "all", "--debug"];

    //     StdioClientTransportOptions transportOptions = new()
    //     {
    //         Name = "Test Server",
    //         Command = executablePath,
    //         Arguments = arguments,
    //         StandardErrorLines = Output.WriteLine
    //     };

    //     if (!string.IsNullOrEmpty(Settings.TestPackage))
    //     {
    //         Environment.CurrentDirectory = Settings.SettingsDirectory;
    //         transportOptions.Command = "npx";
    //         transportOptions.Arguments = ["-y", Settings.TestPackage, .. arguments];
    //     }

    //     var clientTransport = new StdioClientTransport(transportOptions);

    //     Client = await McpClientFactory.CreateAsync(clientTransport);
    // }

    public virtual async ValueTask InitializeAsync()
    {
        var settingsFixture = new LiveTestSettingsFixture();
        await settingsFixture.InitializeAsync();
        Settings = settingsFixture.Settings;

        string testAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string executablePath = OperatingSystem.IsWindows() ? Path.Combine(testAssemblyPath, "azmcp.exe") : Path.Combine(testAssemblyPath, "azmcp");

        // Use custom arguments if provided, otherwise default to ["server", "start", "--debug"]
        var arguments = _customArguments ?? ["server", "start", "--mode", "all", "--debug"];

        StdioClientTransportOptions transportOptions = new()
        {
            Name = "Test Server",
            Command = executablePath,
            Arguments = arguments,
            StandardErrorLines = Output.WriteLine
        };

        if (!string.IsNullOrEmpty(Settings.TestPackage))
        {
            Environment.CurrentDirectory = Settings.SettingsDirectory;
            transportOptions.Command = "npx";
            transportOptions.Arguments = ["-y", Settings.TestPackage, .. arguments];
        }

        var clientTransport = new StdioClientTransport(transportOptions);

        Client = await McpClientFactory.CreateAsync(clientTransport);
    }

    public virtual ValueTask DisposeAsync()
    {
        Client?.DisposeAsync();
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
        // Failure output may contain request and response details that should be output for failed tests.
        if (TestContext.Current.TestState?.Result == TestResult.Failed && FailureOutput.Length > 0)
        {
            Output.WriteLine(FailureOutput.ToString());
        }
    }
}