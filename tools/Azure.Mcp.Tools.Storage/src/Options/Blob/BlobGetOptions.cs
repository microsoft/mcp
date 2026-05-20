// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BlobGetOptions : ISubscriptionOption
{
    [Option("The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').")]
    public string? Blob { get; set; }

    [Option("The prefix to filter blobs when listing blobs in a container. Only blobs whose names start with the specified prefix will be listed.")]
    public string? Prefix { get; set; }

    [Option("The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').")]
    public required string Account { get; set; }

    [Option("The name of the container to access within the storage account.")]
    public required string Container { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option("Comma-separated list of specific fields to include in the output. If not specified, all fields will be included.")]
    public string? SelectedFields { get; set; }

    // TODO: Remove unused option — registered and visible to the user but never consumed by the command
    [Option(OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
