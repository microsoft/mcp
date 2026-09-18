// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;

public class SnapshotGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account containing the volume.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the capacity pool containing the volume.")]
    public required string Pool { get; set; }

    [Option(Description = "The name of the Azure NetApp Files volume containing the snapshot.")]
    public required string Volume { get; set; }

    [Option(Description = "The name of the snapshot to retrieve. Must be 1-255 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string Snapshot { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
