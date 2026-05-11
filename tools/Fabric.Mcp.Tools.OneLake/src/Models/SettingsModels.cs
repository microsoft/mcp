// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>
/// OneLake settings for a workspace.
/// </summary>
public class OneLakeSettings
{
    [JsonPropertyName("diagnostics")]
    public DiagnosticsSettings? Diagnostics { get; set; }

    [JsonPropertyName("immutabilityPolicy")]
    public ImmutabilityPolicySettings? ImmutabilityPolicy { get; set; }
}

/// <summary>
/// Diagnostic logging configuration for OneLake.
/// </summary>
public class DiagnosticsSettings
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("logAnalyticsWorkspaceId")]
    public string? LogAnalyticsWorkspaceId { get; set; }

    [JsonPropertyName("categories")]
    public List<DiagnosticsCategory>? Categories { get; set; }
}

/// <summary>
/// A diagnostic logging category.
/// </summary>
public class DiagnosticsCategory
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}

/// <summary>
/// Immutability policy settings for OneLake.
/// </summary>
public class ImmutabilityPolicySettings
{
    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("immutabilityPeriodSinceCreationInDays")]
    public int? ImmutabilityPeriodSinceCreationInDays { get; set; }
}

/// <summary>
/// Request body for modifying diagnostics.
/// </summary>
public class DiagnosticsModifyRequest
{
    [JsonPropertyName("diagnostics")]
    public DiagnosticsSettings? Diagnostics { get; set; }
}

/// <summary>
/// Request body for modifying immutability policy.
/// </summary>
public class ImmutabilityPolicyModifyRequest
{
    [JsonPropertyName("immutabilityPolicy")]
    public ImmutabilityPolicySettings? ImmutabilityPolicy { get; set; }
}
