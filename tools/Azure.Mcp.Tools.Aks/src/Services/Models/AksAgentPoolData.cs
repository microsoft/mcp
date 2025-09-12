// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands;

namespace Azure.Mcp.Tools.Aks.Services.Models;

/// <summary>
/// A class representing the AksAgentPoolData data model.
/// </summary>
internal sealed class AksAgentPoolData
{
    /// <summary> The resource ID for the resource. </summary>
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }
    /// <summary> The type of the resource. </summary>
    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }
    /// <summary> The name of the resource. </summary>
    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }
    /// <summary> The properties of the agent pool. </summary>
    public AksAgentPoolProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static AksAgentPoolData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize<AksAgentPoolData>(source, AksJsonContext.Default.AksAgentPoolData);
    }
}
