// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:words slis

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

    [Option(Description = "The drill's existing supporting-resource group. Specify only together with subscription and region; the service rejects a different resource group after creation.")]
    public string? ResourceGroup { get; set; }

    [Option(Description = "The RBAC setup mode. Supported values: AutomatedCustomRole, AutomatedBuiltinRoles, Manual.")]
    public DrillRbacSetupMode? RbacSetupMode { get; set; }

    [Option(Description = "JSON goal-assignment properties to update, containing identity (type and optional userAssignedIdentity). goalAssignmentId is read-only. Omit to preserve the current configuration.")]
    public string? GoalAssignmentProperties { get; set; }

    [Option(Description = "JSON Health Model monitoring properties to update (identity and/or healthModelId). Pass the string 'null' as this JSON payload to clear monitoring; omit to preserve it.")]
    public string? HealthModelMonitoringProperties { get; set; }

    [Option(Description = "JSON SLI monitoring properties to update (identity and/or slis). A supplied slis array replaces the selection and must contain one or two unique sliId/type entries (Availability or Latency). Pass the string 'null' as this JSON payload to clear; omit to preserve.")]
    public string? SliMonitoringProperties { get; set; }

    [Option(Description = "JSON drill managed identity to set: {\"type\":\"None|SystemAssigned|UserAssigned|SystemAssigned, UserAssigned\",\"userAssignedIdentities\":[\"<user-assigned identity ARM ID>\"]}. Omit to preserve.")]
    public string? Identity { get; set; }

    [Option(Description = "JSON Chaos resource properties to update: identity and/or chaosResourceIdentityForFaults, each with type and userAssignedIdentity when user-assigned. Omit to preserve.")]
    public string? ChaosResourceProperties { get; set; }

    [Option(Description = "JSON recovery-plan properties to update: {\"identity\":{\"type\":\"...\",\"userAssignedIdentity\":\"<ARM ID>\"}}. recoveryPlanId is read-only. Omit to preserve.")]
    public string? RecoveryPlanProperties { get; set; }

    [Option(Description = "JSON monitoring properties to update: {\"identity\":{\"type\":\"...\",\"userAssignedIdentity\":\"<ARM ID>\"}}. Omit to preserve.")]
    public string? MonitoringProperties { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
