// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareDeleteCommand.
/// </summary>
public sealed class FileShareDeleteOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the name of the file share to delete.
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.Name)]
    public required string Name { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
