// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>
/// Managed identity configuration of an Azure Monitor Health Model.
/// </summary>
public sealed class HealthModelIdentity
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("principalId")]
    public string? PrincipalId { get; set; }

    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonPropertyName("userAssignedIdentities")]
    public IReadOnlyList<string>? UserAssignedIdentities { get; set; }
}
