// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Configuration for Azure SDK authentication (outbound calls to Azure services).
/// </summary>
public class OutboundAuthenticationConfig
{
    /// <summary>
    /// The type of outbound authentication to use.
    /// </summary>
    [JsonPropertyName("type")]
    public required OutboundAuthenticationType Type { get; set; }

    /// <summary>
    /// Optional client ID for user-assigned managed identity.
    /// Only used when <see cref="Type"/> is <see cref="OutboundAuthenticationType.ManagedIdentity"/>.
    /// When not specified, system-assigned managed identity is used.
    /// </summary>
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    /// <summary>
    /// Client credential configuration for <see cref="OutboundAuthenticationType.JwtObo"/> flow.
    /// Required when <see cref="Type"/> is <see cref="OutboundAuthenticationType.JwtObo"/>.
    /// Azure AD configuration is inherited from <see cref="InboundAuthenticationConfig.AzureAd"/>.
    /// </summary>
    [JsonPropertyName("clientCredential")]
    public ClientCredentialConfig? ClientCredential { get; set; }
}
