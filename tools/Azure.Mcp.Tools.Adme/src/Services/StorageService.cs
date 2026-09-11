// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.Adme.Models.Storage;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Retrieves OSDU records from the ADME storage service.
/// </summary>
public sealed class StorageService(
    IAzureTokenCredentialProvider credentialProvider,
    IHttpClientFactory httpClientFactory) : IStorageService
{
    private const string BasePath = "/api/storage/v2";
    private const string FrameOfReferenceHeader = "frame-of-reference";
    private const string FrameOfReferenceNone = "none";
    private const string FrameOfReferenceNormalized =
        "units=SI;crs=wgs84;elevation=msl;azimuth=true north;dates=utc;";

    private readonly IAzureTokenCredentialProvider _credentialProvider = credentialProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public Task<StorageRecord> GetRecordAsync(
        string endpoint,
        string dataPartition,
        string id,
        long? version,
        IReadOnlyList<string>? attributes,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var path = version is null
            ? $"{BasePath}/records/{Uri.EscapeDataString(id)}"
            : $"{BasePath}/records/{Uri.EscapeDataString(id)}/{version.Value.ToString(CultureInfo.InvariantCulture)}";

        return AdmeServiceHelper.SendAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            AppendAttributes(path, attributes), AdmeJsonContext.Default.StorageRecord, cancellationToken);
    }

    /// <summary>
    /// Lists the numeric versions of a record.
    /// </summary>
    public Task<RecordVersionsResponse> ListRecordVersionsAsync(
        string endpoint,
        string dataPartition,
        string id,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AdmeServiceHelper.SendAsync(
            _credentialProvider,
            _httpClientFactory,
            endpoint,
            dataPartition,
            tenant,
            $"{BasePath}/records/versions/{Uri.EscapeDataString(id)}",
            AdmeJsonContext.Default.RecordVersionsResponse,
            cancellationToken);
    }

    public Task<QueryRecordsResponse> QueryRecordsByKindAsync(
        string endpoint,
        string dataPartition,
        string kind,
        int limit,
        string? cursor,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        var query = new List<KeyValuePair<string, string>>();
        AdmeServiceHelper.Add(query, "kind", kind);
        AdmeServiceHelper.Add(query, "limit", limit.ToString(CultureInfo.InvariantCulture));
        AdmeServiceHelper.Add(query, "cursor", cursor);

        return AdmeServiceHelper.SendAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            AdmeServiceHelper.AppendQuery($"{BasePath}/query/records", query),
            AdmeJsonContext.Default.QueryRecordsResponse, cancellationToken,
            sendJsonContentTypeHint: true);
    }

    public Task<FetchRecordsResponse> FetchRecordsAsync(
        string endpoint,
        string dataPartition,
        IReadOnlyList<string> ids,
        IReadOnlyList<string>? attributes,
        bool frameOfReference,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Count == 0)
        {
            throw new ArgumentException("At least one record id is required.", nameof(ids));
        }

        var projecting = attributes is { Count: > 0 };
        var body = new FetchRecordsRequest
        {
            Records = ids,
            Attributes = projecting ? attributes : null,
        };

        return AdmeServiceHelper.PostAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            projecting ? $"{BasePath}/query/records" : $"{BasePath}/query/records:batch",
            body, AdmeJsonContext.Default.FetchRecordsRequest,
            AdmeJsonContext.Default.FetchRecordsResponse,
            projecting ? null : [new(FrameOfReferenceHeader,
                frameOfReference ? FrameOfReferenceNormalized : FrameOfReferenceNone)],
            cancellationToken);
    }

    public Task<UpsertRecordsResponse> UpsertRecordsAsync(
        string endpoint,
        string dataPartition,
        IReadOnlyList<StorageRecord> records,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(records);
        if (records.Count == 0)
        {
            throw new ArgumentException("At least one record is required.", nameof(records));
        }

        return AdmeServiceHelper.PutAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            $"{BasePath}/records", records.ToArray(), AdmeJsonContext.Default.StorageRecordArray,
            AdmeJsonContext.Default.UpsertRecordsResponse, cancellationToken);
    }

    public Task DeleteRecordAsync(
        string endpoint,
        string dataPartition,
        string id,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AdmeServiceHelper.DeleteAsync(
            _credentialProvider, _httpClientFactory, endpoint, dataPartition, tenant,
            $"{BasePath}/records/{Uri.EscapeDataString(id)}", cancellationToken);
    }

    private static string AppendAttributes(string path, IReadOnlyList<string>? attributes)
    {
        if (attributes is not { Count: > 0 })
        {
            return path;
        }

        var query = new List<KeyValuePair<string, string>>();
        foreach (var attribute in attributes)
        {
            AdmeServiceHelper.Add(query, "attribute", attribute);
        }

        return AdmeServiceHelper.AppendQuery(path, query);
    }
}
