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
    /// Gets or sets the ARM/RPaaS ingress origin used for Private.InfraIQ calls (<c>InfraIq:ArmIngressOrigin</c>).
    /// Defaults to the approved development origin when the configuration value is absent or blank.
    /// </summary>
    public string? ArmIngressOrigin { get; set; }
}
