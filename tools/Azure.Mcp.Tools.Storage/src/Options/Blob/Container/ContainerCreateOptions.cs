// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Blob.Container;

public class ContainerCreateOptions : ISubscriptionOption
{
    [Option("The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').")]
    public required string Account { get; set; }

    [Option("The name of the container to access within the storage account.")]
    public required string Container { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    // TODO: Remove unused option — registered and visible to the user but never consumed by the command
    [Option(OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
