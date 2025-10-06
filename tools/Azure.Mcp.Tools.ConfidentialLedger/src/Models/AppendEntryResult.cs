// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ConfidentialLedger.Models;

public sealed class AppendEntryRequest
{
    [JsonPropertyName("contents")] public string Contents { get; init; } = string.Empty;
}

public sealed class AppendEntryResult
{
    [JsonPropertyName("transactionId")] public string TransactionId { get; init; } = string.Empty;
    [JsonPropertyName("state")] public string State { get; init; } = string.Empty; // e.g. Committed / Pending
}
