// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class CopyJobOptions : BaseCosmosOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.CopyJobName)]
    public string? JobName { get; set; }
}
