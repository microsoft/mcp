// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public class RecoveryPlanCreateOptions
{
    [Option(Description = "The name of the Azure service group that owns the recovery plan.")]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to create or fully update.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The recovery plan type. Supported value: Zonal. The type cannot be changed after creation.")]
    public required RecoveryPlanKind PlanType { get; set; }

    [Option(Description = "The recovery plan description, up to 50 characters.")]
    public required string PlanDescription { get; set; }

    [Option(Description = "The full resource ID of a pre-provisioned user-assigned managed identity. Omit when creating a plan to use a system-assigned identity. On update, omit to preserve the existing identity or specify the same user-assigned identity. Ensure the identity has the Azure RBAC roles required by the recovery resources because Recovery Orchestration role assignment is best effort.")]
    public string? UserAssignedIdentity { get; set; }

    [Option(Description = "The default recovery group description. On update, the existing description is preserved when omitted.")]
    public string? DefaultGroupDescription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}