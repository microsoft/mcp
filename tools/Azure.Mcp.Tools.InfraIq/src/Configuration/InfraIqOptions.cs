// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Configuration;

/// <summary>
/// Operator-only InfraIQ configuration. Values are mapped manually from <c>IConfiguration</c> so no
/// reflection-based binder is required.
/// </summary>
public sealed class InfraIqOptions
{
    /// <summary>
    /// Gets or sets the ARM ingress origin used for Microsoft.AIInfraOptimization calls (<c>InfraIq:ArmIngressOrigin</c>).
    /// When absent or blank, calls go to global ARM and fall back to the Canary endpoint while the action is unavailable
    /// there. An approved value pins calls to that origin with no fallback.
    /// </summary>
    public string? ArmIngressOrigin { get; set; }
}
