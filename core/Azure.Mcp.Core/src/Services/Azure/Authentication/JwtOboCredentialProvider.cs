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
        return Task.FromResult<TokenCredential>(new JwtOboTokenCredential(_tokenAcquisition, _httpContextAccessor));
    }

    /// <summary>
    /// Gets the current user identity extracted from the incoming token.
    /// </summary>
    /// <returns>User identity string extracted from the incoming token, or null if not available.</returns>
    public string? GetCurrentUserId()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var principal = httpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("User is not authenticated. JWT exchange authentication requires an authenticated user context.");
            }

            // Extract user object ID using the same pattern as the reference code
            var userObjectId = ExtractUserObjectId(principal);
            var tenantId = ExtractTenantId(principal);

            // Combine tenant and user for unique identifier (similar to cache key pattern)
            if (!string.IsNullOrEmpty(userObjectId))
            {
                return !string.IsNullOrEmpty(tenantId)
                    ? $"{tenantId}_{userObjectId}"
                    : userObjectId;
            }

            throw new InvalidOperationException("Unable to extract user object ID from authentication context. Required claims (oid, sub, etc.) are missing from the token.");
        }
        catch (Exception ex)
        {
            // Throw exception to prevent cache contamination between users
            // Better to fail the request than risk cross-user data leakage
            throw new InvalidOperationException("Failed to extract user identity from authentication context. This is required for secure multi-user operations.", ex);
        }
    }

    /// <summary>
    /// Extracts the tenant ID from the ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal containing user claims.</param>
    /// <returns>The tenant ID, or null if not found.</returns>
    private static string? ExtractTenantId(ClaimsPrincipal principal)
    {
        return principal.FindFirst("tid")?.Value
               ?? principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
    }

    /// <summary>
    /// Extracts the user object ID from the ClaimsPrincipal.
    /// </summary>
    /// <param name="principal">The ClaimsPrincipal containing user claims.</param>
    /// <returns>The user object ID, or null if not found.</returns>
    private static string? ExtractUserObjectId(ClaimsPrincipal principal)
    {
        return principal.FindFirst("oid")?.Value
               ?? principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
               ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? principal.FindFirst("sub")?.Value;
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

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available. This credential can only be used within an HTTP request.");
            }

            if (requestContext.Scopes == null || requestContext.Scopes.Length == 0)
            {
                throw new ArgumentException("Token request context must have at least one scope.", nameof(requestContext));
            }

            try
            {
                var scopes = requestContext.Scopes.ToArray();
                // This automatically uses the current user's ClaimsPrincipal from HTTP context to perform JWT exchange
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Token acquisition returned null or empty token.");
                }

                var expiresOn = ExtractJwtExpiration(accessToken);

                return new AccessToken(accessToken, expiresOn);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new AuthenticationFailedException($"Failed to acquire JWT exchange token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Attempts to extract the expiration time from a JWT token.
        /// </summary>
        /// <param name="jwt">The JWT token to parse.</param>
        /// <returns>The expiration time as a DateTimeOffset, or 15 seconds from now if parsing fails.</returns>
        private static DateTimeOffset ExtractJwtExpiration(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2)
                {
                    // Invalid JWT format
                    return DateTimeOffset.UtcNow.AddSeconds(15);
                }

                string payload = parts[1];

                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2:
                        payload += "==";
                        break;
                    case 3:
                        payload += "=";
                        break;
                }

                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

                using var document = JsonDocument.Parse(payloadJson);

                if (document.RootElement.TryGetProperty("exp", out var expElement))
                {
                    var expUnix = expElement.GetInt64();
                    return DateTimeOffset.FromUnixTimeSeconds(expUnix);
                }
                // No exp claim found
                return DateTimeOffset.UtcNow.AddSeconds(15);
            }
            catch
            {
                // If parsing fails, return short expiration to use fallback
                return DateTimeOffset.UtcNow.AddSeconds(15);
            }
        }
    }
}
