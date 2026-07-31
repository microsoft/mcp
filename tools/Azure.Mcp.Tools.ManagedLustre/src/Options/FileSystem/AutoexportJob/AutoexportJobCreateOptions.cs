// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;

public sealed class AutoexportJobCreateOptions : ISubscriptionOption
{
    [Option(Description = ManagedLustreOptionDescriptions.FileSystemName)]
    public required string FilesystemName { get; set; }

    [Option(Description = "The name of the autoexport job. If not specified, a timestamped name will be generated.")]
    public string? JobName { get; set; }

    [Option(Description = "Blob path/prefix that gets auto exported from the cluster namespace. Default: '/'. Note: Only 1 prefix is supported for autoexport jobs. Example: --autoexport-prefix /data")]
    public string? AutoexportPrefix { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.AdminStatus)]
    public string? AdminStatus { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
