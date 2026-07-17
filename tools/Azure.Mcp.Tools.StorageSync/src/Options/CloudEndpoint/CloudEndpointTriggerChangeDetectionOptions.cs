// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Options.CloudEndpoint;

/// <summary>
/// Options for CloudEndpointTriggerChangeDetectionCommand.
/// </summary>
public sealed class CloudEndpointTriggerChangeDetectionOptions : ISubscriptionOption
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
    /// Gets or sets the cloud endpoint name.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.CloudEndpoint.CloudEndpointNameDescription, Aliases = ["ce"])]
    public required string CloudEndpointName { get; set; }

    /// <summary>
    /// Gets or sets the relative path to a directory Azure File share for which change detection is to be performed.
    /// </summary>
    [Option(Description = "Relative path to a directory on the Azure File share for which change detection is to be performed")]
    public required string DirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the change detection mode. Applies to a directory specified in directoryPath parameter.
    /// </summary>
    [Option(Description = "Change detection mode: 'Default' (directory only) or 'Recursive' (directory and subdirectories). Applies to the directory specified in directory-path")]
    public string? ChangeDetectionMode { get; set; }

    /// <summary>
    /// Gets or sets the array of relative paths on the Azure File share to be included in the change detection. Can be files and directories.
    /// </summary>
    [Option(Description = "Array of relative paths on the Azure File share to be included in change detection. Can be files and directories")]
    public string[]? Paths { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
