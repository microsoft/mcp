// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ToolMetadataExporter.Models;

/// <summary>
/// The result of a `server info` request from the MCP server.
/// </summary>
public class ServerInfoResult
{
    [JsonPropertyName("status")]

    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("results")]
    public ServerInfo? Results { get; set; }
}

public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}
