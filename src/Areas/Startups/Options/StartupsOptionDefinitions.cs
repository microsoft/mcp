// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Areas.Startups.Options;

public static class StartupsOptionDefinitions
{

    public const string SubscriptionParam = "subscription";
    public const string ResourceGroupParam = "resource-group";
    public const string StorageAccountParam = "storage-account";
    public const string SourcePathParam = "source-path";

    public static readonly Option<string> Subscription = new(
        $"--{SubscriptionParam}",
        "The Id/Name of the subscription"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> ResourceGroup = new(
        $"--{ResourceGroupParam}",
        "The name of the Azure resource group"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> StorageAccount = new(
        $"--{StorageAccountParam}",
        "The name of the storage account"
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> SourcePath = new(
        $"--{SourcePathParam}",
        "The path to the source directory containing website files"
    )
    {
        IsRequired = true
    };
}
