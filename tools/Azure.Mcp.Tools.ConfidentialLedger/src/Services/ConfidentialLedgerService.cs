// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        ValidateRequiredParameters(
            (nameof(ledgerName), ledgerName),
            (nameof(entryData), entryData));

        var credential = await GetCredential();

        // Configure client (retry etc. could be extended later)
        ConfidentialLedgerClient client = new(BuildLedgerUri(ledgerName), credential);

        // Build RequestContent manually to avoid trimming issues from reflection-based serialization.
        using var content = CreateAppendEntryContent(entryData);
        var operation = await client.PostLedgerEntryAsync(WaitUntil.Completed, content, collectionId);
        var response = operation.GetRawResponse();

        return new AppendEntryResult
        {
            TransactionId = operation.Id,
            State = operation.HasCompleted ? "Committed" : "Pending"
        };
    }

    public async Task<LedgerEntryGetResult> GetLedgerEntryAsync(string ledgerName, string transactionId, string? collectionId = null)
    {
        ValidateRequiredParameters(
            (nameof(ledgerName), ledgerName),
            (nameof(transactionId), transactionId));

        // throw if strings are blank (whitespace)
        if (string.IsNullOrWhiteSpace(ledgerName))
        {
            throw new ArgumentException("Ledger name cannot be empty or whitespace.");
        }
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("Transaction ID cannot be empty or whitespace.", nameof(transactionId));
        }

        var credential = await GetCredential();
        ConfidentialLedgerClient client = new(BuildLedgerUri(ledgerName), credential);

        Response? getByCollectionResponse = null;
        bool loaded = false;
        string? contents = null;
        string? actualTransactionId = null;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        while (!loaded)
        {
            if (cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException($"Timed out waiting for ledger entry to load after 15 seconds. Transaction ID: {transactionId}");
            }
            getByCollectionResponse = await client.GetLedgerEntryAsync(transactionId, collectionId).ConfigureAwait(false);
            using (JsonDocument jsonDoc = JsonDocument.Parse(getByCollectionResponse.Content))
            {
                loaded = jsonDoc.RootElement.GetProperty("state").GetString() != "Loading";
                if (!loaded)
                {
                    // Add a small delay to avoid tight polling
                    await Task.Delay(500, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    if (jsonDoc.RootElement.TryGetProperty("entry", out var entryElement))
                    {
                        contents = entryElement.TryGetProperty("contents", out var contentsElement) ? contentsElement.GetString() : null;
                        actualTransactionId = entryElement.TryGetProperty("transactionId", out var txElement) ? txElement.GetString() : null;
                    }
                }
            }
        }

        return new LedgerEntryGetResult
        {
            LedgerName = ledgerName,
            TransactionId = actualTransactionId ?? transactionId,
            Contents = contents ?? string.Empty,
        };
    }
}
