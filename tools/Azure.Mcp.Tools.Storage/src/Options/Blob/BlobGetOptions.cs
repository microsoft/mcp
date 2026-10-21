// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Blob;

public class BlobGetOptions
{
    [Option(Description = "The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').")]
    public string? Blob { get; set; }

    [Option(Description = "The prefix to filter blobs when listing blobs in a container. Only blobs whose names start with the specified prefix will be listed.")]
    public string? Prefix { get; set; }

    [Option(Description = "The delimiter used to group blobs into virtual directories, such as '/'. Returns blobs and directory prefixes at one level. Enables paged listing; cannot be used with --blob.")]
    public string? Delimiter { get; set; }

    [Option(Description = "The maximum number of blobs and directory prefixes to return in one page, from 1 to 5000. Defaults to 100 when --delimiter or --continuation-token is specified. Cannot be used with --blob.")]
    public int? MaxResults { get; set; }

    [Option(Description = "The continuation token from a previous listing response. Use the same account, container, prefix, and delimiter to retrieve the next page. Cannot be used with --blob.")]
    public string? ContinuationToken { get; set; }

    [Option(Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').")]
    public required string Account { get; set; }

    [Option(Description = "The name of the container to access within the storage account.")]
    public required string Container { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

}
