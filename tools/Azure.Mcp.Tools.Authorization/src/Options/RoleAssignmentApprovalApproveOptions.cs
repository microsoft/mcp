// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Authorization.Options;

public sealed class RoleAssignmentApprovalApproveOptions : ISubscriptionOption
{
    [Option(Description = "Scope where the Azure RBAC PIM role assignment approval request exists.")]
    public required string Scope { get; set; }

    [Option(Description = "The Azure RBAC PIM role assignment approval name or full approval resource ID. Use `role approval list` to discover pending approvals.")]
    public required string Approval { get; set; }

    [Option(Description = "The Azure RBAC PIM approval stage name or full stage resource ID. Use `role approval list` to discover pending approval stages.")]
    public required string Stage { get; set; }

    [Option(Description = "The business justification to record when approving the Azure RBAC PIM request.")]
    public required string Justification { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
