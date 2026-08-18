// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Service-retirement details associated with an Advisor recommendation type.
/// </summary>
public sealed record RecommendationServiceRetirement(
    string? RetirementDate,
    string? RetirementFeatureName,
    IReadOnlyList<string>? TrackingIds,
    IReadOnlyList<string>? AshUrls);
