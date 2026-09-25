// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public sealed class AppendEntryOptions : BaseConfidentialLedgerOptions
{
    [Option(Description = "The JSON or text payload to append as a tamper-proof ledger entry.")]
    public required string Content { get; set; }

    [Option(Description = ConfidentialLedgerOptionDescriptions.CollectionId)]
    public string? CollectionId { get; set; }
}
