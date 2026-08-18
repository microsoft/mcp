// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Service Health data associated with an Advisor service retirement. </summary>
internal sealed record RecommendationMetadataServiceHealthData(
    List<string>? TrackingIds,
    List<string>? AshUrls);
