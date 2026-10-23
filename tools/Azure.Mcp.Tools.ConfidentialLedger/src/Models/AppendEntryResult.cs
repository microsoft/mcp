// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ConfidentialLedger.Models;

public sealed record AppendEntryResult(
    [property: JsonPropertyName("transactionId")] string TransactionId,
    [property: JsonPropertyName("state")] string State); // e.g. Committed / Pending
