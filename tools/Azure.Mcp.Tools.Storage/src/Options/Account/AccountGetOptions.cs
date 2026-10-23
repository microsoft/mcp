// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Account;

public class AccountGetOptions : ISubscriptionOption, IPagingOption
{
    [Option(Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').")]
    public string? Account { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = "Maximum number of resources per page, from 1 to 1000. Defaults to 50. Follow the returned paging.continuationToken to retrieve more resources.")]
    public int? PageSize { get; set; }

    [Option(Description = "Opaque continuation token from a previous page. Replay the returned paging.continuationArguments unchanged to continue, including on another compatible server instance.")]
    public string? ContinuationToken { get; set; }
}
