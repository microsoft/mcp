// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Options.StorageSyncService;

/// <summary>
/// Options for StorageSyncServiceUpdateCommand.
/// </summary>
public sealed class StorageSyncServiceUpdateOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.StorageSyncService.NameDescription, Aliases = ["n"])]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the incoming traffic policy.
    /// </summary>
    [Option(Description = "Incoming traffic policy for the service (AllowAllTraffic or AllowVirtualNetworksOnly)")]
    public string? IncomingTrafficPolicy { get; set; }

    /// <summary>
    /// Gets or sets tags for the resource.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.StorageSyncService.TagsDescription)]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the managed service identity type.
    /// </summary>
    [Option(Description = "Managed service identity type (None, SystemAssigned, UserAssigned, SystemAssigned,UserAssigned)")]
    public string? IdentityType { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
