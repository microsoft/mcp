// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionUpdateOptions : ISubscriptionOption
{
    [Option(Description = FileSharesOptionDescriptions.FileShareName)]
    public required string FileShareName { get; set; }

    [Option(Description = FileSharesOptionDescriptions.ConnectionName)]
    public required string ConnectionName { get; set; }

    [Option(Description = "The connection status (Approved, Rejected, or Pending)")]
    public required string Status { get; set; }

    [Option(Description = "Description for the connection state change")]
    public string? Description { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
