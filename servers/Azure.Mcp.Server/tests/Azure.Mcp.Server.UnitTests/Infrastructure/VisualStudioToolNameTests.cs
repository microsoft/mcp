// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Mcp.Core.Protocol.Models.Requests;
using Microsoft.Mcp.Core.Protocol.Models.Responses;

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
            await Task.Delay(3000);

            // Send tools/list request
            var listToolsRequest = new ListToolsRequest
            {
                Method = "tools/list",
                Params = new ListToolsParams { }
            };

            var requestJson = JsonSerializer.Serialize(listToolsRequest);
            await process.StandardInput.WriteLineAsync(requestJson);
            await process.StandardInput.FlushAsync();

            // Read response
            var responseLine = await process.StandardOutput.ReadLineAsync();
            Assert.NotNull(responseLine);

            var response = JsonSerializer.Deserialize<ListToolsResponse>(responseLine);
            Assert.NotNull(response);
            Assert.NotNull(response.Result);
            Assert.NotNull(response.Result.Tools);

            var toolNames = response.Result.Tools.Select(t => t.Name).ToList();

            // Assert - Verify the Azure Best Practices tool name exists and hasn't changed
            Assert.Contains(AzureBestPracticesToolName, toolNames,
                $"""
                CRITICAL: The tool name '{AzureBestPracticesToolName}' was not found!
                
                Visual Studio has a hard-coded dependency on this exact tool name in FirstPartyToolsProvider.cs.
                Changing this name will break Visual Studio's integration with Azure MCP Server.
                
                Reference: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
                
                If this tool name must change, you MUST coordinate with the Visual Studio team first.
                Available tools: {string.Join(", ", toolNames.Where(n => n.Contains("bestpractices", StringComparison.OrdinalIgnoreCase)))}
                """);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
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
            await Task.Delay(3000);

            // Send tools/list request
            var listToolsRequest = new ListToolsRequest
            {
                Method = "tools/list",
                Params = new ListToolsParams { }
            };

            var requestJson = JsonSerializer.Serialize(listToolsRequest);
            await process.StandardInput.WriteLineAsync(requestJson);
            await process.StandardInput.FlushAsync();

            // Read response
            var responseLine = await process.StandardOutput.ReadLineAsync();
            Assert.NotNull(responseLine);

            var response = JsonSerializer.Deserialize<ListToolsResponse>(responseLine);
            Assert.NotNull(response);
            Assert.NotNull(response.Result);
            Assert.NotNull(response.Result.Tools);

            var toolNames = response.Result.Tools.Select(t => t.Name).ToList();

            // Assert - Verify the Extension CLI Generate tool name exists and hasn't changed
            Assert.Contains(ExtensionCliGenerateToolName, toolNames,
                $"""
                CRITICAL: The tool name '{ExtensionCliGenerateToolName}' was not found!
                
                Visual Studio has a hard-coded dependency on this exact tool name in FirstPartyToolsProvider.cs.
                Changing this name will break Visual Studio's integration with Azure MCP Server.
                
                Reference: https://devdiv.visualstudio.com/DevDiv/_git/VisualStudio.Conversations/pullrequest/705038
                
                If this tool name must change, you MUST coordinate with the Visual Studio team first.
                Available tools: {string.Join(", ", toolNames.Where(n => n.Contains("extension", StringComparison.OrdinalIgnoreCase) || n.Contains("cli", StringComparison.OrdinalIgnoreCase)))}
                """);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
    }
}
