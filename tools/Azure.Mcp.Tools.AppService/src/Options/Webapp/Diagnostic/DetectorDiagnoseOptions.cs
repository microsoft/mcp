// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;

public sealed class DetectorDiagnoseOptions : ISubscriptionOption
{
    [Option(Description = AppServiceOptionDefinitions.App)]
    public required string App { get; set; }

    [Option(Description = "The ID of the diagnostic detector to run. Use the 'id' field from 'azmcp appservice webapp diagnostic list' output (e.g., LinuxContainerRecycle, LinuxMemoryDrillDown).")]
    public required string DetectorId { get; set; }

    [Option(Description = "The start time in ISO format (e.g., 2023-01-01T00:00:00Z).")]
    public DateTimeOffset? StartTime { get; set; }

    [Option(Description = "The end time in ISO format (e.g., 2023-01-01T00:00:00Z).")]
    public DateTimeOffset? EndTime { get; set; }

    [Option(Description = "The time interval (e.g., PT1H for 1 hour, PT5M for 5 minutes).")]
    public string? Interval { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
