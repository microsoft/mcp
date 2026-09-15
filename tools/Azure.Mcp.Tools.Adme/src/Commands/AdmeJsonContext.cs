// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Adme.Models;
using Azure.Mcp.Tools.Adme.Models.Schema;
using Azure.Mcp.Tools.Adme.Models.Search;
using Azure.Mcp.Tools.Adme.Models.Storage;

namespace Azure.Mcp.Tools.Adme;

/// <summary>
/// Provides source-generated JSON metadata for ADME/OSDU responses.
/// </summary>
[JsonSerializable(typeof(HealthCheckResult))]
[JsonSerializable(typeof(AdmeResponse<HealthCheckResult>))]
[JsonSerializable(typeof(FetchRecordsRequest))]
[JsonSerializable(typeof(FetchRecordsResponse))]
[JsonSerializable(typeof(AdmeResponse<FetchRecordsResponse>))]
[JsonSerializable(typeof(AdmeResponse<JsonElement>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(QueryRecordsResponse))]
[JsonSerializable(typeof(AdmeResponse<QueryRecordsResponse>))]
[JsonSerializable(typeof(RecordVersionsResponse))]
[JsonSerializable(typeof(AdmeResponse<RecordVersionsResponse>))]
[JsonSerializable(typeof(SearchAggregation))]
[JsonSerializable(typeof(SearchCursorRequest))]
[JsonSerializable(typeof(SearchCursorResponse))]
[JsonSerializable(typeof(SearchQueryRequest))]
[JsonSerializable(typeof(SearchQueryResponse))]
[JsonSerializable(typeof(SearchResponse))]
[JsonSerializable(typeof(AdmeResponse<SearchResponse>))]
[JsonSerializable(typeof(SearchSort))]
[JsonSerializable(typeof(SchemaListResponse))]
[JsonSerializable(typeof(AdmeResponse<SchemaListResponse>))]
[JsonSerializable(typeof(StorageRecord))]
[JsonSerializable(typeof(AdmeResponse<StorageRecord>))]
[JsonSerializable(typeof(StorageRecord[]))]
[JsonSerializable(typeof(UpsertRecordsResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)]
public partial class AdmeJsonContext : JsonSerializerContext;
