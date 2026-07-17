// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.SubnetSize;

public sealed class SubnetSizeValidateOptions : ISubscriptionOption
{
    [Option(Description = ManagedLustreOptionDescriptions.Sku)]
    public required string Sku { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.Size)]
    public required int Size { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.SubnetId)]
    public required string SubnetId { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.Location)]
    public required string Location { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
