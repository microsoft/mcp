// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>GET /workspaces/{id}/onelake/settings response (swagger: GetOneLakeSettingsResponse).</summary>
public class OneLakeSettings
{
    [JsonPropertyName("diagnostics")]
    public OneLakeDiagnosticSettings? Diagnostics { get; set; }

    /// <summary>Swagger field is plural "immutabilityPolicies" and is an array.</summary>
    [JsonPropertyName("immutabilityPolicies")]
    public List<ImmutabilityPolicy>? ImmutabilityPolicies { get; set; }

    [JsonPropertyName("lifecycle")]
    public LifecycleSettings? Lifecycle { get; set; }
}

/// <summary>
/// Body of POST /onelake/settings/modifyDiagnostics AND the diagnostics block in GET.
/// Swagger: OneLakeDiagnosticSettings.
/// </summary>
public class OneLakeDiagnosticSettings
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Required when Status == Enabled; omitted when Disabled.</summary>
    [JsonPropertyName("destination")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public LakehouseDiagnosticDestination? Destination { get; set; }
}

/// <summary>
/// Swagger: LakehouseOneLakeDiagnosticSettingsDestination (discriminator type="Lakehouse").
/// Single-variant today — promote to polymorphic when Fabric adds another destination type.
/// </summary>
public sealed class LakehouseDiagnosticDestination
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Lakehouse";

    [JsonPropertyName("lakehouse")]
    public ItemReferenceById? Lakehouse { get; set; }
}

/// <summary>Swagger: ItemReferenceById (discriminator referenceType="ById").</summary>
public sealed class ItemReferenceById
{
    [JsonPropertyName("referenceType")]
    public string ReferenceType { get; set; } = "ById";

    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>
/// Body of POST /onelake/settings/modifyImmutabilityPolicy AND items in GET response.
/// Swagger: ImmutabilityPolicyRequest / ImmutabilityPolicy.
/// </summary>
public class ImmutabilityPolicy
{
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("retentionDays")]
    public int? RetentionDays { get; set; }
}

/// <summary>Lifecycle management settings for a workspace.</summary>
public class LifecycleSettings
{
    [JsonPropertyName("defaultTier")]
    public string? DefaultTier { get; set; }

    [JsonPropertyName("policy")]
    public string? Policy { get; set; }
}
