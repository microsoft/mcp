// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp account via ARM generic resource API.
/// </summary>
internal sealed class NetAppAccountCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public NetAppAccountCreateProperties? Properties { get; set; }
}

internal sealed class NetAppAccountCreateProperties
{
}
