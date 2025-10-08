// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Configuration;

/// <summary>
/// Specifies the type of outbound authentication (credentials for Azure SDK calls).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<OutboundAuthenticationType>))]
public enum OutboundAuthenticationType
{
    /// <summary>
    /// Uses the default credential chain (az login, Visual Studio, VS Code, etc.).
    /// Suitable for stdio mode or HTTP mode without per-user authentication.
    /// </summary>
    Default,

    /// <summary>
    /// Uses Azure Managed Identity for authentication.
    /// The server authenticates to Azure using its managed identity.
    /// </summary>
    ManagedIdentity,

    /// <summary>
    /// Uses a bearer token from HTTP request header for Azure API calls.
    /// Requires HeaderName configuration.
    /// </summary>
    BearerToken,

    /// <summary>
    /// Uses On-Behalf-Of (OBO) flow to exchange incoming token for Azure access token.
    /// Requires InboundAuthentication.Type to be EntraIDAccessToken.
    /// Requires ClientSecret in AzureAd configuration.
    /// </summary>
    OnBehalfOf
}
