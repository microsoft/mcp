// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Tables.Options;

public static class TablesOptionDefinitions
{
    public const string AccountName = "account";

    public static readonly Option<string> Account = new($"--{AccountName}")
    {
        Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
        Required = true
    };
}
