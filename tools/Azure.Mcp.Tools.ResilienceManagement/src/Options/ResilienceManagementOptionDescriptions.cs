// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Options;

internal static class ResilienceManagementOptionDescriptions
{
    internal const string ServiceGroup = "The name of the service group.";
    internal const string GoalAssignment = "The name of the resilience goal assignment (3-24 ASCII letters, digits, or hyphens).";
    internal const string RecoveryPlan = "The name of the recovery plan.";
    internal const string Drill = "The name of the resilience drill.";
    internal const string OperationId = "An optional caller-owned operation ID in canonical GUID format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx). Reuse the same ID when safely retrying or resuming the same request. A new GUID is generated when omitted.";
}
