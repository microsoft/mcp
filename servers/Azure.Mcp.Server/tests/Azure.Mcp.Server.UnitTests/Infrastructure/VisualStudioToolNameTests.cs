// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

/// <summary>
/// Tests to ensure specific tool names that Visual Studio depends on remain stable.
/// Visual Studio has hard-coded dependencies on these tool names in FirstPartyToolsProvider.cs
/// See: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
/// </summary>
public class VisualStudioToolNameTests
{
    private const string AzureBestPracticesToolName = "get_azure_bestpractices_get";
    private const string ExtensionCliGenerateToolName = "extension_cli_generate";

    /// <summary>
    /// Starts the Azure MCP server in 'all' mode, performs MCP initialization handshake,
    /// and retrieves the list of tool names.
    /// </summary>
    private static async Task<List<string>> GetAllModeToolNamesAsync()
    {
        // Arrange
        var exeName = OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp";
        var azmcpPath = Path.Combine(AppContext.BaseDirectory, exeName);

        Assert.True(File.Exists(azmcpPath), $"Executable not found at {azmcpPath}. Please build the Azure.Mcp.Server project first.");

        // Act - Start the server process in "all" mode which exposes individual tools
        var processStartInfo = new ProcessStartInfo
        {
            FileName = azmcpPath,
            Arguments = "server start --mode all",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        Assert.NotNull(process);

        try
        {
            // Use a generous timeout for server initialization. In --mode all, the server connects
            // to all external MCP registry servers (which can take 15-30 seconds) before
            // responding. We use an independent timeout rather than TestContext.Current.CancellationToken
            // to avoid being cancelled by the test runner's per-test timeout.
            using var initializationTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var timeoutToken = initializationTimeout.Token;

            // Attempt to read any startup output/errors to ensure process is running
            // If the process exits immediately, this will help catch that
            await Task.Delay(500, timeoutToken); // Brief delay to allow process to fail fast if misconfigured
            Assert.False(process.HasExited, "Server process exited immediately after start");

            // Send initialize request first (required by MCP protocol)
            var initRequest = new JsonRpcRequest
            {
                Method = "initialize",
                Params = JsonSerializer.SerializeToNode(new
                {
                    protocolVersion = "2024-11-05",
                    capabilities = new { },
                    clientInfo = new { name = "test-client", version = "1.0" }
                }),
                Id = new RequestId(1)
            };

            var initRequestJson = JsonSerializer.Serialize(initRequest);
            await process.StandardInput.WriteLineAsync(initRequestJson.AsMemory(), timeoutToken);
            await process.StandardInput.FlushAsync(timeoutToken);

            // Read initialize response — in --mode all the server may take 15+ seconds to
            // respond while it connects to external MCP registry servers.
            var initResponseLine = await ReadJsonLineAsync(process.StandardOutput, timeoutToken);
            Assert.NotNull(initResponseLine);

            // Send initialized notification
            var initializedNotification = new JsonRpcNotification
            {
                Method = "notifications/initialized"
            };

            var notificationJson = JsonSerializer.Serialize(initializedNotification);
            await process.StandardInput.WriteLineAsync(notificationJson.AsMemory(), timeoutToken);
            await process.StandardInput.FlushAsync(timeoutToken);

            // Send tools/list request
            var request = new JsonRpcRequest
            {
                Method = "tools/list",
                Params = JsonSerializer.SerializeToNode(new ListToolsRequestParams()),
                Id = new RequestId(2)
            };

            var requestJson = JsonSerializer.Serialize(request);
            await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), timeoutToken);
            await process.StandardInput.FlushAsync(timeoutToken);

            // Read response — skip any non-JSON lines (e.g. server log output written to stdout
            // by the process or a failing external subprocess such as azd).
            var responseLine = await ReadJsonLineAsync(process.StandardOutput, timeoutToken);
            Assert.NotNull(responseLine);

            var response = JsonSerializer.Deserialize<JsonRpcResponse>(responseLine);
            Assert.NotNull(response);
            Assert.NotNull(response.Result);

            var result = JsonSerializer.Deserialize<ListToolsResult>(JsonSerializer.Serialize(response.Result));
            Assert.NotNull(result);
            Assert.NotNull(result.Tools);

            return result.Tools.Select(t => t.Name).ToList();
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// Reads lines from <paramref name="reader"/> until a line that starts with '{' is found,
    /// skipping any non-JSON output (log messages, subprocess errors, etc.) the server process
    /// may write to stdout alongside MCP JSON-RPC messages.
    /// </summary>
    private static async Task<string?> ReadJsonLineAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (line.AsSpan().TrimStart().StartsWith('{'))
            {
                return line;
            }
            // Non-JSON line — skip it. The server or a failing stdio subprocess may write
            // diagnostic text to stdout; we don't want that to break JSON-RPC parsing.
        }
        return null;
    }

    [Fact(Timeout = 180000)] // 3-minute timeout: --mode all connects to external MCP registry servers on startup
    public async Task AllMode_VisualStudioToolNames_MustNotChange()
    {
        // Act - Get tool names from server
        var toolNames = await GetAllModeToolNamesAsync();

        // Assert - Verify both Visual Studio tool names exist and haven't changed
        // Visual Studio has hard-coded dependencies on these exact tool names in FirstPartyToolsProvider.cs
        // Changing these names will break Visual Studio's integration with Azure MCP Server
        // Reference: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
        Assert.Contains(AzureBestPracticesToolName, toolNames);
        Assert.Contains(ExtensionCliGenerateToolName, toolNames);
    }
}
