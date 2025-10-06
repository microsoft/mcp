// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public class AppendEntryOptions : BaseConfidentialLedgerOptions
{
    [JsonPropertyName(ConfidentialLedgerOptionDefinitions.ContentName)]
    public string? Content { get; set; }

    [JsonPropertyName(ConfidentialLedgerOptionDefinitions.CollectionIdName)]
    public string? CollectionId { get; set; }
}
