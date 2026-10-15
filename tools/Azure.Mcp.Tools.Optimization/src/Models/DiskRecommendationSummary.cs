// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>
/// Summary of unattached-disk cost recommendations. These are reported as a single summary line
/// with a link to take action in the Azure portal rather than as individual recommendation rows.
/// </summary>
public sealed record DiskRecommendationSummary(
    string Message,
    int UnattachedDiskCount,
    IReadOnlyList<string> SubscriptionIds,
    string ActionUrl);
