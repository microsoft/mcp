// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Configuration for validating incoming HTTP requests.
/// </summary>
public class InboundAuthenticationConfig
{
    /// <summary>
    /// The type of inbound authentication to use.
    /// </summary>
    [JsonPropertyName("type")]
    public required InboundAuthenticationType Type { get; set; }

    /// <summary>
    /// Azure AD configuration for <see cref="InboundAuthenticationType.JwtBearerScheme"/> based validation.
    /// Required when <see cref="Type"/> is set to <see cref="InboundAuthenticationType.JwtBearerScheme"/>.
    /// </summary>
    [JsonPropertyName("azureAd")]
    public AzureAdConfig? AzureAd { get; set; }
}
