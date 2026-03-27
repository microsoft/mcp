// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record VaultCreateResult(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vaultType")] string VaultType,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState);
