// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// Settings concerning key vault encryption for a configuration store.
/// </summary>
internal sealed class AppConfigurationKeyVaultProperties
{
    /// <summary> The URI of the key vault key used to encrypt data. </summary>
    public string? KeyIdentifier { get; set; }
    /// <summary> The client id of the identity which will be used to access key vault. </summary>
    public string? IdentityClientId { get; set; }
}
