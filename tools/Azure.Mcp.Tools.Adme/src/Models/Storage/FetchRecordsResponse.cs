// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents the response of a multi-record fetch.
/// </summary>
public sealed record FetchRecordsResponse
{
    [JsonPropertyName("records")]
    public required IReadOnlyList<StorageRecord> Records { get; init; }

    [JsonPropertyName("invalidRecords")]
    public IReadOnlyList<string>? InvalidRecords { get; init; }

    [JsonPropertyName("retryRecords")]
    public IReadOnlyList<string>? RetryRecords { get; init; }

    [JsonPropertyName("notFound")]
    public IReadOnlyList<string>? NotFound { get; init; }

    [JsonPropertyName("conversionStatuses")]
    public IReadOnlyList<ConversionStatus>? ConversionStatuses { get; init; }
}
