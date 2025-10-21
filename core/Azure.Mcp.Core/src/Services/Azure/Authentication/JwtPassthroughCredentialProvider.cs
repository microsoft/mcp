// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Areas.Server.Configuration;
using Microsoft.AspNetCore.Http;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Token credential provider that passes through JWT bearer tokens from HTTP Authorization header.
/// </summary>
public sealed class JwtPassthroughCredentialProvider : ITokenCredentialProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the JwtPassthroughCredentialProvider class.
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor to read request headers.</param>
    /// <exception cref="ArgumentNullException">Thrown when httpContextAccessor is null.</exception>
    public JwtPassthroughCredentialProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Creates a TokenCredential using the JWT bearer token from the current HTTP Authorization header.
    /// </summary>
    /// <param name="tenant">Optional tenant ID (may be used for validation).</param>
    /// <returns>A TokenCredential that uses the JWT bearer token from the Authorization header.</returns>
    public Task<TokenCredential> CreateAsync(string? tenant = null)
    {
        return Task.FromResult<TokenCredential>(new JwtPassthroughTokenCredential(_httpContextAccessor));
    }

    /// <summary>
    /// Gets the current user identity extracted from the JWT bearer token.
    /// </summary>
    /// <returns>User identity string extracted from the token, or null if not available.</returns>
    public string? GetCurrentUserId()
    {
        // TODO: In the initial phase, we disable caching for JWT passthrough scenarios
        // by injecting NoCacheService implementation. When caching is enabled in the future,
        // this method should extract user identity from the JWT token (sub, oid, etc.)
        // to provide proper cache isolation between users.
        // 
        // For now, return null since no caching occurs anyway.
        return null;
    }

    /// <summary>
    /// Inner TokenCredential implementation that passes through JWT bearer tokens from HTTP Authorization header.
    /// </summary>
    private sealed class JwtPassthroughTokenCredential : TokenCredential
    {
        private const string AuthorizationHeader = "Authorization";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtPassthroughTokenCredential(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Synchronous token acquisition is not supported. Use GetTokenAsync instead.");
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("No HTTP context available. This credential can only be used within an HTTP request.");
            }

            // Extract token from the Authorization header
            var headerValue = httpContext.Request.Headers[AuthorizationHeader].FirstOrDefault();
            if (string.IsNullOrEmpty(headerValue))
            {
                throw new InvalidOperationException("No JWT bearer token found in Authorization header.");
            }

            // Remove "Bearer " prefix if present
            var token = headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? headerValue.Substring(7)
                : headerValue;

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Empty JWT bearer token in Authorization header.");
            }

            // Since we get a fresh token from HTTP headers for each request,
            // so use a short expiration and prevent any caching
            var expiresOn = DateTimeOffset.UtcNow.AddSeconds(15);

            return ValueTask.FromResult(new AccessToken(token, expiresOn));
        }
    }
}
