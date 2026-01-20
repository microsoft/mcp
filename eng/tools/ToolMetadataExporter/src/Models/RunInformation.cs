// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ToolMetadataExporter.Services;

namespace ToolMetadataExporter.Models;

/// <summary>
/// Represents information about a specific run of the tool analysis.
/// </summary>
public class RunInformation
{
    public RunInformation(AzmcpProgram mcpServer)
    {
        McpServer = mcpServer;
    }

    public string Id { get; } = Guid.NewGuid().ToString();

    public AzmcpProgram McpServer { get; }

    public async Task<string> GetRunInfoFileNameAsync(string baseFileName)
    {
        var version = await McpServer.GetServerVersionAsync();

        return $"{version}_{baseFileName}";
    }
}
