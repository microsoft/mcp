// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Describes the records created, updated, or skipped by an ADME storage upsert.
/// </summary>
public sealed record UpsertRecordsResponse
{
    [JsonPropertyName("recordIds")]
    public IReadOnlyList<string>? RecordIds { get; init; }

    [JsonPropertyName("skippedRecordIds")]
    public IReadOnlyList<string>? SkippedRecordIds { get; init; }

    [JsonPropertyName("recordCount")]
    public int? RecordCount { get; init; }
}
