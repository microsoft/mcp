// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Options.CloudEndpoint;

/// <summary>
/// Options for CloudEndpointCreateCommand.
/// </summary>
public sealed class CloudEndpointCreateOptions : ISubscriptionOption
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
    /// Gets or sets the storage account resource ID.
    /// </summary>
    [Option(Description = "The resource ID of the Azure storage account")]
    public required string StorageAccountResourceId { get; set; }

    /// <summary>
    /// Gets or sets the Azure file share name.
    /// </summary>
    [Option(Description = "The name of the Azure file share")]
    public required string AzureFileShareName { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
