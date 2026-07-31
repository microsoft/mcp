// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options;

public sealed class WorkspaceLogQueryOptions : ISubscriptionOption
{
    [Option(Description = MonitorOptionDescriptions.Workspace)]
    public required string Workspace { get; set; }

    [Option(Description = MonitorOptionDescriptions.Query)]
    public required string Query { get; set; }

    [Option(Description = MonitorOptionDescriptions.Hours, DefaultValue = 24)]
    public int? Hours { get; set; }

    [Option(Description = MonitorOptionDescriptions.Limit, DefaultValue = 20)]
    public int? Limit { get; set; }

    [Option(Description = MonitorOptionDescriptions.Table)]
    public required string Table { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
