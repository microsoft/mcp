// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Result containing file share limits and provisioning constants.
/// </summary>
public sealed record FileShareLimitsResult(FileShareLimits Limits, FileShareProvisioningConstants ProvisioningConstants);

/// <summary>
/// File share limits for a subscription and location.
/// </summary>
public class FileShareLimits
{
    public int MaxFileShares { get; set; }
    public int MaxFileShareSnapshots { get; set; }
    public int MaxFileShareSubnets { get; set; }
    public int MaxFileSharePrivateEndpointConnections { get; set; }
    public int MinProvisionedStorageGiB { get; set; }
    public int MaxProvisionedStorageGiB { get; set; }
    public int MinProvisionedIOPerSec { get; set; }
    public int MaxProvisionedIOPerSec { get; set; }
    public int MinProvisionedThroughputMiBPerSec { get; set; }
    public int MaxProvisionedThroughputMiBPerSec { get; set; }
}

/// <summary>
/// Constants used for calculating recommended values of file share provisioning properties.
/// </summary>
public sealed record FileShareProvisioningConstants(
    int BaseIOPerSec,
    double ScalarIOPerSec,
    int BaseThroughputMiBPerSec,
    double ScalarThroughputMiBPerSec);
