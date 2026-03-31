// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp backup vault via ARM generic resource API.
/// </summary>
internal sealed class BackupVaultCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public BackupVaultCreateProperties? Properties { get; set; }
}

internal sealed class BackupVaultCreateProperties
{
}
