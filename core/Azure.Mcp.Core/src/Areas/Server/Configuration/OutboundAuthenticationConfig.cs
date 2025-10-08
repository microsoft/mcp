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
    /// The HTTP header name to read the bearer token from.
    /// Required when <see cref="Type"/> is <see cref="OutboundAuthenticationType.BearerToken"/>.
    /// </summary>
    [JsonPropertyName("headerName")]
    public string? HeaderName { get; set; }

    /// <summary>
    /// Azure AD configuration for <see cref="OutboundAuthenticationType.OnBehalfOf"/> flow.
    /// Required when <see cref="Type"/> is <see cref="OutboundAuthenticationType.OnBehalfOf"/>.
    /// Must match <see cref="InboundAuthenticationConfig.AzureAd"/> except for <see cref="AzureAdConfig.ClientSecret"/>.
    /// </summary>
    [JsonPropertyName("azureAd")]
    public AzureAdConfig? AzureAd { get; set; }
}
