// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Templates;

public class GoalTemplateCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the service group.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the goal template.")]
    public required string GoalTemplate { get; set; }

    [Option(Description = "The goal type for the goal template. Supported values: Resiliency.")]
    public required GoalTemplateKind GoalType { get; set; }

    [Option(Description = "Whether high availability is required for the goal template. Supported values: Required, NotRequired.")]
    public required GoalRequirement RequireHighAvailability { get; set; }

    [Option(Description = "Whether disaster recovery is required for the goal template. Supported values: Required, NotRequired.")]
    public required GoalRequirement RequireDisasterRecovery { get; set; }

    [Option(Description = "The regional recovery point objective as an ISO 8601 duration (e.g. PT15M).")]
    public required string RegionalRecoveryPointObjective { get; set; }

    [Option(Description = "The regional recovery time objective as an ISO 8601 duration (e.g. PT30M).")]
    public required string RegionalRecoveryTimeObjective { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
