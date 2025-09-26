// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// A class representing the AppConfig store encryption properties model.
/// </summary>
internal sealed class AppConfigStoreEncryptionProperties
{
    /// <summary> Key vault properties. </summary>
    [JsonPropertyName("keyVaultProperties")]
    public AppConfigurationKeyVaultProperties KeyVaultProperties { get; set; }
}