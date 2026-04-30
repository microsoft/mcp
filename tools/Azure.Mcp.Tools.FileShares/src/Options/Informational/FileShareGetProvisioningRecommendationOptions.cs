// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.Informational;

public class FileShareGetProvisioningRecommendationOptions : SubscriptionOptions
{
    public string? Location { get; set; }
    public int? ProvisionedStorageGiB { get; set; }
}
