// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Authentication.HttpHost.Implementations;
using Azure.Mcp.Core.Areas.Server.Configuration;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost;

/// <summary>
/// Factory for creating HTTP host authentication configuration implementations 
/// based on the server configuration settings.
/// </summary>
public static class HttpHostAuthenticationConfigurationFactory
{
    /// <summary>
    /// Creates an appropriate HTTP host authentication configuration implementation
    /// based on the provided server configuration.
    /// </summary>
    /// <param name="serverConfiguration">
    /// The server configuration containing authentication settings. 
    /// If null or if inbound authentication is None, returns a no-op implementation.
    /// </param>
    /// <returns>
    /// An implementation of IHttpHostAuthenticationConfiguration that handles
    /// the authentication requirements specified in the server configuration.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the authentication configuration combination is not supported.
    /// </exception>
    public static IHttpHostAuthenticationConfiguration Create(ServerConfiguration? serverConfiguration)
    {
        if (serverConfiguration?.InboundAuthentication?.Type != InboundAuthenticationType.EntraIDAccessToken)
        {
            // No authentication required
            return new NoneAuthentication();
        }

        var outboundType = serverConfiguration.OutboundAuthentication?.Type;

        if (outboundType == OutboundAuthenticationType.OnBehalfOf)
        {
            return new OnBehalfOfAuthentication(serverConfiguration);
        }
        else if (outboundType == OutboundAuthenticationType.ManagedIdentity)
        {
            return new JwtBearerAuthentication(serverConfiguration);
        }
        else if (outboundType == OutboundAuthenticationType.BearerToken)
        {
            return new JwtBearerAuthentication(serverConfiguration);
        }
        else
        {
            throw new NotSupportedException(
                $"Outbound authentication type '{outboundType}' is not supported with EntraIDAccessToken inbound authentication. " +
                $"Supported types are: ManagedIdentity, BearerToken, OnBehalfOf.");
        }
    }
}
