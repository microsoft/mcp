// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Kusto.Services.Models;

/// <summary>
/// A class representing the Kusto Managed Service Identity model.
/// </summary>
internal sealed class KustoManagedServiceIdentity
{
    /// <summary> The service principal ID of the system assigned identity. This property will only be provided for a system assigned identity. </summary>
    public Guid? PrincipalId { get; set; }
    /// <summary> The tenant ID of the system assigned identity. This property will only be provided for a system assigned identity. </summary>
    public Guid? TenantId { get; set; }
    /// <summary> Type of managed service identity (where both SystemAssigned and UserAssigned types are allowed). </summary>
    [JsonPropertyName("type")]
    public string? ManagedServiceIdentityType { get; set; }
}
