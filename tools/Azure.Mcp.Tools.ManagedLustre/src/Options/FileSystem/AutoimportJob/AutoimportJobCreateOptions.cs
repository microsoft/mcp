// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoimportJob;

public sealed class AutoimportJobCreateOptions : ISubscriptionOption
{
    [Option(Description = ManagedLustreOptionDescriptions.FileSystemName)]
    public required string FilesystemName { get; set; }

    [Option(Description = "The name of the autoimport job. If not specified, a timestamped name will be generated.")]
    public string? JobName { get; set; }

    [Option(Description = "How the autoimport job handles conflicts. Fail: stop immediately on conflict. Skip: pass over the conflict. OverwriteIfDirty: delete and re-import if conflicting type, dirty, or currently released. OverwriteAlways: extends OverwriteIfDirty to include releasing restored but not dirty files. Default: Skip. Allowed values: Fail, Skip, OverwriteIfDirty, OverwriteAlways.")]
    public string? ConflictResolutionMode { get; set; }

    [Option(Description = "Array of blob paths/prefixes that get auto imported to the cluster namespace. Default: '/'. Maximum: 100 paths. Example: --autoimport-prefixes /data --autoimport-prefixes /logs")]
    public string[]? AutoimportPrefixes { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.AdminStatus)]
    public string? AdminStatus { get; set; }

    [Option(Description = "Whether to enable deletions during auto import. This only affects overwrite-dirty mode. Default: false.")]
    public bool? EnableDeletions { get; set; }

    [Option(Description = "Total non-conflict-oriented errors (e.g., OS errors) that import will tolerate before exiting with failure. -1: infinite. 0: exit immediately on any error.")]
    public long? MaximumErrors { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
