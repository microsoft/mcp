// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public enum DrillMonitoringSourceKind
{
    HealthModel,
    Sli,
}

public enum DrillMonitoringIdentityKind
{
    SystemAssigned,
    UserAssigned,
}

public enum DrillSliKind
{
    Availability,
    Latency,
}

public sealed record DrillMonitoringSource(
    DrillMonitoringSourceKind Kind,
    string ResourceId,
    DrillMonitoringIdentityKind IdentityKind,
    string? UserAssignedIdentityId = null,
    DrillSliKind? SliKind = null);
