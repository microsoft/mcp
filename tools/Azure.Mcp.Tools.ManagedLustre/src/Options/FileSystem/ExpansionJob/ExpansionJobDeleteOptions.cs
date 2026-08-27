// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ExpansionJob;

public sealed class ExpansionJobDeleteOptions : ISubscriptionOption
{
    [Option(Description = ManagedLustreOptionDescriptions.FileSystemName)]
    public required string FilesystemName { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.ExpansionJobName)]
    public required string ExpansionJobName { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
