// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public class BaseConfidentialLedgerOptions : GlobalOptions
{
    [JsonPropertyName(ConfidentialLedgerOptionDefinitions.LedgerNameName)]
    public string? LedgerName { get; set; }
}
