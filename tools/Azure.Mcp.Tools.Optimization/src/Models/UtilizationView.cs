// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>Which utilization view(s) the explanation tool returns.</summary>
public enum UtilizationView
{
    /// <summary>7-day window in 30-minute maximum buckets (default).</summary>
    Detail,

    /// <summary>7-day window in 6-hour maximum buckets.</summary>
    Trend,

    /// <summary>Both the 30-minute detail and 6-hour trend views.</summary>
    Both,
}
