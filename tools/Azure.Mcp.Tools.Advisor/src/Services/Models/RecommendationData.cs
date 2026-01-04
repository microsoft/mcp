// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.Advisor.Commands;

namespace Azure.Mcp.Tools.Advisor.Services.Models
{
    /// <summary>
    /// A class representing the AdvisorRecommendation data model.
    /// </summary>
    internal sealed class RecommendationData
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

        /// <summary> The location of the resource. </summary>
        public string? Location { get; set; }

        // <summary> The SKU of the resource. </summary>
        public ResourceSku? Sku { get; set; }

        /// <summary> Properties of the Advisor Recommendation. </summary>
        public RecommendationProperties? Properties { get; set; }

        // Read the JSON response content and create a model instance from it.
        public static RecommendationData? FromJson(JsonElement source)
        {
            return JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.RecommendationData);
        }
    }
}
