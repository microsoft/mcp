// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.StorageSync.Options.CloudEndpoint;

/// <summary>
/// Options for CloudEndpointUpdateCommand.
/// </summary>
public sealed class CloudEndpointUpdateOptions : ISubscriptionOption
{
    [Option(Description = StorageSyncOptionDescriptions.StorageSyncService.NameDescription, Aliases = ["n"])]
    public required string Name { get; set; }

    [Option(Description = StorageSyncOptionDescriptions.SyncGroup.SyncGroupNameDescription, Aliases = ["sg"])]
    public required string SyncGroupName { get; set; }

    [Option(Description = StorageSyncOptionDescriptions.CloudEndpoint.CloudEndpointNameDescription, Aliases = ["ce"])]
    public required string CloudEndpointName { get; set; }

    [Option(Description = StorageSyncOptionDescriptions.CloudEndpoint.ChangeEnumerationIntervalDaysDescription)]
    public int? ChangeEnumerationIntervalDays { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
