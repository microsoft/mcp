// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    [Option(Name = "recoveryplan", Description = "The optional recoveryplan name in the same service group to associate with the drill.")]
    public string? RecoveryPlan { get; set; }

    [Option(Description = "The approved monitoring source type. Supported values: HealthModel, Sli.")]
    public DrillMonitoringSourceKind? MonitoringSourceType { get; set; }

    [Option(Description = "The exact ARM ID of the approved Microsoft.CloudHealth/healthModels or service-group-scoped Microsoft.Monitor/slis resource.")]
    public string? MonitoringSourceId { get; set; }

    [Option(Description = "The identity used by the drill to read the monitoring source. Supported values: SystemAssigned, UserAssigned.")]
    public DrillMonitoringIdentityKind? MonitoringIdentityType { get; set; }

    [Option(Description = "The exact user-assigned managed identity ARM ID. Required only when monitoring-identity-type is UserAssigned.")]
    public string? MonitoringUserAssignedIdentityId { get; set; }

    [Option(Description = "The selected SLI category. Supported values: Availability, Latency. Required only for an Sli monitoring source.")]
    public DrillSliKind? MonitoringSliType { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
