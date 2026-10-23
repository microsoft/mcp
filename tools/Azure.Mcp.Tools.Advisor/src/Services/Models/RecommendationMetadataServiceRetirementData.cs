// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record RecommendationMetadataServiceRetirementData(
    string? RetirementDate,
    string? RetirementFeatureName,
    RecommendationMetadataServiceHealthData? ServiceHealth);
