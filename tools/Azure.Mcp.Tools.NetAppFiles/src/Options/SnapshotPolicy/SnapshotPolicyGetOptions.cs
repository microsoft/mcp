// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;

public sealed class SnapshotPolicyGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account containing the snapshot policy.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the snapshot policy to retrieve. Must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string SnapshotPolicy { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
