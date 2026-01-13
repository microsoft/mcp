// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ConfidentialLedger.Models;

namespace Azure.Mcp.Tools.ConfidentialLedger.Services;

public interface IConfidentialLedgerService
{
    Task<AppendEntryResult> AppendEntryAsync(string ledgerName, string entryData, string? collectionId = null, CancellationToken cancellationToken = default);
    Task<LedgerEntryGetResult> GetLedgerEntryAsync(string ledgerName, string transactionId, string? collectionId = null, CancellationToken cancellationToken = default);
}
