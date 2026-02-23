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

    [Fact]
    public async Task AllMode_BestPracticesToolName_MustNotChange()
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
            // Wait for server to initialize
            await Task.Delay(3000, TestContext.Current.CancellationToken);

            // Send tools/list request
            var request = new JsonRpcRequest
            {
                Method = "tools/list",
                Params = JsonSerializer.SerializeToNode(new ListToolsRequestParams()),
                Id = new RequestId(1)
            };

            var requestJson = JsonSerializer.Serialize(request);
            await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), TestContext.Current.CancellationToken);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read response
            var responseLine = await process.StandardOutput.ReadLineAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(responseLine);

            var response = JsonSerializer.Deserialize<JsonRpcResponse>(responseLine);
            Assert.NotNull(response);
            Assert.NotNull(response.Result);

            var result = JsonSerializer.Deserialize<ListToolsResult>(JsonSerializer.Serialize(response.Result));
            Assert.NotNull(result);
            Assert.NotNull(result.Tools);

            var toolNames = result.Tools.Select(t => t.Name).ToList();

            // Assert - Verify the Azure Best Practices tool name exists and hasn't changed
            // Visual Studio has a hard-coded dependency on this exact tool name in FirstPartyToolsProvider.cs
            // Changing this name will break Visual Studio's integration with Azure MCP Server
            // Reference: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
            Assert.Contains(AzureBestPracticesToolName, toolNames);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            }
        }
    }

    [Fact]
    public async Task AllMode_ExtensionCliGenerateToolName_MustNotChange()
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
            // Wait for server to initialize
            await Task.Delay(3000, TestContext.Current.CancellationToken);

            // Send tools/list request
            var request = new JsonRpcRequest
            {
                Method = "tools/list",
                Params = JsonSerializer.SerializeToNode(new ListToolsRequestParams()),
                Id = new RequestId(2)
            };

            var requestJson = JsonSerializer.Serialize(request);
            await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), TestContext.Current.CancellationToken);
            await process.StandardInput.FlushAsync(TestContext.Current.CancellationToken);

            // Read response
            var responseLine = await process.StandardOutput.ReadLineAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(responseLine);

            var response = JsonSerializer.Deserialize<JsonRpcResponse>(responseLine);
            Assert.NotNull(response);
            Assert.NotNull(response.Result);

            var result = JsonSerializer.Deserialize<ListToolsResult>(JsonSerializer.Serialize(response.Result));
            Assert.NotNull(result);
            Assert.NotNull(result.Tools);

            var toolNames = result.Tools.Select(t => t.Name).ToList();

            // Assert - Verify the Extension CLI Generate tool name exists and hasn't changed
            // Visual Studio has a hard-coded dependency on this exact tool name in FirstPartyToolsProvider.cs
            // Changing this name will break Visual Studio's integration with Azure MCP Server 
            // Reference: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
            Assert.Contains(ExtensionCliGenerateToolName, toolNames);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            }
        }
    }
}
