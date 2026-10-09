// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record ServiceGroupStatusInsightsPage(
    List<ServiceGroupStatusInsight> ServiceGroups,
    bool MoreAvailable,
    string? ContinuationToken);
