// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Service-retirement data returned in Advisor recommendation metadata. </summary>
internal sealed record RecommendationMetadataServiceRetirementData(
    string? RetirementDate,
    string? RetirementFeatureName,
    RecommendationMetadataServiceHealthData? ServiceHealth);
