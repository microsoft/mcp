// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Configuration;

namespace Azure.Mcp.Core.Areas.Server.Authentication.HttpHost;

/// <summary>
/// Factory for creating HTTP host authentication configuration implementations
/// based on the server configuration settings.
/// </summary>
public static class HttpHostAuthSetupFactory
{
    /// <summary>
    /// Creates an appropriate HTTP host authentication setup implementation
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
    public static IHttpHostAuthSetup Create(ServerConfiguration serverConfiguration)
    {
        var inboundType = serverConfiguration.InboundAuthentication.Type;
        if (inboundType == InboundAuthenticationType.None)
        {
            return IHttpHostAuthSetup.Default;
        }

        if (inboundType != InboundAuthenticationType.JwtBearerScheme)
        {
            throw new InvalidOperationException($"Only supported inbound authentication validation is JwtBearerScheme, but was {inboundType}");
        }

        var outboundType = serverConfiguration.OutboundAuthentication?.Type;
        if (outboundType == OutboundAuthenticationType.ManagedIdentity)
        {
            return new JwtHttpHostAuthSetup(serverConfiguration);
        }
        else if (outboundType == OutboundAuthenticationType.JwtPassthrough)
        {
            return new JwtHttpHostAuthSetup(serverConfiguration);
        }
        else
        {
            throw new NotSupportedException(
                $"Outbound authentication type '{outboundType}' is not supported with JwtBearerScheme inbound authentication. " +
                $"Supported types are: ManagedIdentity, BearerToken, OnBehalfOf.");
        }
    }
}
