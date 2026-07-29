// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Account;

public class AccountCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Storage account to create. Must be globally unique, 3-24 characters, lowercase letters and numbers only.")]
    public required string Account { get; set; }

    [Option(Description = "The Azure region where the storage account will be created (e.g., 'eastus', 'westus2').")]
    public required string Location { get; set; }

    [Option(Description = "The storage account SKU. Valid values: Standard_LRS, Standard_GRS, Standard_RAGRS, Standard_ZRS, Premium_LRS, Premium_ZRS, Standard_GZRS, Standard_RAGZRS.")]
    public string? Sku { get; set; }

    [Option(Description = "The default access tier for blob storage. Valid values: Hot, Cool.")]
    public string? AccessTier { get; set; }

    [Option(Description = "Whether to enable hierarchical namespace (Data Lake Storage Gen2) for the storage account.")]
    public bool? EnableHierarchicalNamespace { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
