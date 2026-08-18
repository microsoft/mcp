// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Service-retirement details for <c>ServiceUpgradeAndRetirement</c> recommendation types
/// (from <c>properties.sourceProperties.serviceRetirement</c>). <see cref="TrackingIds"/>
/// are the authoritative Service Health tracking IDs; <see cref="AshUrls"/> are the parallel
/// ready-made Service Health deep links.
public sealed record RecommendationServiceRetirement(
    string? RetirementDate,
    string? RetirementFeatureName,
    IReadOnlyList<string>? TrackingIds,
    IReadOnlyList<string>? AshUrls);
