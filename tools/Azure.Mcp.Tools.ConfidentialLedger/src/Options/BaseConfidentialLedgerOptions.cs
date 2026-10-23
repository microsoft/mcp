// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public class BaseConfidentialLedgerOptions
{
    [Option(Description = "The name of the Confidential Ledger instance (e.g., 'myledger').")]
    public required string Ledger { get; set; }
}
