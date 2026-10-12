// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Configuration;

/// <summary>
/// Operator-only AI Infra Optimization configuration. Values are mapped manually from <c>IConfiguration</c> so no
/// reflection-based binder is required.
/// </summary>
public sealed class AIInfraOptimizationOptions
{
    /// <summary>
    /// Gets or sets the ARM ingress origin used for Microsoft.AIInfraOptimization calls (<c>AIInfraOptimization:ArmIngressOrigin</c>).
    /// When absent or blank, calls go to global ARM and fall back to the Canary endpoint while the action is unavailable
    /// there. An approved value pins calls to that origin with no fallback.
    /// </summary>
    public string? ArmIngressOrigin { get; set; }
}
