// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Security.ConfidentialLedger;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ConfidentialLedger.Services;

public class ConfidentialLedgerService(ITenantService tenantService)
    : BaseAzureService(tenantService), IConfidentialLedgerService
{
    // NOTE: We construct the data-plane endpoint from the ledger name.
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));

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

    public async Task<AppendEntryResult> AppendEntryAsync(string ledgerName, string entryData, string? collectionId = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(ledgerName), ledgerName),
            (nameof(entryData), entryData));

        var ledgerUri = new Uri(GetLedgerUri(ledgerName));
        var credential = await GetCredential(cancellationToken);

        // Configure client (retry etc. could be extended later)
        ConfidentialLedgerClient client = new(ledgerUri, credential);

        // Build RequestContent manually to avoid trimming issues from reflection-based serialization.
        using var content = CreateAppendEntryContent(entryData);
        var operation = await client.PostLedgerEntryAsync(WaitUntil.Started, content, collectionId, new RequestContext() { CancellationToken = cancellationToken });
        await WaitForLroCompletionAsync(operation, cancellationToken);
        var response = operation.GetRawResponse();

        return new()
        {
            TransactionId = operation.Id,
            State = operation.HasCompleted ? "Committed" : "Pending"
        };
    }

    public async Task<LedgerEntryGetResult> GetLedgerEntryAsync(string ledgerName, string transactionId, string? collectionId = null, CancellationToken cancellationToken = default)
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

        var ledgerUri = new Uri(GetLedgerUri(ledgerName));
        var credential = await GetCredential(cancellationToken);
        ConfidentialLedgerClient client = new(ledgerUri, credential);

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
            var getByCollectionResponse = await client.GetLedgerEntryAsync(transactionId, collectionId).ConfigureAwait(false);
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

        return new()
        {
            LedgerName = ledgerName,
            TransactionId = actualTransactionId ?? transactionId,
            Contents = contents ?? string.Empty,
        };
    }

    private string GetLedgerUri(string ledgerName)
    {
        ValidateLedgerName(ledgerName);

        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud =>
                $"https://{ledgerName}.confidential-ledger.azure.com",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud =>
                $"https://{ledgerName}.confidential-ledger.azure.cn",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud =>
                $"https://{ledgerName}.confidential-ledger.azure.us",
            _ =>
                $"https://{ledgerName}.confidential-ledger.azure.com"
        };
    }

    /// <summary>
    /// Validates that a ledger name contains only ASCII characters valid for an Azure Confidential Ledger name
    /// (a-z, A-Z, 0-9, and hyphens, starting with an ASCII letter).
    /// </summary>
    private static void ValidateLedgerName(string ledgerName)
    {
        if (string.IsNullOrWhiteSpace(ledgerName))
        {
            throw new ArgumentException("Ledger name cannot be null or empty.", nameof(ledgerName));
        }

        if (!char.IsAsciiLetter(ledgerName[0]))
        {
            throw new ArgumentException(
                $"Ledger name must start with an ASCII letter. Got: '{ledgerName[0]}'.", nameof(ledgerName));
        }

        foreach (var c in ledgerName)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '-')
            {
                throw new ArgumentException(
                    $"Ledger name contains invalid character '{c}'. Only ASCII alphanumeric characters and hyphens are allowed.", nameof(ledgerName));
            }
        }
    }
}
