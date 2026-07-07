// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Policy.Models;

public class ManagedIdentityInfo
{
    public string? Type { get; set; }
    public string? PrincipalId { get; set; }
    public string? TenantId { get; set; }
    public Dictionary<string, UserAssignedIdentityDetails>? UserAssignedIdentities { get; set; }
}

public class UserAssignedIdentityDetails
{
    public string? PrincipalId { get; set; }
    public string? ClientId { get; set; }
}
