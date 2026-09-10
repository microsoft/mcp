// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Metadata identifying the resource affected by an Azure Advisor recommendation.</summary>
public sealed record RecommendationResourceMetadata(
    [property: JsonPropertyName("resourceId")] string? ResourceId);
