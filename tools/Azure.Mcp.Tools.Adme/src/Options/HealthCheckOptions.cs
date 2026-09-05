// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options;

/// <summary>
/// Configures the ADME health check target.
/// </summary>
public sealed class HealthCheckOptions
{
    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
