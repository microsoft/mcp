// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Factory for creating ITokenCredentialProvider instances based on server configuration.
/// Similar to HttpHostAuthenticationConfigurationFactory pattern.
/// </summary>
public static class TokenCredentialProviderFactory
{
    /// <summary>
    /// Creates an appropriate ITokenCredentialProvider implementation
    /// based on the provided server configuration.
    /// </summary>
    /// <param name="serverConfiguration">
    /// The server configuration containing authentication settings.
    /// If Default type, returns the default marker provider.
    /// </param>
    /// <param name="serviceProvider">
    /// Service provider for resolving dependencies like IHttpContextAccessor and ITokenAcquisition.
    /// Required for BearerToken, OnBehalfOf cases.
    /// </param>
    /// <returns>
    /// An implementation of ITokenCredentialProvider that handles
    /// the authentication requirements specified in the server configuration.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the authentication configuration is not supported.
    /// </exception>
    public static ITokenCredentialProvider Create(ServerConfiguration serverConfiguration, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serverConfiguration, nameof(serverConfiguration));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        var outboundType = serverConfiguration.OutboundAuthentication.Type;

        if (outboundType == OutboundAuthenticationType.Default)
        {
            return ITokenCredentialProvider.Default;
        }
        else if (outboundType == OutboundAuthenticationType.ManagedIdentity)
        {
            // TODO: Extract clientId from configuration if needed for user-assigned identity
            return new ManagedIdentityCredentialProvider(clientId: null);
        }
        else if (outboundType == OutboundAuthenticationType.JwtPassthrough)
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            return new JwtPassthroughCredentialProvider(httpContextAccessor);
        }
        else if (outboundType == OutboundAuthenticationType.JwtObo)
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var tokenAcquisition = serviceProvider.GetRequiredService<ITokenAcquisition>();
            return new JwtOboCredentialProvider(httpContextAccessor, tokenAcquisition);
        }
        else
        {
            throw new NotSupportedException($"Outbound authentication type '{outboundType}' is not supported.");
        }
    }
}
