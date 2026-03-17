// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Services.Azure.Models;

/// <summary> User assigned identity properties. </summary>
public sealed class UserAssignedIdentity
{
    /// <summary> The principal ID of the assigned identity. </summary>
    public Guid? PrincipalId { get; }
    /// <summary> The client ID of the assigned identity. </summary>
    public Guid? ClientId { get; }
}

/// <summary>
/// A class representing the Managed Service Identity model.
/// </summary>
public sealed class ManagedServiceIdentity
{
    /// <summary> The service principal ID of the system assigned identity. This property will only be provided for a system assigned identity. </summary>
    public Guid? PrincipalId { get; set; }
    /// <summary> The tenant ID of the system assigned identity. This property will only be provided for a system assigned identity. </summary>
    public Guid? TenantId { get; set; }
    /// <summary> Type of managed service identity (where both SystemAssigned and UserAssigned types are allowed). </summary>
    [JsonPropertyName("type")]
    public string? ManagedServiceIdentityType { get; set; }
    /// <summary> The set of user assigned identities associated with the resource. The userAssignedIdentities dictionary keys will be ARM resource ids in the form: &apos;/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{identityName}. The dictionary values can be empty objects ({}) in requests. </summary>
    public IDictionary<string, UserAssignedIdentity>? UserAssignedIdentities { get; set; }
}
