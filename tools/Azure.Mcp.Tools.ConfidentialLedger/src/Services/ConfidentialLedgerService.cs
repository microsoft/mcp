// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Security.ConfidentialLedger;

namespace Azure.Mcp.Tools.ConfidentialLedger.Services;

public class ConfidentialLedgerService : BaseAzureService, IConfidentialLedgerService
{
    // NOTE: We construct the data-plane endpoint from the ledger name.
    private static Uri BuildLedgerUri(string ledgerName) => new($"https://{ledgerName}.confidential-ledger.azure.com");

    private static RequestContent CreateAppendEntryContent(string entryData)
    {
        // We must always send an object with a 'contents' property. If the caller provided JSON, embed it as JSON;
        // otherwise treat it as a string literal.
        ArrayBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("contents");
            writer.WriteStringValue(entryData);
            writer.WriteEndObject();
        }

        BinaryData binary = new(buffer.WrittenSpan.ToArray());
        return RequestContent.Create(binary);
    }

    public async Task<AppendEntryResult> AppendEntryAsync(string ledgerName, string entryData, string? collectionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(entryData);

        var credential = await GetCredential();

        // Configure client (retry etc. could be extended later)
        ConfidentialLedgerClient client = new(BuildLedgerUri(ledgerName), credential);

        // Build RequestContent manually to avoid trimming issues from reflection-based serialization.
        using var content = CreateAppendEntryContent(entryData);
        var operation = await client.PostLedgerEntryAsync(WaitUntil.Completed, content, collectionId);
        var response = operation.GetRawResponse();

        string transactionId = string.Empty;
        if (response.Headers.TryGetValue("x-ms-ccf-transaction-id", out var headerValue))
        {
            transactionId = headerValue ?? string.Empty;
        }

        string state = string.Empty;
        try
        {
            if (response.ContentStream is not null)
            {
                using var doc = JsonDocument.Parse(response.ContentStream);
                if (doc.RootElement.TryGetProperty("state", out var stateProp))
                {
                    state = stateProp.GetString() ?? string.Empty;
                }
            }
        }
        catch
        {
            throw new RequestFailedException(response.Status, "Failed to parse response content.");
        }

        return new AppendEntryResult
        {
            TransactionId = transactionId,
            State = state
        };
    }
}
