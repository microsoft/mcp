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
[JsonSerializable(typeof(FetchRecordsRequest))]
[JsonSerializable(typeof(FetchRecordsResponse))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(QueryRecordsResponse))]
[JsonSerializable(typeof(RecordVersionsResponse))]
[JsonSerializable(typeof(SearchAggregation))]
[JsonSerializable(typeof(SearchCursorRequest))]
[JsonSerializable(typeof(SearchCursorResponse))]
[JsonSerializable(typeof(SearchQueryRequest))]
[JsonSerializable(typeof(SearchQueryResponse))]
[JsonSerializable(typeof(SearchResponse))]
[JsonSerializable(typeof(SearchSort))]
[JsonSerializable(typeof(SchemaListResponse))]
[JsonSerializable(typeof(StorageRecord))]
[JsonSerializable(typeof(StorageRecord[]))]
[JsonSerializable(typeof(UpsertRecordsResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)]
public partial class AdmeJsonContext : JsonSerializerContext;
