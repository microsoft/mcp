// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp capacity pool via ARM generic resource API.
/// </summary>
internal sealed class CapacityPoolCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public CapacityPoolCreateProperties? Properties { get; set; }
}

internal sealed class CapacityPoolCreateProperties
{
    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("serviceLevel")]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName("qosType")]
    public string? QosType { get; set; }

    [JsonPropertyName("coolAccess")]
    public bool? CoolAccess { get; set; }

    [JsonPropertyName("encryptionType")]
    public string? EncryptionType { get; set; }
}
