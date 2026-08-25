// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;

public sealed class RecoveryPlanCreateOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the recovery plan to create or fully update.")]
    public required string RecoveryPlan { get; set; }

    [Option(Description = "The recovery plan type. Supported value: Zonal. The type cannot be changed after creation.")]
    public required RecoveryPlanKind PlanType { get; set; }

    [Option(Description = "The recovery plan description, from 5 to 50 characters. Required when creating a plan; on update, the existing description is preserved when omitted.")]
    public string? PlanDescription { get; set; }

    [Option(Description = "The customer-selected managed identity type for the recovery plan. Supported values: SystemAssigned, UserAssigned, and SystemAndUserAssigned. Do not assume a default; ask the customer when they have not specified an identity type. Specify this on every create or update; updates can switch identity types, but cannot replace an existing user-assigned identity with a different user-assigned identity.")]
    public required RecoveryPlanIdentityKind IdentityType { get; set; }

    [Option(Description = "The full resource ID of the user-assigned managed identity. Required when --identity-type is UserAssigned or SystemAndUserAssigned and not allowed when it is SystemAssigned. On update, specify the existing user-assigned identity because changing it to a different user-assigned identity is not supported.")]
    public string? UserAssignedIdentity { get; set; }

    [Option(Description = "The default recovery group description, from 5 to 50 characters. On update, the existing description is preserved when omitted.")]
    public string? DefaultGroupDescription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer<RetryPolicyOptions>(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
