// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Authorization.Options;

public static class AuthorizationOptionDefinitions
{
    public const string ApprovalName = "approval";
    public const string StageName = "stage";
    public const string JustificationName = "justification";

    public static readonly Option<string> Approval = new($"--{ApprovalName}")
    {
        Description = "The Azure RBAC PIM role assignment approval name or full approval resource ID. Use role approval list to discover pending approvals.",
        Required = true
    };

    public static readonly Option<string> Stage = new($"--{StageName}")
    {
        Description = "The Azure RBAC PIM approval stage name or full stage resource ID. Use role approval list to discover pending approval stages.",
        Required = true
    };

    public static readonly Option<string> Justification = new($"--{JustificationName}")
    {
        Description = "The business justification to record when approving the Azure RBAC PIM request.",
        Required = true
    };
}
