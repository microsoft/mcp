// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareUpdateCommand.
/// </summary>
public sealed class FileShareUpdateOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the name of the file share to update.
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.Name)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the provisioned storage size in GiB.
    /// </summary>
    [Option(Name = "provisioned-storage-in-gib", Description = FileSharesOptionDescriptions.ProvisionedStorageGiB)]
    public int? ProvisionedStorageInGiB { get; set; }

    /// <summary>
    /// Gets or sets the provisioned IOPS.
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.ProvisionedIOPerSec)]
    public int? ProvisionedIoPerSec { get; set; }

    /// <summary>
    /// Gets or sets the provisioned throughput in MiB/sec.
    /// </summary>
    [Option(Name = "provisioned-throughput-mib-per-sec", Description = FileSharesOptionDescriptions.ProvisionedThroughputMiBPerSec)]
    public int? ProvisionedThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the public network access setting (e.g., "Enabled", "Disabled").
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.PublicNetworkAccess)]
    public string? PublicNetworkAccess { get; set; }

    /// <summary>
    /// Gets or sets the NFS root squash setting (e.g., "NoRootSquash", "RootSquash", "AllSquash").
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.NfsRootSquash)]
    public string? NfsRootSquash { get; set; }

    /// <summary>
    /// Gets or sets the NFS encryption in transit setting (e.g., "Enabled", "Disabled").
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.NfsEncryptionInTransit)]
    public string? NfsEncryptionInTransit { get; set; }

    /// <summary>
    /// Gets or sets the allowed subnets for public access (comma-separated list).
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.AllowedSubnets)]
    public string? AllowedSubnets { get; set; }

    /// <summary>
    /// Gets or sets the tags for the file share (JSON format).
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
