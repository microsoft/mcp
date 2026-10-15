// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Current Azure Advisor status of a Service Group.
/// </summary>
/// <param name="ServiceGroupName">Service Group name, for example 'commerce'.</param>
/// <param name="ServiceGroupId">Full ARM ID of the Service Group.</param>
/// <param name="Criticality">Business criticality tier label: MissionCritical (Tier 0), BusinessCritical (Tier 1), BusinessOperational (Tier 2), Administrative (Tier 3) or NotApplicable.</param>
/// <param name="Status">Status: On Track, Attention Needed, At Risk, Critical or Unavailable.</param>
/// <param name="StatusDescription">Short explanation of the status.</param>
/// <param name="Description">Where to focus to improve the status, when available.</param>
/// <param name="Goal">Business goal configured for the Service Group, when set.</param>
/// <param name="LastUpdatedTime">When Advisor last refreshed the status.</param>
public sealed record AdvisorServiceGroupStatus(
    string ServiceGroupName,
    string ServiceGroupId,
    string? Criticality,
    string? Status,
    string? StatusDescription,
    string? Description,
    string? Goal,
    DateTimeOffset? LastUpdatedTime);
