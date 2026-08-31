// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options;

/// <summary>
/// Configures the ADME health checks to perform.
/// </summary>
public sealed class HealthCheckOptions
{
    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = "Verify Microsoft Entra authentication by acquiring an access token for the platform scope. Reports authOk and authError.")]
    public bool IncludeAuth { get; set; }

    [Option(Description = "Verify connectivity by calling the storage info endpoint with an access token. Implies the auth check and is skipped if it fails. Reports connectivityOk, connectivityError, and connectivityStatusCode.")]
    public bool IncludeConnectivity { get; set; }
}
