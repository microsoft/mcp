// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Specifies the type of client credential for JWT On-Behalf-Of (OBO) authentication.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<JwtOboClientCredentialKind>))]
public enum JwtOboClientCredentialKind
{
    /// <summary>
    /// Uses a client secret for authentication.
    /// Requires the Secret property to be configured.
    /// </summary>
    ClientSecret,

    /// <summary>
    /// Uses a certificate from the local certificate store for authentication.
    /// Requires the Thumbprint property to be configured.
    /// </summary>
    CertificateLocal,

    /// <summary>
    /// Uses a certificate from Azure Key Vault for authentication.
    /// Requires both Thumbprint and KeyVaultUrl properties to be configured.
    /// </summary>
    CertificateKeyVault
}
