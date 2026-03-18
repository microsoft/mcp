// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class CopyJobCreateOptions : CopyJobOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.JobPropertiesConst)]
    public string? JobProperties { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.ModeConst)]
    public string? Mode { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.WorkerCountConst)]
    public int? WorkerCount { get; set; }
}
