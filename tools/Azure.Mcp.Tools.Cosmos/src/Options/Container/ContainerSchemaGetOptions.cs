// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Cosmos.Options.Container;

public class ContainerSchemaGetOptions : BaseContainerOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.SampleSizeName)]
    public int? SampleSize { get; set; }
}
