// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceQueryCancelOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = "Trace account name or full Azure Monitor Workspace resource id.")]
    public string? Account { get; set; }

    [Option(Description = "Operation id returned by 'monitor traces query run'.")]
    public string? OperationId { get; set; }
}
