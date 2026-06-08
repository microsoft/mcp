// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Catalog entry returned from the Advisor metadata API. The first two fields are always
/// populated; the remaining fields are only populated when the entry comes from the
/// 'recommendationType' dimension (the ARM metadata API only carries category/impact/
/// sub-category/resource type on individual recommendation type entries).
/// </summary>
public record RecommendationType(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("category"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Category = null,
    [property: JsonPropertyName("impact"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Impact = null,
    [property: JsonPropertyName("subCategory"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? SubCategory = null,
    [property: JsonPropertyName("resourceType"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ResourceType = null
);
