// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Disk;

/// <summary>
/// Options for the DiskDelete command.
/// </summary>
public sealed class DiskDeleteOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the name of the disk.
    /// </summary>
    [Option(Description = ComputeOptionDescriptions.DiskName, Aliases = ["name"])]
    public required string DiskName { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
