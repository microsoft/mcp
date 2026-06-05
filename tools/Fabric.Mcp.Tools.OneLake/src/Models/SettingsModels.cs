// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>
/// OneLake settings response for a workspace (GET /workspaces/{id}/onelake/settings).
/// </summary>
public class OneLakeSettings
{
    [JsonPropertyName("diagnostics")]
    public DiagnosticsSettings? Diagnostics { get; set; }

    [JsonPropertyName("immutabilityPolicies")]
    public List<ImmutabilityPolicy>? ImmutabilityPolicies { get; set; }

    [JsonPropertyName("lifecycle")]
    public LifecycleSettings? Lifecycle { get; set; }
}

/// <summary>
/// OneLake diagnostic settings object.
/// Used in both the GET response (nested under "diagnostics") and the
/// POST /modifyDiagnostics request body.
/// </summary>
public class DiagnosticsSettings
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("destination")]
    public DiagnosticsDestination? Destination { get; set; }
}

/// <summary>
/// Lakehouse destination for OneLake diagnostic logs.
/// </summary>
public class DiagnosticsDestination
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("lakehouse")]
    public ItemReferenceById? Lakehouse { get; set; }
}

/// <summary>
/// An item reference by ID object.
/// </summary>
public class ItemReferenceById
{
    [JsonPropertyName("referenceType")]
    public string? ReferenceType { get; set; }

    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>
/// Immutability policy object (GET response and POST /modifyImmutabilityPolicy request).
/// </summary>
public class ImmutabilityPolicy
{
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("retentionDays")]
    public int? RetentionDays { get; set; }
}

/// <summary>
/// Lifecycle management settings for a workspace.
/// </summary>
public class LifecycleSettings
{
    [JsonPropertyName("defaultTier")]
    public string? DefaultTier { get; set; }

    [JsonPropertyName("policy")]
    public string? Policy { get; set; }
}
