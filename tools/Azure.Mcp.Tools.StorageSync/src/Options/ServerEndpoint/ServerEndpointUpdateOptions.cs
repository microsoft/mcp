// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Options.ServerEndpoint;

/// <summary>
/// Options for ServerEndpointUpdateCommand.
/// </summary>
public sealed class ServerEndpointUpdateOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.StorageSyncService.NameDescription, Aliases = ["n"])]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the sync group name.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.SyncGroup.SyncGroupNameDescription, Aliases = ["sg"])]
    public required string SyncGroupName { get; set; }

    /// <summary>
    /// Gets or sets the server endpoint name.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.ServerEndpoint.ServerEndpointNameDescription, Aliases = ["se"])]
    public required string ServerEndpointName { get; set; }

    /// <summary>
    /// Gets or sets whether cloud tiering is enabled.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.ServerEndpoint.CloudTieringDescription, Aliases = ["ct"])]
    public bool? CloudTiering { get; set; }

    /// <summary>
    /// Gets or sets the percentage of free space to maintain on the volume (1-99).
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.ServerEndpoint.VolumeFreeSpacePercentDescription)]
    public int? VolumeFreeSpacePercent { get; set; }

    /// <summary>
    /// Gets or sets the number of days after which files should be tiered if not accessed.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.ServerEndpoint.TierFilesOlderThanDaysDescription)]
    public int? TierFilesOlderThanDays { get; set; }

    /// <summary>
    /// Gets or sets the local cache mode (DownloadNewAndModifiedFiles or UpdateLocallyCachedFiles).
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.ServerEndpoint.LocalCacheModeDescription)]
    public string? LocalCacheMode { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
