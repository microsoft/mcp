// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Entities;

public record ProviderResourceTypes
{
    public ProviderResourceTypes(string provider)
    {
        Provider = provider;
        ResourceTypes = [];
    }

    [JsonPropertyName("provider")]
    public string Provider { get; init; }


    [JsonPropertyName("resourceTypes")]
    public Dictionary<string, UniqueResourceType> ResourceTypes { get; init; }
}
