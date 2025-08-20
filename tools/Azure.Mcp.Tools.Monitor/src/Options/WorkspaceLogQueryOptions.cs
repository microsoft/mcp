// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options;

public class WorkspaceLogQueryOptions : WorkspaceOptions
{
    public string? Query { get; set; }
    public int? Hours { get; set; }
    public int? Limit { get; set; }
    [JsonPropertyName(MonitorOptionDefinitions.TableNameName)]
    public string? TableName { get; set; }
}
