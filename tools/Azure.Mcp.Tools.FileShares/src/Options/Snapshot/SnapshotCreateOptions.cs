// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.Snapshot;

public sealed class SnapshotCreateOptions : ISubscriptionOption
{
    [Option(Description = FileSharesOptionDescriptions.SnapshotFileShareName)]
    public required string FileShareName { get; set; }

    [Option(Description = FileSharesOptionDescriptions.SnapshotName)]
    public required string SnapshotName { get; set; }

    [Option(Description = FileSharesOptionDescriptions.Metadata)]
    public string? Metadata { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
