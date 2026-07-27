// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Options.StorageSyncService;

/// <summary>
/// Options for StorageSyncServiceCreateCommand.
/// </summary>
public sealed class StorageSyncServiceCreateOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the storage sync service name.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.StorageSyncService.NameDescription, Aliases = ["n"])]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the location for the service.
    /// </summary>
    [Option(Description = "The Azure region/location name (e.g., EastUS, WestEurope)", Aliases = ["l"])]
    public required string Location { get; set; }

    /// <summary>
    /// Gets or sets tags for the resource.
    /// </summary>
    [Option(Description = StorageSyncOptionDescriptions.StorageSyncService.TagsDescription)]
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
