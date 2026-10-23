// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Result containing provisioning recommendations for a file share.
/// </summary>
public sealed record FileShareProvisioningRecommendationResult(
    int ProvisionedIOPerSec,
    int ProvisionedThroughputMiBPerSec,
    List<string> AvailableRedundancyOptions);
