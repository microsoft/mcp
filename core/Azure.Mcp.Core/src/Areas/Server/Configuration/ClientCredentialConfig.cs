// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Configuration for client credentials used in JWT On-Behalf-Of (OBO) authentication.
/// The required properties depend on the Kind specified.
/// </summary>
public class ClientCredentialConfig
{
    /// <summary>
    /// The type of client credential to use.
    /// </summary>
    [JsonPropertyName("kind")]
    public required JwtOboClientCredentialKind Kind { get; set; }

    /// <summary>
    /// The client secret for authentication.
    /// Required when Kind is ClientSecret.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    /// <summary>
    /// The thumbprint of the certificate for authentication.
    /// Required when Kind is CertificateLocal or CertificateKeyVault.
    /// </summary>
    [JsonPropertyName("thumbprint")]
    public string? Thumbprint { get; set; }

    /// <summary>
    /// The Azure Key Vault URL where the certificate is stored.
    /// Required when Kind is CertificateKeyVault.
    /// </summary>
    [JsonPropertyName("keyVaultUrl")]
    public string? KeyVaultUrl { get; set; }
}
