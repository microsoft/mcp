// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:words slis

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills;

public sealed class DrillCreateOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the resilience drill.")]
    public required string Drill { get; set; }

    [Option(Description = "The subscription ID or name where the drill's supporting resources will be created.")]
    public required string Subscription { get; set; }

    [Option(Description = "The Azure region where the drill's supporting resources will be created.")]
    public required string Region { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = "The drill type. Supported values: Zonal, Regional.")]
    public required DrillKind DrillType { get; set; }

    [Option(Description = "The RBAC setup mode. Supported values: AutomatedCustomRole, AutomatedBuiltinRoles, Manual.")]
    public required DrillRbacSetupMode RbacSetupMode { get; set; }

    [Option(Description = "JSON goal-assignment properties containing identity (type and optional userAssignedIdentity). goalAssignmentId is read-only and must not be supplied.")]
    public string? GoalAssignmentProperties { get; set; }

    [Option(Description = "JSON Health Model monitoring properties containing identity and the full Microsoft.CloudHealth/healthmodels ARM ID as healthModelId.")]
    public string? HealthModelMonitoringProperties { get; set; }

    [Option(Description = "JSON SLI monitoring properties containing identity and slis: one or two objects with sliId (Microsoft.Monitor/slis ARM ID) and type (Availability or Latency). IDs and types must be unique.")]
    public string? SliMonitoringProperties { get; set; }

    [Option(Description = "JSON drill managed identity: {\"type\":\"None|SystemAssigned|UserAssigned|SystemAssigned, UserAssigned\",\"userAssignedIdentities\":[\"<user-assigned identity ARM ID>\"]}. userAssignedIdentities is required exactly when type includes UserAssigned. Defaults to SystemAssigned.")]
    public string? Identity { get; set; }

    [Option(Description = "JSON Chaos resource properties: {\"identity\":{...},\"chaosResourceIdentityForFaults\":{...}}, each with type (None, SystemAssigned, UserAssigned, or SystemAssigned, UserAssigned) and userAssignedIdentity when user-assigned. Defaults to SystemAssigned for both.")]
    public string? ChaosResourceProperties { get; set; }

    [Option(Description = "JSON recovery-plan properties: {\"identity\":{\"type\":\"...\",\"userAssignedIdentity\":\"<ARM ID>\"}} used for recovery plan operations. recoveryPlanId is read-only; the service links the service group's plan. Defaults to SystemAssigned.")]
    public string? RecoveryPlanProperties { get; set; }

    [Option(Description = "JSON monitoring properties: {\"identity\":{\"type\":\"...\",\"userAssignedIdentity\":\"<ARM ID>\"}} used for drill monitoring. Defaults to SystemAssigned.")]
    public string? MonitoringProperties { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
