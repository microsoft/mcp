using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ConfidentialLedger.Models;

public sealed record LedgerEntryGetResult(
    [property: JsonPropertyName("ledgerName")] string LedgerName,
    [property: JsonPropertyName("transactionId")] string TransactionId,
    [property: JsonPropertyName("contents")] string? Contents);
