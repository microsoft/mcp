// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class BaseContainerOptions : BaseDatabaseOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.ContainerName)]
    public string? Container { get; set; }
}
