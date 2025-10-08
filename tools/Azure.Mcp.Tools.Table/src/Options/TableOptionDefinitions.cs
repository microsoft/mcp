// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Table.Options;

public static class TableOptionDefinitions
{
    public const string AccountName = "account";
    public const string TableName = "table";

    public static readonly Option<string> Account = new($"--{AccountName}")
    {
        Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
        Required = true
    };

    public static readonly Option<string> Table = new($"--{TableName}")
    {
        Description = "The name of the table to access within the storage account.",
    };
}
