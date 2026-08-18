// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Advisor.Commands;
using Microsoft.Mcp.Core.Services.Azure.Models;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary>
/// Resource Graph representation of one Advisor recommendation.
/// </summary>
internal sealed class RecommendationData
{
    /// <summary> The ARM ID of the recommendation. </summary>
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }

    /// <summary> The ARM type of the recommendation. </summary>
    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }

    /// <summary> The name of the recommendation. </summary>
    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }

    /// <summary> The resource group containing the impacted resource. </summary>
    public string? ResourceGroup { get; set; }

    /// <summary> The location associated with the recommendation. </summary>
    public string? Location { get; set; }

    /// <summary> The SKU associated with the recommendation. </summary>
    public ResourceSku? Sku { get; set; }

    /// <summary> Recommendation properties returned by Advisor. </summary>
    public RecommendationProperties? Properties { get; set; }

    /// <summary> Hardware details for the assessed resource. </summary>
    public JsonElement? HardwareDetails { get; set; }

    /// <summary>
    /// Deserializes a Resource Graph recommendation row.
    /// </summary>
    public static RecommendationData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.RecommendationData);
}
