// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Resource Graph representation of one Advisor recommendation metadata row. </summary>

internal sealed record RecommendationMetadataData(RecommendationMetadataDataProperties? Properties)
{
    /// <summary>
    /// Deserializes a Resource Graph metadata row.
    /// </summary>
    public static RecommendationMetadataData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.RecommendationMetadataData);
}
