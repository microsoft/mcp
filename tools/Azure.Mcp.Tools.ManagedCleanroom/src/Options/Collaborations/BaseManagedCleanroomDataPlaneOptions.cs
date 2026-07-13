// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Options.Collaborations;

/// <summary>
/// Base options for all data-plane Managed Cleanroom commands.
/// Contains shared authentication, transport, and retry configuration.
/// </summary>
public class BaseManagedCleanroomDataPlaneOptions
{
    [Option(Description = ManagedCleanroomOptionDescriptions.Endpoint)]
    public required string Endpoint { get; set; }

    [Option(Name = "allow-untrusted-cert", Description = ManagedCleanroomOptionDescriptions.AllowUntrustedCert)]
    public bool AllowUntrustedCert { get; set; }

    [Option(Description = ManagedCleanroomOptionDescriptions.TokenScope)]
    public string? TokenScope { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
