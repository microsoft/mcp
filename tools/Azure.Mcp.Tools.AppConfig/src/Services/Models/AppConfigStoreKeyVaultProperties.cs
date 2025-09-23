// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// A class representing the AppConfig store KeyVault properties model.
/// </summary>
internal sealed class AppConfigStoreKeyVaultProperties
{
    /// <summary> The URI of the key vault key used to encrypt data. </summary>
    [JsonPropertyName("keyIdentifier")]
    public string? KeyIdentifier { get; set; }
    /// <summary> The client id of the identity which will be used to access key vault. </summary>
    [JsonPropertyName("identityClientId")]
    public string? IdentityClientId { get; set; }
}