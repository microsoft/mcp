// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

// Wire model for Azure Resource Graph 'advisorresources' rows of type
// 'microsoft.advisor/metadata' (the global Advisor recommendation-metadata catalog).
internal sealed record RecommendationMetadataData(RecommendationMetadataDataProperties? Properties)
{
    public static RecommendationMetadataData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.RecommendationMetadataData);
}
