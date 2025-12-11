// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.Informational;

public class FileShareGetProvisioningRecommendationOptions : BaseFileSharesOptions
{
    public string? Location { get; set; }

    public string? WorkloadProfile { get; set; }

    public int? EstimatedThroughput { get; set; }

    public int? EstimatedSize { get; set; }
}
