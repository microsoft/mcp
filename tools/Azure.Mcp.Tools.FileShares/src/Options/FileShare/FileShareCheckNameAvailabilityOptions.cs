// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

/// <summary>
/// Options for FileShareCheckNameAvailabilityCommand.
/// </summary>
public sealed class FileShareCheckNameAvailabilityOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the name of the file share to check availability for.
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.Name)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the location to check name availability in.
    /// </summary>
    [Option(Description = FileSharesOptionDescriptions.Location)]
    public required string Location { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
