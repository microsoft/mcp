// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Storage;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Provides access to OSDU records held by the ADME storage service.
/// </summary>
public interface IStorageService
{
    Task<StorageRecord> GetRecordAsync(
        string endpoint,
        string dataPartition,
        string id,
        long? version,
        IReadOnlyList<string>? attributes,
        string? tenant,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists the numeric versions of a record.
    /// </summary>
    Task<RecordVersionsResponse> ListRecordVersionsAsync(
        string endpoint,
        string dataPartition,
        string id,
        string? tenant,
        CancellationToken cancellationToken);

    Task<QueryRecordsResponse> QueryRecordsByKindAsync(
        string endpoint,
        string dataPartition,
        string kind,
        int limit,
        string? cursor,
        string? tenant,
        CancellationToken cancellationToken);

    Task<FetchRecordsResponse> FetchRecordsAsync(
        string endpoint,
        string dataPartition,
        IReadOnlyList<string> ids,
        IReadOnlyList<string>? attributes,
        bool frameOfReference,
        string? tenant,
        CancellationToken cancellationToken);

    Task<UpsertRecordsResponse> UpsertRecordsAsync(
        string endpoint,
        string dataPartition,
        IReadOnlyList<StorageRecord> records,
        string? tenant,
        CancellationToken cancellationToken);

    Task DeleteRecordAsync(
        string endpoint,
        string dataPartition,
        string id,
        string? tenant,
        CancellationToken cancellationToken);
}
