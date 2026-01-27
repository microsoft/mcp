// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ToolMetadataExporter.Services;

namespace ToolMetadataExporter.Models;

/// <summary>
/// Represents information about a specific run of the tool analysis.
/// </summary>
public class RunInformation(AzmcpProgram mcpServer)
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public AzmcpProgram McpServer { get; } = mcpServer;

    public async Task<string> GetRunInfoFileNameAsync(string baseFileName)
    {
        var version = await McpServer.GetServerVersionAsync();

        return $"{version}_{baseFileName}";
    }
}
