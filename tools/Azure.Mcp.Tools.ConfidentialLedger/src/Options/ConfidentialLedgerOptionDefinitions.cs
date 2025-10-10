// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public static class ConfidentialLedgerOptionDefinitions
{
    public const string LedgerNameName = "ledger";
    public const string ContentName = "content";
    public const string CollectionIdName = "collection-id";
    public const string TransactionIdName = "transaction-id";

    public static readonly Option<string> LedgerName = new($"--{LedgerNameName}")
    {
        Description = "The name of the Confidential Ledger instance (e.g., 'myledger').",
        Required = true
    };

    public static readonly Option<string> Content = new($"--{ContentName}")
    {
        Description = "The JSON or text payload to append as a tamper-proof ledger entry.",
        Required = true
    };

    public static readonly Option<string?> CollectionId = new($"--{CollectionIdName}")
    {
        Description = "Optional ledger collection identifier. If omitted the default collection is used.",
        Required = false
    };

    public static readonly Option<string> TransactionId = new($"--{TransactionIdName}")
    {
        Description = "The Confidential Ledger transaction identifier (for example: '2.199').",
        Required = true
    };
}
