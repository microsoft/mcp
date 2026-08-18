// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Source-specific properties for an Advisor recommendation metadata row. </summary>
internal sealed record RecommendationMetadataSourceProperties(
    RecommendationMetadataServiceRetirementData? ServiceRetirement);
