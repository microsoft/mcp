using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ConfidentialLedger.Options;

public sealed class GetEntryOptions : BaseConfidentialLedgerOptions
{
    [JsonPropertyName(ConfidentialLedgerOptionDefinitions.TransactionIdName)]
    public string? TransactionId { get; set; }

    [JsonPropertyName(ConfidentialLedgerOptionDefinitions.CollectionIdName)]
    public string? CollectionId { get; set; }
}
