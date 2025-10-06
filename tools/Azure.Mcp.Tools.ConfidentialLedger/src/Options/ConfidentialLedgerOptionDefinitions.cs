// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public static class ConfidentialLedgerOptionDefinitions
{
    public const string LedgerNameName = "ledger";
    public const string EntryDataName = "entry-data";
    public const string CollectionIdName = "collection-id";

    public static readonly Option<string> LedgerName = new($"--{LedgerNameName}")
    {
        Description = "The name of the Confidential Ledger instance (e.g., 'myledger').",
        Required = true
    };

    public static readonly Option<string> EntryData = new($"--{EntryDataName}")
    {
        Description = "The JSON or text payload to append as a tamper-proof ledger entry.",
        Required = true
    };

    public static readonly Option<string?> CollectionId = new($"--{CollectionIdName}")
    {
        Description = "Optional ledger collection identifier. If omitted the default collection is used.",
        Required = false
    };
}
