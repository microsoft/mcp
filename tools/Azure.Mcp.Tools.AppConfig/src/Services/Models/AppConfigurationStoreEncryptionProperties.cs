// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// The encryption settings for a configuration store.
/// </summary>
internal sealed class AppConfigurationStoreEncryptionProperties
{
    /// <summary> Key vault properties. </summary>
    public AppConfigurationKeyVaultProperties? KeyVaultProperties { get; set; }
}
