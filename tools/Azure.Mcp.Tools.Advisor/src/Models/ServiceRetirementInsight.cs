// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record ServiceRetirementInsight(
    string InsightResourceName,
    string InsightResourceId,
    DateTimeOffset LastUpdatedTime,
    long Day1ImpactedResources,
    long CurrentImpactedResources,
    string InsightName,
    bool IsDeleted,
    string Domain);
