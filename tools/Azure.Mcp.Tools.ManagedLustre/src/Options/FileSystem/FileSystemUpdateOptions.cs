// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class FileSystemUpdateOptions : ISubscriptionOption
{
    [Option(Description = ManagedLustreOptionDescriptions.Name)]
    public required string Name { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.MaintenanceDay)]
    public string? MaintenanceDay { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.MaintenanceTime)]
    public string? MaintenanceTime { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.RootSquashMode)]
    public string? RootSquashMode { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.NoSquashNidLists)]
    public string? NoSquashNidList { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.SquashUid)]
    public long? SquashUid { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.SquashGid)]
    public long? SquashGid { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
