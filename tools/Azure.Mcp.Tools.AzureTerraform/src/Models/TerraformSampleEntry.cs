// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

internal sealed class TerraformSampleEntry
{
    [JsonPropertyName("ResourceType")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;
}
