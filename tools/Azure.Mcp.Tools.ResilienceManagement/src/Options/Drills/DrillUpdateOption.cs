// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills;

public sealed class DrillUpdateOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the resilience drill to update.")]
    public required string Drill { get; set; }

    [Option(Description = "The subscription ID or name where the drill's supporting resources will be created. Specify together with region.")]
    public string? Subscription { get; set; }

    [Option(Description = "The Azure region where the drill's supporting resources will be created. Specify together with subscription.")]
    public string? Region { get; set; }

    [Option(Description = "The RBAC setup mode. Supported values: AutomatedCustomRole, AutomatedBuiltinRoles, Manual.")]
    public DrillRbacSetupMode? RbacSetupMode { get; set; }

    [Option(Description = "The recovery plan name in the same service group to associate with the drill.")]
    public string? RecoveryPlan { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}