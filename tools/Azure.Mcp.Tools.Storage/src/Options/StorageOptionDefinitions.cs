// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Options;

public static class StorageOptionDefinitions
{
    public const string AccountName = "account";
    public const string AccountCreateName = "account";
    public const string ContainerName = "container";
    public const string BlobName = "blob";
    public const string LocalFilePathName = "local-file-path";
    public const string LocationName = "location";
    public const string SkuName = "sku";
    public const string AccessTierName = "access-tier";
    public const string EnableHierarchicalNamespaceName = "enable-hierarchical-namespace";

    public static readonly Option<string> Account = new(
        $"--{AccountName}"
    )
    {
        Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
        Required = true
    };

    public static readonly Option<string> AccountCreate = new(
        $"--{AccountCreateName}"
    )
    {
        Description = "The name of the Azure Storage account to create. Must be globally unique, 3-24 characters, lowercase letters and numbers only.",
        Required = true
    };

    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The Azure region where the storage account will be created (e.g., 'eastus', 'westus2').",
        Required = true
    };

    public static readonly Option<string> Sku = new(
        $"--{SkuName}"
    )
    {
        Description = "The storage account SKU. Valid values: Standard_LRS, Standard_GRS, Standard_RAGRS, Standard_ZRS, Premium_LRS, Premium_ZRS, Standard_GZRS, Standard_RAGZRS.",
        Required = false
    };

    public static readonly Option<string> AccessTier = new(
        $"--{AccessTierName}"
    )
    {
        Description = "The default access tier for blob storage. Valid values: Hot, Cool.",
        Required = false
    };

    public static readonly Option<bool> EnableHierarchicalNamespace = new($"--{EnableHierarchicalNamespaceName}")
    {
        Description = "Whether to enable hierarchical namespace (Data Lake Storage Gen2) for the storage account.",
        DefaultValueFactory = _ => false,
        Required = false
    };

    public static readonly Option<string> Container = new(
        $"--{ContainerName}"
    )
    {
        Description = "The name of the container to access within the storage account.",
        Required = true
    };

    public static readonly Option<string> Blob = new(
        $"--{BlobName}"
    )
    {
        Description = "The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').",
        Required = true
    };

    public static readonly Option<string> LocalFilePath = new(
        $"--{LocalFilePathName}"
    )
    {
        Description = "The local file path to read content from or to write content to. This should be the full path to the file on your local system.",
        Required = true
    };
}
