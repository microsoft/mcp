using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ConfidentialLedger.Models;

public sealed class LedgerEntryGetCommandResult
{
    [JsonPropertyName("ledgerName")]
    public string LedgerName { get; init; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("contents")]
    public string? Contents { get; init; }
}
