// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Tables.Options;

public static class TablesOptionDefinitions
{
    public const string StorageAccountName = "storage-account";

    public static readonly Option<string> StorageAccount = new($"--{StorageAccountName}")
    {
        Description = "The name of the Azure Storage account (e.g., 'mystorageaccount').",
        Required = true
    };
}
