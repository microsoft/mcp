// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class ResourceIdentity
{
    /// <summary> Identity type (e.g., SystemAssigned, UserAssigned). </summary>
    public string? Type { get; set; }

    /// <summary> Principal ID of the identity. </summary>
    public string? PrincipalId { get; set; }

    /// <summary> Tenant ID associated with the identity. </summary>
    public string? TenantId { get; set; }
}

public sealed class ManagedIdentityReference
{
    /// <summary> Resource ID of the user-assigned identity. </summary>
    public string? ResourceId { get; set; }

    /// <summary> Client ID of the identity. </summary>
    public string? ClientId { get; set; }

    /// <summary> Object ID of the identity. </summary>
    public string? ObjectId { get; set; }
}

