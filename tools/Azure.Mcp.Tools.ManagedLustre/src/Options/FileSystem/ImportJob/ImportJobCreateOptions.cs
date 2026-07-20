// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;

public sealed class ImportJobCreateOptions : ISubscriptionOption
{
    /// <summary>
    /// The name of the filesystem.
    /// </summary>
    [Option(Description = ManagedLustreOptionDescriptions.FileSystemName)]
    public required string FilesystemName { get; set; }

    /// <summary>
    /// The name of the import job.
    /// </summary>
    [Option(Description = "The name of the import job. If not specified, a timestamped name will be generated.")]
    public string? JobName { get; set; }

    /// <summary>
    /// How to handle conflicting files during import.
    /// </summary>
    [Option(Description = "How the import job handles conflicts. Fail: stop immediately on conflict. Skip: pass over the conflict. OverwriteIfDirty: delete and re-import if conflicting type, dirty, or currently released. OverwriteAlways: extends OverwriteIfDirty to include releasing restored but not dirty files. Default: Skip. Allowed values: Fail, Skip, OverwriteIfDirty, OverwriteAlways.")]
    public string? ConflictResolutionMode { get; set; }

    /// <summary>
    /// Blob prefixes to import.
    /// </summary>
    [Option(Description = "Array of blob paths/prefixes to import from blob storage. Default: '/'. Maximum: 100 paths. Example: --import-prefixes /data --import-prefixes /logs")]
    public string[]? ImportPrefixes { get; set; }

    /// <summary>
    /// Maximum errors allowed before job failure.
    /// </summary>
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
