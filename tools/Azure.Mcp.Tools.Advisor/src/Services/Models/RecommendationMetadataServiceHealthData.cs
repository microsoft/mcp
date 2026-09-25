// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record RecommendationMetadataServiceHealthData(
    List<string>? TrackingIds,
    List<string>? AshUrls);
