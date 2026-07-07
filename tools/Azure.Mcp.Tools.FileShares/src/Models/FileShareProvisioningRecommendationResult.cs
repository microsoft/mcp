// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Result containing provisioning recommendations for a file share.
/// </summary>
public class FileShareProvisioningRecommendationResult
{
    public int ProvisionedIOPerSec { get; set; }
    public int ProvisionedThroughputMiBPerSec { get; set; }
    public List<string> AvailableRedundancyOptions { get; set; } = new();
}
