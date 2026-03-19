// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class CopyJobCreateOptions : CopyJobOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.JobPropertiesName)]
    public string? JobProperties { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.ModeName)]
    public string? Mode { get; set; }

    [JsonPropertyName(CosmosOptionDefinitions.WorkerCountName)]
    public int? WorkerCount { get; set; }
}
