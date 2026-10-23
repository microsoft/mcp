// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public sealed class GetEntryOptions : BaseConfidentialLedgerOptions
{
    [Option(Description = "The Confidential Ledger transaction identifier (for example: '2.199').")]
    public required string TransactionId { get; set; }

    [Option(Description = ConfidentialLedgerOptionDescriptions.CollectionId)]
    public string? CollectionId { get; set; }
}
