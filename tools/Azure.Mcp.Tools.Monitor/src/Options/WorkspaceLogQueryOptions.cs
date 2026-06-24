// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options;

public sealed class WorkspaceLogQueryOptions : ISubscriptionOption
{
    [Option(Description = MonitorOptionDefinitions.WorkspaceDescription)]
    public required string Workspace { get; set; }

    [Option(Description = MonitorOptionDefinitions.QueryDescription)]
    public required string Query { get; set; }

    [Option(Description = MonitorOptionDefinitions.HoursDescription, DefaultValue = 24)]
    public int? Hours { get; set; }

    [Option(Description = MonitorOptionDefinitions.LimitDescription, DefaultValue = 20)]
    public int? Limit { get; set; }

    [Option(Description = MonitorOptionDefinitions.TableDescription)]
    public required string Table { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
