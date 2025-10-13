// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Claims;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Token credential provider that uses JWT exchange flow (On-Behalf-Of) to exchange incoming tokens for Azure access tokens.
/// This is a multi-user provider that performs token exchange per request.
/// Azure AD configuration is inherited from inbound authentication.
/// </summary>
public sealed class JwtOboCredentialProvider : ITokenCredentialProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenAcquisition _tokenAcquisition;

    /// <summary>
    /// Initializes a new instance of the JwtOboCredentialProvider class.
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor to read incoming tokens.</param>
    /// <param name="tokenAcquisition">Token acquisition service for JWT exchange flows.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public JwtOboCredentialProvider(IHttpContextAccessor httpContextAccessor, ITokenAcquisition tokenAcquisition)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _tokenAcquisition = tokenAcquisition ?? throw new ArgumentNullException(nameof(tokenAcquisition));
    }

    /// <summary>
    /// Creates a TokenCredential using JWT exchange flow with the incoming token from HTTP context.
    /// </summary>
    /// <param name="tenant">Optional tenant ID to override the configured tenant.</param>
    /// <returns>A TokenCredential that performs JWT exchange.</returns>
    public Task<TokenCredential> CreateAsync(string? tenant = null)
    {
        throw new NotImplementedException("anuchan to discuss with svukel (Steven).");
    }

    /// <summary>
    /// Gets the current user identity extracted from the incoming token.
    /// </summary>
    /// <returns>User identity string extracted from the incoming token, or null if not available.</returns>
    public string? GetCurrentUserId()
    {
        throw new NotImplementedException("anuchan to discuss with svukel (Steven).");
    }

    /// <summary>
    /// Inner TokenCredential implementation that uses ITokenAcquisition for JWT OBO flows.
    /// </summary>
    private sealed class JwtOboTokenCredential : TokenCredential
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtOboTokenCredential(ITokenAcquisition tokenAcquisition, IHttpContextAccessor httpContextAccessor)
        {
            _tokenAcquisition = tokenAcquisition;
            _httpContextAccessor = httpContextAccessor;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Synchronous token acquisition is not supported. Use GetTokenAsync instead.");
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("anuchan to discuss with svukel (Steven).");
        }
    }
}
