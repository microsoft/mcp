// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents an OSDU record envelope returned by the ADME storage service.
/// </summary>
public sealed record StorageRecord
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("version")]
    public long? Version { get; init; }

    [JsonPropertyName("acl")]
    public RecordAcl? Acl { get; init; }

    [JsonPropertyName("legal")]
    public RecordLegal? Legal { get; init; }

    [JsonPropertyName("ancestry")]
    public RecordAncestry? Ancestry { get; init; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; init; }

    [JsonPropertyName("createUser")]
    public string? CreateUser { get; init; }

    [JsonPropertyName("createTime")]
    public string? CreateTime { get; init; }

    [JsonPropertyName("modifyUser")]
    public string? ModifyUser { get; init; }

    [JsonPropertyName("modifyTime")]
    public string? ModifyTime { get; init; }

    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }

    [JsonPropertyName("meta")]
    public JsonElement? Meta { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }
}
