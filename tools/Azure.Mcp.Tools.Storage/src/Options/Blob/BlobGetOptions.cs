// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BlobGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').")]
    public string? Blob { get; set; }

    [Option(Description = "The prefix to filter blobs when listing blobs in a container. Only blobs whose names start with the specified prefix will be listed.")]
    public string? Prefix { get; set; }

    [Option(Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container to access within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
