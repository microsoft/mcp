// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public interface ITraceCompareOptions : ISubscriptionOption
{
    string? Query { get; }
    string StartTime { get; }
    string EndTime { get; }
    string? CompareQuery { get; }
    string? CompareStartTime { get; }
    string? CompareEndTime { get; }
    int? MaxResults { get; }
    string Workspace { get; }
    string ResourceGroup { get; }
    string? Tenant { get; }
}
