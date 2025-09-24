// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ApplicationInsights.Options;

/// <summary>
/// Options for listing Application Insights trace metadata.
/// </summary>
public class AppTraceListOptions : SubscriptionOptions
{
    /// <summary>
    /// Optional start time (UTC) for the trace window.
    /// </summary>
    public DateTime? StartTimeUtc { get; set; }

    /// <summary>
    /// Optional end time (UTC) for the trace window.
    /// </summary>
    public DateTime? EndTimeUtc { get; set; }
}
