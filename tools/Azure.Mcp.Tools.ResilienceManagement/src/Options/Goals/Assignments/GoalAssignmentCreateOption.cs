// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public class GoalAssignmentCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the service group to create the goal assignment in.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the goal assignment.")]
    public required string GoalAssignment { get; set; }

    [Option(Description = "The name of the goal template to assign.")]
    public required string GoalTemplate { get; set; }

    [Option(Description = "The name of the service group that contains the goal template to assign.")]
    public required string GoalTemplateServiceGroup { get; set; }

    [Option(Description = "The goal assignment type. Supported values: Resiliency.")]
    public required GoalAssignmentKind GoalAssignmentType { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
