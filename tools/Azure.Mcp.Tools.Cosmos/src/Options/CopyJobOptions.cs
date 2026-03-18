// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class CopyJobOptions : BaseCosmosOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.JobNameConst)]
    public string? JobName { get; set; }
}
